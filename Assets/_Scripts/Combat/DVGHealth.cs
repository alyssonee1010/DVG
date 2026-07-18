using System;
using UnityEngine;

public class DVGHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    [SerializeField] bool destroyWhenDead = true;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    public event Action<DVGHealth> Died;
    public event Action<DVGHealth, int> HealthChanged;

    void Awake()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
    }

    public void SetMaxHealth(int value, bool refill = true)
    {
        maxHealth = Mathf.Max(1, value);
        if (refill)
        {
            CurrentHealth = maxHealth;
            HealthChanged?.Invoke(this, CurrentHealth);
        }
        else
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || !IsAlive)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        HealthChanged?.Invoke(this, CurrentHealth);

        if (CurrentHealth == 0)
        {
            Died?.Invoke(this);
            if (destroyWhenDead)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || !IsAlive)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(this, CurrentHealth);
    }
}
