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
    public float stallFallSpeed = 12f;
    public float maxLiftEnergy = 1f;
    public float liftDrainRate = 1f;
    public float liftRegenRate = 0.5f;
    public float baseLift = 5f;
    public float momentumMultiplier = 0.5f;

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
    bool isGroundPounding;
    bool isAnticipating;

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

    // ───────── SPEED BOOST ─────────
    [Header("Speed Boost")]
    public float hitSpeedBoost = 10f;
    public float hitSpeedBoostDuration = 0.3f;
    public float hitSpeedBoostTimer = 0f;

    // ───────── HEALTH ─────────
    [Header("Health")]
    public int maxHearts = 3;
    public int currentHearts;

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
        jumpsRemaining = maxJumps;
        liftEnergy = maxLiftEnergy;
        currentHearts = maxHearts;
    }

    void Update()
    {
        GroundCheck();
        ApplyGravity();
        HandleMovement();
        HandleGlide();
        HandleRoll();
        HandleGroundPound();
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
            }
            else if (!IsGrounded() && !glideUsed && !isGroundPounding)
            {
                StartGlide();
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
        if (!context.performed || !IsGrounded() || isRolling || rollCooldownTimer > 0f) return;
        isRolling = true;
        rollTimer = rollDuration;
        currentState = PlayerState.Rolling;
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

        if (isStalled)
            verticalVelocity = Mathf.MoveTowards(verticalVelocity, -stallFallSpeed, 40f * Time.deltaTime);
        else if (relativeInput < 0)
            verticalVelocity = Mathf.Lerp(verticalVelocity, baseLift - linearVelocity.y * momentumMultiplier, 5f * Time.deltaTime);
        else
            verticalVelocity = Mathf.MoveTowards(verticalVelocity, -maxGlideDescendSpeed, 25f * Time.deltaTime);

        verticalVelocity = Mathf.Clamp(verticalVelocity, -stallFallSpeed, maxGlideAscendSpeed);
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
        }
    }

    // ───────── GROUND POUND ─────────
    IEnumerator GroundPoundAnticipation()
    {
        isAnticipating = true;
        isGliding = false;
        isStalled = false;
        liftEnergy = 0f;

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
        if (yVel < -groundPoundFallCap)
            yVel = -groundPoundFallCap;

        linearVelocity = new Vector2(linearVelocity.x, yVel);

        if (IsGrounded())
        {
            isGroundPounding = false;
            rb.gravityScale = 1f;
            currentState = PlayerState.Normal;
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
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawCube(groundCheck.position, groundCheckSize);
        }
    }
}

