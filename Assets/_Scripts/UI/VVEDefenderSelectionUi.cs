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
        gameObject.SetActive(VVEManager.Instance.MenuIsOpen);
    }

    void  OnDestroy() {
        VVEManager.OnToggleMenu -= ToggleDefenderSelectionUI;
    }

    public Vector3 GetDefenderPositionInCatalog(VVEDefender type)
    {
        var entries = VVEDefenderCatalog.Instance.Entries;
        var index = entries.TakeWhile(entry => entry.prefab != type).Count();
    
        var prefab = entries[index].prefab;

        var width = CellSize.x * Cols;
        var height = CellSize.y * Rows;

        var col = index % Cols;
        var row = index / Rows;

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
            if (VVEManager.Instance.SelectedDefenders.Contains(entry.prefab))
                continue;

            var obj = Instantiate(cardPrefab, transform);
            obj.defenderType = entry.prefab;
            obj.transform.position = GetDefenderPosition(entry.prefab);
        }
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
