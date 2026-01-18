using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Spike Settings")]
    public int damage = 1;
    public float bounceForce = 12f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        PlayerScript player = collision.GetComponent<PlayerScript>();
        PlayerHealth health = collision.GetComponent<PlayerHealth>();

        if (rb == null) return;

        // ✅ Cancel ground pound so player regains control
        if (player != null)
            player.CancelGroundPound();

        // ✅ Deal damage (enemy & spikes use same system)
        if (health != null)
            health.TakeDamage(damage);

        // ✅ Reset downward velocity
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // ✅ Bounce player upward
        rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
    }
}


