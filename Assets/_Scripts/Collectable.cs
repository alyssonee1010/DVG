using UnityEngine;

public class Collectable : MonoBehaviour
{
    enum CollectableType { Coin, Health, Key }
    [SerializeField] CollectableType collectableType;
    [SerializeField] int amount = 1;
    [SerializeField] AudioClip collectionSound;
    [SerializeField] float collectionSoundVolume;

    [Header("Optional Key Info")]
    [Tooltip("Only used if this collectable is a key!")]
    [SerializeField] Sprite keyUISprite;
    [Tooltip("Only used if this collectable is a key!")]
    [SerializeField] string keyName;

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        //If the player touches the collectable, destroy!
        if (collision.gameObject == PlayerController.Instance.gameObject)
        {
            if (collectableType == CollectableType.Coin)
            {
                PlayerController.Instance.coins += amount;
            }
            else if (collectableType == CollectableType.Health)
            {
                if (PlayerController.Instance.health < PlayerController.Instance.maxHealth) {
                    PlayerController.Instance.health += amount;
                }
            }
            else if (collectableType == CollectableType.Key)
            {
                //Set the key sprite in the UI and also add the key string to the player's stats
                PlayerController.Instance.AddKey(keyName, keyUISprite);
            }

            PlayerController.Instance.audioSource.PlayOneShot(collectionSound, collectionSoundVolume);
            PlayerController.Instance.UpdateUI();
            Destroy(gameObject);
        }
    }
}
