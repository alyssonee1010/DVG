using UnityEngine;

// Minimal master-volume setting: applies AudioListener.volume and persists it via PlayerPrefs.
// Deliberately has no broader settings/audio-mixer architecture until one is actually needed.
public static class VVEAudioSettings
{
    const string MasterVolumePrefsKey = "VVE_MasterVolume";
    const float DefaultMasterVolume = 1f;

    public static float MasterVolume
    {
        get { return PlayerPrefs.GetFloat(MasterVolumePrefsKey, DefaultMasterVolume); }
    }

    public static void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(MasterVolumePrefsKey, volume);
        PlayerPrefs.Save();
    }

    public static void ApplySavedVolume()
    {
        AudioListener.volume = MasterVolume;
    }
}
