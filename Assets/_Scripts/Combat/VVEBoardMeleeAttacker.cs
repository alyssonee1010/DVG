using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(VVEBoardCharacter))]
public class VVEBoardMeleeAttacker : MonoBehaviour
{
    [SerializeField] float attackRange = 1.1f;
    [SerializeField] float attackCooldown = 1.1f;
    [SerializeField] int attackDamage = 20;
    [SerializeField] float recoilMultiplier = 1f;
    [SerializeField] Vector2 attackDirection = Vector2.right;
    [FormerlySerializedAs("laneTolerance")]
    [SerializeField, Min(0f)] float depthTolerance = VVELaneDepth.DefaultDepthTolerance;
    [SerializeField] string attackTriggerName = "Attack";

    VVEBoardCharacter boardCharacter;
    VVEHitRecoil stunProfile;
    Animator animator;
    IVVEEnemyLaneWalker attackTarget;
    float attackTimer;

    void Awake()
    {
        boardCharacter = GetComponent<VVEBoardCharacter>();
        stunProfile = GetComponent<VVEHitRecoil>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f || animator == null || string.IsNullOrWhiteSpace(attackTriggerName))
        {
            return;
        }

        if (!TryFindEnemyInRange(out IVVEEnemyLaneWalker target))
        {
            attackTarget = null;
            animator.ResetTrigger(attackTriggerName);
            return;
        }

        attackTarget = target;
        attackTimer = Mathf.Max(0.01f, attackCooldown);
        animator.ResetTrigger(attackTriggerName);
        animator.SetTrigger(attackTriggerName);
    }

    public void DealAttackDamage()
    {
        if (!IsValidTarget(attackTarget) || !IsInSameLane(attackTarget) || !IsInRange(attackTarget))
        {
            if (!TryFindEnemyInRange(out attackTarget))
            {
                return;
            }
        }

        VVEHealth targetHealth = attackTarget.Health;
        targetHealth.TakeDamage(attackDamage, recoilMultiplier);
        if (targetHealth.IsAlive && stunProfile != null && TryGetEnemyObject(attackTarget, out GameObject targetObject))
        {
            stunProfile.TryStunTarget(targetObject);
        }
    }

    public void DealAttackDamageAnimationEvent()
    {
        DealAttackDamage();
    }

    bool TryFindEnemyInRange(out IVVEEnemyLaneWalker target)
    {
        target = null;
        float bestForwardDistance = float.PositiveInfinity;
        Vector2 forward = attackDirection.sqrMagnitude > 0f ? attackDirection.normalized : Vector2.right;
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not IVVEEnemyLaneWalker enemy || !TryGetEnemyObject(enemy, out GameObject enemyObject))
            {
                continue;
            }

            if (!IsValidTarget(enemy))
            {
                continue;
            }

            if (!IsInSameLane(enemy))
            {
                continue;
            }

            Vector2 toEnemy = enemyObject.transform.position - transform.position;
            float forwardDistance = Vector2.Dot(toEnemy, forward);
            if (forwardDistance >= 0f && forwardDistance <= attackRange)
            {
                if (forwardDistance < bestForwardDistance)
                {
                    bestForwardDistance = forwardDistance;
                    target = enemy;
                }
            }
        }

        return target != null;
    }

    bool IsValidTarget(IVVEEnemyLaneWalker enemy)
    {
        return TryGetEnemyObject(enemy, out _) && enemy.Health != null && enemy.Health.IsAlive;
    }

    bool IsInRange(IVVEEnemyLaneWalker enemy)
    {
        if (!TryGetEnemyObject(enemy, out GameObject enemyObject))
        {
            return false;
        }

        Vector2 forward = attackDirection.sqrMagnitude > 0f ? attackDirection.normalized : Vector2.right;
        Vector2 toEnemy = enemyObject.transform.position - transform.position;
        float forwardDistance = Vector2.Dot(toEnemy, forward);
        return forwardDistance >= 0f && forwardDistance <= attackRange;
    }

    bool IsInSameLane(IVVEEnemyLaneWalker enemy)
    {
        return TryGetEnemyObject(enemy, out GameObject enemyObject)
            && (boardCharacter == null || !boardCharacter.HasCell || enemy.LaneIndex == boardCharacter.Cell.y)
            && VVELaneDepth.IsSameDepth(transform, enemyObject.transform, depthTolerance);
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
