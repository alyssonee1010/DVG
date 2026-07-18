using UnityEngine;

public class DVGDamageProjectile : MonoBehaviour
{
    [SerializeField] int damage = 25;
    [SerializeField] float speed = 5f;
    [SerializeField] float lifetime = 5f;
    [SerializeField] bool destroyOnHit = true;
    [SerializeField] bool useLaneBasedHit = true;
    [SerializeField] float hitPadding = 0.15f;

    Vector2 direction = Vector2.right;
    int laneIndex;
    bool useLaneFilter;
    bool returnToPool;
    bool hasHit;
    float lifetimeTimer;

    void OnEnable()
    {
        lifetimeTimer = lifetime;
        hasHit = false;
    }

    void Update()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + (Vector3)(direction.normalized * speed * Time.deltaTime);
        transform.position = endPosition;

        TryDamageAlongPath(startPosition, endPosition);

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Finish();
        }
    }

    public void SetReturnToPool(bool value)
    {
        returnToPool = value;
    }

    public void Launch(Vector2 launchDirection, int targetLaneIndex)
    {
        hasHit = false;
        direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector2.right;
        laneIndex = targetLaneIndex;
        useLaneFilter = true;
        ApplyDirectionVisual();
    }

    public void Launch(Vector2 launchDirection)
    {
        hasHit = false;
        direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector2.right;
        useLaneFilter = false;
        ApplyDirectionVisual();
    }

    void ApplyDirectionVisual()
    {
        transform.right = direction;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    void TryDamage(Collider2D other)
    {
        if (hasHit)
        {
            return;
        }

        IDVGEnemyLaneWalker enemy = GetEnemyLaneWalker(other);
        if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
        {
            return;
        }

        if (useLaneFilter && enemy.LaneIndex != laneIndex)
        {
            return;
        }

        DamageEnemy(enemy);
    }

    void TryDamageAlongPath(Vector3 startPosition, Vector3 endPosition)
    {
        if (hasHit)
        {
            return;
        }

        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not IDVGEnemyLaneWalker enemy || enemy.gameObject == null)
            {
                continue;
            }

            if (enemy.Health == null || !enemy.Health.IsAlive)
            {
                continue;
            }

            if (useLaneFilter && enemy.LaneIndex != laneIndex)
            {
                continue;
            }

            Bounds enemyBounds = GetEnemyBounds(enemy);
            enemyBounds.Expand(hitPadding * 2f);
            bool hitEnemy = useLaneFilter && useLaneBasedHit
                ? SegmentOverlapsAxis(startPosition.x, endPosition.x, enemyBounds.min.x, enemyBounds.max.x)
                : SegmentIntersectsBounds(startPosition, endPosition, enemyBounds);

            if (!hitEnemy)
            {
                continue;
            }

            DamageEnemy(enemy);
            return;
        }
    }

    void DamageEnemy(IDVGEnemyLaneWalker enemy)
    {
        hasHit = true;
        enemy.Health.TakeDamage(damage);
        if (destroyOnHit)
        {
            Finish();
        }
    }

    void Finish()
    {
        if (returnToPool)
        {
            gameObject.SetActive(false);
            return;
        }

        Destroy(gameObject);
    }

    Bounds GetEnemyBounds(IDVGEnemyLaneWalker enemy)
    {
        Collider2D collider = enemy.gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds;
        }

        collider = enemy.gameObject.GetComponentInChildren<Collider2D>();
        if (collider != null)
        {
            return collider.bounds;
        }

        return new Bounds(enemy.gameObject.transform.position, Vector3.one * 0.5f);
    }

    bool SegmentOverlapsAxis(float start, float end, float min, float max)
    {
        float segmentMin = Mathf.Min(start, end);
        float segmentMax = Mathf.Max(start, end);
        return segmentMax >= min && segmentMin <= max;
    }

    bool SegmentIntersectsBounds(Vector3 startPosition, Vector3 endPosition, Bounds bounds)
    {
        if (bounds.Contains(startPosition) || bounds.Contains(endPosition))
        {
            return true;
        }

        Vector2 start = startPosition;
        Vector2 end = endPosition;
        Vector2 delta = end - start;
        float enter = 0f;
        float exit = 1f;

        return ClipSegmentAxis(start.x, delta.x, bounds.min.x, bounds.max.x, ref enter, ref exit)
            && ClipSegmentAxis(start.y, delta.y, bounds.min.y, bounds.max.y, ref enter, ref exit);
    }

    bool ClipSegmentAxis(float start, float delta, float min, float max, ref float enter, ref float exit)
    {
        if (Mathf.Approximately(delta, 0f))
        {
            return start >= min && start <= max;
        }

        float inverseDelta = 1f / delta;
        float first = (min - start) * inverseDelta;
        float second = (max - start) * inverseDelta;
        if (first > second)
        {
            float swap = first;
            first = second;
            second = swap;
        }

        enter = Mathf.Max(enter, first);
        exit = Mathf.Min(exit, second);
        return enter <= exit;
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
