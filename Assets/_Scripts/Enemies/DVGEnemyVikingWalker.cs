using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(DVGHealth))]
public class DVGEnemyVikingWalker : MonoBehaviour, IDVGEnemyLaneWalker
{
    const string AttackTriggerName = "Attack";
    const string AfterKillTriggerName = "AfterKill";

    [Header("Movement")]
    [SerializeField] float moveSpeed = 0.75f;
    [SerializeField] float reachDistance = 0.05f;

    [Header("Targeting")]
    [SerializeField] float attackStartDistance = 0.8f;
    [SerializeField] float laneTolerance = 0.45f;
    [SerializeField] float overlapTolerance = 0.05f;

    [Header("Attack")]
    [SerializeField] int attackDamage = 25;
    [SerializeField] float afterKillLockSeconds = 0.8f;

    [Header("Sorting")]
    [SerializeField] int sortingOrderBase = 1000;
    [SerializeField] int sortingOrderPerRow = 120;
    [SerializeField] int sortingOrderOffset = 30;

    DVGHealth health;
    Animator animator;
    Vector3 targetPosition;
    DVGBoardCharacter attackTarget;
    bool hasTarget;
    bool hasAttackTarget;
    bool isAfterKillLocked;
    float afterKillTimer;
    int lastHealth;
    float walkDirection = -1f;

    public int LaneIndex { get; private set; }
    public DVGHealth Health => health;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    void Awake()
    {
        health = GetComponent<DVGHealth>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (health == null)
        {
            health = GetComponent<DVGHealth>();
        }

        if (health != null)
        {
            lastHealth = health.CurrentHealth;
            health.HealthChanged += OnHealthChanged;
        }
    }

    void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
        }
    }

    void Update()
    {
        if (!hasTarget || health == null || !health.IsAlive)
        {
            return;
        }

        if (isAfterKillLocked)
        {
            afterKillTimer -= Time.deltaTime;
            if (afterKillTimer <= 0f)
            {
                isAfterKillLocked = false;
            }

            return;
        }

        if (hasAttackTarget)
        {
            if (attackTarget == null || attackTarget.Health == null || !attackTarget.Health.IsAlive)
            {
                PlayAfterKill();
            }

            return;
        }

        if (TryFindAttackTarget())
        {
            hasAttackTarget = true;
            if (animator != null)
            {
                animator.SetTrigger(AttackTriggerName);
            }

            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if ((transform.position - targetPosition).sqrMagnitude <= reachDistance * reachDistance)
        {
            Destroy(gameObject);
        }
    }

    public void BeginLaneWalk(int laneIndex, Vector3 startPosition, Vector3 endPosition, float speed, int maxHealth)
    {
        LaneIndex = laneIndex;
        transform.position = startPosition;
        targetPosition = endPosition;
        moveSpeed = Mathf.Max(0f, speed);
        hasTarget = true;
        hasAttackTarget = false;
        isAfterKillLocked = false;
        attackTarget = null;
        walkDirection = Mathf.Sign(endPosition.x - startPosition.x);
        if (Mathf.Approximately(walkDirection, 0f))
        {
            walkDirection = -1f;
        }

        if (health == null)
        {
            health = GetComponent<DVGHealth>();
        }

        health.SetMaxHealth(maxHealth);
        lastHealth = health.CurrentHealth;
        ApplyRowSorting(laneIndex);
    }

    void OnHealthChanged(DVGHealth changedHealth, int currentHealth)
    {
        bool tookDamage = currentHealth < lastHealth;
        lastHealth = currentHealth;

        if (!tookDamage || currentHealth <= 0 || !hasAttackTarget)
        {
            return;
        }

        if (attackTarget == null || attackTarget.Health == null || !attackTarget.Health.IsAlive)
        {
            return;
        }

        hasAttackTarget = false;
        attackTarget = null;
        if (animator != null)
        {
            animator.ResetTrigger(AttackTriggerName);
        }
    }

    bool TryFindAttackTarget()
    {
        DVGBoardCharacter[] characters = FindObjectsByType<DVGBoardCharacter>(FindObjectsInactive.Exclude);
        foreach (DVGBoardCharacter character in characters)
        {
            if (character == null || !character.isActiveAndEnabled)
            {
                continue;
            }

            if (character.Health == null || !character.Health.IsAlive)
            {
                continue;
            }

            if (character.HasCell)
            {
                if (character.Cell.y != LaneIndex)
                {
                    continue;
                }
            }
            else if (Mathf.Abs(character.transform.position.y - transform.position.y) > laneTolerance)
            {
                continue;
            }

            float forwardDistance = GetForwardDistanceTo(character);
            if (forwardDistance < -overlapTolerance || forwardDistance > attackStartDistance)
            {
                continue;
            }

            attackTarget = character;
            return true;
        }

        return false;
    }

    public void DealAttackDamage()
    {
        if (attackTarget == null || attackTarget.Health == null || !attackTarget.Health.IsAlive)
        {
            ResumeWalking();
            return;
        }

        attackTarget.Health.TakeDamage(attackDamage);
        if (!attackTarget.Health.IsAlive)
        {
            PlayAfterKill();
        }
    }

    public void DealAttackDamageAnimationEvent()
    {
        DealAttackDamage();
    }

    void ResumeWalking()
    {
        attackTarget = null;
        hasAttackTarget = false;
        if (animator != null)
        {
            animator.ResetTrigger(AttackTriggerName);
            animator.Play("walking");
        }
    }

    void PlayAfterKill()
    {
        attackTarget = null;
        hasAttackTarget = false;
        isAfterKillLocked = true;
        afterKillTimer = Mathf.Max(0f, afterKillLockSeconds);

        if (animator != null)
        {
            animator.ResetTrigger(AttackTriggerName);
            animator.SetTrigger(AfterKillTriggerName);
        }
    }

    float GetForwardDistanceTo(DVGBoardCharacter character)
    {
        Bounds selfBounds = GetBounds(GetComponent<Collider2D>(), transform.position);
        Bounds targetBounds = GetBounds(character.GetComponent<Collider2D>(), character.transform.position);

        float selfFrontX = walkDirection < 0f ? selfBounds.min.x : selfBounds.max.x;
        float targetFrontX = walkDirection < 0f ? targetBounds.max.x : targetBounds.min.x;
        return (targetFrontX - selfFrontX) * walkDirection;
    }

    Bounds GetBounds(Collider2D collider, Vector3 fallbackPosition)
    {
        if (collider != null)
        {
            return collider.bounds;
        }

        return new Bounds(fallbackPosition, Vector3.one * 0.5f);
    }

    void ApplyRowSorting(int row)
    {
        SortingGroup sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup == null)
        {
            sortingGroup = gameObject.AddComponent<SortingGroup>();
        }

        sortingGroup.sortingOrder = sortingOrderBase - row * sortingOrderPerRow + sortingOrderOffset;
    }
}
