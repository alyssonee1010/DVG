using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("Breakable Attributes")]
    [SerializeField] int health = 100;
    [SerializeField] Animator animator;
    [SerializeField] AudioClip takeDamageSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField] GameObject deathParticles;

    [Header("Drops")]
    [SerializeField] GameObject collectablePrefab; //Which collectable prefab should drop?
    [SerializeField] int dropCount = 1; //How many collectables should drop?
    [SerializeField] float launchForceY = 5f; //How much upward force to apply
    [SerializeField] float launchForceX = 3f; //How much horizontal force to apply (randomized between negative and positive)

    //Take damage at a specific amount
    public virtual void TakeDamage(int amount)
    {
        health -= amount;
        animator.SetTrigger("flash");

        if (health <= 0)
        {
            DropCollectables();
            deathParticles.SetActive(true);
            Destroy(deathParticles, 3f);
            deathParticles.transform.parent = null;
            PlayerController.Instance.audioSource.PlayOneShot(deathSound);
            Destroy(gameObject);
        }
        else
        {
            PlayerController.Instance.audioSource.PlayOneShot(takeDamageSound);
        }
    }

    //Spawn collectables and launch them with a random force
    void DropCollectables()
    {
        if (collectablePrefab == null) return;

        for (int i = 0; i < dropCount; i++)
        {
            GameObject drop = Instantiate(collectablePrefab, transform.position, Quaternion.identity);

            Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float randomX = Random.Range(-launchForceX, launchForceX);
                float randomY = Random.Range(launchForceY * 0.5f, launchForceY);
                rb.AddForce(new Vector2(randomX, randomY), ForceMode2D.Impulse);
            }
        }
    }
}