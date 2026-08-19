using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlantPlacementManager : MonoBehaviour
{
    [SerializeField] private Tilemap placementTilemap;
    [SerializeField] private VVEUsableWallet usableWallet;
    [SerializeField] private VVEHealingPotionUseController healingPotionUseController;
    [SerializeField] private Vector2 placementOffset = new Vector2(-0.2f, 0.2f);
    [SerializeField, Range(0.1f, 1f)] private float previewAlpha = 0.45f;
    [SerializeField] private Color validPreviewTint = Color.white;
    [SerializeField] private Color invalidPreviewTint = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Remove Tool")]
    [SerializeField] private bool enableRemoveTool = true;
    [SerializeField] private KeyCode toggleRemoveToolKey = KeyCode.X;
    [SerializeField] private KeyCode holdRemoveToolKey = KeyCode.LeftShift;

    private Dictionary<Vector3Int, VVEDefender> occupiedCells = new Dictionary<Vector3Int, VVEDefender>();
    private GameObject selectedPlantPrefab;
    private VVEDefenderCard selectedCard;
    private GameObject placementPreview;
    private SpriteRenderer[] previewRenderers;
    private bool removeToolSelected;

    public GameObject SelectedPlantPrefab => selectedPlantPrefab;
    public bool IsRemoveToolSelected => removeToolSelected;

    public void ResetBoard()
    {
        foreach (KeyValuePair<Vector3Int, VVEDefender> occupiedCell in occupiedCells)
        {
            if (occupiedCell.Value != null)
            {
                Destroy(occupiedCell.Value.gameObject);
            }
        }

        occupiedCells.Clear();
        ClearSelection();
    }

    private void Awake()
    {
        if (usableWallet == null)
        {
            usableWallet = VVEUsableWallet.Instance != null ? VVEUsableWallet.Instance : FindAnyObjectByType<VVEUsableWallet>();
        }

        if (healingPotionUseController == null)
        {
            healingPotionUseController = VVEHealingPotionUseController.Instance != null
                ? VVEHealingPotionUseController.Instance
                : FindAnyObjectByType<VVEHealingPotionUseController>();
        }
    }

    private void Update()
    {
        if (enableRemoveTool && toggleRemoveToolKey != KeyCode.None && Input.GetKeyDown(toggleRemoveToolKey))
        {
            ToggleRemoveTool();
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandlePrimaryClick();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ClearSelection();
        }

        UpdatePlacementPreview();
    }

    private void HandlePrimaryClick()
    {
        if (IsAimingHealingPotion())
        {
            if (TryUseHealingPotion())
            {
                ClearSelection();
            }

            return;
        }

        if (TryCollectBoardPickup())
        {
            return;
        }

        if (TrySelectCharacterSlot())
        {
            return;
        }

        if (IsRemoveToolActive())
        {
            TryRemovePlacedCharacter();
            return;
        }

        if (TryUseHealingPotion())
        {
            ClearSelection();
            return;
        }

        TryPlacePlant();
    }

    private bool IsAimingHealingPotion()
    {
        if (healingPotionUseController == null)
        {
            healingPotionUseController = VVEHealingPotionUseController.Instance != null
                ? VVEHealingPotionUseController.Instance
                : FindAnyObjectByType<VVEHealingPotionUseController>();
        }

        return healingPotionUseController != null && healingPotionUseController.IsAiming;
    }

    private bool IsRemoveToolActive()
    {
        return enableRemoveTool
            && (removeToolSelected || (holdRemoveToolKey != KeyCode.None && Input.GetKey(holdRemoveToolKey)));
    }

    private bool TryRemovePlacedCharacter()
    {
        CleanupOccupiedCells();
        if (!TryGetPlacedCharacterAt(GetMouseWorldPosition(), out Vector3Int cellPosition, out VVEDefender character))
        {
            Debug.Log("No placed character to remove here.");
            return false;
        }

        occupiedCells.Remove(cellPosition);
        if (character.Health != null && character.Health.IsAlive)
        {
            character.Health.TakeDamage(character.Health.CurrentHealth);
        }
        else if (character != null)
        {
            Destroy(character.gameObject);
        }

        Debug.Log("Removed character at cell " + cellPosition);
        return true;
    }

    private bool TryGetPlacedCharacterAt(Vector3 worldPosition, out Vector3Int cellPosition, out VVEDefender character)
    {
        cellPosition = placementTilemap.WorldToCell(worldPosition);
        if (occupiedCells.TryGetValue(cellPosition, out character) && character != null)
        {
            return true;
        }

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);
        foreach (Collider2D hit in hits)
        {
            VVEDefender hitCharacter = hit.GetComponentInParent<VVEDefender>();
            if (hitCharacter == null)
            {
                continue;
            }

            if (hitCharacter.HasCell
                && occupiedCells.TryGetValue(hitCharacter.Cell, out VVEDefender occupiedCharacter)
                && occupiedCharacter == hitCharacter)
            {
                cellPosition = hitCharacter.Cell;
                character = hitCharacter;
                return true;
            }

            foreach (KeyValuePair<Vector3Int, VVEDefender> occupiedCell in occupiedCells)
            {
                if (occupiedCell.Value == hitCharacter)
                {
                    cellPosition = occupiedCell.Key;
                    character = hitCharacter;
                    return true;
                }
            }
        }

        character = null;
        return false;
    }

    private bool TryCollectBoardPickup()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPosition);
        foreach (Collider2D hit in hits)
        {
            VVEBoardPickup pickup = hit.GetComponentInParent<VVEBoardPickup>();
            if (pickup == null)
            {
                pickup = hit.GetComponentInChildren<VVEBoardPickup>();
            }

            if (pickup == null)
            {
                continue;
            }

            return pickup.Collect();
        }

        return false;
    }

    private bool TryUseHealingPotion()
    {
        if (healingPotionUseController == null)
        {
            healingPotionUseController = VVEHealingPotionUseController.Instance != null
                ? VVEHealingPotionUseController.Instance
                : FindAnyObjectByType<VVEHealingPotionUseController>();
        }

        return healingPotionUseController != null && healingPotionUseController.TryHandlePrimaryClick(GetMouseWorldPosition());
    }

    private bool TrySelectCharacterSlot()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPosition);
        foreach (Collider2D hit in hits)
        {
            VVEDefenderCard slot = hit.GetComponentInParent<VVEDefenderCard>();
            if (slot == null)
            {
                slot = hit.GetComponentInChildren<VVEDefenderCard>();
            }

            if (slot == null)
            {
                continue;
            }

            if (slot.defenderType == null)
            {
                continue;
            }

            if (slot == selectedCard)
            {
                ClearSelection();
            }
            else
            {
                SelectCharacter(slot);
            }

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

        int selectedCost = selectedCard != null ? selectedCard.Cost : 0;
        if (usableWallet != null && !usableWallet.CanAfford(selectedCost))
        {
            Debug.Log("Not enough diamonds to place " + selectedPlantPrefab.name + ". Cost: " + selectedCost);
            return;
        }

        Vector3 spawnPosition = placementTilemap.GetCellCenterWorld(cellPosition);
        spawnPosition += (Vector3)placementOffset;
        spawnPosition = VVELaneDepth.WithLaneZ(spawnPosition, cellPosition.y);
        if (usableWallet != null && !usableWallet.TrySpendDiamonds(selectedCost))
        {
            Debug.Log("Not enough diamonds to place " + selectedPlantPrefab.name + ". Cost: " + selectedCost);
            return;
        }

        GameObject spawnedPlant = Instantiate(selectedPlantPrefab, spawnPosition, Quaternion.identity);
        VVEDefender boardCharacter = spawnedPlant.GetComponent<VVEDefender>();
        if (boardCharacter == null)
        {
            boardCharacter = spawnedPlant.AddComponent<VVEDefender>();
        }

        boardCharacter.SetCell(cellPosition);

        occupiedCells[cellPosition] = boardCharacter;

        Debug.Log("Placed " + spawnedPlant.name + " at cell " + cellPosition);
    }

    public void SelectCharacter(VVEDefenderCard slot)
    {
        if (slot == null || slot.CharacterPrefab == null)
        {
            return;
        }

        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
        }

        if (healingPotionUseController == null)
        {
            healingPotionUseController = VVEHealingPotionUseController.Instance != null
                ? VVEHealingPotionUseController.Instance
                : FindAnyObjectByType<VVEHealingPotionUseController>();
        }

        if (healingPotionUseController != null)
        {
            healingPotionUseController.CancelAiming();
        }

        selectedCard = slot;
        selectedPlantPrefab = slot.CharacterPrefab;
        removeToolSelected = false;
        selectedCard.SetSelected(true);
        RebuildPlacementPreview();
        Debug.Log("Selected " + selectedPlantPrefab.name);
    }

    public void SelectRemoveTool()
    {
        if (!enableRemoveTool)
        {
            return;
        }

        ClearSelection();
        removeToolSelected = true;
        Debug.Log("Selected remove tool.");
    }

    public void ToggleRemoveTool()
    {
        if (!enableRemoveTool)
        {
            return;
        }

        if (removeToolSelected)
        {
            ClearSelection();
        }
        else
        {
            SelectRemoveTool();
        }
    }

    private void ClearSelection()
    {
        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
        }

        selectedCard = null;
        selectedPlantPrefab = null;
        previewRenderers = null;
        removeToolSelected = false;

        if (placementPreview != null)
        {
            Destroy(placementPreview);
            placementPreview = null;
        }
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
            ? VVELaneDepth.WithLaneZ(placementTilemap.GetCellCenterWorld(cellPosition) + (Vector3)placementOffset, cellPosition.y)
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
        foreach (KeyValuePair<Vector3Int, VVEDefender> occupiedCell in occupiedCells)
        {
            VVEDefender character = occupiedCell.Value;
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
