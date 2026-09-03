using System.Collections.Generic;
using UnityEngine;

// Persists the stable ids of levels the player has completed.
public static class VVELevelCompletion
{
    const string CompletedLevelIdsPrefsKey = "VVE_CompletedLevelIds";

    static HashSet<string> completedLevelIds;

    static void EnsureLoaded()
    {
        // Unity can clear PlayerPrefs without reloading scripts, so do not retain stale history.
        if (completedLevelIds != null && !PlayerPrefs.HasKey(CompletedLevelIdsPrefsKey))
        {
            completedLevelIds = null;
        }

        if (completedLevelIds != null)
        {
            return;
        }

        completedLevelIds = new HashSet<string>();
        string saved = PlayerPrefs.GetString(CompletedLevelIdsPrefsKey, "");
        foreach (string id in saved.Split(','))
        {
            if (!string.IsNullOrEmpty(id))
            {
                completedLevelIds.Add(id);
            }
        }
    }

    public static bool IsCompleted(string levelId)
    {
        EnsureLoaded();
        return !string.IsNullOrEmpty(levelId) && completedLevelIds.Contains(levelId);
    }

    public static IReadOnlyCollection<string> GetCompletedLevelIds()
    {
        EnsureLoaded();
        return completedLevelIds;
    }

    public static bool MarkCompleted(string levelId)
    {
        if (string.IsNullOrEmpty(levelId))
        {
            return false;
        }

        EnsureLoaded();
        if (!completedLevelIds.Add(levelId))
        {
            return false;
        }

        PlayerPrefs.SetString(CompletedLevelIdsPrefsKey, string.Join(",", completedLevelIds));
        PlayerPrefs.Save();
        return true;
    }

    // Keep runtime state in sync when PlayerPrefs are cleared without restarting Unity.
    public static void ReloadAfterPlayerPrefsClear()
    {
        completedLevelIds = null;
    }
}
