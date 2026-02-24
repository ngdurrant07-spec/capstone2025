using UnityEngine;
using UnityEngine.InputSystem;

public class DirectionalDashPad : MonoBehaviour
{
    [Header("Barrel")]
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private Vector2 defaultLaunchDirection = Vector2.right;
    [SerializeField] private bool allowAimInput = true;
    [SerializeField] private float aimDeadZone = 0.25f;
    [SerializeField] private bool usePadRotationForDefaultDirection = true;
    [SerializeField] private bool snapTo4Directions = true;
    [SerializeField] private Transform seatPoint;
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string jumpActionName = "Jump";

    [Header("Control Lock")]
    [SerializeField] private float postLaunchControlLockTime = 0.12f;
    [SerializeField] private float reenterCooldown = 0.15f;
    [SerializeField] private float launchExitOffset = 0.2f;
    [SerializeField] private float collisionRestoreDelay = 0.12f;
    [SerializeField] private float collisionClearTimeout = 0.5f;
    [SerializeField] private float horizontalGravityDelay = 0.16f;
    [SerializeField] private float horizontalAimThreshold = 0.35f;
    [SerializeField] private float launchVelocityHoldTime = 0.1f;

    [Header("Audio")]
    [SerializeField] private bool playEnterSound = true;
    [SerializeField] private string enterSoundName = "EnterDDashPad";
    [SerializeField] private bool playReleaseSound = true;
    [SerializeField] private string releaseSoundName = "DDashPadRelease";

    private PlayerScript loadedPlayer;
    private Rigidbody2D loadedRb;
    private float loadedOriginalGravity;
    private Vector2 currentAimDirection = Vector2.right;
    private float cooldownTimer;
    private Collider2D[] barrelColliders;
    private Collider2D[] loadedPlayerColliders;
    private PlayerInput loadedPlayerInput;
    private InputAction loadedMoveAction;
    private InputAction loadedJumpAction;

    private void Awake()
    {
        MigrateLegacyReleaseSoundName();
        barrelColliders = GetComponentsInChildren<Collider2D>(true);
    }

    private void OnValidate()
    {
        MigrateLegacyReleaseSoundName();
    }

    private void OnDisable()
    {
        SetIgnoreBarrelCollisions(false);
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (loadedPlayer == null || loadedRb == null)
            return;

        currentAimDirection = GetAimDirection();
        HoldPlayerInsideBarrel();

        if (IsJumpPressedThisFrame())
            LaunchLoadedPlayer();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryLoadPlayer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryLoadPlayer(collision.collider);
    }

    private void TryLoadPlayer(Collider2D other)
    {
        if (cooldownTimer > 0f || loadedPlayer != null)
            return;

        PlayerScript player = other.GetComponent<PlayerScript>();
        if (player == null)
            player = other.GetComponentInParent<PlayerScript>();
        if (player == null)
            return;

        Rigidbody2D rb = player.rb != null ? player.rb : other.attachedRigidbody;
        if (rb == null)
            return;

        if (playEnterSound && !string.IsNullOrWhiteSpace(enterSoundName))
            SoundEffectManager.Play(enterSoundName);

        player.CancelGroundPound();
        player.enabled = false;

        loadedPlayer = player;
        loadedRb = rb;
        loadedOriginalGravity = rb.gravityScale;
        loadedPlayerColliders = player.GetComponentsInChildren<Collider2D>(true);
        loadedPlayerInput = player.GetComponent<PlayerInput>();
        loadedMoveAction = null;
        loadedJumpAction = null;
        if (loadedPlayerInput != null && loadedPlayerInput.actions != null)
        {
            loadedMoveAction = loadedPlayerInput.actions.FindAction(moveActionName, false);
            loadedJumpAction = loadedPlayerInput.actions.FindAction(jumpActionName, false);
            if (loadedMoveAction != null && !loadedMoveAction.enabled)
                loadedMoveAction.Enable();
            if (loadedJumpAction != null && !loadedJumpAction.enabled)
                loadedJumpAction.Enable();
        }
        SetIgnoreBarrelCollisions(true);
        currentAimDirection = GetAimDirection();
        HoldPlayerInsideBarrel();
    }

    private void HoldPlayerInsideBarrel()
    {
        loadedRb.gravityScale = 0f;
        loadedRb.linearVelocity = Vector2.zero;
        Vector3 holdPosition = seatPoint != null ? seatPoint.position : transform.position;
        loadedPlayer.transform.position = holdPosition;
    }

    private Vector2 GetAimDirection()
    {
        Vector2 dir = usePadRotationForDefaultDirection
            ? (Vector2)transform.right
            : defaultLaunchDirection;

        if (allowAimInput)
        {
            Vector2 input = ReadAimInput();
            if (input.sqrMagnitude >= aimDeadZone * aimDeadZone || HasDigitalAimInput())
                dir = input.sqrMagnitude > 0.0001f ? input : dir;
        }

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        dir.Normalize();

        if (snapTo4Directions)
            dir = SnapToFourDirections(dir);

        return dir;
    }

    private Vector2 ReadAimInput()
    {
        if (loadedMoveAction != null)
        {
            Vector2 actionValue = loadedMoveAction.ReadValue<Vector2>();
            if (actionValue.sqrMagnitude > 0.0001f)
                return actionValue;
        }

        Vector2 stick = Vector2.zero;

        if (Gamepad.current != null)
        {
            Vector2 left = Gamepad.current.leftStick.ReadValue();
            Vector2 right = Gamepad.current.rightStick.ReadValue();
            Vector2 dpad = Gamepad.current.dpad.ReadValue();

            stick = left;
            if (right.sqrMagnitude > stick.sqrMagnitude)
                stick = right;
            if (dpad.sqrMagnitude > stick.sqrMagnitude)
                stick = dpad;
        }

        if (stick.sqrMagnitude < 0.01f && Keyboard.current != null)
        {
            float x = 0f;
            float y = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
            stick = new Vector2(x, y);
        }

        return stick;
    }

    private bool HasDigitalAimInput()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) return true;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) return true;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) return true;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) return true;
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.left.isPressed || Gamepad.current.dpad.right.isPressed) return true;
            if (Gamepad.current.dpad.up.isPressed || Gamepad.current.dpad.down.isPressed) return true;
        }

        return false;
    }

    private Vector2 SnapToFourDirections(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            return new Vector2(Mathf.Sign(dir.x), 0f);

        return new Vector2(0f, Mathf.Sign(dir.y));
    }

    private bool IsJumpPressedThisFrame()
    {
        if (loadedJumpAction != null)
            return loadedJumpAction.WasPressedThisFrame();

        bool oldInputJump = Input.GetButtonDown("Jump");
        bool keyboardJump = Keyboard.current != null && (
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.zKey.wasPressedThisFrame ||
            Keyboard.current.xKey.wasPressedThisFrame
        );
        bool gamepadJump = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        return oldInputJump || keyboardJump || gamepadJump;
    }

    private void LaunchLoadedPlayer()
    {
        if (loadedPlayer == null || loadedRb == null)
            return;

        PlayReleaseSound();

        // Re-sample aim on the launch frame so left/right pressed with jump works immediately.
        currentAimDirection = GetAimDirection();
        bool mostlyHorizontal = Mathf.Abs(currentAimDirection.y) <= horizontalAimThreshold;
        loadedRb.gravityScale = mostlyHorizontal ? 0f : loadedOriginalGravity;
        Vector3 launchPosition = seatPoint != null ? seatPoint.position : transform.position;
        launchPosition += (Vector3)(currentAimDirection * launchExitOffset);
        loadedPlayer.transform.position = launchPosition;
        Vector2 launchVelocity = currentAimDirection.normalized * launchForce;
        loadedRb.linearVelocity = launchVelocity;
        Collider2D[] playerCollidersAtLaunch = loadedPlayerColliders;
        LayerMask playerGroundLayer = loadedPlayer.groundLayer;
        StartCoroutine(RestoreBarrelCollisionsAfterDelay(playerCollidersAtLaunch));
        float additionalControlLock = 0f;

        if (!mostlyHorizontal && launchVelocityHoldTime > 0f)
        {
            StartCoroutine(HoldLaunchVelocity(loadedRb, launchVelocity, launchVelocityHoldTime, playerCollidersAtLaunch, playerGroundLayer));
            additionalControlLock = Mathf.Max(additionalControlLock, launchVelocityHoldTime);
        }

        if (mostlyHorizontal && horizontalGravityDelay > 0f)
        {
            StartCoroutine(KeepHorizontalShotFlat(loadedRb, loadedOriginalGravity, horizontalGravityDelay, playerCollidersAtLaunch, playerGroundLayer));
            additionalControlLock = Mathf.Max(additionalControlLock, horizontalGravityDelay);
        }

        PlayerScript playerToRelease = loadedPlayer;
        loadedPlayer = null;
        loadedRb = null;
        loadedPlayerColliders = null;
        loadedPlayerInput = null;
        loadedMoveAction = null;
        loadedJumpAction = null;
        cooldownTimer = reenterCooldown;
        StartCoroutine(ReEnablePlayerController(
            playerToRelease,
            Mathf.Max(postLaunchControlLockTime, additionalControlLock),
            playerCollidersAtLaunch,
            playerGroundLayer
        ));
    }

    private void PlayReleaseSound()
    {
        if (!playReleaseSound)
            return;

        if (!string.IsNullOrWhiteSpace(releaseSoundName))
            SoundEffectManager.TryPlay(releaseSoundName);
    }

    private void MigrateLegacyReleaseSoundName()
    {
        if (releaseSoundName == "DashPadRelease")
            releaseSoundName = "DDashPadRelease";
    }

    private System.Collections.IEnumerator KeepHorizontalShotFlat(Rigidbody2D rb, float gravity, float duration, Collider2D[] playerColliders, LayerMask stopOnLayers)
    {
        if (rb == null)
            yield break;

        float xSpeed = rb.linearVelocity.x;
        float expectedAbsX = Mathf.Abs(xSpeed);
        float timer = 0f;

        while (rb != null && timer < duration)
        {
            if (timer > 0f && IsTouchingAnyLayer(playerColliders, stopOnLayers))
                break;

            // If a wall has effectively killed the horizontal launch, stop forcing a flat shot.
            if (timer > 0f && expectedAbsX > 0.01f)
            {
                float actualAbsX = Mathf.Abs(rb.linearVelocity.x);
                if (actualAbsX < expectedAbsX * 0.15f)
                    break;
            }

            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(xSpeed, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
            rb.gravityScale = gravity;
    }

    private System.Collections.IEnumerator RestoreBarrelCollisionsAfterDelay(Collider2D[] playerColliders)
    {
        if (collisionRestoreDelay > 0f)
            yield return new WaitForSeconds(collisionRestoreDelay);

        float timer = 0f;
        while (AreCollidersTouchingBarrel(playerColliders) && timer < collisionClearTimeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SetIgnoreBarrelCollisions(false, playerColliders);
    }

    private System.Collections.IEnumerator HoldLaunchVelocity(Rigidbody2D rb, Vector2 velocity, float duration, Collider2D[] playerColliders, LayerMask stopOnLayers)
    {
        if (rb == null)
            yield break;

        float targetSpeed = velocity.magnitude;
        Vector2 targetDir = targetSpeed > 0.0001f ? velocity / targetSpeed : Vector2.zero;
        float timer = 0f;
        while (rb != null && timer < duration)
        {
            if (timer > 0f && IsTouchingAnyLayer(playerColliders, stopOnLayers))
                break;

            // Stop forcing launch velocity if collision impact has already canceled the shot.
            if (timer > 0f && targetSpeed > 0.01f)
            {
                float speedAlongLaunch = Vector2.Dot(rb.linearVelocity, targetDir);
                if (speedAlongLaunch < targetSpeed * 0.15f)
                    break;
            }

            rb.linearVelocity = velocity;
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private bool IsTouchingAnyLayer(Collider2D[] colliders, LayerMask layers)
    {
        if (colliders == null || layers == 0)
            return false;

        foreach (Collider2D col in colliders)
        {
            if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
                continue;

            if (col.IsTouchingLayers(layers))
                return true;
        }

        return false;
    }

    private void SetIgnoreBarrelCollisions(bool ignore)
    {
        SetIgnoreBarrelCollisions(ignore, loadedPlayerColliders);
    }

    private void SetIgnoreBarrelCollisions(bool ignore, Collider2D[] playerColliders)
    {
        if (barrelColliders == null || playerColliders == null)
            return;

        foreach (Collider2D barrel in barrelColliders)
        {
            if (barrel == null)
                continue;

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider == null)
                    continue;

                Physics2D.IgnoreCollision(barrel, playerCollider, ignore);
            }
        }
    }

    private bool AreCollidersTouchingBarrel(Collider2D[] playerColliders)
    {
        if (barrelColliders == null || playerColliders == null)
            return false;

        foreach (Collider2D barrel in barrelColliders)
        {
            if (barrel == null || !barrel.enabled || !barrel.gameObject.activeInHierarchy)
                continue;

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider == null || !playerCollider.enabled || !playerCollider.gameObject.activeInHierarchy)
                    continue;

                if (barrel.bounds.Intersects(playerCollider.bounds))
                    return true;
            }
        }

        return false;
    }

    private System.Collections.IEnumerator ReEnablePlayerController(PlayerScript player, float delay, Collider2D[] playerColliders, LayerMask earlyReleaseLayers)
    {
        if (player == null)
            yield break;

        float timer = 0f;
        while (timer < delay)
        {
            if (timer > 0f && IsTouchingAnyLayer(playerColliders, earlyReleaseLayers))
                break;

            timer += Time.deltaTime;
            yield return null;
        }
        if (player != null)
            player.enabled = true;
    }
}
