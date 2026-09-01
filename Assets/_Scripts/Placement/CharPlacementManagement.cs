using System.Collections.Generic;
using UnityEngine;

public class PlantPlacementManager : MonoBehaviour
{
    [SerializeField] private VVEBoardGrid boardGrid;
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
    [SerializeField] private VVERemoveToolCursor removeTool;
    [SerializeField] private VVECharacterTargetHighlight removeTargetHighlight;
    [SerializeField, Range(0f, 1f)] private float removeTargetAlpha = 0.45f;

    private Dictionary<Vector2Int, VVEDefender> occupiedCells = new Dictionary<Vector2Int, VVEDefender>();
    private GameObject selectedPlantPrefab;
    private VVEDefenderCard selectedCard;
    private GameObject placementPreview;
    private SpriteRenderer[] previewRenderers;
    private bool removeToolSelected;
    private bool cursorShowingRemoveTool;

    public GameObject SelectedPlantPrefab => selectedPlantPrefab;
    public bool IsRemoveToolSelected => removeToolSelected;

    public void ResetBoard()
    {
        VVECharacterPotionTargeting.Cancel();

        foreach (KeyValuePair<Vector2Int, VVEDefender> occupiedCell in occupiedCells)
        {
            if (occupiedCell.Value != null)
            {
                Destroy(occupiedCell.Value.gameObject);
            }
        }

        occupiedCells.Clear();
        ClearSelection();
        ClearRemainingPickups();
    }

    // Diamonds/potions dropped during the level (and never clicked) shouldn't carry over into
    // the next one lying on the board.
    void ClearRemainingPickups()
    {
        VVEBoardPickup[] pickups = FindObjectsByType<VVEBoardPickup>(FindObjectsSortMode.None);
        foreach (VVEBoardPickup pickup in pickups)
        {
            if (pickup != null)
            {
                Destroy(pickup.gameObject);
            }
        }
    }

    private void Awake()
    {
        if (usableWallet == null)
        {
            usableWallet = VVEUsableWallet.Instance != null ? VVEUsableWallet.Instance : FindAnyObjectByType<VVEUsableWallet>();
        }

        if (boardGrid == null)
        {
            boardGrid = FindAnyObjectByType<VVEBoardGrid>();
        }

        if (healingPotionUseController == null)
        {
            healingPotionUseController = VVEHealingPotionUseController.Instance != null
                ? VVEHealingPotionUseController.Instance
                : FindAnyObjectByType<VVEHealingPotionUseController>();
        }

        if (removeTool != null)
        {
            removeTool.FollowCursor(false);
        }

        if (removeTargetHighlight == null)
        {
            removeTargetHighlight = gameObject.AddComponent<VVECharacterTargetHighlight>();
        }

        removeTargetHighlight.ConfigureTransparency(removeTargetAlpha);
    }

    private void Update()
    {
        if (enableRemoveTool && toggleRemoveToolKey != KeyCode.None && Input.GetKeyDown(toggleRemoveToolKey))
        {
            ToggleRemoveTool();
        }

        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectDefenderHotkey(i);
                break;
            }
        }

        UpdateRemoveToolCursor();

        if (Input.GetMouseButtonDown(0))
        {
            HandlePrimaryClick();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ClearSelection();
        }

        UpdateRemoveTargetHighlight();
        UpdatePlacementPreview();
    }

    private void HandlePrimaryClick()
    {
        if (VVECharacterPotionTargeting.TryHandlePrimaryClick(GetMouseWorldPosition()))
        {
            ClearSelection();
            return;
        }

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

        if (TryClickRemoveToolIcon())
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

    // Clicking the remove-tool icon does the same thing as pressing toggleRemoveToolKey. Only
    // checked while the tool isn't already active, since once active the icon itself is sitting
    // right under the mouse (it's following the cursor), so a click there is meant for the board.
    private bool TryClickRemoveToolIcon()
    {
        if (removeTool == null || IsRemoveToolActive())
        {
            return false;
        }

        if (!removeTool.ContainsPoint(GetMouseWorldPosition()))
        {
            return false;
        }

        ToggleRemoveTool();
        return true;
    }

    private bool TryRemovePlacedCharacter()
    {
        CleanupOccupiedCells();
        if (!TryGetPlacedCharacterAt(GetMouseWorldPosition(), out Vector2Int cellPosition, out VVEDefender character))
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

    private bool TryGetPlacedCharacterAt(Vector3 worldPosition, out Vector2Int cellPosition, out VVEDefender character)
    {
        cellPosition = default;
        character = null;
        if (boardGrid == null)
        {
            return false;
        }

        boardGrid.TryGetCellFromWorldPosition(worldPosition, out int row, out int column);
        cellPosition = new Vector2Int(column, row);
        if (occupiedCells.TryGetValue(cellPosition, out character) && character != null)
        {
            return true;
        }

        character = VVEWorldPointer.FindClosest<VVEDefender>(
            worldPosition,
            0f,
            candidate => candidate != null && occupiedCells.ContainsValue(candidate));
        if (character == null)
        {
            return false;
        }

        if (character.HasCell
            && occupiedCells.TryGetValue(character.Cell, out VVEDefender occupiedCharacter)
            && occupiedCharacter == character)
        {
            cellPosition = character.Cell;
            return true;
        }

        foreach (KeyValuePair<Vector2Int, VVEDefender> occupiedCell in occupiedCells)
        {
            if (occupiedCell.Value == character)
            {
                cellPosition = occupiedCell.Key;
                return true;
            }
        }

        return false;
    }

    private bool TryCollectBoardPickup()
    {
        VVEBoardPickup pickup = VVEWorldPointer.FindClosest<VVEBoardPickup>(GetMouseWorldPosition(), 0f);
        return pickup != null && pickup.Collect();
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

        int row = 0;
        int column = 0;
        if (boardGrid == null || !boardGrid.TryGetCellFromWorldPosition(mouseWorldPosition, out row, out column))
        {
            Debug.Log("Cannot place here. This is not a valid placement tile.");
            return;
        }

        Vector2Int cellPosition = new Vector2Int(column, row);
        Debug.Log("Clicked cell: " + cellPosition);

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

        Vector3 spawnPosition = boardGrid.GetCellCenterWorld(row, column);
        spawnPosition += (Vector3)placementOffset;
        spawnPosition = VVELaneDepth.WithLaneZ(spawnPosition, row);
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

    // Pressing "1".."6" selects the defender at that index in VVEManager.Instance.SelectedDefenders
    // (key "1" -> index 0), same as clicking its card in the top bar - VVEDefenderSelectBar
    // instantiates that bar's cards in SelectedDefenders order, so the child index lines up
    // directly. Routed through SelectCharacter so the existing selected-card scale-up
    // (VVEDefenderCard.SetSelected) and placement-preview rebuild happen exactly as they do for
    // a mouse click, instead of duplicating that logic here.
    void SelectDefenderHotkey(int index)
    {
        if (VVEManager.Instance == null || index >= VVEManager.Instance.SelectedDefenders.Count)
        {
            return;
        }

        VVEDefenderSelectBar selectBar = VVEUiWidgetRefs.Instance != null ? VVEUiWidgetRefs.Instance.defenderSelectionTopBar : null;
        if (selectBar == null || selectBar.cardsContainer == null || index >= selectBar.cardsContainer.childCount)
        {
            return;
        }

        VVEDefenderCard card = selectBar.cardsContainer.GetChild(index).GetComponent<VVEDefenderCard>();
        if (card != null)
        {
            SelectCharacter(card);
        }
    }

    public void SelectCharacter(VVEDefenderCard slot)
    {
        if (VVEManager.Instance.MenuIsOpen)
            return;

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

    public void SelectRemoveTool(bool isSelected)
    {
        ClearSelection(); 
        if (removeTool != null)
        {
            removeTool.FollowCursor(isSelected);
        }

        if (isSelected){
            if (!enableRemoveTool)
            {
                return;
            }

            removeToolSelected = true;
            Debug.Log("Selected remove tool.");
        }
    }

    public void ToggleRemoveTool()
    {
        if (!enableRemoveTool)
        {
            return;
        }

        if (removeToolSelected)
        {
            SelectRemoveTool(false);
        }
        else
        {
            SelectRemoveTool(true);
        }
    }

    // Makes the remove-tool icon (VVERemoveToolCursor) follow the mouse whenever the tool is
    // active (toggled via X, clicked, or held via Shift, see IsRemoveToolActive), and sends it
    // back to its home position otherwise, only on an actual state change rather than every frame.
    private void UpdateRemoveToolCursor()
    {
        bool removeActive = IsRemoveToolActive();
        if (removeActive == cursorShowingRemoveTool)
        {
            return;
        }

        cursorShowingRemoveTool = removeActive;

        if (removeTool != null)
        {
            removeTool.FollowCursor(removeActive);
        }
    }

    private void UpdateRemoveTargetHighlight()
    {
        if (removeTargetHighlight == null)
        {
            return;
        }

        if (!IsRemoveToolActive())
        {
            removeTargetHighlight.Clear();
            return;
        }

        CleanupOccupiedCells();
        TryGetPlacedCharacterAt(GetMouseWorldPosition(), out _, out VVEDefender character);
        removeTargetHighlight.Show(character);
    }

    private void OnDisable()
    {
        if (cursorShowingRemoveTool && removeTool != null)
        {
            removeTool.FollowCursor(false);
        }

        cursorShowingRemoveTool = false;
        if (removeTargetHighlight != null)
        {
            removeTargetHighlight.Clear();
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

        if (removeTargetHighlight != null)
        {
            removeTargetHighlight.Clear();
        }

        if (placementPreview != null)
        {
            Destroy(placementPreview);
            placementPreview = null;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        return VVEWorldPointer.GetPosition();
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
        int row = 0;
        int column = 0;
        bool validCell = boardGrid != null
            && boardGrid.TryGetCellFromWorldPosition(mouseWorldPosition, out row, out column)
            && !occupiedCells.ContainsKey(new Vector2Int(column, row));

        Vector3 previewPosition = validCell
            ? VVELaneDepth.WithLaneZ(boardGrid.GetCellCenterWorld(row, column) + (Vector3)placementOffset, row)
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
        List<Vector2Int> clearedCells = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, VVEDefender> occupiedCell in occupiedCells)
        {
            VVEDefender character = occupiedCell.Value;
            if (character == null || character.Health == null || !character.Health.IsAlive)
            {
                clearedCells.Add(occupiedCell.Key);
            }
        }

        foreach (Vector2Int cell in clearedCells)
        {
            occupiedCells.Remove(cell);
        }
    }
}
