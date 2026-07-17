using UnityEngine;

public class Enemy : PhysicsObject
{
    [Header("Enemy Movement")]
    [SerializeField] float maxSpeed = 1f;
    [SerializeField] Vector2 raycastOffset;
    int direction = 1;
    [SerializeField] LayerMask rayCastLayerMask; //Which layers does the raycast hit?
    [SerializeField] float wallDetectionLength = 1; //How far away should a wall be before I turn around?
    [SerializeField] float floorDetectionLength = 1; //How far away should a floor be before I turn around?
    [SerializeField] int attackDamage = 1; //How much damage occurs when the enemy hurts the player

    // Update is called once per frame
    void Update()
    {
        targetVelocity = new Vector2(maxSpeed * direction, 0);

        //If I'm shooting a ray down one unit to the right, and I don't detect ground, then reverse my speed!
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + raycastOffset.x, transform.position.y + raycastOffset.y), Vector2.down, floorDetectionLength, rayCastLayerMask);
        Debug.DrawRay(new Vector2(transform.position.x + raycastOffset.x, transform.position.y + raycastOffset.y), Vector2.down * floorDetectionLength, Color.red);

        if (!hit) direction = -1;

        //If I'm shooting a ray down one unit to the left, and I don't detect ground, then reverse my speed!
        hit = Physics2D.Raycast(new Vector2(transform.position.x - raycastOffset.x, transform.position.y + raycastOffset.y), Vector2.down, floorDetectionLength, rayCastLayerMask);
        Debug.DrawRay(new Vector2(transform.position.x - raycastOffset.x, transform.position.y + raycastOffset.y), Vector2.down * floorDetectionLength, Color.green);

        if (!hit) direction = 1;

        //If I'm shooting a ray to the right, and I do detect wall, then reverse my speed!
        hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.right, wallDetectionLength, rayCastLayerMask);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y), Vector2.right * wallDetectionLength, Color.blue);

        if (hit)
        {
            if (hit.transform.gameObject != gameObject) direction = -1;
        }

        //If I'm shooting a ray to the left, and I do detect wall, then reverse my speed!
        hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.left, wallDetectionLength, rayCastLayerMask);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y), Vector2.left * wallDetectionLength, Color.yellow);

        if (hit)
        {
            if (hit.transform.gameObject != gameObject) direction = 1;
        }
    }
}