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
            return;

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

    Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
    PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

    if (playerRb == null || playerHealth == null)
        return;

    // ✅ Check if player is ABOVE enemy
    bool playerAbove = collision.transform.position.y > transform.position.y + 0.3f;

    // ✅ Check if player is FALLING
    bool falling = playerRb.linearVelocity.y < 0f;

    if (playerAbove && falling)
    {
        // This is a stomp → DO NOTHING here
        // Enemy will die via OnStomp()
        return;
    }

    // ❌ Otherwise, player gets hurt
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

    // -------------------------
    // DEATH
    // -------------------------
    //private void Die()
    //{
        //Destroy(gameObject);
    //}
}
