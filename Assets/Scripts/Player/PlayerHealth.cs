using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [Tooltip("Lives the player starts with. This is not a limit: extra lives can go above it.")]
    [FormerlySerializedAs("maxHealth")] // keeps the value you already set in the Inspector
    [SerializeField] private int startingHealth = 1000;
    [SerializeField] private float invincibilityDuration = 1.5f;

    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => startingHealth; // only here so the HUD code that reads it still compiles; nothing is capped
    public bool IsDead => isDead;

    void Awake()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(int amount)
    {
        if (PlayerUpgrades.IsInvincible) return; // Invincible Suit blocks every kind of damage
        if (isDead || isInvincible) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        OnHealthChanged?.Invoke(currentHealth, startingHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            GetComponent<PlayerAnimationController>()?.PlayHit();
            StartCoroutine(InvincibilityFrames());
        }
    }

    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        GetComponent<PlayerAnimationController>()?.PlayDie();

        OnDeath?.Invoke();

        GetComponent<PlayerGridMovement>().enabled = false;
        GetComponent<BombPlacer>().enabled = false;
    }

    // Used when a stage or a save is loaded
    public void SetHealth(int newHealth)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(newHealth, 0);
        OnHealthChanged?.Invoke(currentHealth, startingHealth);

        if (currentHealth <= 0)
            Die();
    }

    // +1 life (or any amount), no limit
    public void AddLife(int amount = 1)
    {
        if (isDead) return;

        currentHealth += amount;
        OnHealthChanged?.Invoke(currentHealth, startingHealth);
    }

    public void ResetHealth()
    {
        currentHealth = startingHealth;
        isDead = false;
        isInvincible = false;
        StopAllCoroutines();

        OnHealthChanged?.Invoke(currentHealth, startingHealth);
    }
}