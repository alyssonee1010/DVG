using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VVEHealingPotionUseController : MonoBehaviour
{
    [SerializeField] VVEUsableWallet wallet;
    [SerializeField] Sprite potionIcon;
    [SerializeField] string scenePotionName = "Healing Potion";
    [SerializeField] string usableUiName = "Usable_UI";
    [SerializeField] string potionCounterTextName = "Healing Potion Counter";
    [SerializeField, Min(1)] int healAmount = 100;
    [SerializeField, Min(0.05f)] float clickSearchRadius = 0.55f;
    [SerializeField] Color validTargetTint = new Color(0.55f, 1f, 0.85f, 0.55f);
    [SerializeField] Color selectedPotionTint = new Color(0.55f, 0.95f, 1f, 0.55f);
    [SerializeField] Color healFlashTint = new Color(0.65f, 1f, 0.65f, 1f);
    [SerializeField] Vector3 scenePotionCounterTextLocalPosition = new Vector3(0.28f, 0.03f, -0.01f);
    [SerializeField] Vector3 fallbackCounterLocalPosition = new Vector3(-1.5f, -0.36f, -0.04f);
    [SerializeField] Vector3 fallbackCounterScale = new Vector3(0.08f, 0.08f, 1f);
    [SerializeField] string fallbackCounterPrefix = "P: ";

    readonly Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
    readonly Dictionary<SpriteRenderer, Color> potionOriginalColors = new Dictionary<SpriteRenderer, Color>();
    readonly Dictionary<SpriteRenderer, SpriteRenderer> targetGhostRenderers = new Dictionary<SpriteRenderer, SpriteRenderer>();

    TextMeshPro fallbackCounterText;
    Transform scenePotionTransform;
    SpriteRenderer potionGhostRenderer;
    SpriteRenderer potionCursorGhostRenderer;
    VVEBoardCharacter hoveredTargetCharacter;
    bool isAiming;

    public static VVEHealingPotionUseController Instance { get; private set; }
    public bool IsAiming => isAiming;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (wallet == null)
        {
            wallet = GetComponent<VVEUsableWallet>();
        }

        if (wallet == null)
        {
            wallet = VVEUsableWallet.Instance != null ? VVEUsableWallet.Instance : FindAnyObjectByType<VVEUsableWallet>();
        }

        EnsureFallbackCounter();
    }

    void OnEnable()
    {
        if (wallet != null)
        {
            wallet.HealingPotionsChanged += UpdateFallbackCounter;
            UpdateFallbackCounter(wallet.HealingPotions);
        }
    }

    void OnDisable()
    {
        if (wallet != null)
        {
            wallet.HealingPotionsChanged -= UpdateFallbackCounter;
        }

        ClearTargetTint();
        ClearPotionTint();
        ClearPotionCursorGhost();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        if (!isAiming)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            EndAiming();
            return;
        }

        UpdateTargetTint();
        UpdatePotionTint();
        UpdatePotionCursorGhost();
    }

    public bool TryHandlePrimaryClick(Vector3 worldPosition)
    {
        if (IsPotionCounterClick(worldPosition))
        {
            if (wallet != null && wallet.CanUseHealingPotion())
            {
                BeginAiming();
            }
            return true;
        }

        if (!isAiming)
        {
            return false;
        }

        VVEBoardCharacter target = FindDamagedCharacterAt(worldPosition);
        if (target != null && wallet != null && wallet.TrySpendHealingPotion())
        {
            ClearTargetTint();
            RestoreCharacterTint(target);
            target.Health.Heal(healAmount);
            RefreshHealthBar(target);
            PlayHealFlash(target);

            if (wallet.CanUseHealingPotion())
            {
                UpdateTargetTint();
                UpdatePotionTint();
            }
            else
            {
                EndAiming();
            }
        }

        return true;
    }

    void BeginAiming()
    {
        isAiming = true;
        UpdateTargetTint();
        UpdatePotionTint();
        UpdatePotionCursorGhost();
    }

    void EndAiming()
    {
        isAiming = false;
        ClearTargetTint();
        ClearPotionTint();
        ClearPotionCursorGhost();
    }

    public void CancelAiming()
    {
        if (isAiming)
        {
            EndAiming();
        }
    }

    bool IsPotionCounterClick(Vector3 worldPosition)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);
        foreach (Collider2D hit in hits)
        {
            if (hit != null && IsPotionCounterTransform(hit.transform))
            {
                return true;
            }
        }

        Transform scenePotion = FindScenePotionTransform();
        if (scenePotion == null)
        {
            return false;
        }

        SpriteRenderer renderer = scenePotion.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null && renderer.bounds.Contains(worldPosition))
        {
            return true;
        }

        return Vector2.Distance(scenePotion.position, worldPosition) <= clickSearchRadius;
    }

    bool IsPotionCounterTransform(Transform candidate)
    {
        while (candidate != null)
        {
            if (candidate.name == "Healing Potion Counter"
                || candidate.name == scenePotionName
                || candidate.name == potionCounterTextName)
            {
                return true;
            }

            candidate = candidate.parent;
        }

        return false;
    }

    VVEBoardCharacter FindDamagedCharacterAt(Vector3 worldPosition)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);
        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            VVEBoardCharacter character = hit.GetComponentInParent<VVEBoardCharacter>();
            if (IsDamagedCharacter(character))
            {
                return character;
            }
        }

        VVEBoardCharacter bestCharacter = null;
        float bestDistance = clickSearchRadius;
        VVEBoardCharacter[] characters = FindObjectsByType<VVEBoardCharacter>(FindObjectsInactive.Exclude);
        foreach (VVEBoardCharacter character in characters)
        {
            if (!IsDamagedCharacter(character))
            {
                continue;
            }

            float distance = Vector2.Distance(character.transform.position, worldPosition);
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                bestCharacter = character;
            }
        }

        return bestCharacter;
    }

    bool HasDamagedCharacter()
    {
        VVEBoardCharacter[] characters = FindObjectsByType<VVEBoardCharacter>(FindObjectsInactive.Exclude);
        foreach (VVEBoardCharacter character in characters)
        {
            if (IsDamagedCharacter(character))
            {
                return true;
            }
        }

        return false;
    }

    bool IsDamagedCharacter(VVEBoardCharacter character)
    {
        return character != null
            && character.isActiveAndEnabled
            && character.Health != null
            && character.Health.IsAlive
            && character.Health.CurrentHealth < character.Health.MaxHealth;
    }

    void UpdateTargetTint()
    {
        VVEBoardCharacter hoveredCharacter = FindDamagedCharacterAt(GetMouseWorldPosition());
        if (hoveredCharacter != hoveredTargetCharacter)
        {
            ClearTargetTint();
            hoveredTargetCharacter = hoveredCharacter;
        }

        if (hoveredCharacter == null)
        {
            return;
        }

        List<SpriteRenderer> stillPreviewed = new List<SpriteRenderer>();
        SpriteRenderer[] renderers = hoveredCharacter.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null || IsHealthBarRenderer(renderer) || IsTargetGhostRenderer(renderer))
            {
                continue;
            }

            EnsureTargetGhostRenderer(renderer);
            stillPreviewed.Add(renderer);
        }

        RemoveMissingTargetGhosts(stillPreviewed);
    }

    Vector3 GetMouseWorldPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return Vector3.zero;
        }

        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;
        return mouseWorldPosition;
    }

    void RestoreMissingTintedRenderers(List<SpriteRenderer> stillTinted)
    {
        List<SpriteRenderer> restore = new List<SpriteRenderer>();
        foreach (KeyValuePair<SpriteRenderer, Color> entry in originalColors)
        {
            if (entry.Key == null || !stillTinted.Contains(entry.Key))
            {
                restore.Add(entry.Key);
            }
        }

        foreach (SpriteRenderer renderer in restore)
        {
            RestoreRenderer(renderer);
        }
    }

    void ClearTargetTint()
    {
        ClearTargetGhosts();

        List<SpriteRenderer> renderers = new List<SpriteRenderer>(originalColors.Keys);
        foreach (SpriteRenderer renderer in renderers)
        {
            RestoreRenderer(renderer);
        }

        hoveredTargetCharacter = null;
    }

    void EnsureTargetGhostRenderer(SpriteRenderer sourceRenderer)
    {
        if (sourceRenderer == null)
        {
            return;
        }

        if (!targetGhostRenderers.TryGetValue(sourceRenderer, out SpriteRenderer ghostRenderer) || ghostRenderer == null)
        {
            GameObject ghostObject = new GameObject("Healing Target Ghost Effect");
            ghostObject.transform.SetParent(sourceRenderer.transform, false);
            ghostObject.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            ghostObject.transform.localRotation = Quaternion.identity;
            ghostObject.transform.localScale = new Vector3(1.08f, 1.08f, 1f);
            ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
            targetGhostRenderers[sourceRenderer] = ghostRenderer;
        }

        ghostRenderer.sprite = sourceRenderer.sprite;
        ghostRenderer.flipX = sourceRenderer.flipX;
        ghostRenderer.flipY = sourceRenderer.flipY;
        ghostRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        ghostRenderer.sortingOrder = sourceRenderer.sortingOrder + 2;
        ghostRenderer.color = validTargetTint;
    }

    void RemoveMissingTargetGhosts(List<SpriteRenderer> stillPreviewed)
    {
        List<SpriteRenderer> remove = new List<SpriteRenderer>();
        foreach (KeyValuePair<SpriteRenderer, SpriteRenderer> entry in targetGhostRenderers)
        {
            if (entry.Key == null || entry.Value == null || !stillPreviewed.Contains(entry.Key))
            {
                remove.Add(entry.Key);
            }
        }

        foreach (SpriteRenderer sourceRenderer in remove)
        {
            DestroyTargetGhost(sourceRenderer);
        }
    }

    void ClearTargetGhosts()
    {
        List<SpriteRenderer> sourceRenderers = new List<SpriteRenderer>(targetGhostRenderers.Keys);
        foreach (SpriteRenderer sourceRenderer in sourceRenderers)
        {
            DestroyTargetGhost(sourceRenderer);
        }
    }

    void DestroyTargetGhost(SpriteRenderer sourceRenderer)
    {
        if (sourceRenderer != null && targetGhostRenderers.TryGetValue(sourceRenderer, out SpriteRenderer ghostRenderer) && ghostRenderer != null)
        {
            Destroy(ghostRenderer.gameObject);
        }

        targetGhostRenderers.Remove(sourceRenderer);
    }

    void RestoreRenderer(SpriteRenderer renderer)
    {
        if (renderer != null && originalColors.TryGetValue(renderer, out Color originalColor))
        {
            renderer.color = originalColor;
        }

        originalColors.Remove(renderer);
    }

    void RestoreCharacterTint(VVEBoardCharacter character)
    {
        if (character == null)
        {
            return;
        }

        SpriteRenderer[] renderers = character.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (IsHealthBarRenderer(renderer) || IsTargetGhostRenderer(renderer))
            {
                continue;
            }

            RestoreRenderer(renderer);
        }
    }

    void UpdatePotionTint()
    {
        Transform scenePotion = FindScenePotionTransform();
        if (scenePotion == null)
        {
            return;
        }

        SpriteRenderer[] renderers = scenePotion.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null || renderer == potionGhostRenderer)
            {
                continue;
            }

            if (!potionOriginalColors.ContainsKey(renderer))
            {
                potionOriginalColors.Add(renderer, renderer.color);
            }

            Color originalColor = potionOriginalColors[renderer];
            renderer.color = new Color(
                originalColor.r * selectedPotionTint.r,
                originalColor.g * selectedPotionTint.g,
                originalColor.b * selectedPotionTint.b,
                Mathf.Min(originalColor.a, selectedPotionTint.a));
        }

        EnsurePotionGhostRenderer(scenePotion);
    }

    void ClearPotionTint()
    {
        if (potionGhostRenderer != null)
        {
            Destroy(potionGhostRenderer.gameObject);
            potionGhostRenderer = null;
        }

        List<SpriteRenderer> renderers = new List<SpriteRenderer>(potionOriginalColors.Keys);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null && potionOriginalColors.TryGetValue(renderer, out Color originalColor))
            {
                renderer.color = originalColor;
            }

            potionOriginalColors.Remove(renderer);
        }
    }

    void UpdatePotionCursorGhost()
    {
        Transform scenePotion = FindScenePotionTransform();
        if (scenePotion == null)
        {
            ClearPotionCursorGhost();
            return;
        }

        SpriteRenderer sourceRenderer = FindScenePotionRenderer(scenePotion);
        if (sourceRenderer == null || sourceRenderer.sprite == null)
        {
            ClearPotionCursorGhost();
            return;
        }

        if (potionCursorGhostRenderer == null)
        {
            GameObject cursorGhostObject = new GameObject("Healing Potion Cursor Ghost");
            potionCursorGhostRenderer = cursorGhostObject.AddComponent<SpriteRenderer>();
        }

        potionCursorGhostRenderer.sprite = sourceRenderer.sprite;
        potionCursorGhostRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        potionCursorGhostRenderer.sortingOrder = 6000;
        potionCursorGhostRenderer.color = selectedPotionTint;
        potionCursorGhostRenderer.transform.position = GetMouseWorldPosition();
        potionCursorGhostRenderer.transform.rotation = Quaternion.identity;
        potionCursorGhostRenderer.transform.localScale = sourceRenderer.transform.lossyScale;
    }

    void ClearPotionCursorGhost()
    {
        if (potionCursorGhostRenderer != null)
        {
            Destroy(potionCursorGhostRenderer.gameObject);
            potionCursorGhostRenderer = null;
        }
    }

    void EnsurePotionGhostRenderer(Transform scenePotion)
    {
        SpriteRenderer sourceRenderer = FindScenePotionRenderer(scenePotion);
        if (sourceRenderer == null || sourceRenderer.sprite == null)
        {
            return;
        }

        if (potionGhostRenderer == null)
        {
            GameObject ghostObject = new GameObject("Healing Potion Ghost Effect");
            ghostObject.transform.SetParent(scenePotion, false);
            ghostObject.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            ghostObject.transform.localRotation = Quaternion.identity;
            ghostObject.transform.localScale = new Vector3(1.18f, 1.18f, 1f);
            potionGhostRenderer = ghostObject.AddComponent<SpriteRenderer>();
        }

        potionGhostRenderer.sprite = sourceRenderer.sprite;
        potionGhostRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        potionGhostRenderer.sortingOrder = sourceRenderer.sortingOrder + 2;
        potionGhostRenderer.color = selectedPotionTint;
    }

    SpriteRenderer FindScenePotionRenderer(Transform scenePotion)
    {
        SpriteRenderer[] renderers = scenePotion.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null && renderer != potionGhostRenderer)
            {
                return renderer;
            }
        }

        return null;
    }

    void PlayHealFlash(VVEBoardCharacter target)
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            StartCoroutine(HealFlashRoutine(target));
        }
    }

    IEnumerator HealFlashRoutine(VVEBoardCharacter target)
    {
        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        Dictionary<SpriteRenderer, Color> flashOriginals = new Dictionary<SpriteRenderer, Color>();
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null || IsTargetGhostRenderer(renderer))
            {
                continue;
            }

            if (IsHealthBarRenderer(renderer))
            {
                continue;
            }

            flashOriginals[renderer] = renderer.color;
            renderer.color = healFlashTint;
        }

        yield return new WaitForSeconds(0.16f);

        foreach (KeyValuePair<SpriteRenderer, Color> entry in flashOriginals)
        {
            if (entry.Key != null)
            {
                entry.Key.color = entry.Value;
            }
        }
    }

    bool IsHealthBarRenderer(SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            return false;
        }

        Transform candidate = renderer.transform;
        while (candidate != null)
        {
            if (candidate.name == "Health Bar")
            {
                return true;
            }

            candidate = candidate.parent;
        }

        return false;
    }

    bool IsTargetGhostRenderer(SpriteRenderer renderer)
    {
        return renderer != null && renderer.gameObject.name == "Healing Target Ghost Effect";
    }

    void RefreshHealthBar(VVEBoardCharacter character)
    {
        if (character == null)
        {
            return;
        }

        VVEWorldHealthBar healthBar = character.GetComponent<VVEWorldHealthBar>();
        if (healthBar != null)
        {
            healthBar.Refresh();
        }
    }

    void EnsureFallbackCounter()
    {
        Transform scenePotion = FindScenePotionTransform();
        if (scenePotion != null)
        {
            EnsurePotionHitbox(scenePotion);
            Transform counterParent = FindUsableUiTransform();
            if (counterParent == null)
            {
                counterParent = scenePotion.parent != null ? scenePotion.parent : scenePotion;
            }

            fallbackCounterText = FindExistingPotionCounterText(counterParent, scenePotion);
            if (fallbackCounterText == null)
            {
                CreateFallbackText(
                    counterParent,
                    GetPotionCounterLocalPosition(counterParent, scenePotion),
                    potionCounterTextName);
            }

            return;
        }

        Transform existingCounter = transform.Find("Healing Potion Counter");
        if (existingCounter != null)
        {
            fallbackCounterText = existingCounter.GetComponentInChildren<TextMeshPro>(true);
            if (fallbackCounterText == null)
            {
                CreateFallbackText(existingCounter, new Vector3(0.28f, 0.03f, -0.01f), "Text");
            }

            return;
        }

        GameObject counterRoot = new GameObject("Healing Potion Counter");
        counterRoot.transform.SetParent(transform, false);
        counterRoot.transform.localPosition = fallbackCounterLocalPosition;
        counterRoot.transform.localScale = Vector3.one;

        BoxCollider2D hitbox = counterRoot.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;
        hitbox.size = new Vector2(1.15f, 0.55f);

        if (potionIcon != null)
        {
            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(counterRoot.transform, false);
            iconObject.transform.localPosition = new Vector3(-0.18f, 0f, 0f);
            iconObject.transform.localScale = fallbackCounterScale;
            SpriteRenderer iconRenderer = iconObject.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = potionIcon;
            iconRenderer.sortingOrder = 5000;
        }

        CreateFallbackText(counterRoot.transform, new Vector3(0.28f, 0.03f, -0.01f), "Text");
    }

    TextMeshPro FindExistingPotionCounterText(Transform counterParent, Transform scenePotion)
    {
        Transform existing = counterParent != null ? counterParent.Find(potionCounterTextName) : null;
        if (existing != null)
        {
            TextMeshPro existingText = existing.GetComponent<TextMeshPro>();
            if (existingText != null)
            {
                return existingText;
            }
        }

        Transform counterRoot = counterParent != null ? counterParent.Find("Healing Potion Counter") : null;
        if (counterRoot != null)
        {
            TextMeshPro counterText = counterRoot.GetComponentInChildren<TextMeshPro>(true);
            if (counterText != null)
            {
                return counterText;
            }
        }

        return scenePotion != null ? scenePotion.GetComponentInChildren<TextMeshPro>(true) : null;
    }

    Transform FindUsableUiTransform()
    {
        if (!string.IsNullOrEmpty(usableUiName))
        {
            if (transform.name == usableUiName)
            {
                return transform;
            }

            GameObject usableUi = GameObject.Find(usableUiName);
            if (usableUi != null)
            {
                return usableUi.transform;
            }
        }

        return transform;
    }

    Vector3 GetPotionCounterLocalPosition(Transform parent, Transform scenePotion)
    {
        if (parent == scenePotion)
        {
            return scenePotionCounterTextLocalPosition;
        }

        Vector3 worldPosition = scenePotion.TransformPoint(scenePotionCounterTextLocalPosition);
        return parent.InverseTransformPoint(worldPosition);
    }

    Transform FindScenePotionTransform()
    {
        if (scenePotionTransform != null)
        {
            return scenePotionTransform;
        }

        if (string.IsNullOrEmpty(scenePotionName))
        {
            return null;
        }

        GameObject scenePotion = GameObject.Find(scenePotionName);
        if (scenePotion != null)
        {
            scenePotionTransform = scenePotion.transform;
        }

        return scenePotionTransform;
    }

    void EnsurePotionHitbox(Transform potionTransform)
    {
        if (potionTransform.GetComponent<Collider2D>() != null)
        {
            return;
        }

        BoxCollider2D hitbox = potionTransform.gameObject.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;

        SpriteRenderer renderer = potionTransform.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null && renderer.sprite != null)
        {
            hitbox.size = renderer.sprite.bounds.size;
        }
        else
        {
            hitbox.size = new Vector2(0.6f, 0.6f);
        }
    }

    void CreateFallbackText(Transform parent, Vector3 localPosition, string objectName)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = localPosition;
        textObject.transform.localScale = Vector3.one;
        fallbackCounterText = textObject.AddComponent<TextMeshPro>();
        fallbackCounterText.fontSize = 0.28f;
        fallbackCounterText.alignment = TextAlignmentOptions.Center;
        fallbackCounterText.color = new Color(1f, 0.82f, 0.86f, 1f);
        fallbackCounterText.sortingOrder = 5001;
    }

    void UpdateFallbackCounter(int potions)
    {
        if (fallbackCounterText != null)
        {
            fallbackCounterText.text = fallbackCounterPrefix + potions;
        }

        if (isAiming && potions <= 0)
        {
            EndAiming();
        }
    }
}
