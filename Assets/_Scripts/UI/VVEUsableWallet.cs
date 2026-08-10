using System;
using UnityEngine;

public class VVEUsableWallet : MonoBehaviour
{
    [SerializeField] int startingDiamonds = 5;
    [SerializeField] int startingHealingPotions;

    public static VVEUsableWallet Instance { get; private set; }

    public int Diamonds { get; private set; }
    public int HealingPotions { get; private set; }

    public event Action<int> DiamondsChanged;
    public event Action<int> HealingPotionsChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        Diamonds = Mathf.Max(0, startingDiamonds);
        HealingPotions = Mathf.Max(0, startingHealingPotions);
    }

    void OnEnable()
    {
        DiamondsChanged?.Invoke(Diamonds);
        HealingPotionsChanged?.Invoke(HealingPotions);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool CanAfford(int cost)
    {
        return Diamonds >= Mathf.Max(0, cost);
    }

    public bool TrySpendDiamonds(int cost)
    {
        cost = Mathf.Max(0, cost);
        if (!CanAfford(cost))
        {
            return false;
        }

        Diamonds -= cost;
        DiamondsChanged?.Invoke(Diamonds);
        return true;
    }

    public void AddDiamonds(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Diamonds += amount;
        DiamondsChanged?.Invoke(Diamonds);
    }

    public bool CanUseHealingPotion(int cost = 1)
    {
        return HealingPotions >= Mathf.Max(1, cost);
    }

    public bool TrySpendHealingPotion(int cost = 1)
    {
        cost = Mathf.Max(1, cost);
        if (!CanUseHealingPotion(cost))
        {
            return false;
        }

        HealingPotions -= cost;
        HealingPotionsChanged?.Invoke(HealingPotions);
        return true;
    }

    public void AddHealingPotions(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        HealingPotions += amount;
        HealingPotionsChanged?.Invoke(HealingPotions);
    }
}
