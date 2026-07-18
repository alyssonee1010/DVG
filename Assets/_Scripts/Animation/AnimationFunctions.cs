using UnityEngine;

public class AnimationFunctions : MonoBehaviour
{
    [SerializeField] AudioClip[] randomSounds;
    [SerializeField] ParticleSystem particleSystem;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(AudioClip audioClip)
    {
        PlayerController.Instance.audioSource.PlayOneShot(audioClip);
    }

    public void PlaySounds(float volume)
    {
        PlayerController.Instance.audioSource.PlayOneShot(randomSounds[Random.Range(0, randomSounds.Length)], volume);
    }

    public void EmitParticles(int particleAmount)
    {
        particleSystem.Emit(particleAmount);
    }
}
