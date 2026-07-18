using UnityEngine;

public class DVGAnimationSoundPlayer : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioClip hitSound;
    [SerializeField] AudioClip mineSound;
    [SerializeField] AudioClip collectSound;
    [SerializeField] AudioClip afterKillSound;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayAttackSound()
    {
        Play(attackSound);
    }

    public void PlayHitSound()
    {
        Play(hitSound);
    }

    public void PlayAfterKillSound()
    {
        Play(afterKillSound);
    }

    public void PlayMineSound()
    {
        Play(mineSound);
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
