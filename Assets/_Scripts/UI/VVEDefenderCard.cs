using TMPro;
using UnityEngine;

public class VVEDefenderCard : MonoBehaviour
{
    public TextMeshPro priceTag;
    public VVEDefender defenderType;
    public Transform previewContainer;

    [SerializeField] PlantPlacementManager placementManager;
    [SerializeField] float selectedScaleMultiplier = 1.2f;

    Vector3 baseScale;
    bool hasBaseScale;

    public GameObject CharacterPrefab => defenderType.gameObject;
    public int Cost => defenderType.GetComponent<VVEDefender>().cost;

    void Start()
    {
        var preview = Instantiate(defenderType, previewContainer, false);
        preview.transform.localScale *= 0.67f;
        priceTag.text = Cost.ToString();

        CaptureBaseScale();
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        CaptureBaseScale();
        transform.localScale = selected ? baseScale * selectedScaleMultiplier : baseScale;
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
