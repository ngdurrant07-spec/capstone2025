using UnityEngine;

public class EnemyType4 : MonoBehaviour, IStompable
{
    [Header("Aggro")]
    public Color chaseColor = Color.red;

    [Header("Throwable Hit")]
    public float deathPopForce = 6f;
    public float deathFallGravity = 2f;
    public float deathLifetime = 0.2f;
    public bool disableCollidersOnDeath = true;

    Rigidbody2D rb;
    bool isDead;
    bool isAggro;
    SpriteRenderer sprite;
    EnemyType4Patrol patrol;
    Color normalColor;
    Transform cachedPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        patrol = GetComponent<EnemyType4Patrol>();
        if (sprite != null)
            normalColor = sprite.color;
    }

    // Called by player stomp/roll and throwable hits via IStompable.
    public void OnStomp()
    {
        if (isDead || isAggro)
            return;

        isAggro = true;

        if (sprite != null)
            sprite.color = chaseColor;

        if (patrol != null)
            patrol.ActivateChase(FindPlayerTransform());
    }

    Transform FindPlayerTransform()
    {
        if (cachedPlayer != null)
            return cachedPlayer;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            cachedPlayer = playerObject.transform;

        return cachedPlayer;
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
        {
            sprite.flipY = true;
            sprite.color = normalColor;
        }

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
