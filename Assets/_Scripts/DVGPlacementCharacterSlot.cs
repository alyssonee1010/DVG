using UnityEngine;

public class DVGPlacementCharacterSlot : MonoBehaviour
{
    [SerializeField] PlantPlacementManager placementManager;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] int cost;
    [SerializeField] GameObject selectionIndicator;
    [SerializeField] float selectedScaleMultiplier = 1.2f;

    Vector3 baseScale;
    bool hasBaseScale;

    public GameObject CharacterPrefab => characterPrefab;
    public int Cost => cost;

    void Awake()
    {
        CaptureBaseScale();
        SetSelected(false);
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
}
