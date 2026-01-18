using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public int healAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
            return;

        // ✅ Only consume item if healing actually occurs
        if (health.TryHeal(healAmount))
        {
            Destroy(gameObject);
        }
    }
}

