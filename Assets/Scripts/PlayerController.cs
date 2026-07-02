using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Hierarchy;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : PhysicsObject
{
    [Header("Movement")]
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip takeDamageSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField] float maxSpeed = 5;
    [SerializeField] float jumpPower = 10;
    [SerializeField] float jumpVelocityResetOnJumpRelease = 2f; //Quickly start dropping the player when jump is released early
    [SerializeField] float coyoteTime = .5f; //If the player falls from a ledge, we give them this amount of time of grace to still be able to jump
    [SerializeField] float coyoteTimer;

    [Header("Temporary Stats")]
    public int coins;
    public int maxHealth = 100;
    public int health = 100;
    public Dictionary<string, Sprite> keys = new Dictionary<string, Sprite>();

    [Header("References")]
    public Animator animator;
    public AudioSource audioSource;

    [Header("UI References")]
    public Image healthBar;
    public float healthBarOrigWidth;
    public TMP_Text coinText;
    public Image key;
    public Sprite knobUISprite; //The default sprite for the key UI on Start

    [Header("Attacking")]
    [SerializeField] AttackBox attackBox; //Reference to the attackBox child

    [Header("Player Knockback")]
    [SerializeField] Vector2 knockbackPower; //How far should the player knockback when getting hit? How far should they fly up when getting hit?
    [SerializeField] float launch;
    [SerializeField] float launchRecovery = 1; //How fast should the player recover after a launch

    //Singleton instantiation
    private static PlayerController instance;
    public static PlayerController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindFirstObjectByType<PlayerController>();
                return instance;
            }
            else
            {
                return instance;
            }
        }
    }

    // Awake is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //Find all gameobjects in scene called Player Controller, if I find one that is not me, destroy me!
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.parent.gameObject);
        }
        else if (instance != this)
        {
            Destroy(transform.parent.gameObject);
            return;
        }

        MoveToSpawnPoint();

        healthBarOrigWidth = healthBar.rectTransform.sizeDelta.x;
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        targetVelocity = new Vector2(Input.GetAxis("Horizontal") * maxSpeed + launch, 0);

        //Move launch back to 0 slowly
        launch += (0 - launch) * Time.deltaTime * launchRecovery;

        //Increase the coyoteTimer
        if (!grounded)
        {
            coyoteTimer += Time.deltaTime;
        }
        else
        {
            coyoteTimer = 0;
        }


        // If the player presses the Space key, set the velocity to the jump power
        if (Input.GetButtonDown("Jump") && coyoteTimer < coyoteTime)
        {
            audioSource.PlayOneShot(jumpSound);
            velocity.y = jumpPower;
            grounded = false;
            coyoteTimer = coyoteTime;
        }

        //If the player releases the jump button while still moving upward, cut the velocity short
        if (Input.GetButtonUp("Jump") && velocity.y > jumpVelocityResetOnJumpRelease)
        {
            velocity.y = jumpVelocityResetOnJumpRelease;
        }

        //If the player is going faster than .01, then set the localScale.x to 1. Otherwise, flip to the left
        if(velocity.x < -.01)
        {
            transform.localScale = new Vector2(-1, 1);
        }
        else if(velocity.x > .01)
        {
            transform.localScale = new Vector2(1, 1);
        }

        //If the player presses the attack button, activate the attack box
        if (Input.GetButtonDown("Fire1")){
            StartCoroutine(ActivateAttackBox());
            animator.SetFloat("attackDirectionY", Input.GetAxis("Vertical"));
            animator.SetTrigger("attack");
        }

        //Set each animator float, bool, and trigger so it knows which animation to play!
        animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
        animator.SetFloat("velocityY", velocity.y);
        animator.SetBool("grounded", grounded);

    }

    IEnumerator ActivateAttackBox()
    {
        attackBox.gameObject.SetActive(true);
        yield return new WaitForSeconds(.1f);
        attackBox.gameObject.SetActive(false);
    }

    public void MoveToSpawnPoint()
    {
        transform.position = GameObject.Find("Spawn Point").transform.position;
    }


    public void AddKey(string keyName, Sprite sprite)
    {
        //Add the key to the player stats and also update the key UI
        keys.Add(keyName, sprite);
        Debug.Log("Added key: " + keys);

        if(keys.Count > 0)
        {
            key.sprite = keys.First().Value;
        }
    }

    public void RemoveKey()
    {
        if (keys.Count > 0)
        {
            keys.Clear();
            key.sprite = knobUISprite;
        }
    }

    public void ResetPlayer()
    {
        PlayerStats.Instance.LoadStats();
        UpdateUI();
        MoveToSpawnPoint();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Update the UI elements based on coins collected, current health, and if I have a key
    public void UpdateUI()
    {
        coinText.text = coins.ToString();
        animator.SetTrigger("flash");
        //Find the percentage of health remaining vs the maximum health
        float healthPercent = (float)health / (float)maxHealth;
        //Set the health bar width to the original width multiplied by the percentage of health remaining
        healthBar.rectTransform.sizeDelta = new Vector2(healthBarOrigWidth * healthPercent, healthBar.rectTransform.sizeDelta.y);
    }

    //Remove health and update the UI
    public void TakeDamage(int amount, int attackSide)
    {
        health -= amount;
        launch = -attackSide * knockbackPower.x;
        animator.SetTrigger("flash");
        velocity.y = knockbackPower.y;
        
        //If my health gets to zero, reload the scene!
        if(health <= 0)
        {
            audioSource.PlayOneShot(deathSound);
            ResetPlayer();
        }
        else
        {
            audioSource.PlayOneShot(takeDamageSound);
        }

            UpdateUI();
    }
}
