using UnityEngine;

public class EnemyType1 : MonoBehaviour, IStompable
{
    [Header("References")]
    public Transform target; 
    private Rigidbody2D rb;

    [Header("Movement")]
    public float chaseSpeed = 2f;
    public float stopDistance = 1f;

    [Header("Combat")]
    public int damage = 1;
    public float attackCooldown = 1f;
    private float attackTimer;

    [Header("Damage Trigger")]
    public Collider2D damageTrigger;

    [Header("Death")]
    public float deathPopForce = 8f;
    public float deathFallGravity = 3f;
    public float deathLifetime = 2f;

    [Header("Gravity")]
    public float gravityScale = 1f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.2f, 0.2f);

    bool isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale; // Apply normal gravity
        rb.freezeRotation = true;

        if (damageTrigger != null)
            damageTrigger.isTrigger = true;
    }

    void Update()
    {
        if (target == null || isDead) return;

        attackTimer -= Time.deltaTime;

        FollowPlayer();
    }

    void FollowPlayer()
    {
        // Horizontal movement only
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= stopDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;

        // Preserve y velocity so gravity works
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (attackTimer > 0f) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;

        damageable.TakeDamage(damage);
        attackTimer = attackCooldown;
    }

    public void OnStomp()
    {
        if (isDead) return;
        isDead = true;

        enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = deathFallGravity;
        rb.AddForce(Vector2.up * deathPopForce, ForceMode2D.Impulse);

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        Destroy(gameObject, deathLifetime);
    }
}
