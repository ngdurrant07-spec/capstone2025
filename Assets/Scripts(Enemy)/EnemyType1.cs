using UnityEngine;

public class EnemyType1 : MonoBehaviour, IStompable
{
    [Header("References")]
    public Transform target;                 // Player transform
    private Rigidbody2D rb;

    [Header("Movement")]
    public float chaseSpeed = 2f;
    public float stopDistance = 1f;

    [Header("Combat")]
    public int damage = 1;
    public float attackCooldown = 1f;
    private float attackTimer;

    [Header("Ground Check")]
    public LayerMask groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Make enemy kinematic to avoid pushing player during roll
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (target == null)
            return;

        attackTimer -= Time.deltaTime;
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
    // DAMAGE PLAYER (NORMAL HIT)
    // -------------------------
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (attackTimer > 0f)
            return;

        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (player == null || playerHealth == null)
            return;

        // If player is stomping (above & falling) → ignore damage
        bool playerAbove = collision.transform.position.y > transform.position.y + 0.3f;
        bool falling = player.linearVelocity.y < 0f;

        if (playerAbove && falling)
        {
            // Stomp handled via StompHitbox / OnStomp
            return;
        }

        // Otherwise, damage player
        playerHealth.TakeDamage(damage);
        attackTimer = attackCooldown;
    }

    // -------------------------
    // STOMPED BY PLAYER
    // -------------------------
    public void OnStomp()
    {
        Destroy(gameObject);
    }
}
