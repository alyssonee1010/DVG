using TMPro;
using UnityEngine;

public class VVEUsableCounterUI : MonoBehaviour
{
    enum CounterResource { Diamonds, HealingPotions }

    [SerializeField] VVEUsableWallet wallet;
    [SerializeField] CounterResource resource = CounterResource.Diamonds;
    [SerializeField] TMP_Text diamondText;
    [SerializeField] string prefix = "";

    void Awake()
    {
        if (IsPlacementSlotPrice())
        {
            enabled = false;
            return;
        }

        if (wallet == null)
        {
            wallet = VVEUsableWallet.Instance != null ? VVEUsableWallet.Instance : FindAnyObjectByType<VVEUsableWallet>();
        }

        if (diamondText == null)
        {
            diamondText = GetComponentInChildren<TMP_Text>();
        }
    }

    void OnEnable()
    {
        if (IsPlacementSlotPrice())
        {
            enabled = false;
            return;
        }

        if (wallet == null)
        {
            wallet = VVEUsableWallet.Instance != null ? VVEUsableWallet.Instance : FindAnyObjectByType<VVEUsableWallet>();
        }

        if (wallet != null)
        {
            if (resource == CounterResource.HealingPotions)
            {
                wallet.HealingPotionsChanged += UpdateCounter;
                UpdateCounter(wallet.HealingPotions);
            }
            else
            {
                wallet.DiamondsChanged += UpdateCounter;
                UpdateCounter(wallet.Diamonds);
            }
        }
    }

    void OnDisable()
    {
        if (wallet != null)
        {
            wallet.DiamondsChanged -= UpdateCounter;
            wallet.HealingPotionsChanged -= UpdateCounter;
        }
    }

    void UpdateCounter(int diamonds)
    {
        if (diamondText != null)
        {
            diamondText.text = prefix + diamonds;
        }
    }

    bool IsPlacementSlotPrice()
    {
        return name == "Price" && GetComponentInParent<VVEPlacementCharacterSlot>() != null;
    }
}
