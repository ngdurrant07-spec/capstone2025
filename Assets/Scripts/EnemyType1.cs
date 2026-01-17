using UnityEngine;

public class EnemyType1 : MonoBehaviour, IStompable
{
    [Header("Target")]
    public Transform Player;

    [Header("Movement")]
    public float chaseSpeed = 2f;
    public float jumpForce = 6f;
    public LayerMask groundLayer;

    [Header("Combat")]
    public int damage = 1;
    public float attackRange = 1f;
    public float attackCooldown = 1f;

    private float lastAttackTime;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Player == null) return;

        // Ground check
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);

        float direction = Mathf.Sign(Player.position.x - transform.position.x);

        if (isGrounded)
        {
            // Chase player (LINEAR VELOCITY)
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

            // Raycasts
            RaycastHit2D groundInFront =
                Physics2D.Raycast(transform.position, Vector2.right * direction, 1.5f, groundLayer);

            RaycastHit2D gapAhead =
                Physics2D.Raycast(transform.position + Vector3.right * direction, Vector2.down, 2f, groundLayer);

            if (!groundInFront && !gapAhead)
                shouldJump = true;
        }

        // Melee attack
        float dist = Vector2.Distance(transform.position, Player.position);
        if (dist <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            Player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    void FixedUpdate()
    {
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // STOMP RESPONSE
    public void OnStomped()
    {
        Destroy(gameObject);
    }
}
