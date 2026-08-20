using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Plants-vs-Zombies-style defender selection screen: a grid of every unlocked (and locked)
// defender on top, and a 6-slot tray below it showing the current loadout. Each card/token shows
// the defender's actual character art plus its diamond cost, matching the in-game placement bar.
// Clicking a grid card flies that visual into the first open tray slot; clicking a filled tray
// slot flies it back out and the tokens to its right slide left to close the gap. Tray order is
// "whatever was picked first stays first" — it is not tied to catalog order.
public class VVEDefenderLoadoutUI : MonoBehaviour
{
    [SerializeField] VVELevelSelectUI levelSelectUI;
    [SerializeField] VVEDefenderSelectBar selectBar;

    [Header("Grid")]
    [SerializeField] Vector2 gridCardSize = new Vector2(1.1f, 1.1f);
    [SerializeField] Vector2 gridCardSpacing = new Vector2(1.35f, 1.35f);
    [SerializeField] int cardsPerRow = 6;
    [SerializeField] Vector3 gridScreenAnchorOffset = new Vector3(0f, -0.6f, 0f);

    [Header("Tray")]
    [SerializeField] Vector2 traySlotSize = new Vector2(1f, 1f);
    [SerializeField] Vector2 traySlotSpacing = new Vector2(1.25f, 0f);
    [SerializeField] int traySlotCount = 6;
    [SerializeField] Vector3 trayScreenAnchorOffset = new Vector3(0f, -2.6f, 0f);
    [SerializeField] Color traySlotEmptyColor = new Color(1f, 1f, 1f, 0.12f);

    [Header("Colors")]
    [SerializeField] Color unlockedColor = new Color(0.16f, 0.22f, 0.3f, 0.95f);
    [SerializeField] Color selectedColor = new Color(0.2f, 0.55f, 0.25f, 0.95f);
    [SerializeField] Color lockedColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);
    [SerializeField] Color textColor = Color.white;
    [SerializeField] Color lockedTextColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] string costPrefix = "";

    [Header("Animation")]
    [SerializeField] float moveDuration = 0.35f;
    [SerializeField] float moveArcHeight = 0.6f;
    [SerializeField] float cameraPlaneDistance = 5f;

    class GridCardVisual
    {
        public GameObject background;
        public VVEDefenderLoadoutCard card;
        public string id;
    }

    GameObject root;
    GameObject gridRoot;
    GameObject trayRoot;
    readonly List<GridCardVisual> gridCards = new List<GridCardVisual>();
    readonly List<GameObject> traySlotBackgrounds = new List<GameObject>();
    readonly Dictionary<string, GameObject> trayTokens = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (levelSelectUI == null)
        {
            levelSelectUI = FindAnyObjectByType<VVELevelSelectUI>();
        }
    }

    void OnEnable()
    {
        VVEDefenderUnlocks.UnlocksChanged += Rebuild;
        VVEDefenderUnlocks.LoadoutChanged += OnLoadoutChanged;
        Rebuild();
    }

    void OnDisable()
    {
        VVEDefenderUnlocks.UnlocksChanged -= Rebuild;
        VVEDefenderUnlocks.LoadoutChanged -= OnLoadoutChanged;
    }

    void Update()
    {
        if (root == null || !root.activeSelf)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    public void SetVisible(bool visible)
    {
        if (root == null)
        {
            Rebuild();
        }

        if (root != null)
        {
            root.SetActive(visible);
        }
    }

    void HandleClick()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPosition);
        foreach (Collider2D hit in hits)
        {
            VVEDefenderLoadoutCard card = hit.GetComponent<VVEDefenderLoadoutCard>();
            if (card == null)
            {
                continue;
            }

            if (card.IsLocked)
            {
                return;
            }

            VVEDefenderUnlocks.ToggleLoadout(card.DefenderId);
            return;
        }
    }

    void Rebuild()
    {
        if (root != null)
        {
            Destroy(root);
        }

        gridCards.Clear();
        traySlotBackgrounds.Clear();
        trayTokens.Clear();

        if (VVEDefenderCatalog.Instance == null)
        {
            return;
        }

        root = new GameObject("Defender Loadout Menu");

        BuildGrid();
        BuildTray();
        SyncTrayTokens(animate: false);
        RefreshVisuals();
        PushSelectedDefendersToManager();
    }

    void BuildGrid()
    {
        gridRoot = new GameObject("Grid");
        gridRoot.transform.SetParent(root.transform, false);

        var entries = VVEDefenderCatalog.Instance.Entries;
        int index = 0;
        foreach (VVEDefenderCatalog.Entry entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id))
            {
                continue;
            }

            int column = index % cardsPerRow;
            int row = index / cardsPerRow;
            Vector3 localPosition = new Vector3(column * gridCardSpacing.x, -row * gridCardSpacing.y, 0f);
            BuildGridCard(entry, localPosition);
            index++;
        }

        int columnsInWidestRow = Mathf.Min(cardsPerRow, Mathf.Max(1, index));
        int rowCount = Mathf.CeilToInt(index / (float)cardsPerRow);
        float widthOfWidestRow = (columnsInWidestRow - 1) * gridCardSpacing.x;
        float totalHeight = (rowCount - 1) * gridCardSpacing.y;

        Vector3 cameraCenter = GetCameraPlaneCenter();
        gridRoot.transform.position = cameraCenter + new Vector3(-widthOfWidestRow / 2f, totalHeight / 2f, 0f) + gridScreenAnchorOffset;
    }

    void BuildGridCard(VVEDefenderCatalog.Entry entry, Vector3 localPosition)
    {
        GameObject card = new GameObject("Defender " + entry.id);
        card.transform.SetParent(gridRoot.transform, false);
        card.transform.localPosition = localPosition;

        BuildSlotVisual(card.transform, entry, gridCardSize);

        BoxCollider2D collider = card.AddComponent<BoxCollider2D>();
        collider.size = gridCardSize;

        VVEDefenderLoadoutCard cardMarker = card.AddComponent<VVEDefenderLoadoutCard>();
        cardMarker.DefenderId = entry.id;

        gridCards.Add(new GridCardVisual { background = card, card = cardMarker, id = entry.id });
    }

    void BuildTray()
    {
        trayRoot = new GameObject("Tray");
        trayRoot.transform.SetParent(root.transform, false);

        float widestRow = (traySlotCount - 1) * traySlotSpacing.x;
        Vector3 cameraCenter = GetCameraPlaneCenter();
        trayRoot.transform.position = cameraCenter + new Vector3(-widestRow / 2f, 0f, 0f) + trayScreenAnchorOffset;

        for (int i = 0; i < traySlotCount; i++)
        {
            GameObject slot = new GameObject("Tray Slot " + i);
            slot.transform.SetParent(trayRoot.transform, false);
            slot.transform.localPosition = new Vector3(i * traySlotSpacing.x, 0f, 0f);
            slot.transform.localScale = new Vector3(traySlotSize.x, traySlotSize.y, 1f);

            SpriteRenderer renderer = slot.AddComponent<SpriteRenderer>();
            renderer.sprite = VVEFlatSpriteFactory.GetWhiteSprite();
            renderer.color = traySlotEmptyColor;
            renderer.sortingLayerName = "UI";
            renderer.sortingOrder = 19999;

            traySlotBackgrounds.Add(slot);
        }
    }

    Vector3 GetTraySlotWorldPosition(int slotIndex)
    {
        return trayRoot.transform.position + new Vector3(slotIndex * traySlotSpacing.x, 0f, 0f);
    }

    void OnLoadoutChanged()
    {
        SyncTrayTokens(animate: true);
        RefreshVisuals();
        PushSelectedDefendersToManager();
    }

    void PushSelectedDefendersToManager()
    {
        if (VVEManager.Instance == null || VVEDefenderCatalog.Instance == null)
        {
            return;
        }

        List<VVEDefender> defenders = new List<VVEDefender>();
        foreach (string id in VVEDefenderUnlocks.GetLoadout())
        {
            VVEDefenderCatalog.Entry entry = VVEDefenderCatalog.Instance.FindById(id);
            VVEDefender defender = entry != null && entry.prefab != null ? entry.prefab.GetComponent<VVEDefender>() : null;
            if (defender != null)
            {
                defenders.Add(defender);
            }
        }

        VVEManager.Instance.SetSelectedDefenders(defenders);
    }

    // Reconciles trayTokens against the authoritative loadout order: removes tokens for
    // defenders no longer in the loadout (flying them out), adds tokens for new ones (flying
    // them in from their grid card), and re-flows every remaining token to its new slot index
    // so removing one from the middle closes the gap by sliding the rest left. When animate is
    // false (initial build), tokens are placed directly with no fly-in/fly-out.
    void SyncTrayTokens(bool animate)
    {
        if (trayRoot == null)
        {
            return;
        }

        IReadOnlyList<string> loadout = VVEDefenderUnlocks.GetLoadout();
        HashSet<string> loadoutSet = new HashSet<string>(loadout);

        List<string> toRemove = new List<string>();
        foreach (var pair in trayTokens)
        {
            if (!loadoutSet.Contains(pair.Key))
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (string id in toRemove)
        {
            GameObject token = trayTokens[id];
            trayTokens.Remove(id);
            if (token == null)
            {
                continue;
            }

            if (!animate)
            {
                Destroy(token);
                continue;
            }

            VVELoadoutTokenMover mover = token.GetComponent<VVELoadoutTokenMover>();
            if (mover == null)
            {
                mover = token.AddComponent<VVELoadoutTokenMover>();
            }

            mover.PlayExitAndDestroy(moveDuration * 0.6f);
        }

        for (int i = 0; i < loadout.Count; i++)
        {
            string id = loadout[i];
            Vector3 slotPosition = GetTraySlotWorldPosition(i);

            if (!trayTokens.TryGetValue(id, out GameObject token) || token == null)
            {
                token = BuildTrayToken(id);
                trayTokens[id] = token;

                if (!animate)
                {
                    token.transform.position = slotPosition;
                    continue;
                }

                Vector3 origin = FindGridCardWorldPosition(id) ?? slotPosition;
                VVELoadoutTokenMover mover = token.AddComponent<VVELoadoutTokenMover>();
                mover.PlayEnterFrom(origin, slotPosition, Vector3.one, moveDuration, moveArcHeight);
            }
            else if (token.transform.position != slotPosition)
            {
                if (!animate)
                {
                    token.transform.position = slotPosition;
                    continue;
                }

                VVELoadoutTokenMover mover = token.GetComponent<VVELoadoutTokenMover>();
                if (mover == null)
                {
                    mover = token.AddComponent<VVELoadoutTokenMover>();
                }

                mover.MoveTo(slotPosition, moveDuration, moveArcHeight * 0.3f);
            }
        }
    }

    GameObject BuildTrayToken(string id)
    {
        VVEDefenderCatalog.Entry entry = VVEDefenderCatalog.Instance.FindById(id);

        GameObject token = new GameObject("Tray Token " + id);
        token.transform.SetParent(root.transform, true);
        token.transform.localScale = Vector3.one;

        if (entry != null)
        {
            BuildSlotVisual(token.transform, entry, traySlotSize);
            Transform backgroundTransform = token.transform.Find("Background");
            SpriteRenderer backgroundRenderer = backgroundTransform != null ? backgroundTransform.GetComponent<SpriteRenderer>() : null;
            if (backgroundRenderer != null)
            {
                backgroundRenderer.color = selectedColor;
            }
        }

        BoxCollider2D collider = token.AddComponent<BoxCollider2D>();
        collider.size = traySlotSize;

        VVEDefenderLoadoutCard cardMarker = token.AddComponent<VVEDefenderLoadoutCard>();
        cardMarker.DefenderId = id;

        return token;
    }

    // Shared visual builder used by both the grid cards and tray tokens so a defender's
    // selection-screen icon is identical to the flying token and matches the in-game placement
    // bar: a background panel, an inert instantiated preview of the actual character (same
    // instantiate-in-a-deactivated-scratch-object trick VVEPlacementCharacterSlot uses, so
    // OnEnable-driven gameplay effects on the prefab never fire), and a diamond cost label.
    void BuildSlotVisual(Transform parent, VVEDefenderCatalog.Entry entry, Vector2 size)
    {
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(parent, false);
        backgroundObject.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer background = backgroundObject.AddComponent<SpriteRenderer>();
        background.sprite = VVEFlatSpriteFactory.GetWhiteSprite();
        background.sortingLayerName = "UI";
        background.sortingOrder = 20000;

        if (entry.prefab != null)
        {
            GameObject iconAnchor = new GameObject("Icon");
            iconAnchor.transform.SetParent(parent, false);
            iconAnchor.transform.localPosition = new Vector3(0f, 0f, -0.01f);

            GameObject preview = InstantiateInertPreview(entry.prefab, iconAnchor.transform);
            ApplyUiSortingRecursively(preview, 20001);

            float largestExtent = Mathf.Max(GetWorldExtent(preview), 0.01f);
            float targetExtent = Mathf.Min(size.x, size.y) * 0.85f;
            float scale = targetExtent / largestExtent;
            iconAnchor.transform.localScale = Vector3.one * scale;
        }

        GameObject textObject = new GameObject("Price");
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = new Vector3(0f, -size.y * 0.42f, -0.02f);

        TextMeshPro text = textObject.AddComponent<TextMeshPro>();
        text.text = costPrefix + Mathf.Max(0, entry.cost);
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 1.3f;
        text.color = textColor;
        text.sortingOrder = 20002;
        text.rectTransform.sizeDelta = new Vector2(size.x, size.y * 0.3f);

        Renderer textRenderer = textObject.GetComponent<Renderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerName = "UI";
        }
    }

    // Instantiates prefab under a scratch object deactivated BEFORE instantiation so Awake/
    // OnEnable never run on its components (some character prefabs have OnEnable-driven visual
    // side effects meant only for gameplay), then strips gameplay components before revealing it.
    GameObject InstantiateInertPreview(GameObject prefab, Transform parent)
    {
        GameObject scratchRoot = new GameObject("Icon Instantiation Scratch");
        scratchRoot.SetActive(false);

        GameObject instance = Instantiate(prefab, scratchRoot.transform);
        instance.name = prefab.name + " Preview";
        DisablePreviewGameplay(instance);

        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        Destroy(scratchRoot);
        return instance;
    }

    void DisablePreviewGameplay(GameObject preview)
    {
        Animator[] animators = preview.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
        {
            animator.enabled = false;
        }

        Collider2D[] colliders = preview.GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D previewCollider in colliders)
        {
            previewCollider.enabled = false;
        }

        MonoBehaviour[] behaviours = preview.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            behaviour.enabled = false;
        }
    }

    void ApplyUiSortingRecursively(GameObject instance, int baseOrder)
    {
        SpriteRenderer[] renderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingLayerName = "UI";
            renderers[i].sortingOrder = baseOrder + i;
        }
    }

    float GetWorldExtent(GameObject instance)
    {
        SpriteRenderer[] renderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0)
        {
            return 0f;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    Vector3? FindGridCardWorldPosition(string id)
    {
        foreach (GridCardVisual visual in gridCards)
        {
            if (visual.id == id && visual.card != null)
            {
                return visual.card.transform.position;
            }
        }

        return null;
    }

    void RefreshVisuals()
    {
        foreach (GridCardVisual visual in gridCards)
        {
            if (visual.background == null || visual.card == null)
            {
                continue;
            }

            Transform backgroundTransform = visual.background.transform.Find("Background");
            SpriteRenderer renderer = backgroundTransform != null ? backgroundTransform.GetComponent<SpriteRenderer>() : null;
            bool unlocked = VVEDefenderUnlocks.IsUnlocked(visual.id);
            bool selected = unlocked && VVEDefenderUnlocks.IsInLoadout(visual.id);

            visual.card.IsLocked = !unlocked;

            if (renderer != null)
            {
                renderer.color = !unlocked ? lockedColor : (selected ? selectedColor : unlockedColor);
            }

            visual.background.SetActive(true);
            SetIconVisible(visual.background.transform, unlocked);
        }
    }

    void SetIconVisible(Transform cardTransform, bool visible)
    {
        Transform icon = cardTransform.Find("Icon");
        if (icon != null)
        {
            icon.gameObject.SetActive(visible);
        }
    }

    Vector3 GetCameraPlaneCenter()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return Vector3.zero;
        }

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, cameraPlaneDistance);
        Vector3 worldCenter = camera.ScreenToWorldPoint(screenCenter);
        worldCenter.z = 0f;
        return worldCenter;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;
        return mouseWorldPosition;
    }
}
