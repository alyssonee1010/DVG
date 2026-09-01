using UnityEngine;

// Master/music/sfx volume settings, persisted via PlayerPrefs. Master applies globally through
// AudioListener.volume (so it affects every sound with no per-source wiring); music and sfx are
// separate multipliers that consumers (VVELevelMusicController, VVEAnimationSoundPlayer) apply to
// their own AudioSource on top of that, since there is no audio-mixer routing set up yet.
public static class VVEAudioSettings
{
    const string MasterVolumePrefsKey = "VVE_MasterVolume";
    const string MusicVolumePrefsKey = "VVE_MusicVolume";
    const string SfxVolumePrefsKey = "VVE_SfxVolume";
    const float DefaultVolume = 1f;

    public static event System.Action MusicVolumeChanged;
    public static event System.Action SfxVolumeChanged;

    public static float MasterVolume
    {
        get { return PlayerPrefs.GetFloat(MasterVolumePrefsKey, DefaultVolume); }
    }

    public static float MusicVolume
    {
        get { return PlayerPrefs.GetFloat(MusicVolumePrefsKey, DefaultVolume); }
    }

    public static float SfxVolume
    {
        get { return PlayerPrefs.GetFloat(SfxVolumePrefsKey, DefaultVolume); }
    }

    public static void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(MasterVolumePrefsKey, volume);
        PlayerPrefs.Save();
    }

    public static void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumePrefsKey, volume);
        PlayerPrefs.Save();
        MusicVolumeChanged?.Invoke();
    }

    public static void SetSfxVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumePrefsKey, volume);
        PlayerPrefs.Save();
        SfxVolumeChanged?.Invoke();
    }

    public static void ApplySavedVolume()
    {
        AudioListener.volume = MasterVolume;
    }
}
