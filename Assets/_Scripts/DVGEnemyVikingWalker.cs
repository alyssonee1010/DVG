using UnityEngine;

[RequireComponent(typeof(DVGHealth))]
public class DVGEnemyVikingWalker : MonoBehaviour, IDVGEnemyLaneWalker
{
    const string AttackTriggerName = "Attack";

    [Header("Movement")]
    [SerializeField] float moveSpeed = 0.75f;
    [SerializeField] float reachDistance = 0.05f;

    [Header("Targeting")]
    [SerializeField] float detectionRange = 0.65f;
    [SerializeField] float laneTolerance = 0.45f;
    [SerializeField] float overlapTolerance = 0.15f;

    DVGHealth health;
    Animator animator;
    Vector3 targetPosition;
    bool hasTarget;
    bool hasAttackTarget;
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

    void Update()
    {
        if (!hasTarget || hasAttackTarget || health == null || !health.IsAlive)
        {
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

            float forwardDistance = (character.transform.position.x - transform.position.x) * walkDirection;
            if (forwardDistance < -overlapTolerance || forwardDistance > detectionRange)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
