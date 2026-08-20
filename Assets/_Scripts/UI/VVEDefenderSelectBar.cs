using UnityEngine;

public class VVEDefenderSelectBar : MonoBehaviour
{

    public Transform cardsContainer;
    public float slotWidth = 1;
    public VVEDefenderCard cardPrefab;

    public Vector3 GetCardPosition(int slot)
    {
        var container = cardsContainer.transform;
        return container.position + new Vector3(slotWidth,0,0) * slot * container.localScale.x;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupCards();
    }

    public void SetupCards()
    {
        ClearSlots();

        int i = 0;
        foreach (var slot in VVEManager.Instance.selectedDefenders)
        {
            if (i >= 6)
                break;

            var card = Instantiate(cardPrefab, cardsContainer);
            card.transform.position = GetCardPosition(i);
            card.defenderType = VVEManager.Instance.selectedDefenders[i];

            i += 1;
        }
    }

    void ClearSlots()
    {
        cardsContainer.DestroyChildren();
    }
}
