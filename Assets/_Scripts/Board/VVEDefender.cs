using UnityEngine;

[RequireComponent(typeof(VVEHealth))]
public class VVEDefender : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] public int cost = 12;
    [SerializeField] int maxHealth = 100;

    VVEHealth health;
    VVEWorldHealthBar healthBar;

    public Vector3Int Cell { get; private set; }
    public bool HasCell { get; private set; }
    public VVEHealth Health => health;

    void Start()
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
        ApplyLaneDepth(cell.y);
    }

    public void ApplyRowSorting(int row)
    {
        ApplyLaneDepth(row);
    }

    public void ApplyLaneDepth(int laneIndex)
    {
        transform.position = VVELaneDepth.WithLaneZ(transform.position, laneIndex);
        VVELaneDepth.ApplyGameplaySortingGroup(gameObject);
    }
}
