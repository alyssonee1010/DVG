using UnityEngine;

public class DVGBoardPickup : MonoBehaviour
{
    [SerializeField] int value = 1;

    bool collected;

    public int Value => value;

    public void Initialize(int pickupValue)
    {
        value = pickupValue;
    }

    void OnMouseDown()
    {
        Collect();
    }

    public void Collect()
    {
        if (collected)
        {
            return;
        }

        collected = true;
        DVGUsableWallet wallet = DVGUsableWallet.Instance != null ? DVGUsableWallet.Instance : FindAnyObjectByType<DVGUsableWallet>();
        if (wallet != null)
        {
            wallet.AddDiamonds(value);
        }

        Destroy(gameObject);
    }
}
