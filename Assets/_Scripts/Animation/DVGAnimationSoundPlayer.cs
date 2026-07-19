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

    public void PlayEffortSounds()
    {
        AudioClip clip = effortSounds[Random.Range(0, effortSounds.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }

    public void PlayHitSounds()
    {
        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }

    public void PlayAfterKillSounds()
    {
        AudioClip clip = afterKillSounds[Random.Range(0, afterKillSounds.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }

    public void PlayMineSounds()
    {
        AudioClip clip = mineSounds[Random.Range(0, mineSounds.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }

    public void PlayCollectSounds()
    {
        AudioClip clip = collectSounds[Random.Range(0, collectSounds.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
