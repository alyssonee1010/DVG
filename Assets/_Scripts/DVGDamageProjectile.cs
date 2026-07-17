using UnityEngine;

public class DVGDamageProjectile : MonoBehaviour
{
    [SerializeField] int damage = 25;
    [SerializeField] float speed = 5f;
    [SerializeField] float lifetime = 5f;
    [SerializeField] bool destroyOnHit = true;

    Vector2 direction = Vector2.right;
    int laneIndex;
    bool useLaneFilter;
    float lifetimeTimer;

    void OnEnable()
    {
        lifetimeTimer = lifetime;
    }

    void Update()
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Vector2 launchDirection, int targetLaneIndex)
    {
        direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector2.right;
        laneIndex = targetLaneIndex;
        useLaneFilter = true;
    }

    public void Launch(Vector2 launchDirection)
    {
        direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector2.right;
        useLaneFilter = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IDVGEnemyLaneWalker enemy = GetEnemyLaneWalker(other);
        if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
        {
            return;
        }

        if (useLaneFilter && enemy.LaneIndex != laneIndex)
        {
            return;
        }

        enemy.Health.TakeDamage(damage);
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }

    IDVGEnemyLaneWalker GetEnemyLaneWalker(Collider2D other)
    {
        MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDVGEnemyLaneWalker enemy)
            {
                return enemy;
            }
        }

        return null;
    }
}
