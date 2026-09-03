using System.Linq;
using UnityEngine;

public class VVEDefenderSelectionUi : MonoBehaviour
{

    public int Cols = 3;
    public int Rows = 3;
    public float Gap = 0.2f;
    public Vector2 CellSize => cardPrefab.GetComponent<BoxCollider2D>().size;

    [SerializeField] VVEDefenderCard cardPrefab;

    void Start()
    {
        VVEManager.OnToggleMenu += ToggleDefenderSelectionUI;
        VVEDefenderUnlocks.UnlocksChanged += RefreshUnlockedCards;
        gameObject.SetActive(VVEManager.Instance.MenuIsOpen);
    }

    void  OnDestroy() {
        VVEManager.OnToggleMenu -= ToggleDefenderSelectionUI;
        VVEDefenderUnlocks.UnlocksChanged -= RefreshUnlockedCards;
    }

    public Vector3 GetDefenderPositionInCatalog(VVEDefender type)
    {
        var entries = VVEDefenderCatalog.Instance.Entries
            .Where(IsAvailable)
            .ToList();
        var index = entries.TakeWhile(entry => entry.prefab != type).Count();

        var width = CellSize.x * Cols;
        var height = CellSize.y * Rows;

        var col = index % Cols;
        var row = index / Cols;

        var x = (-0.5f*width) + (CellSize.x + 0.5f*Gap) * (col + 0.5f);
        var y = -1 * ((-0.5f*height) + (CellSize.y + 0.5f*Gap) * (row + 0.5f));

        return new Vector3(x,y,0);
    }

    public Vector3 GetDefenderPosition(VVEDefender type)
    {
        return transform.position + GetDefenderPositionInCatalog(type);
    }

    void BuildGrid()
    {
        foreach (var entry in VVEDefenderCatalog.Instance.Entries)
        {
            if (!IsAvailable(entry))
                continue;

            if (VVEManager.Instance.SelectedDefenders.Contains(entry.prefab))
                continue;

            var obj = Instantiate(cardPrefab, transform);
            obj.defenderType = entry.prefab;
            obj.transform.position = GetDefenderPosition(entry.prefab);
        }
    }

    bool IsAvailable(VVEDefenderCatalog.Entry entry)
    {
        return entry != null
            && entry.prefab != null
            && !string.IsNullOrEmpty(entry.id)
            && VVEDefenderUnlocks.IsUnlocked(entry.id);
    }

    void RefreshUnlockedCards()
    {
        if (!gameObject.activeSelf)
            return;

        transform.DestroyChildren();
        BuildGrid();
    }

    void ToggleDefenderSelectionUI(bool isOpen)
    {
        gameObject.SetActive(isOpen);
        if (isOpen)
        {
            transform.DestroyChildren();
            BuildGrid();
        }
    }
}
