using System;
using UnityEngine;

public class DVGUsableWallet : MonoBehaviour
{
    [SerializeField] int startingDiamonds = 5;

    public static DVGUsableWallet Instance { get; private set; }

    public int Diamonds { get; private set; }

    public event Action<int> DiamondsChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        Diamonds = Mathf.Max(0, startingDiamonds);
    }

    void OnEnable()
    {
        DiamondsChanged?.Invoke(Diamonds);
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
}
