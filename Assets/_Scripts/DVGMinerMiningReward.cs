using UnityEngine;
using UnityEngine.Rendering;

public class DVGMinerMiningReward : MonoBehaviour
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
    [SerializeField] int sortingOrderOffset = 25;

    [Header("Throw")]
    [SerializeField] float blueGemThrowDuration = 0.32f;
    [SerializeField] float blueGemThrowArcHeight = 0.45f;

    [Header("Collection")]
    [SerializeField] int blueGemValue = 1;
    [SerializeField] float blueGemColliderRadius = 0.28f;

    int callsUntilGem;

    void Awake()
    {
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

        DVGThrownPickup thrownPickup = gem.AddComponent<DVGThrownPickup>();
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

            DVGBoardPickup pickup = spawned.AddComponent<DVGBoardPickup>();
            pickup.Initialize(blueGemValue);
        }

        if (lifetime > 0f)
        {
            Destroy(spawned, lifetime);
        }

        return spawned;
    }

    void ApplySorting(SpriteRenderer spawnedRenderer)
    {
        SortingGroup sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup != null)
        {
            spawnedRenderer.sortingLayerID = sortingGroup.sortingLayerID;
            spawnedRenderer.sortingOrder = sortingGroup.sortingOrder + sortingOrderOffset;
            return;
        }

        SpriteRenderer sourceRenderer = GetComponentInChildren<SpriteRenderer>();
        if (sourceRenderer == null)
        {
            spawnedRenderer.sortingOrder = sortingOrderOffset;
            return;
        }

        spawnedRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        spawnedRenderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;
    }

    Vector2 GetRandomizedLandingOffset()
    {
        float randomX = Random.Range(-blueGemLandingRandomRange.x, blueGemLandingRandomRange.x);
        float randomY = Random.Range(-blueGemLandingRandomRange.y, blueGemLandingRandomRange.y);
        return blueGemLandingOffset + new Vector2(randomX, randomY);
    }
}
