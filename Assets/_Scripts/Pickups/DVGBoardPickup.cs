using UnityEngine;

public class DVGBoardPickup : MonoBehaviour
{
    public enum PickupResource { Diamonds, HealingPotions }

    [SerializeField] int value = 1;
    [SerializeField] PickupResource resource = PickupResource.Diamonds;
    [SerializeField] DVGAnimationSoundPlayer collectSoundPlayer;

    [Header("Collect Effect")]
    [SerializeField] bool playCollectEffect = true;
    [SerializeField] float collectEffectDuration = 0.18f;
    [SerializeField] float collectEffectScaleMultiplier = 1.55f;
    [SerializeField] Vector3 collectEffectDrift = new Vector3(0f, 0.2f, 0f);
    [SerializeField] Color collectEffectTint = new Color(0.55f, 0.9f, 1f, 1f);

    static int lastCollectedFrame = -1;
    bool collected;

    public int Value => value;
    public PickupResource Resource => resource;

    public void Initialize(int pickupValue)
    {
        value = pickupValue;
    }

    public void Initialize(int pickupValue, PickupResource pickupResource)
    {
        value = pickupValue;
        resource = pickupResource;
    }

    public void Initialize(int pickupValue, DVGAnimationSoundPlayer soundPlayer)
    {
        value = pickupValue;
        collectSoundPlayer = soundPlayer;
    }

    public void Initialize(int pickupValue, PickupResource pickupResource, DVGAnimationSoundPlayer soundPlayer)
    {
        value = pickupValue;
        resource = pickupResource;
        collectSoundPlayer = soundPlayer;
    }

    public bool Collect()
    {
        if (collected || lastCollectedFrame == Time.frameCount)
        {
            return false;
        }

        collected = true;
        lastCollectedFrame = Time.frameCount;
        DVGUsableWallet wallet = DVGUsableWallet.Instance != null ? DVGUsableWallet.Instance : FindAnyObjectByType<DVGUsableWallet>();
        if (wallet != null)
        {
            if (resource == PickupResource.HealingPotions)
            {
                wallet.AddHealingPotions(value);
            }
            else
            {
                wallet.AddDiamonds(value);
            }
        }

        if (collectSoundPlayer != null)
        {
            collectSoundPlayer.PlayCollectSounds();
        }

        PlayCollectEffect();
        Destroy(gameObject);
        return true;
    }

    void PlayCollectEffect()
    {
        if (!playCollectEffect)
        {
            return;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        GameObject effectObject = new GameObject(name + " Collect Effect");
        DVGPickupCollectEffect effect = effectObject.AddComponent<DVGPickupCollectEffect>();
        effect.Initialize(spriteRenderer, collectEffectDuration, collectEffectScaleMultiplier, collectEffectDrift, collectEffectTint);
    }
}
