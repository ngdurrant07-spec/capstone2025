using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossRivalRacer : MonoBehaviour
{
    public enum BossBehaviorMode
    {
        Race,
        MetalSonic
    }

    private enum MetalAttackState
    {
        Hover,
        Telegraph,
        Dash,
        Recover
    }

    [Header("Mode")]
    [SerializeField] private BossBehaviorMode behaviorMode = BossBehaviorMode.MetalSonic;
    [SerializeField] private PlayerScript playerTarget;
    [SerializeField] private bool autoStartOnPlay = true;

    [Header("Race Path")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointReachDistance = 0.35f;

    [Header("Race Movement")]
    [SerializeField] private float runSpeed = 7.5f;
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private float jumpHeightThreshold = 0.8f;
    [SerializeField] private float horizontalAcceleration = 30f;

    [Header("Race Glide")]
    [SerializeField] private float glideSpeed = 9.5f;
    [SerializeField] private float glideGravityScale = 0.45f;
    [SerializeField] private float glideStartFallSpeed = -1f;
    [SerializeField] private float maxGlideDropSpeed = 3.5f;
    [SerializeField] private float minGlideDistance = 2.5f;

    [Header("Metal Sonic Flight")]
    [SerializeField] private float flySpeed = 9f;
    [SerializeField] private float hoverLerpSpeed = 10f;
    [SerializeField] private Vector2 hoverOffset = new Vector2(4f, 2.5f);
    [SerializeField] private float hoverBobAmplitude = 0.5f;
    [SerializeField] private float hoverBobFrequency = 3f;
    [SerializeField] private float attackInterval = 2.5f;
    [SerializeField] private float telegraphDuration = 0.45f;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.45f;
    [SerializeField] private float recoveryHeight = 3f;
    [SerializeField] private float recoveryDuration = 0.45f;
    [SerializeField] private float raceCruiseHeight = 2.25f;
    [SerializeField] private float attackPlayerAheadDistance = 1.5f;
    [SerializeField] private float attackDetectionRange = 6f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.2f);
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Contact Attack")]
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float hitCooldown = 0.75f;
    [SerializeField] private float downwardSlamSpeed = 16f;
    [SerializeField] private float horizontalSlamSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float defaultGravityScale;
    private float lastHitTime = float.NegativeInfinity;
    private float stateTimer;
    private float hoverSide = 1f;
    private Vector2 dashDirection;
    private int waypointIndex;
    private bool raceActive;
    private bool isGliding;
    private MetalAttackState metalAttackState;

    public bool IsRacing => raceActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        defaultGravityScale = rb.gravityScale;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Start()
    {
        if (!autoStartOnPlay)
            return;

        BeginRace();
    }

    private void FixedUpdate()
    {
        if (!raceActive)
            return;

        if (behaviorMode == BossBehaviorMode.MetalSonic)
        {
            RunMetalSonicBehavior();
            return;
        }

        RunRaceBehavior();
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
        stateTimer = 0f;
        hoverSide = 1f;
        dashDirection = Vector2.zero;
        metalAttackState = MetalAttackState.Hover;
        raceActive = true;
        isGliding = false;
        rb.gravityScale = behaviorMode == BossBehaviorMode.MetalSonic ? 0f : defaultGravityScale;
        rb.linearVelocity = Vector2.zero;

        if (playerTarget == null)
            playerTarget = FindFirstObjectByType<PlayerScript>();
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
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = defaultGravityScale;
        waypointIndex = 0;
        stateTimer = 0f;
        hoverSide = 1f;
        dashDirection = Vector2.zero;
        metalAttackState = MetalAttackState.Hover;
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

    private void RunRaceBehavior()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[Mathf.Clamp(waypointIndex, 0, waypoints.Length - 1)];
        Vector2 current = rb.position;
        Vector2 targetPos = target.position;
        Vector2 delta = targetPos - current;

        bool grounded = IsGrounded();
        UpdateRaceGlideState(delta, grounded);
        MoveTowardsTarget(delta);

        if (grounded && delta.y > jumpHeightThreshold)
            Jump();

        if (delta.magnitude <= waypointReachDistance && waypointIndex < waypoints.Length - 1)
            waypointIndex++;
    }

    private void RunMetalSonicBehavior()
    {
        if (playerTarget == null)
            playerTarget = FindFirstObjectByType<PlayerScript>();
        if (playerTarget == null)
            return;

        AdvanceWaypointIfReached();
        rb.gravityScale = 0f;
        stateTimer += Time.fixedDeltaTime;

        switch (metalAttackState)
        {
            case MetalAttackState.Hover:
                FollowRaceLine();
                if (stateTimer >= attackInterval && ShouldAttackPlayer())
                    EnterTelegraph();
                break;
            case MetalAttackState.Telegraph:
                HoldAttackLine();
                if (stateTimer >= telegraphDuration)
                    EnterDash();
                break;
            case MetalAttackState.Dash:
                rb.linearVelocity = dashDirection * dashSpeed;
                UpdateFacing(rb.linearVelocity.x);
                if (stateTimer >= dashDuration)
                    EnterRecover();
                break;
            case MetalAttackState.Recover:
                RecoverAbovePlayer();
                if (stateTimer >= recoveryDuration)
                    EnterHover();
                break;
        }
    }

    private void FollowRaceLine()
    {
        Vector2 playerPosition = playerTarget.transform.position;
        Vector2 forwardTarget = GetRaceAnchor();
        float bob = Mathf.Sin(Time.time * hoverBobFrequency) * hoverBobAmplitude;
        float desiredX = Mathf.Max(forwardTarget.x, playerPosition.x - hoverOffset.x * 0.25f);
        Vector2 hoverTarget = new Vector2(desiredX, forwardTarget.y + bob);
        Vector2 next = Vector2.Lerp(rb.position, hoverTarget, hoverLerpSpeed * Time.fixedDeltaTime);
        rb.linearVelocity = (next - rb.position) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
        rb.MovePosition(next);
        UpdateFacing(forwardTarget.x - rb.position.x);
    }

    private void HoldAttackLine()
    {
        Vector2 playerPosition = playerTarget.transform.position;
        Vector2 telegraphTarget = new Vector2(playerPosition.x - hoverOffset.x * 0.6f, playerPosition.y + hoverOffset.y);
        Vector2 next = Vector2.Lerp(rb.position, telegraphTarget, hoverLerpSpeed * Time.fixedDeltaTime);
        rb.linearVelocity = (next - rb.position) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
        rb.MovePosition(next);
        UpdateFacing(playerPosition.x - rb.position.x);
    }

    private void RecoverAbovePlayer()
    {
        Vector2 raceAnchor = GetRaceAnchor();
        Vector2 recoveryTarget = new Vector2(raceAnchor.x, raceAnchor.y + recoveryHeight);
        Vector2 next = Vector2.MoveTowards(rb.position, recoveryTarget, flySpeed * Time.fixedDeltaTime);
        rb.linearVelocity = (next - rb.position) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
        rb.MovePosition(next);
        UpdateFacing(recoveryTarget.x - rb.position.x);
    }

    private void EnterHover()
    {
        stateTimer = 0f;
        metalAttackState = MetalAttackState.Hover;
        hoverSide *= -1f;
        rb.linearVelocity = Vector2.zero;
    }

    private void EnterTelegraph()
    {
        stateTimer = 0f;
        metalAttackState = MetalAttackState.Telegraph;
        rb.linearVelocity = Vector2.zero;
    }

    private void EnterDash()
    {
        stateTimer = 0f;
        metalAttackState = MetalAttackState.Dash;

        Vector2 targetPoint = playerTarget.transform.position;
        dashDirection = (targetPoint - rb.position).normalized;
        if (dashDirection.sqrMagnitude <= 0.001f)
            dashDirection = hoverSide > 0f ? Vector2.left : Vector2.right;
    }

    private void EnterRecover()
    {
        stateTimer = 0f;
        metalAttackState = MetalAttackState.Recover;
        rb.linearVelocity = Vector2.zero;
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

    private void UpdateRaceGlideState(Vector2 delta, bool grounded)
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
        if (!raceActive || other == null || Time.time < lastHitTime + hitCooldown)
            return;
        if (!other.CompareTag("Player"))
            return;
        if (!CanDamagePlayerOnContact())
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

    private bool CanDamagePlayerOnContact()
    {
        if (behaviorMode == BossBehaviorMode.MetalSonic)
            return metalAttackState == MetalAttackState.Dash;

        return isGliding;
    }

    private bool ShouldAttackPlayer()
    {
        Vector2 playerPosition = playerTarget.transform.position;
        Vector2 currentPosition = rb.position;
        float horizontalLead = playerPosition.x - currentPosition.x;

        if (horizontalLead < attackPlayerAheadDistance)
            return false;

        return horizontalLead <= attackDetectionRange;
    }

    private Vector2 GetRaceAnchor()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            Transform target = waypoints[Mathf.Clamp(waypointIndex, 0, waypoints.Length - 1)];
            if (target != null)
                return new Vector2(target.position.x, target.position.y + raceCruiseHeight);
        }

        return rb.position + new Vector2(flySpeed * 0.5f, raceCruiseHeight);
    }

    private void AdvanceWaypointIfReached()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[Mathf.Clamp(waypointIndex, 0, waypoints.Length - 1)];
        if (target == null)
            return;

        Vector2 anchor = GetRaceAnchor();
        if (Vector2.Distance(rb.position, anchor) <= Mathf.Max(waypointReachDistance, 1f) && waypointIndex < waypoints.Length - 1)
            waypointIndex++;
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

        if (behaviorMode != BossBehaviorMode.Race || waypoints == null || waypoints.Length == 0)
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
