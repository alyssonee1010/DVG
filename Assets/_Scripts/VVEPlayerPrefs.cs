using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
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

    [MenuItem("Tools/VVE/Unlock Everything (Dev)")]
    static void UnlockEverythingFromMenu()
    {
        if (VVEDefenderCatalog.Instance != null)
        {
            List<string> allDefenderIds = VVEDefenderCatalog.Instance.Entries
                .Where(entry => entry != null)
                .Select(entry => entry.id)
                .ToList();
            VVEDefenderUnlocks.UnlockAll(allDefenderIds);
        }
        else
        {
            Debug.LogWarning("VVE Unlock Everything: no VVEDefenderCatalog instance found (enter Play Mode first to unlock defenders).");
        }

        foreach (VVELevelDefinition level in VVELevelLoader.DiscoverLevels())
        {
            VVELevelCompletion.MarkCompleted(level.Id);
        }

        Debug.Log("VVE dev unlock: all defenders and levels unlocked.");
    }
#endif
}
