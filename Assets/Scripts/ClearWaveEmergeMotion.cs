using UnityEngine;

public class ClearWaveEmergeMotion : MonoBehaviour
{
    public float speed = 0.75f;
    public float phase;
    public float emergeHeight = 0.35f;
    public float tiltAmount = 12f;

    private Vector3 startLocalPosition;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float angle = (Time.time * speed + phase) * Mathf.PI * 2f;
        float emerge = (Mathf.Sin(angle) + 1f) * 0.5f;
        float tilt = Mathf.Sin(angle - Mathf.PI * 0.35f) * tiltAmount;

        transform.localPosition = startLocalPosition + Vector3.up * (emerge * emergeHeight);
        transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }
}
