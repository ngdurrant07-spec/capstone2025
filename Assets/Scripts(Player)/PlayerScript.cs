using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
    // ───────── PLAYER STATE ─────────
    public enum PlayerState { Normal, Jumping, Rolling, Gliding, GroundPounding }
    [HideInInspector] public PlayerState currentState = PlayerState.Normal;

    // ───────── COMPONENTS ─────────
    [Header("Components")]
    public Rigidbody2D rb;
    public ParticleSystem particleFX;
    public StompHitbox stompHitbox;

    // ───────── MOVEMENT ─────────
    [Header("Movement")]
    public float moveSpeed = 6f;
    float horizontalInput;
    float facingDirection = 1f;

    // ───────── JUMP ─────────
    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 1;
    int jumpsRemaining;
    bool jumpHeld;

    [Header("Stomp")]
    public float stompBounceForce = 10f;

    // ───────── GROUND CHECK ─────────
    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.2f, 0.2f);
    public LayerMask groundLayer;
    public float coyoteTime = 0.1f;
    float coyoteTimer;

    // ───────── GRAVITY & GLIDE ─────────
    [Header("Gravity & Glide")]
    public float baseGravity = 30f;
    public float maxFallSpeed = 22f;
    public float glideGravityScale = 0.4f;
    public float glideAcceleration = 12f;
    public float glideDrag = 3f;
    public float minGlideSpeed = 3f;
    public float maxGlideSpeed = 14f;
    public float maxGlideAscendSpeed = 6f;
    public float maxGlideDescendSpeed = 6f;
    public float glideAscendSpeedThreshold = 8f;
    public float glideLiftMinSpeed = 4f;
    public float glideLiftMaxSpeed = 14f;
    public AnimationCurve glideLiftCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float stallFallSpeed = 12f;
    public float maxLiftEnergy = 3f;
    public float liftDrainRate = 1f;
    public float liftRegenRate = 0.5f;
    public float baseLift = 5f;
    public float momentumMultiplier = 0.5f;

    // ───────── AIR BOOST PAD INTERFACE ─────────
public void AirBoost(Vector2 boostVelocity, float liftRestore = 1f, float gravityLockTime = 0.12f)
{
    // Cancel conflicting states
    CancelGroundPound();
    isRolling = false;

    // Reset vertical fall so boost feels clean
    linearVelocity = new Vector2(linearVelocity.x, Mathf.Max(linearVelocity.y, 0f));

    // Apply boost
    linearVelocity += boostVelocity;

    // Force glide state
    isGliding = true;
    glideUsed = false;                 // IMPORTANT: allows chaining pads
    glideDirection = Mathf.Sign(boostVelocity.x);
    liftEnergy = Mathf.Clamp(liftEnergy + liftRestore, 0f, maxLiftEnergy);
    isStalled = false;
    currentState = PlayerState.Gliding;

    // Brief gravity lock so player doesn't instantly drop
    StartCoroutine(AirBoostGravityLock(gravityLockTime));
}

IEnumerator AirBoostGravityLock(float time)
{
    float originalGravity = rb.gravityScale;
    rb.gravityScale = 0f;

    yield return new WaitForSeconds(time);

    if (currentState == PlayerState.Gliding)
        rb.gravityScale = glideGravityScale;
    else
        rb.gravityScale = originalGravity;
}


    bool isGliding;
    bool glideUsed;
    float glideDirection;
    float liftEnergy;
    bool isStalled;

    // ───────── GROUND POUND ─────────
    [Header("Ground Pound")]
    public float groundPoundSpeed = 30f;
    public float groundPoundFallCap = 35f;
    public float anticipationTime = 0.06f;
    public Vector2 groundPoundHitboxSize = new Vector2(0.9f, 0.9f);
    public Vector2 groundPoundHitboxOffset = new Vector2(0f, -0.2f);
    bool isGroundPounding;
    bool isAnticipating;
    bool groundPoundInvincibilityActive;
    public float groundPoundInvincibilityTime = 0.5f;

    // ───────── ROLL ─────────
    [Header("Roll")]
    public float rollSpeed = 20f;
    public float rollDuration = 0.5f;
    public float rollCooldown = 0.3f;
    public float rollSpeedBoost = 10f;
    public float rollSpeedBoostDuration = 0.3f;
    bool isRolling;
    float rollTimer;
    float rollCooldownTimer;
    float speedBoostTimer;
    bool rollInvincibilityActive;

    [Header("Roll Hitbox")]
    public Transform rollHitbox;       // Empty child object in front of player
    public Vector2 rollHitboxSize = new Vector2(1.5f, 1f);
    public float rollHitboxDuration = 0.2f;


    // ───────── SPEED BOOST ─────────
    [Header("Speed Boost")]
    public float hitSpeedBoost = 10f;
    public float hitSpeedBoostDuration = 0.3f;
    public float hitSpeedBoostTimer = 0f;

    // ───────── HEALTH ─────────
    [Header("Health")]
    public int maxHearts = 3;
    public int currentHearts;

    [Header("References")]
    public PlayerHealth playerHealth;

    [Header("Fall Death")]
    public float fallDeathY = -20f;
    public bool dieWhenOffScreen = true;
    public float offScreenBottomPadding = 1f;
    bool hasFallenToDeath;

    // ───────── THROWABLE ─────────
    [Header("Throwable")]
    public GameObject throwablePrefab;
    public Transform throwPoint;
    public float throwSpeed = 12f;
    private bool hasThrowable;
    public GameObject heldThrowableVisual;


    // ───────── ENEMIES ─────────
    [Header("Enemies")]
    public LayerMask enemyLayer;

    // ───────── VELOCITY WRAPPER ─────────
    public Vector2 linearVelocity
    {
        get => rb.linearVelocity;
        set => rb.linearVelocity = value;
    }

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
        liftEnergy = maxLiftEnergy;
        currentHearts = maxHearts;
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

    }

    void Update()
    {
        GroundCheck();
        ApplyGravity();
        HandleMovement();
        HandleGlide();
        HandleRoll();
        HandleGroundPound();
        HandleHeldThrowableInput();
        CheckFallDeath();
    }

    // ───────── INPUT SYSTEM ─────────
    public void Move(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;
        if (horizontalInput != 0 && currentState != PlayerState.Gliding && currentState != PlayerState.GroundPounding)
            facingDirection = Mathf.Sign(horizontalInput);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpHeld = true;

            if (IsGrounded())
            {
                linearVelocity = new Vector2(linearVelocity.x, jumpForce);
                jumpsRemaining = maxJumps - 1;
                coyoteTimer = 0f;
                currentState = PlayerState.Jumping;

                //jump sound effect plays

                SoundEffectManager.Play("Jump");
            }
            else if (!IsGrounded() && !glideUsed && !isGroundPounding)
            {
                if (!hasThrowable)
                    StartGlide();

                //jump sound effect plays

                SoundEffectManager.Play("Glide");
            }
            else if (coyoteTimer > 0f)
            {
                linearVelocity = new Vector2(linearVelocity.x, jumpForce);
                coyoteTimer = 0f;
                currentState = PlayerState.Jumping;
            }
        }

        if (context.canceled)
        {
            jumpHeld = false;
            isGliding = false;
        }
    }

    public void GroundPound(InputAction.CallbackContext context)
    {
        if (!context.performed || IsGrounded() || isGroundPounding || isAnticipating) return;
        StartCoroutine(GroundPoundAnticipation());
    }

    public void RollAttack(InputAction.CallbackContext context)
    {
        if (!context.performed || !IsGrounded() || isRolling || rollCooldownTimer > 0f || hasThrowable) return;
        isRolling = true;
        rollTimer = rollDuration;
        currentState = PlayerState.Rolling;
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth != null && !rollInvincibilityActive)
        {
            playerHealth.SetInvincible(true);
            rollInvincibilityActive = true;
        }
        SoundEffectManager.Play("Hit_Tail");
    }

    public bool TryPickupThrowable(GameObject prefab = null)
    {
        if (hasThrowable) return false;
        if (prefab != null)
            throwablePrefab = prefab;
        hasThrowable = true;
        if (heldThrowableVisual != null)
            heldThrowableVisual.SetActive(true);
        isGliding = false;
        return true;
    }

    void HandleHeldThrowableInput()
    {
        if (!hasThrowable)
            return;

        if (Input.GetKeyUp(KeyCode.E))
        {
            Debug.Log($"[PlayerScript] E released. hasThrowable={hasThrowable} prefab={(throwablePrefab != null)} throwPoint={(throwPoint != null)}");
            if (throwablePrefab == null || throwPoint == null)
                return;
            GameObject obj = Instantiate(throwablePrefab, throwPoint.position, Quaternion.identity);
            ThrowableItem throwable = obj.GetComponent<ThrowableItem>();
            Vector2 dir = new Vector2(facingDirection, 0f);
            if (throwable != null)
                throwable.Launch(dir, throwSpeed);
            SoundEffectManager.Play("Fruit Throw");
            hasThrowable = false;
            if (heldThrowableVisual != null)
                heldThrowableVisual.SetActive(false);
        }
    }


    // ───────── MOVEMENT ─────────
    void HandleMovement()
    {
        if (currentState == PlayerState.Gliding || currentState == PlayerState.GroundPounding || isRolling)
            return;

        float speed = horizontalInput * moveSpeed;

        if (speedBoostTimer > 0f && IsGrounded())
            speed += rollSpeedBoost * facingDirection;

        if (hitSpeedBoostTimer > 0f)
        {
            speed += hitSpeedBoostTimer / hitSpeedBoostDuration * hitSpeedBoost * facingDirection;
            hitSpeedBoostTimer -= Time.deltaTime;
        }

        linearVelocity = new Vector2(speed, linearVelocity.y);
    }

    void ApplyGravity()
    {
        if (currentState == PlayerState.GroundPounding) return;

        if (currentState != PlayerState.Gliding)
        {
            rb.gravityScale = 1f;
            linearVelocity += Vector2.down * baseGravity * Time.deltaTime;
            linearVelocity = new Vector2(linearVelocity.x, Mathf.Max(linearVelocity.y, -maxFallSpeed));
        }
    }

    // ───────── GLIDE ─────────
    void StartGlide()
    {
        isGliding = true;
        glideUsed = true;
        glideDirection = facingDirection;
        liftEnergy = maxLiftEnergy;
        isStalled = false;
        currentState = PlayerState.Gliding;

        if (linearVelocity.y > 0f)
            linearVelocity = new Vector2(linearVelocity.x, 0f);
    }

    void HandleGlide()
    {
        if (!isGliding || !jumpHeld || currentState == PlayerState.GroundPounding)
        {
            rb.gravityScale = 1f;
            return;
        }

        rb.gravityScale = glideGravityScale;

        float speed = Mathf.Abs(linearVelocity.x);
        float relativeInput = horizontalInput * glideDirection;

        speed -= glideDrag * Time.deltaTime;

        if (relativeInput > 0)
        {
            speed += glideAcceleration * Time.deltaTime;
            liftEnergy += liftRegenRate * Time.deltaTime;
            isStalled = false;
        }
        else if (relativeInput < 0)
        {
            liftEnergy -= liftDrainRate * Time.deltaTime;
        }

        liftEnergy = Mathf.Clamp(liftEnergy, 0f, maxLiftEnergy);
        speed = Mathf.Clamp(speed, minGlideSpeed, maxGlideSpeed);

        linearVelocity = new Vector2(glideDirection * speed, linearVelocity.y);

        float verticalVelocity = linearVelocity.y;

        if (liftEnergy <= 0f) isStalled = true;

        float liftT = Mathf.InverseLerp(glideLiftMinSpeed, glideLiftMaxSpeed, speed);
        float liftScale = glideLiftCurve.Evaluate(liftT);
        float targetLift = baseLift * liftScale;
        bool canAscend = speed >= glideAscendSpeedThreshold && liftScale > 0f;

        if (isStalled)
            verticalVelocity = Mathf.MoveTowards(verticalVelocity, -stallFallSpeed, 40f * Time.deltaTime);
        else if (relativeInput < 0 && canAscend)
            verticalVelocity = Mathf.Lerp(verticalVelocity, targetLift - linearVelocity.y * momentumMultiplier, 5f * Time.deltaTime);
        else
            verticalVelocity = Mathf.MoveTowards(verticalVelocity, -maxGlideDescendSpeed, 25f * Time.deltaTime);

        float maxAscend = canAscend ? maxGlideAscendSpeed * liftScale : 0f;
        verticalVelocity = Mathf.Clamp(verticalVelocity, -stallFallSpeed, maxAscend);
        linearVelocity = new Vector2(linearVelocity.x, verticalVelocity);
    }

    // ───────── ROLL ─────────
    void HandleRoll()
    {
        if (rollCooldownTimer > 0f) rollCooldownTimer -= Time.deltaTime;
        if (speedBoostTimer > 0f) speedBoostTimer -= Time.deltaTime;
        if (!isRolling) return;

        rollTimer -= Time.deltaTime;

        // Keep horizontal movement, preserve vertical
        linearVelocity = new Vector2(facingDirection * rollSpeed, linearVelocity.y);

        // Detect enemies
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position + Vector3.right * facingDirection, new Vector2(1f, 1f), 0f, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            IStompable stompable = hit.GetComponent<IStompable>();
            if (stompable != null)
            {
                stompable.OnStomp();
                    speedBoostTimer = rollSpeedBoostDuration;
            }
        }

        if (rollTimer <= 0f)
        {
            isRolling = false;
            currentState = PlayerState.Normal;
            rollCooldownTimer = rollCooldown;
            if (playerHealth != null && rollInvincibilityActive)
            {
                playerHealth.SetInvincible(false);
                rollInvincibilityActive = false;
            }
        }
    }


    // ───────── GROUND POUND ─────────
    IEnumerator GroundPoundAnticipation()
    {
        isAnticipating = true;
        isGliding = false;
        isStalled = false;
        liftEnergy = 0f;

        if (playerHealth != null && !groundPoundInvincibilityActive)
        {
            playerHealth.SetInvincible(true);
            groundPoundInvincibilityActive = true;
        }

        linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        yield return new WaitForSeconds(anticipationTime);

        isAnticipating = false;
        isGroundPounding = true;
        currentState = PlayerState.GroundPounding;

        linearVelocity = Vector2.down * groundPoundSpeed;
        particleFX.Play();
    }

    void HandleGroundPound()
    {
        if (!isGroundPounding) return;

        float yVel = linearVelocity.y;
        if (yVel > 0f)
            yVel = -groundPoundSpeed;
        if (yVel < -groundPoundFallCap)
            yVel = -groundPoundFallCap;

        linearVelocity = new Vector2(linearVelocity.x, yVel);

        // Damage stompable enemies while ground pounding
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + groundPoundHitboxOffset,
            groundPoundHitboxSize,
            0f,
            enemyLayer
        );
        foreach (Collider2D hit in hits)
        {
            IStompable stompable = hit.GetComponentInParent<IStompable>();
            if (stompable != null)
                stompable.OnStomp();
        }

        if (IsGrounded())
        {
            isGroundPounding = false;
            rb.gravityScale = 1f;
            currentState = PlayerState.Normal;
            if (groundPoundInvincibilityActive)
                StartCoroutine(EndGroundPoundInvincibility());
        }
    }

    // ───────── WALL COLLISION ─────────
    void OnCollisionEnter2D(Collision2D collision)
    {
        TryCancelGlideOnWall(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryCancelGlideOnWall(collision);
    }

    void TryCancelGlideOnWall(Collision2D collision)
    {
        if (currentState != PlayerState.Gliding || !isGliding)
            return;
        if (((1 << collision.gameObject.layer) & groundLayer) == 0)
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f && contact.normal.y < 0.5f)
            {
                isGliding = false;
                isStalled = false;
                rb.gravityScale = 1f;
                currentState = PlayerState.Normal;
                return;
            }
        }
    }

    public void CancelGroundPound()
    {
        if (isGroundPounding || isAnticipating)
        {
            isGroundPounding = false;
            isAnticipating = false;
            currentState = PlayerState.Normal;

            rb.gravityScale = 1f;
            linearVelocity = new Vector2(linearVelocity.x, 0f);

            if (particleFX != null && particleFX.isPlaying)
                particleFX.Stop();
        }
    }

    // ───────── GROUND CHECK ─────────
    void GroundCheck()
    {
        if (groundCheck == null)
            return;
        if (IsGrounded())
        {
            jumpsRemaining = maxJumps;
            isGliding = false;
            glideUsed = false;
            isStalled = false;
            liftEnergy = maxLiftEnergy;
            rb.gravityScale = 1f;
            coyoteTimer = coyoteTime;
            if (currentState != PlayerState.Rolling)
                currentState = PlayerState.Normal;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    bool IsGrounded()
    {
        if (groundCheck == null)
            return false;
        // BoxCast down so walls (side hits) don't count as grounded
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            groundCheck.position,
            groundCheckSize,
            0f,
            Vector2.down,
            0.02f,
            groundLayer
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            // Only accept surfaces that are mostly "up"
            if (hit.normal.y > 0.5f)
                return true;
        }

        return false;
    }

    public bool CanTakeDamage()
    {
        Debug.Log($"[PlayerScript] CanTakeDamage? state={currentState} anticipating={isAnticipating}");
        if (playerHealth != null && playerHealth.IsInvincible())
            return false;
        return currentState != PlayerState.Rolling &&
               currentState != PlayerState.GroundPounding &&
               !isAnticipating;
    }

    IEnumerator EndGroundPoundInvincibility()
    {
        yield return new WaitForSeconds(groundPoundInvincibilityTime);
        if (playerHealth != null)
            playerHealth.SetInvincible(false);
        groundPoundInvincibilityActive = false;
    }


    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawCube(groundCheck.position, groundCheckSize);
        }
    }

    public void RespawnAt(Vector3 position)
    {
        transform.position = position;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1f;
        currentState = PlayerState.Normal;
        isGliding = false;
        glideUsed = false;
        isStalled = false;
        isRolling = false;
        isGroundPounding = false;
        isAnticipating = false;
        jumpsRemaining = maxJumps;
        coyoteTimer = 0f;
        liftEnergy = maxLiftEnergy;
        hasFallenToDeath = false;
        if (particleFX != null && particleFX.isPlaying)
            particleFX.Stop();
        if (playerHealth != null)
            playerHealth.ResetToFull();
    }

    void CheckFallDeath()
    {
        if (hasFallenToDeath)
            return;
        if (dieWhenOffScreen)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                if (cam.orthographic)
                {
                    float halfHeight = cam.orthographicSize;
                    Vector3 camPos = cam.transform.position;
                    float bottomY = camPos.y - halfHeight;
                    if (transform.position.y < bottomY - offScreenBottomPadding)
                    {
                        KillPlayer();
                    }
                }
                else
                {
                    Vector3 vp = cam.WorldToViewportPoint(transform.position);
                    if (vp.y < 0f || vp.x < 0f || vp.x > 1f)
                        KillPlayer();
                }
                return;
            }
        }

        if (transform.position.y >= fallDeathY)
            return;
        KillPlayer();
    }

    void KillPlayer()
    {
        if (hasFallenToDeath)
            return;
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Kill();
        hasFallenToDeath = true;
    }
}
