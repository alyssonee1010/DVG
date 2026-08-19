using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VVELevelSelectUI : MonoBehaviour
{
    [SerializeField] PlantPlacementManager placementManager;
    [SerializeField] VVEWaveDirector waveDirector;
    [SerializeField] VVEDefenderLoadoutUI loadoutUI;
    [SerializeField] VVEPlacementSlotBinder slotBinder;
    [SerializeField] Vector2 cardSize = new Vector2(2.2f, 1f);
    [SerializeField] Vector2 cardSpacing = new Vector2(2.5f, 1.3f);
    [SerializeField] int cardsPerRow = 3;
    [SerializeField] Color cardColor = new Color(0.12f, 0.16f, 0.24f, 0.95f);
    [SerializeField] Color cardTextColor = Color.white;
    [SerializeField] float cameraPlaneDistance = 5f;

    GameObject root;

    void Awake()
    {
        if (placementManager == null)
        {
            placementManager = FindAnyObjectByType<PlantPlacementManager>();
        }

        if (waveDirector == null)
        {
            waveDirector = FindAnyObjectByType<VVEWaveDirector>();
        }

        if (loadoutUI == null)
        {
            loadoutUI = FindAnyObjectByType<VVEDefenderLoadoutUI>();
        }

        if (slotBinder == null)
        {
            slotBinder = FindAnyObjectByType<VVEPlacementSlotBinder>();
        }

        if (waveDirector != null)
        {
            waveDirector.LevelCompleted += OnLevelCompleted;
        }
    }

    void OnDestroy()
    {
        if (waveDirector != null)
        {
            waveDirector.LevelCompleted -= OnLevelCompleted;
        }
    }

    void Start()
    {
        OpenMenu();
    }

    void OnLevelCompleted(VVELevelDefinition level)
    {
        if (placementManager != null)
        {
            placementManager.ResetBoard();
        }

        if (VVEBoardLife.Instance != null)
        {
            VVEBoardLife.Instance.ResetLife();
        }

        if (level != null && level.Unlocks.Count > 0)
        {
            VVEDefenderUnlocks.UnlockAll(level.Unlocks);
        }

        OpenMenu();
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

    public void OpenMenu()
    {
        if (placementManager != null)
        {
            placementManager.enabled = false;
        }

        if (loadoutUI != null)
        {
            loadoutUI.SetVisible(true);
        }

        BuildMenu();
    }

    void HandleClick()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPosition);
        foreach (Collider2D hit in hits)
        {
            VVELevelSelectCard card = hit.GetComponent<VVELevelSelectCard>();
            if (card != null)
            {
                SelectLevel(card.Level);
                return;
            }
        }
    }

    void SelectLevel(VVELevelDefinition level)
    {
        if (level == null)
        {
            return;
        }

        CloseMenu();

        if (slotBinder != null)
        {
            slotBinder.Rebind();
        }

        if (waveDirector != null)
        {
            waveDirector.StartLevel(level);
        }
    }

    void CloseMenu()
    {
        if (root != null)
        {
            Destroy(root);
            root = null;
        }

        if (loadoutUI != null)
        {
            loadoutUI.SetVisible(false);
        }

        if (placementManager != null)
        {
            placementManager.enabled = true;
        }
    }

    void BuildMenu()
    {
        if (root != null)
        {
            Destroy(root);
        }

        root = new GameObject("Level Select Menu");

        List<VVELevelDefinition> levels = VVELevelLoader.DiscoverLevels();
        if (levels.Count == 0)
        {
            Debug.LogWarning("No level files found in Assets/Levels.");
        }

        for (int i = 0; i < levels.Count; i++)
        {
            int column = i % cardsPerRow;
            int row = i / cardsPerRow;
            Vector3 localPosition = new Vector3(column * cardSpacing.x, -row * cardSpacing.y, 0f);
            BuildCard(levels[i], localPosition);
        }

        CenterMenu(levels.Count);
    }

    void CenterMenu(int levelCount)
    {
        if (levelCount == 0)
        {
            root.transform.position = GetCameraPlaneCenter();
            return;
        }

        int columnsInWidestRow = Mathf.Min(cardsPerRow, levelCount);
        int rowCount = Mathf.CeilToInt(levelCount / (float)cardsPerRow);
        float widthOfWidestRow = (columnsInWidestRow - 1) * cardSpacing.x;
        float totalHeight = (rowCount - 1) * cardSpacing.y;

        Vector3 cameraCenter = GetCameraPlaneCenter();
        root.transform.position = cameraCenter + new Vector3(-widthOfWidestRow / 2f, totalHeight / 2f, 0f);
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

    void BuildCard(VVELevelDefinition level, Vector3 localPosition)
    {
        GameObject card = new GameObject("Level " + level.Id);
        card.transform.SetParent(root.transform, false);
        card.transform.localPosition = localPosition;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(card.transform, false);
        backgroundObject.transform.localScale = new Vector3(cardSize.x, cardSize.y, 1f);

        SpriteRenderer background = backgroundObject.AddComponent<SpriteRenderer>();
        background.sprite = VVEFlatSpriteFactory.GetWhiteSprite();
        background.color = cardColor;
        background.sortingLayerName = "UI";
        background.sortingOrder = 20000;

        BoxCollider2D collider = card.AddComponent<BoxCollider2D>();
        collider.size = cardSize;

        VVELevelSelectCard cardMarker = card.AddComponent<VVELevelSelectCard>();
        cardMarker.Level = level;

        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(card.transform, false);
        textObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        TextMeshPro text = textObject.AddComponent<TextMeshPro>();
        text.text = level.Stage + "-" + level.Level.ToString("00") + "\n" + level.Name;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 2.4f;
        text.color = cardTextColor;
        text.sortingOrder = 20001;
        text.rectTransform.sizeDelta = cardSize;

        Renderer textRenderer = textObject.GetComponent<Renderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerName = "UI";
        }
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
