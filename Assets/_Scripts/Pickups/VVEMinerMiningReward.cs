using UnityEngine;

public class VVEMinerMiningReward : MonoBehaviour
{
    [Header("Reward Timing")]
    [Min(1)]
    [SerializeField] int eventCallsPerGem = 5;

    [Header("Spawn Assets")]
    [SerializeField] Sprite flareSprite;
    [SerializeField] Sprite blueGemSprite;

    [Header("Spawn Positions")]
    [SerializeField] Transform spawnPoint;
    [SerializeField] Vector2 flareOffset = new Vector2(0.45f, -0.1f);
    [SerializeField] Vector2 blueGemStartOffset = new Vector2(0.35f, 0.25f);
    [SerializeField] Vector2 blueGemLandingOffset = new Vector2(0.25f, -0.32f);
    [SerializeField] Vector2 blueGemLandingRandomRange = new Vector2(0.2f, 0.08f);

    [Header("Visuals")]
    [SerializeField] Vector3 flareScale = new Vector3(0.6f, 0.6f, 1f);
    [SerializeField] Vector3 blueGemScale = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] float flareLifetime = 0.05f;

    [Header("Throw")]
    [SerializeField] float blueGemThrowDuration = 0.32f;
    [SerializeField] float blueGemThrowArcHeight = 0.45f;

    [Header("Collection")]
    [SerializeField] int blueGemValue = 1;
    [SerializeField] float blueGemColliderRadius = 0.28f;

    [Header("Audio")]
    [SerializeField] VVEAnimationSoundPlayer soundPlayer;

    [Header("Editor Preview")]
    [SerializeField] bool showBlueGemPreview = true;
    [SerializeField] Color blueGemVisualPreviewColor = new Color(0.1f, 0.55f, 1f, 0.9f);
    [SerializeField] Color blueGemColliderPreviewColor = new Color(0.1f, 1f, 0.25f, 0.9f);

    int callsUntilGem;

    void Awake()
    {
        CacheSoundPlayer();
        ResetCounter();
    }

    public void MiningEvent()
    {
        HandleMiningEvent();
    }

    public void Mine()
    {
        HandleMiningEvent();
    }

    public void MineEvent()
    {
        HandleMiningEvent();
    }

    public void OnMineHit()
    {
        HandleMiningEvent();
    }

    public void OnMiningEvent()
    {
        HandleMiningEvent();
    }

    void HandleMiningEvent()
    {
        SpawnSprite(flareSprite, flareOffset, flareScale, "Mining Flare", flareLifetime, false);

        callsUntilGem--;
        if (callsUntilGem > 0)
        {
            return;
        }

        SpawnBlueGem();
        ResetCounter();
    }

    void ResetCounter()
    {
        callsUntilGem = Mathf.Max(1, eventCallsPerGem);
    }

    void SpawnBlueGem()
    {
        if (blueGemSprite == null)
        {
            return;
        }

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        Vector3 startPosition = origin.TransformPoint(blueGemStartOffset);
        Vector3 landingPosition = origin.TransformPoint(GetRandomizedLandingOffset());

        GameObject gem = SpawnSpriteAtWorldPosition(blueGemSprite, startPosition, blueGemScale, "Blue Gem", 0f, true);
        if (gem == null)
        {
            return;
        }

        VVEThrownPickup thrownPickup = gem.AddComponent<VVEThrownPickup>();
        thrownPickup.Launch(startPosition, landingPosition, blueGemThrowArcHeight, blueGemThrowDuration);
    }

    void SpawnSprite(Sprite sprite, Vector2 localOffset, Vector3 scale, string objectName, float lifetime, bool collectable)
    {
        if (sprite == null)
        {
            return;
        }

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        SpawnSpriteAtWorldPosition(sprite, origin.TransformPoint(localOffset), scale, objectName, lifetime, collectable);
    }

    GameObject SpawnSpriteAtWorldPosition(Sprite sprite, Vector3 worldPosition, Vector3 scale, string objectName, float lifetime, bool collectable)
    {
        if (sprite == null)
        {
            return null;
        }

        GameObject spawned = new GameObject(objectName);
        spawned.transform.position = worldPosition;
        spawned.transform.rotation = Quaternion.identity;
        spawned.transform.localScale = scale;

        SpriteRenderer spriteRenderer = spawned.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        ApplySorting(spriteRenderer);

        if (collectable)
        {
            CircleCollider2D collider = spawned.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = blueGemColliderRadius;

            VVEBoardPickup pickup = spawned.AddComponent<VVEBoardPickup>();
            pickup.Initialize(blueGemValue, CacheSoundPlayer());
        }

        if (lifetime > 0f)
        {
            Destroy(spawned, lifetime);
        }

        return spawned;
    }

    void ApplySorting(SpriteRenderer spawnedRenderer)
    {
        VVELaneDepth.ApplyGameplayRenderer(spawnedRenderer);
    }

    Vector2 GetRandomizedLandingOffset()
    {
        float randomX = Random.Range(-blueGemLandingRandomRange.x, blueGemLandingRandomRange.x);
        float randomY = Random.Range(-blueGemLandingRandomRange.y, blueGemLandingRandomRange.y);
        return blueGemLandingOffset + new Vector2(randomX, randomY);
    }

    VVEAnimationSoundPlayer CacheSoundPlayer()
    {
        if (soundPlayer == null)
        {
            soundPlayer = GetComponent<VVEAnimationSoundPlayer>();
        }

        return soundPlayer;
    }

    void OnDrawGizmosSelected()
    {
        if (!showBlueGemPreview || blueGemSprite == null)
        {
            return;
        }

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        Vector3 startPosition = origin.TransformPoint(blueGemStartOffset);
        Vector3 landingPosition = origin.TransformPoint(blueGemLandingOffset);
        Vector3 visualSize = Vector3.Scale(blueGemSprite.bounds.size, blueGemScale);
        float colliderPreviewRadius = blueGemColliderRadius * Mathf.Max(Mathf.Abs(blueGemScale.x), Mathf.Abs(blueGemScale.y));

        Gizmos.color = blueGemVisualPreviewColor;
        Gizmos.DrawWireCube(startPosition, visualSize);
        Gizmos.DrawWireCube(landingPosition, visualSize);
        Gizmos.DrawLine(startPosition, landingPosition);

        Gizmos.color = blueGemColliderPreviewColor;
        Gizmos.DrawWireSphere(startPosition, colliderPreviewRadius);
        Gizmos.DrawWireSphere(landingPosition, colliderPreviewRadius);
    }
}
