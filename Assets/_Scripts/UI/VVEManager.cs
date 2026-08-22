using System;
using System.Collections.Generic;
using UnityEngine;

public class VVEManager : MonoBehaviour
{
    public static VVEManager Instance { get; private set; }

    // GLOBAL GAME VARIABLES
    public List<VVEDefender> SelectedDefenders = new();

    public const int MaxDefenderTypes = 6;


    [SerializeField] VVEWaveDirector waveDirector;

    public bool MenuIsOpen = false;

    void OnEnable() {
         waveDirector.LevelStarted += LevelStartedHandler;
        waveDirector.LevelCompleted += LevelCompletedHandler;
    }

    void OnDisable()
    {
        waveDirector.LevelStarted -= LevelStartedHandler;
        waveDirector.LevelCompleted -= LevelCompletedHandler;
    }

    private void LevelCompletedHandler(object evArgs)
    {
        SetMenuIsOpen(true);
    }

    private void LevelStartedHandler(object evArgs)
    {
        SetMenuIsOpen(false);
    }

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

    void Start()
    {
        if (!MenuIsOpen)
            ToggleMenu();
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
        SetMenuIsOpen(!MenuIsOpen);
    }

    public void SetMenuIsOpen(bool isOpen)
    {
        MenuIsOpen = isOpen;
        OnToggleMenu.Invoke(isOpen);
    }
}