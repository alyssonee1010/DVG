using UnityEngine;

[RequireComponent(typeof(DVGBoardCharacter))]
public class DVGBoardMeleeAttacker : MonoBehaviour
{
    [SerializeField] float attackRange = 1.1f;
    [SerializeField] float attackCooldown = 1.1f;
    [SerializeField] int attackDamage = 20;
    [SerializeField] Vector2 attackDirection = Vector2.right;
    [SerializeField] float laneTolerance = 0.45f;
    [SerializeField] string attackTriggerName = "Attack";

    DVGBoardCharacter boardCharacter;
    Animator animator;
    IDVGEnemyLaneWalker attackTarget;
    float attackTimer;

    void Awake()
    {
        boardCharacter = GetComponent<DVGBoardCharacter>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f || animator == null || string.IsNullOrWhiteSpace(attackTriggerName))
        {
            return;
        }

        if (!TryFindEnemyInRange(out IDVGEnemyLaneWalker target))
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

        attackTarget.Health.TakeDamage(attackDamage);
    }

    public void DealAttackDamageAnimationEvent()
    {
        DealAttackDamage();
    }

    bool TryFindEnemyInRange(out IDVGEnemyLaneWalker target)
    {
        target = null;
        float bestForwardDistance = float.PositiveInfinity;
        Vector2 forward = attackDirection.sqrMagnitude > 0f ? attackDirection.normalized : Vector2.right;
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not IDVGEnemyLaneWalker enemy || enemy.gameObject == null)
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

            Vector2 toEnemy = enemy.gameObject.transform.position - transform.position;
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

    bool IsValidTarget(IDVGEnemyLaneWalker enemy)
    {
        return enemy != null && enemy.gameObject != null && enemy.Health != null && enemy.Health.IsAlive;
    }

    bool IsInRange(IDVGEnemyLaneWalker enemy)
    {
        Vector2 forward = attackDirection.sqrMagnitude > 0f ? attackDirection.normalized : Vector2.right;
        Vector2 toEnemy = enemy.gameObject.transform.position - transform.position;
        float forwardDistance = Vector2.Dot(toEnemy, forward);
        return forwardDistance >= 0f && forwardDistance <= attackRange;
    }

    bool IsInSameLane(IDVGEnemyLaneWalker enemy)
    {
        if (boardCharacter != null && boardCharacter.HasCell)
        {
            return enemy.LaneIndex == boardCharacter.Cell.y;
        }

        return Mathf.Abs(enemy.gameObject.transform.position.y - transform.position.y) <= laneTolerance;
    }
}
