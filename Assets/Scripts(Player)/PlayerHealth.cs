using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    public HealthUI healthUI;

    private SpriteRenderer spriteRenderer;

    public static event Action OnPlayerDied;

    // ───────── INVINCIBILITY ─────────
    [Header("Invincibility")]
    public float invincibilityTime = 0.5f;

    private bool isInvincible;
    private int invincibilitySources = 0; // ⭐ KEY FIX

    void Start()
    {
        currentHealth = maxHealth;

        if (healthUI != null)
        {
            healthUI.SetMaxHearts(maxHealth);
            healthUI.UpdateHearts(currentHealth);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // ───────── DAMAGE ─────────
    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthUI != null)
            healthUI.UpdateHearts(currentHealth);

        StartCoroutine(FlashRed());

        // Start normal i-frames
        StartCoroutine(DamageInvincibility());

        if (currentHealth <= 0)
        {
            OnPlayerDied?.Invoke();
        }
    }

    // ───────── HEAL ─────────
    public bool TryHeal(int amount)
    {
        if (currentHealth >= maxHealth)
            return false;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (healthUI != null)
            healthUI.UpdateHearts(currentHealth);

        return true;
    }

    // ───────── VISUAL FEEDBACK ─────────
    private IEnumerator FlashRed()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

    // ───────── DAMAGE I-FRAMES ─────────
    private IEnumerator DamageInvincibility()
    {
        AddInvincibilitySource();

        yield return new WaitForSeconds(invincibilityTime);

        RemoveInvincibilitySource();
    }

    // ───────── MANUAL INVINCIBILITY CONTROL ─────────
    public void SetInvincible(bool value)
    {
        if (value)
            AddInvincibilitySource();
        else
            RemoveInvincibilitySource();
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    private void AddInvincibilitySource()
    {
        invincibilitySources++;
        isInvincible = true;
    }

    private void RemoveInvincibilitySource()
    {
        invincibilitySources = Mathf.Max(0, invincibilitySources - 1);
        isInvincible = invincibilitySources > 0;
    }
}

