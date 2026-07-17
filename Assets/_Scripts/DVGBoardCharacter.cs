using UnityEngine;
using UnityEngine.Rendering;

public class DVGBoardCharacter : MonoBehaviour
{
    [SerializeField] int sortingOrderBase = 1000;
    [SerializeField] int sortingOrderPerRow = 120;
    [SerializeField] int sortingOrderOffset;

    public Vector3Int Cell { get; private set; }
    public bool HasCell { get; private set; }

    public void SetCell(Vector3Int cell)
    {
        Cell = cell;
        HasCell = true;
        ApplyRowSorting(cell.y);
    }

    public void ApplyRowSorting(int row)
    {
        SortingGroup sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup == null)
        {
            sortingGroup = gameObject.AddComponent<SortingGroup>();
        }

        sortingGroup.sortingOrder = sortingOrderBase - row * sortingOrderPerRow + sortingOrderOffset;
    }
}
