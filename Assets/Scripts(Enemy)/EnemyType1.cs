using UnityEngine;

/// <summary>
/// Enemy that can chase the player, deal damage, and be stomped.
/// </summary>
public class EnemyType1 : MonoBehaviour, IStompable
{
    [Header("References")]
    public Transform target; // Player transform
    private Rigidbody2D rb;

    [Header("Movement")]
    public float chaseSpeed = 2f;
    public float stopDistance = 1f;

    [Header("Combat")]
    public int damage = 1;
    public float attackCooldown = 1f;
    private float attackTimer;

    [Header("Damage Trigger")]
    public Collider2D damageTrigger; // Trigger collider for dealing damage

    [Header("Death")]
    public float deathPopForce = 8f;
    public float deathFallGravity = 3f;
    public float deathLifetime = 2f;

    bool isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Rigidbody setup
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Ensure trigger is enabled
        if (damageTrigger != null)
            damageTrigger.isTrigger = true;
    }

    void Update()
    {
        if (target == null) return;

        // Reduce attack cooldown
        attackTimer -= Time.deltaTime;

        // Move toward player
        FollowPlayer();
    }

    // -------------------------
    // MOVEMENT
    // -------------------------
    void FollowPlayer()
    {
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= stopDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
    }

    // -------------------------
    // DAMAGE PLAYER (TRIGGER)
    // -------------------------
    private void OnTriggerStay2D(Collider2D other)
{
    if (attackTimer > 0f) return;

    PlayerScript player = other.GetComponent<PlayerScript>();
    if (player == null) return;

    // ❗ BLOCK DAMAGE WHILE ROLLING
    if (!player.CanTakeDamage())
        return;

    IDamageable damageable = other.GetComponent<IDamageable>();
    if (damageable == null) return;

    damageable.TakeDamage(damage);
    attackTimer = attackCooldown;
}

   // -------------------------
    // STOMPED BY PLAYER
    // -------------------------
    public void OnStomp()
{
    if (isDead) return;
    isDead = true;
    SoundEffectManager.Play("Hit_Stomp");

    // Disable AI logic
    enabled = false;

    // Stop movement
    rb.linearVelocity = Vector2.zero;

    // Physics fall
    rb.bodyType = RigidbodyType2D.Dynamic;
    rb.gravityScale = deathFallGravity;

    // Pop upward
    rb.AddForce(Vector2.up * deathPopForce, ForceMode2D.Impulse);

    // Disable colliders so it can't hurt or block player
    foreach (Collider2D col in GetComponents<Collider2D>())
        col.enabled = false;

    Destroy(gameObject, deathLifetime);
}

    // -------------------------
    // OPTIONAL DEBUG
    // -------------------------
    private void OnDrawGizmosSelected()
    {
        if (damageTrigger != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(damageTrigger.bounds.center, damageTrigger.bounds.size);
        }
    }
    
}

