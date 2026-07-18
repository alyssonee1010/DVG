using UnityEngine;
using TMPro;

public class DVGPlacementCharacterSlot : MonoBehaviour
{
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
        FreezeSelectorAnimator();
        CaptureBaseScale();
        UpdateCostText();
        SetSelected(false);
    }

    void OnValidate()
    {
        UpdateCostText();
    }

    void OnMouseDown()
    {
        if (placementManager == null)
        {
            placementManager = FindAnyObjectByType<PlantPlacementManager>();
        }

        if (placementManager != null)
        {
            placementManager.SelectCharacter(this);
        }
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
}
