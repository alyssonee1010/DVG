using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds the Main Menu's Canvas and its three panels (Main / Stage Select / Settings) entirely in
// code at runtime, the same way VVELevelSelectUI/VVEDefenderLoadoutUI build their menus, so the
// scene itself only needs a Camera and this component. Only one panel is ever
// interactable/visible at a time.
public class VVEMainMenuController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] Camera menuCamera;
    [SerializeField] string gameplaySceneName = "Level 1";

    [Header("Defender Showcase")]
    [SerializeField] VVEDefender defenderPrefab;
    [SerializeField] Vector3 defenderPosition = new Vector3(-4f, -1f, 0f);
    [SerializeField] float idleBobHeight = 0.15f;
    [SerializeField] float idleBobDuration = 1.4f;

    [Header("Layout")]
    [SerializeField] Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] float panelFadeDuration = 0.2f;

    CanvasGroup mainPanel;
    CanvasGroup stageSelectPanel;
    CanvasGroup settingsPanel;
    RectTransform stageSelectListParent;
    Slider volumeSlider;
    Coroutine activeFade;

    void Awake()
    {
        if (menuCamera == null)
        {
            menuCamera = Camera.main;
        }

        VVEAudioSettings.ApplySavedVolume();
        EnsureEventSystem();

        Canvas canvas = BuildCanvas();
        mainPanel = BuildMainPanel(canvas.transform);
        stageSelectPanel = BuildStageSelectPanel(canvas.transform);
        settingsPanel = BuildSettingsPanel(canvas.transform);

        SetActivePanel(mainPanel, instant: true);
    }

    void Start()
    {
        SpawnDefenderShowcase();
    }

    // ---- Navigation -----------------------------------------------------

    public void ShowMainPanel()
    {
        SetActivePanel(mainPanel, instant: false);
    }

    public void ShowStageSelect()
    {
        SetActivePanel(stageSelectPanel, instant: false);
    }

    public void ShowSettings()
    {
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(VVEAudioSettings.MasterVolume);
        }

        SetActivePanel(settingsPanel, instant: false);
    }

    void SetActivePanel(CanvasGroup target, bool instant)
    {
        CanvasGroup[] allPanels = { mainPanel, stageSelectPanel, settingsPanel };
        foreach (CanvasGroup panel in allPanels)
        {
            if (panel == null)
            {
                continue;
            }

            bool isTarget = panel == target;
            panel.interactable = isTarget;
            panel.blocksRaycasts = isTarget;
        }

        if (activeFade != null)
        {
            StopCoroutine(activeFade);
            activeFade = null;
        }

        if (instant || !isActiveAndEnabled)
        {
            foreach (CanvasGroup panel in allPanels)
            {
                if (panel != null)
                {
                    panel.alpha = panel == target ? 1f : 0f;
                }
            }

            return;
        }

        activeFade = StartCoroutine(FadePanels(allPanels, target));
    }

    IEnumerator FadePanels(CanvasGroup[] panels, CanvasGroup target)
    {
        float[] startAlphas = panels.Select(panel => panel != null ? panel.alpha : 0f).ToArray();
        float elapsed = 0f;

        while (elapsed < panelFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / panelFadeDuration);
            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] == null)
                {
                    continue;
                }

                float targetAlpha = panels[i] == target ? 1f : 0f;
                panels[i].alpha = Mathf.Lerp(startAlphas[i], targetAlpha, t);
            }

            yield return null;
        }

        foreach (CanvasGroup panel in panels)
        {
            if (panel != null)
            {
                panel.alpha = panel == target ? 1f : 0f;
            }
        }

        activeFade = null;
    }

    // ---- Button/slider handlers -------------------------------------------

    void OnPlayClicked()
    {
        ShowStageSelect();
    }

    void OnSettingsClicked()
    {
        ShowSettings();
    }

    void OnBackToMainClicked()
    {
        ShowMainPanel();
    }

    void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnLevelSelected(VVELevelDefinition level)
    {
        VVEPendingLevelSelection.Set(level.Id);
        SceneManager.LoadScene(gameplaySceneName);
    }

    void OnVolumeChanged(float value)
    {
        VVEAudioSettings.SetMasterVolume(value);
    }

    // ---- Construction ------------------------------------------------

    void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    Canvas BuildCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = menuCamera;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    CanvasGroup BuildPanel(Transform parent, string name)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);

        RectTransform rect = panelObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CanvasGroup canvasGroup = panelObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        return canvasGroup;
    }

    CanvasGroup BuildMainPanel(Transform canvasTransform)
    {
        CanvasGroup panel = BuildPanel(canvasTransform, "MainPanel");

        CreateLabel(panel.transform, "Title", "VIKINGS VS EVERYONE", 64f, new Vector2(0.5f, 0.85f), new Vector2(900f, 120f));

        CreateButton(panel.transform, "PlayButton", "Play", new Vector2(0.5f, 0.55f), OnPlayClicked);
        CreateButton(panel.transform, "SettingsButton", "Settings", new Vector2(0.5f, 0.4f), OnSettingsClicked);
        CreateButton(panel.transform, "QuitButton", "Quit", new Vector2(0.5f, 0.25f), OnQuitClicked);

        return panel;
    }

    CanvasGroup BuildStageSelectPanel(Transform canvasTransform)
    {
        CanvasGroup panel = BuildPanel(canvasTransform, "StageSelectPanel");

        CreateLabel(panel.transform, "Title", "SELECT STAGE", 48f, new Vector2(0.5f, 0.9f), new Vector2(800f, 100f));
        CreateButton(panel.transform, "BackButton", "Back", new Vector2(0.12f, 0.9f), OnBackToMainClicked, new Vector2(200f, 60f));

        GameObject listObject = new GameObject("StageList");
        listObject.transform.SetParent(panel.transform, false);

        RectTransform listRect = listObject.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0.5f, 0f);
        listRect.anchorMax = new Vector2(0.5f, 0.8f);
        listRect.pivot = new Vector2(0.5f, 1f);
        listRect.anchoredPosition = Vector2.zero;
        listRect.sizeDelta = new Vector2(700f, 0f);

        VerticalLayoutGroup layout = listObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 12f;

        ContentSizeFitter fitter = listObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        stageSelectListParent = listRect;
        PopulateStageSelect();

        return panel;
    }

    // Groups by VVELevelDefinition.Stage (the level YAML's own "stage" int field) rather than
    // parsing the filename, so this stays correct for however many stages exist without a second
    // hard-coded notion of "stage" living in the Main Menu.
    void PopulateStageSelect()
    {
        if (stageSelectListParent == null)
        {
            return;
        }

        List<VVELevelDefinition> levels = VVELevelLoader.DiscoverLevels();
        if (levels.Count == 0)
        {
            Debug.LogWarning("No level files found in Assets/Levels.");
        }

        var groupedByStage = levels.GroupBy(level => level.Stage).OrderBy(group => group.Key);

        foreach (var stageGroup in groupedByStage)
        {
            CreateStageHeading(stageSelectListParent, "STAGE " + stageGroup.Key);

            foreach (VVELevelDefinition level in stageGroup.OrderBy(l => l.Level))
            {
                string label = level.Stage + "-" + level.Level.ToString("00") + (string.IsNullOrEmpty(level.Name) ? "" : "  " + level.Name);
                VVELevelDefinition capturedLevel = level;
                CreateListButton(stageSelectListParent, label, () => OnLevelSelected(capturedLevel));
            }
        }
    }

    CanvasGroup BuildSettingsPanel(Transform canvasTransform)
    {
        CanvasGroup panel = BuildPanel(canvasTransform, "SettingsPanel");

        CreateLabel(panel.transform, "Title", "SETTINGS", 48f, new Vector2(0.5f, 0.9f), new Vector2(800f, 100f));
        CreateButton(panel.transform, "BackButton", "Back", new Vector2(0.12f, 0.9f), OnBackToMainClicked, new Vector2(200f, 60f));

        CreateLabel(panel.transform, "VolumeLabel", "Master Volume", 28f, new Vector2(0.5f, 0.55f), new Vector2(500f, 50f));
        volumeSlider = CreateSlider(panel.transform, new Vector2(0.5f, 0.45f), new Vector2(500f, 40f), VVEAudioSettings.MasterVolume, OnVolumeChanged);

        return panel;
    }

    void SpawnDefenderShowcase()
    {
        if (defenderPrefab == null)
        {
            return;
        }

        VVEDefender instance = Instantiate(defenderPrefab, defenderPosition, Quaternion.identity, transform);
        StartCoroutine(IdleBob(instance.transform, defenderPosition));
    }

    IEnumerator IdleBob(Transform target, Vector3 basePosition)
    {
        while (target != null)
        {
            float offset = Mathf.Sin(Time.time * (Mathf.PI * 2f / idleBobDuration)) * idleBobHeight;
            target.position = basePosition + new Vector3(0f, offset, 0f);
            yield return null;
        }
    }

    // ---- UI element helpers -------------------------------------------

    TextMeshProUGUI CreateLabel(Transform parent, string name, string text, float fontSize, Vector2 anchor, Vector2 size, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;

        return label;
    }

    void CreateStageHeading(Transform parent, string text)
    {
        GameObject headingObject = new GameObject("Heading_" + text);
        headingObject.transform.SetParent(parent, false);

        LayoutElement layoutElement = headingObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 36f;

        TextMeshProUGUI heading = headingObject.AddComponent<TextMeshProUGUI>();
        heading.text = text;
        heading.fontSize = 26f;
        heading.alignment = TextAlignmentOptions.Left;
        heading.color = new Color(1f, 0.85f, 0.3f, 1f);
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick, Vector2? size = null)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = size ?? new Vector2(320f, 80f);
        rect.anchoredPosition = Vector2.zero;

        Image background = buttonObject.AddComponent<Image>();
        background.color = new Color(0.15f, 0.18f, 0.24f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        CreateLabel(buttonObject.transform, "Label", label, 32f, new Vector2(0.5f, 0.5f), rect.sizeDelta);

        return button;
    }

    Button CreateListButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject("Level_" + label);
        buttonObject.transform.SetParent(parent, false);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 56f;

        Image background = buttonObject.AddComponent<Image>();
        background.color = new Color(0.15f, 0.18f, 0.24f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 0f);
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 26f;
        text.alignment = TextAlignmentOptions.Left;
        text.color = Color.white;

        return button;
    }

    Slider CreateSlider(Transform parent, Vector2 anchor, Vector2 size, float initialValue, UnityEngine.Events.UnityAction<float> onChanged)
    {
        GameObject sliderObject = new GameObject("MasterVolumeSlider");
        sliderObject.transform.SetParent(parent, false);

        RectTransform rect = sliderObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(sliderObject.transform, false);
        RectTransform backgroundRect = backgroundObject.AddComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.12f, 0.8f);

        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5f, 0f);
        fillAreaRect.offsetMax = new Vector2(-5f, 0f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.sizeDelta = new Vector2(10f, 0f);
        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.6f, 0.35f, 1f);

        GameObject handleAreaObject = new GameObject("Handle Slide Area");
        handleAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform handleAreaRect = handleAreaObject.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handleObject = new GameObject("Handle");
        handleObject.transform.SetParent(handleAreaObject.transform, false);
        RectTransform handleRect = handleObject.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20f, size.y);
        Image handleImage = handleObject.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.targetGraphic = handleImage;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;
        slider.SetValueWithoutNotify(initialValue);
        slider.onValueChanged.AddListener(onChanged);

        return slider;
    }
}
