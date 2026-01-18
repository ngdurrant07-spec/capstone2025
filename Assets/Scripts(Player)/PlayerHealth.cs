using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public HealthUI healthUI;

    private SpriteRenderer spriteRenderer;

    public static event Action OnPlayerDied;

    [Header("Invincibility")]
    public float invincibilityTime = 0.5f;
    private bool isInvincible;

    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
        healthUI.UpdateHearts(currentHealth);

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return; // 🚫 ignore extra hits

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        healthUI.UpdateHearts(currentHealth);
        StartCoroutine(FlashRed());
        StartCoroutine(Invincibility());

        if (currentHealth <= 0)
        {
            OnPlayerDied?.Invoke();
        }
    }

    public bool TryHeal(int amount)
    {
        if (currentHealth >= maxHealth)
            return false;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        healthUI.UpdateHearts(currentHealth);
        return true;
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

    private IEnumerator Invincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }
}

