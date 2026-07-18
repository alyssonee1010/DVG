using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlantPlacementManager : MonoBehaviour
{
    [SerializeField] private Tilemap placementTilemap;
    [SerializeField] private Vector2 placementOffset = new Vector2(-0.2f, 0.2f);
    [SerializeField, Range(0.1f, 1f)] private float previewAlpha = 0.45f;
    [SerializeField] private Color validPreviewTint = Color.white;
    [SerializeField] private Color invalidPreviewTint = new Color(1f, 0.35f, 0.35f, 1f);

    private Dictionary<Vector3Int, DVGBoardCharacter> occupiedCells = new Dictionary<Vector3Int, DVGBoardCharacter>();
    private GameObject selectedPlantPrefab;
    private DVGPlacementCharacterSlot selectedSlot;
    private GameObject placementPreview;
    private SpriteRenderer[] previewRenderers;

    public GameObject SelectedPlantPrefab => selectedPlantPrefab;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (TryCollectBoardPickup())
            {
                return;
            }

            if (TrySelectCharacterSlot())
            {
                return;
            }

            TryPlacePlant();
        }

        UpdatePlacementPreview();
    }

    private bool TryCollectBoardPickup()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPosition);
        foreach (Collider2D hit in hits)
        {
            DVGBoardPickup pickup = hit.GetComponentInParent<DVGBoardPickup>();
            if (pickup == null)
            {
                pickup = hit.GetComponentInChildren<DVGBoardPickup>();
            }

            if (pickup == null)
            {
                continue;
            }

            pickup.Collect();
            return true;
        }

        return false;
    }

    private bool TrySelectCharacterSlot()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPosition);
        foreach (Collider2D hit in hits)
        {
            DVGPlacementCharacterSlot slot = hit.GetComponentInParent<DVGPlacementCharacterSlot>();
            if (slot == null)
            {
                slot = hit.GetComponentInChildren<DVGPlacementCharacterSlot>();
            }

            if (slot == null)
            {
                continue;
            }

            if (slot.CharacterPrefab == null)
            {
                continue;
            }

            SelectCharacter(slot);
            return true;
        }

        return false;
    }

    private void TryPlacePlant()
    {
        CleanupOccupiedCells();
        if (selectedPlantPrefab == null)
        {
            Debug.Log("Select a character first.");
            return;
        }

        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        Vector3Int cellPosition = placementTilemap.WorldToCell(mouseWorldPosition);

        Debug.Log("Clicked cell: " + cellPosition);
        Debug.Log("Has placement tile: " + placementTilemap.HasTile(cellPosition));

        if (!placementTilemap.HasTile(cellPosition))
        {
            Debug.Log("Cannot place here. This is not a valid placement tile.");
            return;
        }

        if (occupiedCells.ContainsKey(cellPosition))
        {
            Debug.Log("Cannot place here. This tile is already occupied.");
            return;
        }

        Vector3 spawnPosition = placementTilemap.GetCellCenterWorld(cellPosition);
        spawnPosition += (Vector3)placementOffset;
        GameObject spawnedPlant = Instantiate(selectedPlantPrefab, spawnPosition, Quaternion.identity);
        DVGBoardCharacter boardCharacter = spawnedPlant.GetComponent<DVGBoardCharacter>();
        if (boardCharacter == null)
        {
            boardCharacter = spawnedPlant.AddComponent<DVGBoardCharacter>();
        }

        boardCharacter.SetCell(cellPosition);

        occupiedCells[cellPosition] = boardCharacter;

        Debug.Log("Placed " + spawnedPlant.name + " at cell " + cellPosition);
    }

    public void SelectCharacter(DVGPlacementCharacterSlot slot)
    {
        if (slot == null || slot.CharacterPrefab == null)
        {
            return;
        }

        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
        }

        selectedSlot = slot;
        selectedPlantPrefab = slot.CharacterPrefab;
        selectedSlot.SetSelected(true);
        RebuildPlacementPreview();
        Debug.Log("Selected " + selectedPlantPrefab.name);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;
        return mouseWorldPosition;
    }

    private void RebuildPlacementPreview()
    {
        if (placementPreview != null)
        {
            Destroy(placementPreview);
        }

        if (selectedPlantPrefab == null)
        {
            placementPreview = null;
            previewRenderers = null;
            return;
        }

        placementPreview = Instantiate(selectedPlantPrefab);
        placementPreview.name = selectedPlantPrefab.name + " Placement Preview";
        DisablePreviewGameplay(placementPreview);
        previewRenderers = placementPreview.GetComponentsInChildren<SpriteRenderer>(true);
        SetPreviewVisible(true);
        UpdatePlacementPreview();
    }

    private void UpdatePlacementPreview()
    {
        if (placementPreview == null)
        {
            return;
        }

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3Int cellPosition = placementTilemap.WorldToCell(mouseWorldPosition);
        bool validCell = placementTilemap.HasTile(cellPosition) && !occupiedCells.ContainsKey(cellPosition);

        Vector3 previewPosition = validCell
            ? placementTilemap.GetCellCenterWorld(cellPosition) + (Vector3)placementOffset
            : mouseWorldPosition;

        placementPreview.transform.position = previewPosition;
        ApplyPreviewTint(validCell ? validPreviewTint : invalidPreviewTint);
    }

    private void DisablePreviewGameplay(GameObject preview)
    {
        Animator[] animators = preview.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
        {
            animator.enabled = false;
        }

        Collider2D[] colliders = preview.GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        MonoBehaviour[] behaviours = preview.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            behaviour.enabled = false;
        }
    }

    private void SetPreviewVisible(bool visible)
    {
        if (placementPreview != null)
        {
            placementPreview.SetActive(visible);
        }
    }

    private void ApplyPreviewTint(Color tint)
    {
        if (previewRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in previewRenderers)
        {
            Color color = tint;
            color.a *= previewAlpha;
            spriteRenderer.color = color;
        }
    }

    private void CleanupOccupiedCells()
    {
        List<Vector3Int> clearedCells = new List<Vector3Int>();
        foreach (KeyValuePair<Vector3Int, DVGBoardCharacter> occupiedCell in occupiedCells)
        {
            DVGBoardCharacter character = occupiedCell.Value;
            if (character == null || character.Health == null || !character.Health.IsAlive)
            {
                clearedCells.Add(occupiedCell.Key);
            }
        }

        foreach (Vector3Int cell in clearedCells)
        {
            occupiedCells.Remove(cell);
        }
    }
}
