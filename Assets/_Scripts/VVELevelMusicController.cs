using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VVELevelMusicController : MonoBehaviour
{
    [SerializeField] AudioClip earlyLevelsMusic;
    [SerializeField] AudioClip laterLevelsMusic;

    AudioSource audioSource;
    VVEWaveDirector waveDirector;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;

        VVEAudioSettings.ApplySavedVolume();
        TrySubscribe();
    }

    void Start()
    {
        TrySubscribe();

        if (waveDirector != null && waveDirector.CurrentLevel != null)
        {
            PlayMusicFor(waveDirector.CurrentLevel);
        }
    }

    void OnDisable()
    {
        if (waveDirector != null)
        {
            waveDirector.LevelStarted -= PlayMusicFor;
        }
    }

    void TrySubscribe()
    {
        if (waveDirector != null)
        {
            return;
        }

        waveDirector = FindAnyObjectByType<VVEWaveDirector>();
        if (waveDirector != null)
        {
            waveDirector.LevelStarted += PlayMusicFor;
        }
    }

    void PlayMusicFor(VVELevelDefinition level)
    {
        if (level == null)
        {
            return;
        }

        AudioClip selectedMusic = IsLastLevelInStage(level)
            ? laterLevelsMusic
            : earlyLevelsMusic;

        if (selectedMusic == null)
        {
            Debug.LogWarning($"No music is assigned for level {level.Id}.", this);
            return;
        }

        if (audioSource.clip != selectedMusic)
        {
            audioSource.Stop();
            audioSource.clip = selectedMusic;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    static bool IsLastLevelInStage(VVELevelDefinition level)
    {
        foreach (VVELevelDefinition candidate in VVELevelLoader.DiscoverLevels())
        {
            if (candidate.Stage == level.Stage && candidate.Level > level.Level)
            {
                return false;
            }
        }

        return true;
    }
}
