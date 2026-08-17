using UnityEngine;

public class VVEConstantRotation : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 60f;

    void Update()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
    }
}
