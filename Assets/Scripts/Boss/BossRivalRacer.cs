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
    [SerializeField] private Camera targetCamera;

    [Header("Race Path")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointReachDistance = 0.35f;

    [Header("Race Movement")]
    [SerializeField] private float runSpeed = 7.5f;
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private float jumpHeightThreshold = 0.8f;
    [SerializeField] private float maxJumpTriggerFallSpeed = 0.1f;
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
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color telegraphColor = Color.red;
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

    [Header("Offscreen Indicator")]
    [SerializeField] private bool showOffscreenIndicator = true;
    [SerializeField] private Sprite offscreenIndicatorSprite;
    [SerializeField] private Color offscreenIndicatorColor = Color.white;
    [SerializeField] private Vector2 offscreenViewportPadding = new Vector2(0.08f, 0.12f);
    [SerializeField] private Vector3 offscreenIndicatorScale = new Vector3(0.6f, 0.6f, 1f);
    [SerializeField] private bool showOffscreenDistance = true;

    [Header("Defeat Pose")]
    [SerializeField] private Sprite defeatedSprite;
    [SerializeField] private Vector3 defeatedSpriteEulerAngles = new Vector3(0f, 0f, -25f);
    [SerializeField] private Color defeatedColor = new Color(1f, 1f, 1f, 0.8f);

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer offscreenIndicatorRenderer;
    private TextMesh offscreenIndicatorText;
    private Transform visualTransform;
    private Sprite defaultSprite;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Quaternion visualStartLocalRotation;
    private float defaultGravityScale;
    private RigidbodyConstraints2D defaultConstraints;
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
        defaultConstraints = rb.constraints;
        startPosition = transform.position;
        startRotation = transform.rotation;
        visualTransform = spriteRenderer != null ? spriteRenderer.transform : transform;
        visualStartLocalRotation = visualTransform.localRotation;
        defaultSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        EnsureOffscreenIndicator();
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

    private void LateUpdate()
    {
        UpdateOffscreenIndicator();
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
        rb.constraints = defaultConstraints;
        rb.gravityScale = behaviorMode == BossBehaviorMode.MetalSonic ? 0f : defaultGravityScale;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        RestoreVisualState();
        ApplyBossColor(normalColor);

        if (playerTarget == null)
            playerTarget = FindFirstObjectByType<PlayerScript>();
    }

    public void StopRace()
    {
        raceActive = false;
        isGliding = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.gravityScale = defaultGravityScale;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        ApplyBossColor(normalColor);
        SetOffscreenIndicatorVisible(false);
    }

    public void ResetToStart()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = defaultConstraints;
        rb.gravityScale = defaultGravityScale;
        waypointIndex = 0;
        stateTimer = 0f;
        hoverSide = 1f;
        dashDirection = Vector2.zero;
        metalAttackState = MetalAttackState.Hover;
        raceActive = false;
        isGliding = false;
        RestoreVisualState();
        ApplyBossColor(normalColor);
        SetOffscreenIndicatorVisible(false);
    }

    public void EnterDefeatedPose()
    {
        raceActive = false;
        isGliding = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.gravityScale = defaultGravityScale;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        metalAttackState = MetalAttackState.Hover;
        stateTimer = 0f;
        SetOffscreenIndicatorVisible(false);

        if (visualTransform != null)
            visualTransform.localRotation = Quaternion.Euler(defeatedSpriteEulerAngles);

        if (spriteRenderer != null && defeatedSprite != null)
            spriteRenderer.sprite = defeatedSprite;

        ApplyBossColor(defeatedColor);
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

        if (grounded && rb.linearVelocity.y <= maxJumpTriggerFallSpeed && delta.y > jumpHeightThreshold)
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
        ApplyBossColor(normalColor);
    }

    private void EnterTelegraph()
    {
        stateTimer = 0f;
        metalAttackState = MetalAttackState.Telegraph;
        rb.linearVelocity = Vector2.zero;
        ApplyBossColor(telegraphColor);
    }

    private void EnterDash()
    {
        stateTimer = 0f;
        metalAttackState = MetalAttackState.Dash;
        ApplyBossColor(normalColor);

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
        ApplyBossColor(normalColor);
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

    private float GetMetalCruiseY()
    {
        return startPosition.y + raceCruiseHeight;
    }

    private Vector2 GetRaceAnchor()
    {
        if (behaviorMode == BossBehaviorMode.MetalSonic)
        {
            float anchorX = rb.position.x + flySpeed * 0.5f;

            if (waypoints != null && waypoints.Length > 0)
            {
                Transform target = waypoints[Mathf.Clamp(waypointIndex, 0, waypoints.Length - 1)];
                if (target != null)
                    anchorX = target.position.x;
            }

            return new Vector2(anchorX, GetMetalCruiseY());
        }

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

    private void ApplyBossColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    private void RestoreVisualState()
    {
        if (visualTransform != null)
            visualTransform.localRotation = visualStartLocalRotation;

        if (spriteRenderer != null)
            spriteRenderer.sprite = defaultSprite;
    }

    private void EnsureOffscreenIndicator()
    {
        if (offscreenIndicatorRenderer != null)
            return;

        GameObject indicator = new GameObject("BossOffscreenIndicator");
        indicator.transform.SetParent(transform, false);
        indicator.hideFlags = HideFlags.DontSave;

        offscreenIndicatorRenderer = indicator.AddComponent<SpriteRenderer>();
        offscreenIndicatorRenderer.sprite = offscreenIndicatorSprite;
        offscreenIndicatorRenderer.color = offscreenIndicatorColor;
        offscreenIndicatorRenderer.sortingLayerID = spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
        offscreenIndicatorRenderer.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 10 : 10;
        indicator.transform.localScale = offscreenIndicatorScale;

        GameObject label = new GameObject("DistanceLabel");
        label.transform.SetParent(indicator.transform, false);
        label.transform.localPosition = new Vector3(0f, -0.8f, 0f);

        offscreenIndicatorText = label.AddComponent<TextMesh>();
        offscreenIndicatorText.text = string.Empty;
        offscreenIndicatorText.fontSize = 48;
        offscreenIndicatorText.characterSize = 0.08f;
        offscreenIndicatorText.anchor = TextAnchor.MiddleCenter;
        offscreenIndicatorText.alignment = TextAlignment.Center;
        offscreenIndicatorText.color = offscreenIndicatorColor;

        SetOffscreenIndicatorVisible(false);
    }

    private void UpdateOffscreenIndicator()
    {
        if (!showOffscreenIndicator || !raceActive)
        {
            SetOffscreenIndicatorVisible(false);
            return;
        }

        EnsureOffscreenIndicator();

        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null)
        {
            SetOffscreenIndicatorVisible(false);
            return;
        }

        Vector3 viewportPoint = cam.WorldToViewportPoint(transform.position);
        bool isVisible = viewportPoint.z > 0f &&
                         viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
                         viewportPoint.y >= 0f && viewportPoint.y <= 1f;

        if (isVisible)
        {
            SetOffscreenIndicatorVisible(false);
            return;
        }

        float clampedX = Mathf.Clamp(viewportPoint.x, offscreenViewportPadding.x, 1f - offscreenViewportPadding.x);
        float clampedY = Mathf.Clamp(viewportPoint.y, offscreenViewportPadding.y, 1f - offscreenViewportPadding.y);
        float worldDepth = Mathf.Abs(transform.position.z - cam.transform.position.z);
        Vector3 indicatorWorldPos = cam.ViewportToWorldPoint(new Vector3(clampedX, clampedY, worldDepth));
        indicatorWorldPos.z = transform.position.z;

        offscreenIndicatorRenderer.transform.position = indicatorWorldPos;

        Vector2 toBoss = (Vector2)(transform.position - indicatorWorldPos);
        float angle = Mathf.Atan2(toBoss.y, toBoss.x) * Mathf.Rad2Deg - 90f;
        offscreenIndicatorRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (offscreenIndicatorText != null)
        {
            offscreenIndicatorText.transform.rotation = Quaternion.identity;
            if (showOffscreenDistance)
            {
                float distance = playerTarget != null
                    ? Vector2.Distance(playerTarget.transform.position, transform.position)
                    : toBoss.magnitude;
                offscreenIndicatorText.text = Mathf.RoundToInt(distance).ToString();
            }
            else
            {
                offscreenIndicatorText.text = string.Empty;
            }
        }

        offscreenIndicatorRenderer.color = offscreenIndicatorColor;
        SetOffscreenIndicatorVisible(true);
    }

    private void SetOffscreenIndicatorVisible(bool visible)
    {
        if (offscreenIndicatorRenderer != null)
            offscreenIndicatorRenderer.enabled = visible && offscreenIndicatorRenderer.sprite != null;

        if (offscreenIndicatorText != null)
            offscreenIndicatorText.gameObject.SetActive(visible && showOffscreenDistance);
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
