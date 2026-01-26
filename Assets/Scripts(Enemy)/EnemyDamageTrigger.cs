using UnityEngine;

public class EnemyDamageTrigger : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerScript player = other.GetComponentInParent<PlayerScript>();
        if (player != null)
        {
            Debug.Log($"[EnemyDamageTrigger] PlayerState={player.currentState}");
            if (!player.CanTakeDamage())
                return;
        }

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null)
            return;

        health.TakeDamage(damage);
    }
}
