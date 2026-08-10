using UnityEngine;

[RequireComponent(typeof(DVGBoardCharacter))]
public class DVGBoardMeleeAttacker : MonoBehaviour
{
    [SerializeField] float attackRange = 1.1f;
    [SerializeField] float attackCooldown = 1.1f;
    [SerializeField] int attackDamage = 20;
    [SerializeField] float recoilMultiplier = 1f;
    [SerializeField] Vector2 attackDirection = Vector2.right;
    [SerializeField] float laneTolerance = 0.45f;
    [SerializeField] string attackTriggerName = "Attack";

    DVGBoardCharacter boardCharacter;
    DVGHitRecoil stunProfile;
    Animator animator;
    IDVGEnemyLaneWalker attackTarget;
    float attackTimer;

    void Awake()
    {
        boardCharacter = GetComponent<DVGBoardCharacter>();
        stunProfile = GetComponent<DVGHitRecoil>();
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

        DVGHealth targetHealth = attackTarget.Health;
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

    bool TryFindEnemyInRange(out IDVGEnemyLaneWalker target)
    {
        target = null;
        float bestForwardDistance = float.PositiveInfinity;
        Vector2 forward = attackDirection.sqrMagnitude > 0f ? attackDirection.normalized : Vector2.right;
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not IDVGEnemyLaneWalker enemy || !TryGetEnemyObject(enemy, out GameObject enemyObject))
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

    bool IsValidTarget(IDVGEnemyLaneWalker enemy)
    {
        return TryGetEnemyObject(enemy, out _) && enemy.Health != null && enemy.Health.IsAlive;
    }

    bool IsInRange(IDVGEnemyLaneWalker enemy)
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

    bool IsInSameLane(IDVGEnemyLaneWalker enemy)
    {
        if (boardCharacter != null && boardCharacter.HasCell)
        {
            return enemy.LaneIndex == boardCharacter.Cell.y;
        }

        return TryGetEnemyObject(enemy, out GameObject enemyObject)
            && Mathf.Abs(enemyObject.transform.position.y - transform.position.y) <= laneTolerance;
    }

    bool TryGetEnemyObject(IDVGEnemyLaneWalker enemy, out GameObject enemyObject)
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
