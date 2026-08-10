using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(VVEHealth))]
public class VVEBoardCharacter : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] int maxHealth = 100;

    [SerializeField] int sortingOrderBase = 1000;
    [SerializeField] int sortingOrderPerRow = 120;
    [SerializeField] int sortingOrderOffset;

    VVEHealth health;
    VVEWorldHealthBar healthBar;

    public Vector3Int Cell { get; private set; }
    public bool HasCell { get; private set; }
    public VVEHealth Health => health;

    void Awake()
    {
        health = GetComponent<VVEHealth>();
        if (health == null)
        {
            health = gameObject.AddComponent<VVEHealth>();
        }

        health.SetMaxHealth(maxHealth);
        healthBar = GetComponent<VVEWorldHealthBar>();
        if (healthBar == null)
        {
            healthBar = gameObject.AddComponent<VVEWorldHealthBar>();
        }
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
