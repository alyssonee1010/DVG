using UnityEngine;

[RequireComponent(typeof(DVGHealth))]
public class DVGHitRecoil : MonoBehaviour
{
    [SerializeField] Transform recoilTarget;
    [SerializeField] bool applyInWorldSpace = true;
    [SerializeField] bool returnAfterRecoil = true;
    [SerializeField] Vector2 recoilDirection = Vector2.right;
    [SerializeField] float recoilDistance = 0.12f;
    [SerializeField] float recoilOutSeconds = 0.05f;
    [SerializeField] float recoilReturnSeconds = 0.12f;

    [Header("Stun Animation")]
    [SerializeField] bool triggerStunAnimation = true;
    [SerializeField] Animator animator;
    [SerializeField] string stunTriggerName = "Stun";

    [Header("Stun Gameplay")]
    [Range(0f, 1f)]
    [SerializeField] float stunHitChance;
    [Min(0f)]
    [SerializeField] float stunResistancePercent;

    [Header("Audio")]
    [SerializeField] DVGAnimationSoundPlayer soundPlayer;

    DVGHealth health;
    int lastHealth;
    float recoilTimer;
    Vector3 appliedOffset;
    float currentRecoilMultiplier = 1f;

    float TotalSeconds => Mathf.Max(0.001f, recoilOutSeconds + recoilReturnSeconds);

    public float StunHitChance => Mathf.Clamp01(stunHitChance);
    public float StunResistancePercent => Mathf.Max(0f, stunResistancePercent);

    void Awake()
    {
        health = GetComponent<DVGHealth>();
        if (recoilTarget == null)
        {
            recoilTarget = transform;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (soundPlayer == null)
        {
            soundPlayer = GetComponent<DVGAnimationSoundPlayer>();
        }
    }

    void OnEnable()
    {
        if (health == null)
        {
            health = GetComponent<DVGHealth>();
        }

        lastHealth = health != null ? health.CurrentHealth : 0;
        if (health != null)
        {
            health.HealthChanged += OnHealthChanged;
        }
    }

    void OnDisable()
    {
        if (returnAfterRecoil)
        {
            RemoveAppliedOffset();
        }

        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
        }
    }

    void LateUpdate()
    {
        if (recoilTarget == null)
        {
            return;
        }

        if (!returnAfterRecoil)
        {
            ApplyPermanentRecoil();
            return;
        }

        RemoveAppliedOffset();
        if (recoilTimer <= 0f)
        {
            return;
        }

        recoilTimer = Mathf.Max(0f, recoilTimer - Time.deltaTime);
        appliedOffset = GetCurrentOffset();
        ApplyOffset(appliedOffset);
    }

    void ApplyPermanentRecoil()
    {
        if (recoilTimer <= 0f)
        {
            appliedOffset = Vector3.zero;
            return;
        }

        recoilTimer = Mathf.Max(0f, recoilTimer - Time.deltaTime);
        Vector3 currentOffset = GetCurrentPushOffset();
        Vector3 deltaOffset = currentOffset - appliedOffset;
        appliedOffset = currentOffset;
        ApplyOffset(deltaOffset);
    }

    void OnHealthChanged(DVGHealth changedHealth, int currentHealth)
    {
        if (currentHealth < lastHealth)
        {
            if (!returnAfterRecoil)
            {
                appliedOffset = Vector3.zero;
            }

            PlayHitSound();
            currentRecoilMultiplier = changedHealth != null ? changedHealth.LastDamageRecoilMultiplier : 1f;
            recoilTimer = TotalSeconds;
        }

        lastHealth = currentHealth;
    }

    bool PlayStunAnimation(int currentHealth)
    {
        if (!triggerStunAnimation
            || currentHealth <= 0
            || animator == null
            || string.IsNullOrWhiteSpace(stunTriggerName)
            || !HasAnimatorTrigger(stunTriggerName))
        {
            return false;
        }

        animator.ResetTrigger(stunTriggerName);
        animator.SetTrigger(stunTriggerName);
        return true;
    }

    bool HasAnimatorTrigger(string triggerName)
    {
        AnimatorControllerParameter[] parameters = animator.parameters;
        foreach (AnimatorControllerParameter parameter in parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryStunTarget(GameObject target)
    {
        if (target == null || StunHitChance <= 0f)
        {
            return false;
        }

        DVGHitRecoil targetStun = target.GetComponent<DVGHitRecoil>();
        if (targetStun == null)
        {
            targetStun = target.GetComponentInParent<DVGHitRecoil>();
        }

        return targetStun != null && targetStun.TryReceiveStun(StunHitChance);
    }

    public bool TryReceiveStun(float incomingChance)
    {
        if (health == null)
        {
            health = GetComponent<DVGHealth>();
        }

        if (health != null && !health.IsAlive)
        {
            return false;
        }

        float resistanceMultiplier = Mathf.Clamp01(1f - StunResistancePercent / 200f);
        float effectiveChance = Mathf.Clamp01(incomingChance) * resistanceMultiplier;
        if (effectiveChance <= 0f || Random.value > effectiveChance)
        {
            return false;
        }

        return PlayStunAnimation(health != null ? health.CurrentHealth : 1);
    }

    void PlayHitSound()
    {
        if (soundPlayer == null)
        {
            soundPlayer = GetComponent<DVGAnimationSoundPlayer>();
        }

        if (soundPlayer != null)
        {
            soundPlayer.PlayHitSounds();
        }
    }

    Vector3 GetCurrentPushOffset()
    {
        Vector2 direction = recoilDirection.sqrMagnitude > 0f ? recoilDirection.normalized : Vector2.right;
        float elapsed = TotalSeconds - recoilTimer;
        float t = recoilOutSeconds <= 0f ? 1f : elapsed / recoilOutSeconds;
        float amount = Mathf.Lerp(0f, recoilDistance * currentRecoilMultiplier, Smooth(t));
        return new Vector3(direction.x, direction.y, 0f) * amount;
    }

    Vector3 GetCurrentOffset()
    {
        Vector2 direction = recoilDirection.sqrMagnitude > 0f ? recoilDirection.normalized : Vector2.right;
        float elapsed = TotalSeconds - recoilTimer;
        float amount;

        if (elapsed <= recoilOutSeconds)
        {
            float t = recoilOutSeconds <= 0f ? 1f : elapsed / recoilOutSeconds;
            amount = Mathf.Lerp(0f, recoilDistance * currentRecoilMultiplier, Smooth(t));
        }
        else
        {
            float t = recoilReturnSeconds <= 0f ? 1f : (elapsed - recoilOutSeconds) / recoilReturnSeconds;
            amount = Mathf.Lerp(recoilDistance * currentRecoilMultiplier, 0f, Smooth(t));
        }

        return new Vector3(direction.x, direction.y, 0f) * amount;
    }

    float Smooth(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    void ApplyOffset(Vector3 offset)
    {
        if (applyInWorldSpace)
        {
            recoilTarget.position += offset;
            return;
        }

        recoilTarget.localPosition += offset;
    }

    void RemoveAppliedOffset()
    {
        if (recoilTarget == null || appliedOffset == Vector3.zero)
        {
            appliedOffset = Vector3.zero;
            return;
        }

        if (applyInWorldSpace)
        {
            recoilTarget.position -= appliedOffset;
        }
        else
        {
            recoilTarget.localPosition -= appliedOffset;
        }

        appliedOffset = Vector3.zero;
    }
}
