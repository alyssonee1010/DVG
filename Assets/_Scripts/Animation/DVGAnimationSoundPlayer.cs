using UnityEngine;

public class DVGAnimationSoundPlayer : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] attackSounds;
    [SerializeField] AudioClip[] hitSounds;
    [SerializeField] AudioClip[] mineSounds;
    [SerializeField] AudioClip collectSound;
    [SerializeField] AudioClip afterKillSound;
    [SerializeField] Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayAttackSounds()
    {
        AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }

    public void PlayHitSounds()
    {
        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }

    public void PlayAfterKillSound()
    {
        audioSource.pitch = Random.Range(pitchRange.x-0.5f, pitchRange.y+1f);
        audioSource.PlayOneShot(afterKillSound);
    }

    public void PlayMineSounds()
    {
        AudioClip clip = mineSounds[Random.Range(0, mineSounds.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }

    public void PlayCollectSound()
    {
        Play(collectSound);
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
