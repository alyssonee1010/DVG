using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public int coins;
    public int health = 100;
    public Dictionary<string, Sprite> keys = new Dictionary<string, Sprite>();

    //Singleton instantiation
    private static PlayerStats instance;
    public static PlayerStats Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindFirstObjectByType<PlayerStats>();
                return instance;
            }
            else
            {
                return instance;
            }
        }
    }

    void Awake()
    {
       
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called whenever a scene finishes loading
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only load stats if we have saved data (health > 0 means we've saved before)
        if (health > 0)
        {
            LoadStats();
        }
    }

    // Save the current stats from PlayerController
    public void SaveStats()
    {
        PlayerController player = PlayerController.Instance;

        if (player != null)
        {
            coins = player.coins;
            health = player.health;

            // Copy the keys dictionary
            keys.Clear();
            foreach (var key in player.keys)
            {
                keys.Add(key.Key, key.Value);
            }

            Debug.Log("Stats saved: Coins=" + coins + ", Health=" + health + ", Keys=" + keys.Count);
        }
    }

    // Load the saved stats and apply them to PlayerController
    public void LoadStats()
    {
        PlayerController player = PlayerController.Instance;

        if (player != null)
        {
            player.coins = coins;
            player.health = health;

            // Copy the keys dictionary to player
            player.keys.Clear();
            foreach (var key in keys)
            {
                player.keys.Add(key.Key, key.Value);
            }

            // Update the key UI
            if (player.keys.Count > 0)
            {
                player.key.sprite = player.keys.First().Value;
            }
            else
            {
                player.key.sprite = player.knobUISprite;
            }

            player.UpdateUI();

            Debug.Log("Stats loaded: Coins=" + coins + ", Health=" + health + ", Keys=" + keys.Count);
        }
    }

    // Clear all saved stats
    public void ClearStats()
    {
        coins = 0;
        health = 0; // Set to 0 to prevent auto-loading
        keys.Clear();

        Debug.Log("Stats cleared");
    }
}