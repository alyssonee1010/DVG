using UnityEngine;

public class VVEUiWidgetRefs : MonoBehaviour
{
    [field: SerializeField] 
    public VVEDefenderSelectBar defenderSelectionTopBar { get; private set; }

    [field: SerializeField] 
    public VVEDefenderSelectionUi defenderSelectionUi { get; private set; }

    public static VVEUiWidgetRefs Instance { get; private set; }


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

}
