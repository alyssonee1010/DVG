using UnityEngine;

[RequireComponent(typeof(DVGHealth))]
public class DVGEnemyWalker : MonoBehaviour, IDVGEnemyLaneWalker
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 0.75f;
    [SerializeField] float reachDistance = 0.05f;

    [Header("Animation")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] walkFrames;
    [SerializeField] float framesPerSecond = 8f;
    [SerializeField] bool flipSpriteX;

    DVGHealth health;
    Vector3 targetPosition;
    bool hasTarget;
    float animationTimer;
    int animationFrame;

    public int LaneIndex { get; private set; }
    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    public DVGHealth Health => health;

    void Awake()
    {
        health = GetComponent<DVGHealth>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flipSpriteX;
        }
    }

    void Update()
    {
        AnimateWalk();

        if (!hasTarget || health == null || !health.IsAlive)
        {
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

        if (health == null)
        {
            health = GetComponent<DVGHealth>();
        }

        health.SetMaxHealth(maxHealth);
    }

    void AnimateWalk()
    {
        if (spriteRenderer == null || walkFrames == null || walkFrames.Length == 0 || framesPerSecond <= 0f)
        {
            return;
        }

        animationTimer += Time.deltaTime;
        float frameDuration = 1f / framesPerSecond;
        while (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration;
            animationFrame = (animationFrame + 1) % walkFrames.Length;
            spriteRenderer.sprite = walkFrames[animationFrame];
        }
    }
}
