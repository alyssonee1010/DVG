using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] string requiredKeyName;
    [SerializeField] AudioClip openDoorSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //If the player enters the trigger zone, check if they have the key, and if they do Destroy this game object
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == PlayerController.Instance.gameObject)
        {
            //Check if the player has the requried key name that this door requires.
            if (PlayerController.Instance.keys.ContainsKey(requiredKeyName))
            {
                PlayerController.Instance.audioSource.PlayOneShot(openDoorSound);
                PlayerController.Instance.RemoveKey();
                Destroy(gameObject);
            }
        }
    }
}
