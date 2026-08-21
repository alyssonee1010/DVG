using System;
using System.Collections.Generic;
using UnityEngine;

public class VVEManager : MonoBehaviour
{
    // 1. Static variable to hold the single instance
    public static VVEManager Instance { get; private set; }

    // GLOBAL GAME VARIABLES
    public List<VVEDefender> SelectedDefenders = new();

    public const int MaxDefenderTypes = 6;


    public bool MenuIsOpen = false;

    private void Awake()
    {
        // 2. Check if an instance already exists in the scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate managers
            return;
        }

        // 3. Set this object as the definitive instance
        Instance = this;

        // 4. Optional: Keep this object alive when changing scenes
        DontDestroyOnLoad(gameObject);
    }

    public void SetSelectedDefenders(IEnumerable<VVEDefender> defenders)
    {
        SelectedDefenders.Clear();
        SelectedDefenders.AddRange(defenders);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    } 

    public static event Action<bool> OnToggleMenu;

    public void ToggleMenu()
    {
        MenuIsOpen = !MenuIsOpen;
        OnToggleMenu.Invoke(MenuIsOpen);
    }
}