using UnityEngine;

public class WaveMovement : MonoBehaviour
{
    public float speed = 1.5f;
    public float resetXPosition = -12f; // Where the wave disappears on the left
    public float startXPosition = -12f;   // Where the wave respawns on the right

    void Update()
    {
        // Move the entire object cleanly to the left
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // If it goes past the left edge, loop it back to the right side
        if (transform.position.x > resetXPosition)
        {
            transform.position = new Vector3(startXPosition, transform.position.y, transform.position.z);
        }
    }
}
