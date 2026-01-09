using UnityEngine;
using System.Collections;
using System;
using JetBrains.Annotations;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth; 

    public HealthUI healthUI;

    private SpriteRenderer spriteRenderer;

    public static event Action OnPlayerDied;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       EnemyType1 enemy = collision.GetComponent<EnemyType1>();
       if(enemy)
        {
            TakeDamage(enemy.damage);
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        StartCoroutine(FlashRed());
        
        if(currentHealth <= 0)
        {
            //Player is dead
            OnPlayerDied.Invoke();
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

}
