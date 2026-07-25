using UnityEngine;

public class DVGAnimationSoundPlayer : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] attackSounds;
    [SerializeField] AudioClip[] effortSounds;
    [SerializeField] AudioClip[] hitSounds;
    [SerializeField] AudioClip[] mineSounds;
    [SerializeField] AudioClip[] collectSounds;
    [SerializeField] AudioClip[] afterKillSounds;
    [SerializeField] Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    void Awake()
    {
        EnsureAudioSource();
    }

    public void PlayAttackSounds()
    {
        PlayRandom(attackSounds);
    }

    public void PlayEffortSounds()
    {
        PlayRandom(effortSounds);
    }

    public void PlayHitSounds()
    {
        PlayRandom(hitSounds);
    }

    public void PlayAfterKillSounds()
    {
        PlayRandom(afterKillSounds);
    }

    public void PlayMineSounds()
    {
        PlayRandom(mineSounds);
    }

    public void PlayCollectSounds()
    {
        PlayRandom(collectSounds);
    }

    bool PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return false;
        }

        EnsureAudioSource();
        if (audioSource == null)
        {
            return false;
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null)
        {
            return false;
        }

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
        return true;
    }

    void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
}
