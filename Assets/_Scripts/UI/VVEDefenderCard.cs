using TMPro;
using Unity.AppUI.UI;
using Unity.VisualScripting;
using UnityEngine;
using PrimeTween;
using UnityEngine.EventSystems;

public class VVEDefenderCard : MonoBehaviour, IPointerClickHandler
{
    public TextMeshPro priceTag;
    public VVEDefender defenderType;
    public Transform previewContainer;

    [SerializeField] float selectedScaleMultiplier = 1.2f;

    Vector3 baseScale;
    bool hasBaseScale;

    public GameObject CharacterPrefab => defenderType.gameObject;
    public int Cost => defenderType.GetComponent<VVEDefender>().cost;

    void ToggleSelect()
    {
        if (VVEManager.Instance.SelectedDefenders.Contains(defenderType))
        {
            VVEManager.Instance.SelectedDefenders.Remove(defenderType);
            var targetPos = VVEUiWidgetRefs.Instance.defenderSelectionUi.GetDefenderPosition(defenderType);
            transform.SetParent(VVEUiWidgetRefs.Instance.defenderSelectionUi.transform);
            VVEUiWidgetRefs.Instance.defenderSelectionTopBar.CloseGaps();
            transform.TweenTo(targetPos, 0.5f);
        } else
        {
            var selectedCount = VVEManager.Instance.SelectedDefenders.Count;
            if (selectedCount >= VVEManager.MaxDefenderTypes)
                return;
        
            VVEManager.Instance.SelectedDefenders.Add(defenderType);
            var targetPos = VVEUiWidgetRefs.Instance.defenderSelectionTopBar.GetCardPosition(selectedCount);
            transform.SetParent(VVEUiWidgetRefs.Instance.defenderSelectionTopBar.cardsContainer);
            transform.TweenTo(targetPos, 0.5f);
        }

    }

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (VVEManager.Instance.MenuIsOpen){
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                ToggleSelect();
            }
        }
    }
}
