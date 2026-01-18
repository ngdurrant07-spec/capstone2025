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
    if (playerRb == null)
        return;

    // 🚫 If player is falling downward, do NOT damage (stomp case)
    if (playerRb.linearVelocity.y <= 0f)
        return;

    PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
    if (playerHealth != null)
    {
        playerHealth.TakeDamage(damage);
        attackTimer = attackCooldown;
    }
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
