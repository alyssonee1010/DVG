using UnityEngine;
using TMPro;

public class DVGPlacementCharacterSlot : MonoBehaviour
{
    const string PriceTextObjectName = "Price";

    [SerializeField] PlantPlacementManager placementManager;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] int cost;
    [SerializeField] TMP_Text costText;
    [SerializeField] string costPrefix = "";
    [SerializeField] GameObject selectionIndicator;
    [SerializeField] float selectedScaleMultiplier = 1.2f;

    Vector3 baseScale;
    bool hasBaseScale;

    public GameObject CharacterPrefab => characterPrefab;
    public int Cost => cost;

    void Awake()
    {
        EnsureCostText();
        FreezeSelectorAnimator();
        CaptureBaseScale();
        UpdateCostText();
        SetSelected(false);
    }

    void OnEnable()
    {
        EnsureCostText();
        UpdateCostText();
    }

    void Reset()
    {
        EnsureCostText();
        UpdateCostText();
    }

    void OnValidate()
    {
        EnsureCostText();
        UpdateCostText();
    }

    public void SetSelected(bool selected)
    {
        CaptureBaseScale();
        transform.localScale = selected ? baseScale * selectedScaleMultiplier : baseScale;
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
    }

    void CaptureBaseScale()
    {
        if (hasBaseScale)
        {
            return;
        }

        baseScale = transform.localScale;
        hasBaseScale = true;
    }

    void FreezeSelectorAnimator()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    void UpdateCostText()
    {
        if (costText != null)
        {
            costText.text = costPrefix + Mathf.Max(0, cost);
        }
    }

    void EnsureCostText()
    {
        if (costText != null)
        {
            return;
        }

        TMP_Text[] childTexts = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text childText in childTexts)
        {
            if (childText.name == PriceTextObjectName)
            {
                costText = childText;
                return;
            }
        }

        if (childTexts.Length == 1)
        {
            costText = childTexts[0];
        }
    }
}
