using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(DVGHealth))]
public class DVGBoardCharacter : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] int maxHealth = 100;

    [SerializeField] int sortingOrderBase = 1000;
    [SerializeField] int sortingOrderPerRow = 120;
    [SerializeField] int sortingOrderOffset;

    DVGHealth health;

    public Vector3Int Cell { get; private set; }
    public bool HasCell { get; private set; }
    public DVGHealth Health => health;

    void Awake()
    {
        health = GetComponent<DVGHealth>();
        if (health == null)
        {
            health = gameObject.AddComponent<DVGHealth>();
        }

        health.SetMaxHealth(maxHealth);
    }

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
