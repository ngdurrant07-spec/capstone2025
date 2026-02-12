using UnityEngine;

public class EnemyType4 : MonoBehaviour, IStompable
{
    [Header("Throwable Hit")]
    public float deathPopForce = 6f;
    public float deathFallGravity = 2f;
    public float deathLifetime = 0.2f;
    public bool disableCollidersOnDeath = true;

    Rigidbody2D rb;
    bool isDead;
    SpriteRenderer sprite;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Called by player stomp/roll or by ThrowableItem via IStompable.
    // Player bounce is handled in the player scripts, so we only react to throwable hits.
    public void OnStomp()
    {
        // Intentionally empty so the player bounces off without damaging this enemy.
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleThrowableHit(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleThrowableHit(other);
    }

    void HandleThrowableHit(Collider2D other)
    {
        if (isDead || other == null) return;

        ThrowableItem throwable = other.GetComponentInParent<ThrowableItem>();
        if (throwable == null) return;

        isDead = true;
        SoundEffectManager.Play("Hit_Stomp");

        // Visual feedback: defeated enemy falls upside down.
        if (sprite != null)
            sprite.flipY = true;

        if (disableCollidersOnDeath)
        {
            foreach (Collider2D col in GetComponentsInChildren<Collider2D>(true))
                col.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = deathFallGravity;
            rb.AddForce(Vector2.up * deathPopForce, ForceMode2D.Impulse);
        }

        Destroy(gameObject, deathLifetime);
    }
}
