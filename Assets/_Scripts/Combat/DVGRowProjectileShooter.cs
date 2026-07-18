using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[RequireComponent(typeof(DVGBoardCharacter))]
public class DVGRowProjectileShooter : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] Vector2 firePointOffset = new Vector2(0.65f, 0.25f);
    [SerializeField] Vector2 projectileDirection = Vector2.right;
    [SerializeField] float fireInterval = 1.25f;

    [Header("Pooling")]
    [SerializeField] bool useProjectilePool = true;
    [SerializeField] int preloadProjectileCount = 6;
    [SerializeField] int maxPooledProjectiles = 24;

    [Header("Targeting")]
    [FormerlySerializedAs("detectionRange")]
    [Tooltip("How far this character can see and start shooting along its row.")]
    [Min(0f)]
    [SerializeField] float sightRange = 12f;
    [Tooltip("How close an enemy can be before this character stops considering it a valid shooting target.")]
    [Min(0f)]
    [SerializeField] float minimumFireDistance = 0.2f;
    [Tooltip("Only used before the character is placed on a board cell.")]
    [Min(0f)]
    [SerializeField] float laneTolerance = 0.45f;

    [Header("Animation")]
    [SerializeField] bool playShootAnimation = true;
    [SerializeField] bool waitForShootAnimationEvent;
    [SerializeField] string shootTriggerName = "Attack";

    [Header("Projectile Sorting")]
    [SerializeField] int projectileSortingOrderBase = 1000;
    [SerializeField] int projectileSortingOrderPerRow = 120;
    [SerializeField] int projectileSortingOrderOffset = 50;

    DVGBoardCharacter boardCharacter;
    Animator animator;
    IDVGEnemyLaneWalker currentTarget;
    readonly List<DVGDamageProjectile> projectilePool = new List<DVGDamageProjectile>();
    float fireTimer;
    bool waitingForShootAnimationEvent;

    void Awake()
    {
        boardCharacter = GetComponent<DVGBoardCharacter>();
        animator = GetComponent<Animator>();
        PreloadProjectiles();
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (projectilePrefab == null || !TryFindTarget(out IDVGEnemyLaneWalker target))
        {
            currentTarget = null;
            waitingForShootAnimationEvent = false;
            ResetShootAnimationTrigger();
            return;
        }

        currentTarget = target;
        if (fireTimer > 0f || waitingForShootAnimationEvent)
        {
            return;
        }

        fireTimer = Mathf.Max(0.01f, fireInterval);
        bool triggeredAnimation = PlayShootAnimation();
        if (waitForShootAnimationEvent && triggeredAnimation)
        {
            waitingForShootAnimationEvent = true;
            return;
        }

        ShootProjectile();
    }

    public void ShootProjectile()
    {
        waitingForShootAnimationEvent = false;
        if (projectilePrefab == null)
        {
            return;
        }

        int laneIndex = GetLaneIndex();
        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.TransformPoint(firePointOffset);

        DVGDamageProjectile projectile = GetProjectile();
        if (projectile == null)
        {
            return;
        }

        GameObject projectileObject = projectile.gameObject;
        projectileObject.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        ApplyProjectileSorting(projectileObject, laneIndex);

        projectileObject.SetActive(true);
        projectile.Launch(projectileDirection, laneIndex);
    }

    public void ShootProjectileAnimationEvent()
    {
        ShootProjectile();
    }

    bool TryFindTarget(out IDVGEnemyLaneWalker bestTarget)
    {
        bestTarget = null;
        float bestForwardDistance = float.PositiveInfinity;
        Vector2 forward = projectileDirection.sqrMagnitude > 0f ? projectileDirection.normalized : Vector2.right;
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

            if (!IsInSameLane(enemy))
            {
                continue;
            }

            Vector2 toEnemy = enemy.gameObject.transform.position - transform.position;
            float forwardDistance = Vector2.Dot(toEnemy, forward);
            if (forwardDistance < minimumFireDistance || forwardDistance > sightRange)
            {
                continue;
            }

            if (forwardDistance < bestForwardDistance)
            {
                bestForwardDistance = forwardDistance;
                bestTarget = enemy;
            }
        }

        return bestTarget != null;
    }

    bool IsInSameLane(IDVGEnemyLaneWalker enemy)
    {
        if (boardCharacter != null && boardCharacter.HasCell)
        {
            return enemy.LaneIndex == boardCharacter.Cell.y;
        }

        return Mathf.Abs(enemy.gameObject.transform.position.y - transform.position.y) <= laneTolerance;
    }

    int GetLaneIndex()
    {
        if (boardCharacter != null && boardCharacter.HasCell)
        {
            return boardCharacter.Cell.y;
        }

        return currentTarget != null ? currentTarget.LaneIndex : 0;
    }

    void PreloadProjectiles()
    {
        if (!useProjectilePool || projectilePrefab == null)
        {
            return;
        }

        int count = Mathf.Clamp(preloadProjectileCount, 0, Mathf.Max(0, maxPooledProjectiles));
        for (int i = projectilePool.Count; i < count; i++)
        {
            CreatePooledProjectile();
        }
    }

    DVGDamageProjectile GetProjectile()
    {
        if (!useProjectilePool)
        {
            return CreateProjectile(false);
        }

        for (int i = 0; i < projectilePool.Count; i++)
        {
            DVGDamageProjectile pooledProjectile = projectilePool[i];
            if (pooledProjectile != null && !pooledProjectile.gameObject.activeSelf)
            {
                return pooledProjectile;
            }
        }

        if (projectilePool.Count >= maxPooledProjectiles)
        {
            return null;
        }

        return CreatePooledProjectile();
    }

    DVGDamageProjectile CreatePooledProjectile()
    {
        DVGDamageProjectile projectile = CreateProjectile(true);
        if (projectile != null)
        {
            projectilePool.Add(projectile);
            projectile.gameObject.SetActive(false);
        }

        return projectile;
    }

    DVGDamageProjectile CreateProjectile(bool pooled)
    {
        GameObject projectileObject = Instantiate(projectilePrefab);
        DVGDamageProjectile projectile = projectileObject.GetComponent<DVGDamageProjectile>();
        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<DVGDamageProjectile>();
        }

        projectile.SetReturnToPool(pooled);
        return projectile;
    }

    void ApplyProjectileSorting(GameObject projectileObject, int laneIndex)
    {
        int sortingOrder = projectileSortingOrderBase - laneIndex * projectileSortingOrderPerRow + projectileSortingOrderOffset;

        SortingGroup sortingGroup = projectileObject.GetComponent<SortingGroup>();
        if (sortingGroup != null)
        {
            sortingGroup.sortingOrder = sortingOrder;
            return;
        }

        SpriteRenderer[] spriteRenderers = projectileObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    bool PlayShootAnimation()
    {
        if (!playShootAnimation || animator == null || string.IsNullOrWhiteSpace(shootTriggerName))
        {
            return false;
        }

        animator.ResetTrigger(shootTriggerName);
        animator.SetTrigger(shootTriggerName);
        return true;
    }

    void ResetShootAnimationTrigger()
    {
        if (animator != null && !string.IsNullOrWhiteSpace(shootTriggerName))
        {
            animator.ResetTrigger(shootTriggerName);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 start = firePoint != null
            ? firePoint.position
            : transform.TransformPoint(firePointOffset);
        Vector3 direction = projectileDirection.sqrMagnitude > 0f
            ? (Vector3)projectileDirection.normalized
            : Vector3.right;

        Gizmos.color = new Color(1f, 0.75f, 0.1f, 0.9f);
        Gizmos.DrawLine(start, start + direction * sightRange);
        Gizmos.DrawWireSphere(start + direction * sightRange, 0.08f);

        Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.65f);
        Gizmos.DrawWireSphere(start + direction * minimumFireDistance, 0.06f);
    }
}
