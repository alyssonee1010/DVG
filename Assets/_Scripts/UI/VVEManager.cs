using System.Collections.Generic;
using UnityEngine;

public class VVEManager : MonoBehaviour
{
    // 1. Static variable to hold the single instance
    public static VVEManager Instance { get; private set; }

    public VVEDefenderSelectBar defenderSelectBar;

    // GLOBAL GAME VARIABLES
    public List<VVEDefender> selectedDefenders = new();

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
        selectedDefenders.Clear();
        selectedDefenders.AddRange(defenders);
        defenderSelectBar.SetupCards();
    }
}