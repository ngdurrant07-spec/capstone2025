using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossRivalRacer : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointReachDistance = 0.35f;

    [Header("Movement")]
    [SerializeField] private float runSpeed = 7.5f;
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private float jumpHeightThreshold = 0.8f;
    [SerializeField] private float horizontalAcceleration = 30f;

    [Header("Glide")]
    [SerializeField] private float glideSpeed = 9.5f;
    [SerializeField] private float glideGravityScale = 0.45f;
    [SerializeField] private float glideStartFallSpeed = -1f;
    [SerializeField] private float maxGlideDropSpeed = 3.5f;
    [SerializeField] private float minGlideDistance = 2.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.2f);
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Glide Attack")]
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float hitCooldown = 0.75f;
    [SerializeField] private float downwardSlamSpeed = 16f;
    [SerializeField] private float horizontalSlamSpeed = 5f;

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float defaultGravityScale;
    private float lastHitTime = float.NegativeInfinity;
    private int waypointIndex;
    private bool raceActive;
    private bool isGliding;
    private SpriteRenderer spriteRenderer;

    public bool IsRacing => raceActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        defaultGravityScale = rb.gravityScale;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (!raceActive || waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[Mathf.Clamp(waypointIndex, 0, waypoints.Length - 1)];
        Vector2 current = rb.position;
        Vector2 targetPos = target.position;
        Vector2 delta = targetPos - current;

        bool grounded = IsGrounded();
        UpdateGlideState(delta, grounded);
        MoveTowardsTarget(delta);

        if (grounded && delta.y > jumpHeightThreshold)
            Jump();

        if (delta.magnitude <= waypointReachDistance && waypointIndex < waypoints.Length - 1)
            waypointIndex++;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitPlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryHitPlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryHitPlayer(other);
    }

    public void BeginRace()
    {
        waypointIndex = 0;
        raceActive = true;
        isGliding = false;
        rb.gravityScale = defaultGravityScale;
    }

    public void StopRace()
    {
        raceActive = false;
        isGliding = false;
        rb.gravityScale = defaultGravityScale;
        rb.linearVelocity = Vector2.zero;
    }

    public void ResetToStart()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = defaultGravityScale;
        waypointIndex = 0;
        raceActive = false;
        isGliding = false;
    }

    public void SetRaceManager(BossRaceManager manager, Transform explicitStartPoint)
    {
        if (explicitStartPoint != null)
        {
            startPosition = explicitStartPoint.position;
            startRotation = explicitStartPoint.rotation;
        }
    }

    private void MoveTowardsTarget(Vector2 delta)
    {
        float targetSpeed = Mathf.Sign(delta.x) * runSpeed;
        if (isGliding)
            targetSpeed = Mathf.Sign(delta.x) * glideSpeed;

        if (Mathf.Abs(delta.x) < 0.05f)
            targetSpeed = 0f;

        float nextX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, horizontalAcceleration * Time.fixedDeltaTime);
        float nextY = rb.linearVelocity.y;

        if (isGliding)
            nextY = Mathf.Max(nextY, -maxGlideDropSpeed);

        rb.linearVelocity = new Vector2(nextX, nextY);
        UpdateFacing(nextX);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void UpdateGlideState(Vector2 delta, bool grounded)
    {
        if (grounded)
        {
            if (isGliding)
            {
                isGliding = false;
                rb.gravityScale = defaultGravityScale;
            }
            return;
        }

        bool shouldGlide = Mathf.Abs(delta.x) >= minGlideDistance && rb.linearVelocity.y <= glideStartFallSpeed;
        if (!shouldGlide)
        {
            if (isGliding)
            {
                isGliding = false;
                rb.gravityScale = defaultGravityScale;
            }
            return;
        }

        isGliding = true;
        rb.gravityScale = glideGravityScale;
    }

    private bool IsGrounded()
    {
        Vector2 center = groundCheck != null ? (Vector2)groundCheck.position : rb.position + Vector2.down * 0.6f;
        return Physics2D.OverlapBox(center, groundCheckSize, 0f, groundMask) != null;
    }

    private void TryHitPlayer(Collider2D other)
    {
        if (!raceActive || !isGliding || other == null || Time.time < lastHitTime + hitCooldown)
            return;
        if (!other.CompareTag("Player"))
            return;

        PlayerScript player = other.GetComponentInParent<PlayerScript>();
        if (player == null || !player.CanTakeDamage())
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null)
            return;

        health.TakeDamage(contactDamage);
        player.CancelGroundPound();

        Vector2 slamDirection = other.transform.position.x >= transform.position.x ? Vector2.right : Vector2.left;
        player.linearVelocity = new Vector2(slamDirection.x * horizontalSlamSpeed, -Mathf.Abs(downwardSlamSpeed));
        lastHitTime = Time.time;
    }

    private void UpdateFacing(float horizontalSpeed)
    {
        if (spriteRenderer == null || Mathf.Abs(horizontalSpeed) <= 0.01f)
            return;

        spriteRenderer.flipX = horizontalSpeed < 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 center = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 0.6f;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, groundCheckSize);

        if (waypoints == null || waypoints.Length == 0)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            Gizmos.DrawSphere(waypoints[i].position, 0.12f);
            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
