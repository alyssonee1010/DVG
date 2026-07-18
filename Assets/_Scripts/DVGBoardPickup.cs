using UnityEngine;

public class DVGBoardPickup : MonoBehaviour
{
    [SerializeField] int value = 1;

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
        Destroy(gameObject);
    }
}
