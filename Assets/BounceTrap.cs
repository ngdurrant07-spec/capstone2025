using UnityEngine;

public class BounceTrap : MonoBehaviour
{
    public float bounceForce = 12f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        PlayerScript player = collision.GetComponent<PlayerScript>();

        if (rb == null) return;

        // ✅ CANCEL ground pound
        if (player != null)
            player.CancelGroundPound();

        // Reset downward velocity
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Apply bounce
        rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
    }
}

