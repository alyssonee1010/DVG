using UnityEngine;

public class VVEDamageProjectile : MonoBehaviour
{
    [SerializeField] int damage = 25;
    [SerializeField] float recoilMultiplier = 1f;
    [SerializeField] float speed = 5f;
    [SerializeField] float lifetime = 5f;
    [SerializeField] bool destroyOnHit = true;
    [SerializeField] bool useLaneBasedHit = true;
    [SerializeField] float hitPadding = 0.15f;
    [SerializeField, Min(0f)] float depthTolerance = VVELaneDepth.DefaultDepthTolerance;

    Vector2 direction = Vector2.right;
    int laneIndex;
    bool useLaneFilter;
    bool returnToPool;
    bool hasHit;
    float lifetimeTimer;
    VVEHitRecoil stunSource;

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

    public void SetStunSource(VVEHitRecoil source)
    {
        stunSource = source;
    }

    public void SetDamage(int value)
    {
        damage = Mathf.Max(0, value);
    }

    public void Launch(Vector2 launchDirection, int targetLaneIndex)
    {
        hasHit = false;
        direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector2.right;
        laneIndex = targetLaneIndex;
        useLaneFilter = true;
        transform.position = VVELaneDepth.WithLaneZ(transform.position, laneIndex);
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

        IVVEEnemyLaneWalker enemy = GetEnemyLaneWalker(other);
        if (!TryGetEnemyObject(enemy, out _) || enemy.Health == null || !enemy.Health.IsAlive)
        {
            return;
        }

        if (!PassesDepthFilter(enemy))
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
            if (behaviour is not IVVEEnemyLaneWalker enemy || !TryGetEnemyObject(enemy, out GameObject enemyObject))
            {
                continue;
            }

            if (enemy.Health == null || !enemy.Health.IsAlive)
            {
                continue;
            }

            if (!PassesDepthFilter(enemy))
            {
                continue;
            }

            Bounds enemyBounds = GetEnemyBounds(enemyObject);
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

    void DamageEnemy(IVVEEnemyLaneWalker enemy)
    {
        hasHit = true;
        VVEHealth enemyHealth = enemy.Health;
        enemyHealth.TakeDamage(damage, recoilMultiplier);
        if (enemyHealth.IsAlive && stunSource != null && TryGetEnemyObject(enemy, out GameObject enemyObject))
        {
            stunSource.TryStunTarget(enemyObject);
        }

        if (destroyOnHit)
        {
            Finish();
        }
    }

    bool PassesDepthFilter(IVVEEnemyLaneWalker enemy)
    {
        if (!useLaneFilter)
        {
            return true;
        }

        if (enemy.LaneIndex != laneIndex)
        {
            return false;
        }

        return TryGetEnemyObject(enemy, out GameObject enemyObject)
            && VVELaneDepth.IsSameDepth(transform, enemyObject.transform, depthTolerance);
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

    Bounds GetEnemyBounds(GameObject enemyObject)
    {
        Collider2D collider = enemyObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds;
        }

        collider = enemyObject.GetComponentInChildren<Collider2D>();
        if (collider != null)
        {
            return collider.bounds;
        }

        return new Bounds(enemyObject.transform.position, Vector3.one * 0.5f);
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

    IVVEEnemyLaneWalker GetEnemyLaneWalker(Collider2D other)
    {
        MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IVVEEnemyLaneWalker enemy)
            {
                return enemy;
            }
        }

        return null;
    }

    bool TryGetEnemyObject(IVVEEnemyLaneWalker enemy, out GameObject enemyObject)
    {
        enemyObject = null;
        if (enemy is not MonoBehaviour enemyBehaviour || enemyBehaviour == null)
        {
            return false;
        }

        enemyObject = enemyBehaviour.gameObject;
        return enemyObject != null;
    }
}
