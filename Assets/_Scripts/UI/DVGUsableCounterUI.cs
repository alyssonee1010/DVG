using TMPro;
using UnityEngine;

public class DVGUsableCounterUI : MonoBehaviour
{
    enum CounterResource { Diamonds, HealingPotions }

    [SerializeField] DVGUsableWallet wallet;
    [SerializeField] CounterResource resource = CounterResource.Diamonds;
    [SerializeField] TMP_Text diamondText;
    [SerializeField] string prefix = "";

    void Awake()
    {
        if (wallet == null)
        {
            wallet = DVGUsableWallet.Instance != null ? DVGUsableWallet.Instance : FindAnyObjectByType<DVGUsableWallet>();
        }

        if (diamondText == null)
        {
            diamondText = GetComponentInChildren<TMP_Text>();
        }
    }

    void OnEnable()
    {
        if (wallet == null)
        {
            wallet = DVGUsableWallet.Instance != null ? DVGUsableWallet.Instance : FindAnyObjectByType<DVGUsableWallet>();
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
}
