using UnityEngine;

public class VVECharacterPotionTargeting : MonoBehaviour
{
    [SerializeField] VVEUsableWallet wallet;
    [SerializeField] Sprite potionIcon;
    [SerializeField, Min(0.01f)] float speedPercent = 0.3f;
    [SerializeField, Min(0f), Tooltip("Seconds before each boost expires. Leave at 0 to use the effect component's 60-second default.")]
    float removeEffectAfterSeconds;
    [SerializeField, Min(0.05f)] float targetSearchRadius = 0.55f;
    [SerializeField] VVECharacterTargetHighlight targetHighlight;
    [SerializeField] Color validTargetColor = new Color(0.55f, 1f, 0.55f, 0.85f);
    [SerializeField] Color invalidTargetColor = new Color(1f, 0.9f, 0.25f, 0.65f);

    static VVECharacterPotionTargeting instance;

    SpriteRenderer iconRenderer;
    SpriteRenderer cursorRenderer;
    bool isAiming;

    public static bool IsAiming => instance != null && instance.isAiming;

    void Awake()
    {
        instance = this;
        wallet = wallet != null
            ? wallet
            : VVEUsableWallet.Instance != null
                ? VVEUsableWallet.Instance
                : FindAnyObjectByType<VVEUsableWallet>();
        iconRenderer = GetComponent<SpriteRenderer>();
        if (targetHighlight == null)
        {
            targetHighlight = GetComponent<VVECharacterTargetHighlight>();
        }

        if (targetHighlight == null)
        {
            targetHighlight = gameObject.AddComponent<VVECharacterTargetHighlight>();
        }

        if (potionIcon == null && iconRenderer != null)
        {
            potionIcon = iconRenderer.sprite;
        }
    }

    void Update()
    {
        if (!isAiming)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Cancel();
            return;
        }

        UpdateTargetHighlight();
        UpdateCursor();
    }

    void OnDisable()
    {
        StopAiming();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static bool TryHandlePrimaryClick(Vector3 worldPosition)
    {
        return instance != null && instance.HandlePrimaryClick(worldPosition);
    }

    public static void Cancel()
    {
        if (instance != null)
        {
            instance.StopAiming();
        }
    }

    bool HandlePrimaryClick(Vector3 worldPosition)
    {
        if (IsPotionIconClick(worldPosition))
        {
            if (wallet != null && wallet.CanUseSpeedPotion())
            {
                BeginAiming();
            }

            return true;
        }

        if (!isAiming)
        {
            return false;
        }

        VVEDefender target = FindTarget(worldPosition);
        if (target == null)
        {
            return true;
        }

        if (wallet == null || !wallet.TrySpendSpeedPotion())
        {
            StopAiming();
            return true;
        }

        VVEActionSpeedModifier modifier = target.GetComponent<VVEActionSpeedModifier>();
        if (modifier == null)
        {
            modifier = target.gameObject.AddComponent<VVEActionSpeedModifier>();
        }

        float? durationOverride = removeEffectAfterSeconds > 0f
            ? removeEffectAfterSeconds
            : (float?)null;
        modifier.AddPercent(speedPercent, durationOverride);

        float appliedDuration = durationOverride
            ?? target.GetComponent<VVEStackingTimedEffect>().DefaultDurationSeconds;
        Debug.Log(
            $"{target.name} action speed increased by {speedPercent:P0} for {appliedDuration:0.##} seconds.",
            target);
        StopAiming();
        return true;
    }

    bool IsPotionIconClick(Vector3 worldPosition)
    {
        foreach (Collider2D hit in Physics2D.OverlapPointAll(worldPosition))
        {
            if (hit != null && (hit.transform == transform || hit.transform.IsChildOf(transform)))
            {
                return true;
            }
        }

        return iconRenderer != null && iconRenderer.bounds.Contains(worldPosition);
    }

    VVEDefender FindTarget(Vector3 worldPosition)
    {
        return VVEWorldPointer.FindClosest<VVEDefender>(
            worldPosition,
            targetSearchRadius,
            VVEActionSpeedModifier.CanAffect);
    }

    void BeginAiming()
    {
        isAiming = true;
        if (cursorRenderer == null)
        {
            GameObject cursorObject = new GameObject("Speed Potion Cursor");
            cursorRenderer = cursorObject.AddComponent<SpriteRenderer>();
        }

        cursorRenderer.sprite = potionIcon;
        cursorRenderer.sortingLayerName = "UI";
        cursorRenderer.sortingOrder = 6000;
        cursorRenderer.transform.localScale = transform.lossyScale;
        UpdateTargetHighlight();
        UpdateCursor();
    }

    void StopAiming()
    {
        isAiming = false;
        ClearTargetHighlight();
        if (cursorRenderer != null)
        {
            Destroy(cursorRenderer.gameObject);
            cursorRenderer = null;
        }
    }

    void UpdateCursor()
    {
        if (cursorRenderer == null)
        {
            return;
        }

        Vector3 pointerPosition = VVEWorldPointer.GetPosition();
        cursorRenderer.transform.position = pointerPosition;
        cursorRenderer.color = FindTarget(pointerPosition) != null
            ? validTargetColor
            : invalidTargetColor;
    }

    void UpdateTargetHighlight()
    {
        if (targetHighlight != null)
        {
            targetHighlight.Show(FindTarget(VVEWorldPointer.GetPosition()));
        }
    }

    void ClearTargetHighlight()
    {
        if (targetHighlight != null)
        {
            targetHighlight.Clear();
        }
    }
}
