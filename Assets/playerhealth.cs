using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class playerhealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    public HealthUI healthUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private SpriteRenderer spriteRenderer;
    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHealthpoints(maxHealth);

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
        healthUI.UpdateHealthpoints(currentHealth);
        if(currentHealth <= 0)
        {
            //player dead!
        }

        //Flash Red
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

}
