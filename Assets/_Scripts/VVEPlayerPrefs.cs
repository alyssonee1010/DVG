using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Single reset point for every PlayerPrefs-backed VVE system.
public static class VVEPlayerPrefs
{
    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        VVEDefenderUnlocks.ReloadAfterPlayerPrefsClear();
        VVELevelCompletion.ReloadAfterPlayerPrefsClear();
        VVEAudioSettings.ReloadAfterPlayerPrefsClear();
    }

#if UNITY_EDITOR
    [MenuItem("Tools/VVE/Clear PlayerPrefs")]
    static void ClearAllFromMenu()
    {
        ClearAll();
        Debug.Log("VVE PlayerPrefs cleared. Progression and audio settings are back to defaults.");
    }
#endif
}
