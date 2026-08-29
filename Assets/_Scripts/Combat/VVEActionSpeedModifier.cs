using UnityEngine;

// Applies independently expiring action-speed stacks to any defender. The shared
// multiplier drives animation-event actions (such as mining), while timer-driven
// actions query GetMultiplier so their gameplay timing stays in sync with the visuals.
[RequireComponent(typeof(VVEStackingTimedEffect))]
public class VVEActionSpeedModifier : MonoBehaviour
{
    VVEStackingTimedEffect stackingEffect;
    Animator[] animators;
    float[] baseAnimatorSpeeds;

    VVEStackingTimedEffect StackingEffect
    {
        get
        {
            if (stackingEffect == null)
            {
                stackingEffect = GetComponent<VVEStackingTimedEffect>();
                if (stackingEffect == null)
                {
                    stackingEffect = gameObject.AddComponent<VVEStackingTimedEffect>();
                }
            }

            return stackingEffect;
        }
    }

    public float Multiplier => Mathf.Max(1f, 1f + StackingEffect.TotalValue);

    void Awake()
    {
        CacheAnimators();
    }

    void Update()
    {
        ApplyAnimatorSpeed();
    }

    void OnDisable()
    {
        RestoreAnimatorSpeed();
    }

    public void AddPercent(float percent, float? durationSeconds = null)
    {
        if (percent <= 0f)
        {
            return;
        }

        StackingEffect.AddStack(percent, durationSeconds);
        ApplyAnimatorSpeed();
    }

    public static float GetMultiplier(Component action)
    {
        if (action == null)
        {
            return 1f;
        }

        VVEActionSpeedModifier modifier = action.GetComponent<VVEActionSpeedModifier>();
        return modifier != null ? modifier.Multiplier : 1f;
    }

    public static bool CanAffect(VVEDefender defender)
    {
        return defender != null
            && defender.isActiveAndEnabled
            && defender.Health != null
            && defender.Health.IsAlive
            && (defender.GetComponentInChildren<Animator>(true) != null
                || defender.GetComponent<VVEBoardMeleeAttacker>() != null
                || defender.GetComponent<VVERowProjectileShooter>() != null
                || defender.GetComponent<VVEMinerMiningReward>() != null
                || defender.GetComponent<VVEWizardPotionReward>() != null);
    }

    void CacheAnimators()
    {
        animators = GetComponentsInChildren<Animator>(true);
        baseAnimatorSpeeds = new float[animators.Length];
        for (int i = 0; i < animators.Length; i++)
        {
            baseAnimatorSpeeds[i] = animators[i] != null ? animators[i].speed : 1f;
        }
    }

    void ApplyAnimatorSpeed()
    {
        if (animators == null)
        {
            CacheAnimators();
        }

        float multiplier = Multiplier;
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].speed = baseAnimatorSpeeds[i] * multiplier;
            }
        }
    }

    void RestoreAnimatorSpeed()
    {
        if (animators == null || baseAnimatorSpeeds == null)
        {
            return;
        }

        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].speed = baseAnimatorSpeeds[i];
            }
        }
    }
}
