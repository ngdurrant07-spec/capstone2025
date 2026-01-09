using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public ParticleSystem particleFX;

    [Header("Movement")]
    public float moveSpeed = 6f;
    float horizontalInput;
    float facingDirection = 1f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 1;
    int jumpsRemaining;
    bool jumpHeld;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.2f, 0.2f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 30f;
    public float maxFallSpeed = 22f;

    [Header("Glide")]
    public float glideGravityScale = 0.4f;
    bool isGliding;
    float glideDirection;

    [Header("Glide Horizontal")]
    public float glideAcceleration = 12f;
    public float glideDrag = 3f;
    public float minGlideSpeed = 3f;
    public float maxGlideSpeed = 14f;

    [Header("Glide Vertical Caps")]
    public float maxGlideAscendSpeed = 6f;
    public float maxGlideDescendSpeed = 6f;
    public float stallFallSpeed = 12f;

    [Header("Lift Energy")]
    public float maxLiftEnergy = 1f;
    public float liftDrainRate = 1f;
    public float liftRegenRate = 0.5f;

    [Header("Momentum Lift")]
    public float momentumMultiplier = 0.5f;
    public float baseLift = 5f;

    float liftEnergy;
    bool isStalled;

    [Header("Ground Pound")]
    public float groundPoundSpeed = 30f;
    public float groundPoundFallCap = 35f;
    public float anticipationTime = 0.06f;

    bool isGroundPounding;
    bool isAnticipating;

    [Header("Roll Attack")]
    public float rollSpeed = 20f;
    public float rollDuration = 0.5f;
    public float rollCooldown = 0.3f;
    public float rollSpeedBoost = 10f; // Extra ground speed after hitting enemy
    public float rollSpeedBoostDuration = 0.3f;
    public LayerMask enemyLayer;

    bool isRolling;
    float rollTimer;
    float rollCooldownTimer;
    float speedBoostTimer;

    [Header("Coyote Jump")]
    public float coyoteTime = 0.1f; // Time window to jump after leaving ground
    float coyoteTimer;

  [Header("Health")]
public float health = 100f;

    void Start()
    {
        jumpsRemaining = maxJumps;
        liftEnergy = maxLiftEnergy;
    }

    void Update()
    {
        GroundCheck();
        HandleGroundPound();
        HandleRoll();
        HandleMovement();
        HandleGlide();
        ApplyGravity();
    }

    // ───────── INPUT ─────────

    public void Move(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;

        if (!isGliding && !isGroundPounding && horizontalInput != 0)
            facingDirection = Mathf.Sign(horizontalInput);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpHeld = true;

            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining = maxJumps - 1;
                coyoteTimer = 0; // Use up coyote time
            }
            else if (!isGliding && !isGroundPounding && jumpsRemaining > 0)
            {
                // Prioritize gliding over coyote jump
                isGliding = true;
                glideDirection = facingDirection;
                jumpsRemaining--;
                liftEnergy = maxLiftEnergy;
                isStalled = false;
            }
            else if (coyoteTimer > 0)
            {
                // Coyote jump - only if glide isn't available
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                coyoteTimer = 0; // Use up coyote time, but don't consume jumps
            }
        }

        if (context.canceled)
        {
            jumpHeld = false;
            isGliding = false;
        }
    }

    // S key
    public void GroundPound(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (IsGrounded() || isGroundPounding || isAnticipating) return;

        StartCoroutine(GroundPoundAnticipation());
    }

    // Roll Attack - bind to a key like Shift or D
    public void RollAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!IsGrounded() || isRolling || rollCooldownTimer > 0) return;

        isRolling = true;
        rollTimer = rollDuration;
    }

    // ───────── ANTICIPATION ─────────

    IEnumerator GroundPoundAnticipation()
    {
        isAnticipating = true;
        isGliding = false;
        isStalled = false;
        liftEnergy = 0f;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        yield return new WaitForSeconds(anticipationTime);

        isAnticipating = false;
        isGroundPounding = true;

        rb.linearVelocity = Vector2.down * groundPoundSpeed;
        particleFX.Play();
    }

    // ───────── MOVEMENT ─────────

    void HandleMovement()
    {
        if (isGliding || isGroundPounding || isAnticipating || isRolling) return;

        float speed = horizontalInput * moveSpeed;
        
        // Apply speed boost if active and grounded
        if (speedBoostTimer > 0 && IsGrounded())
            speed += rollSpeedBoost * facingDirection;

        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    void ApplyGravity()
    {
        if (isGliding || isGroundPounding || isAnticipating) return;

        rb.linearVelocity += Vector2.down * baseGravity * Time.deltaTime;
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            Mathf.Max(rb.linearVelocity.y, -maxFallSpeed)
        );
    }

    // ───────── GLIDE ─────────

    void HandleGlide()
    {
        if (!isGliding || !jumpHeld || isGroundPounding || isAnticipating)
        {
            rb.gravityScale = 1f;
            return;
        }

        rb.gravityScale = glideGravityScale;

        float speed = Mathf.Abs(rb.linearVelocity.x);
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

        rb.linearVelocity = new Vector2(glideDirection * speed, rb.linearVelocity.y);

        float verticalVelocity = rb.linearVelocity.y;

        if (liftEnergy <= 0f)
            isStalled = true;

        if (isStalled)
        {
            verticalVelocity = Mathf.MoveTowards(verticalVelocity, -stallFallSpeed, 40f * Time.deltaTime);
        }
        else if (relativeInput < 0)
        {
            float fallSpeed = -rb.linearVelocity.y;
            float momentumLift = baseLift + fallSpeed * momentumMultiplier;

            verticalVelocity = Mathf.Lerp(verticalVelocity, momentumLift, 5f * Time.deltaTime);
        }
        else
        {
            verticalVelocity = Mathf.MoveTowards(verticalVelocity, -maxGlideDescendSpeed, 25f * Time.deltaTime);
        }

        verticalVelocity = Mathf.Clamp(verticalVelocity, -stallFallSpeed, maxGlideAscendSpeed);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalVelocity);
    }

    // ───────── GROUND POUND ─────────

    void HandleRoll()
    {
        // Decrement cooldown timer
        if (rollCooldownTimer > 0)
            rollCooldownTimer -= Time.deltaTime;

        // Decrement speed boost timer
        if (speedBoostTimer > 0)
            speedBoostTimer -= Time.deltaTime;

        if (!isRolling) return;

        rollTimer -= Time.deltaTime;

        // Move the player in the roll direction
        rb.linearVelocity = new Vector2(facingDirection * rollSpeed, rb.linearVelocity.y);

        // Check for enemies to hit
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.right * facingDirection,
            1f,
            enemyLayer
        );

        if (hit.collider != null)
        {
            // Deal damage or destroy enemy
            EnemyType1 enemy = hit.collider.GetComponent<EnemyType1>();
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
                // Give player a speed boost on the ground after hitting enemy
                speedBoostTimer = rollSpeedBoostDuration;
            }
        }

        // End roll when timer expires
        if (rollTimer <= 0)
        {
            isRolling = false;
            rollCooldownTimer = rollCooldown;
        }
    }

    void HandleGroundPound()
    {
        if (!isGroundPounding) return;

        rb.linearVelocity = new Vector2(
            0f,
            Mathf.Max(rb.linearVelocity.y, -groundPoundFallCap)
        );

        if (IsGrounded())
        {
            isGroundPounding = false;
            rb.gravityScale = 1f;
        }
    }

    // ───────── GROUND ─────────

    void GroundCheck()
    {
        if (IsGrounded())
        {
            jumpsRemaining = maxJumps;
            isGliding = false;
            isStalled = false;
            liftEnergy = maxLiftEnergy;
            rb.gravityScale = 1f;
            coyoteTimer = coyoteTime; // Reset coyote timer when grounded
        }
        else
        {
            // Decrement coyote timer while in the air
            coyoteTimer -= Time.deltaTime;
        }
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            groundLayer
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheck.position, groundCheckSize);
    }
}


