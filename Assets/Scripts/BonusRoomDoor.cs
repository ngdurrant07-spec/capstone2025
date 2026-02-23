using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BonusRoomDoor : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = false;
    [SerializeField] private Transform teleportTarget;

    [Header("Enter Door Setup")]
    [SerializeField] private BonusRoomTimer bonusTimer;
    [SerializeField] private float countdownSeconds = 20f;
    [SerializeField] private float enterDelaySeconds = 0.2f;
    [SerializeField] private Transform successReturnPoint;
    [SerializeField] private Transform timeoutReturnPoint;
    [SerializeField] private bool hideDoorVisualsAfterUse = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onDoorUsed;
    [SerializeField] private UnityEvent onBonusStarted;
    [SerializeField] private UnityEvent onBonusCompleted;
    [SerializeField] private UnityEvent onBonusFailed;

    private Collider2D cachedCollider;
    private Collider2D activePlayer;
    private bool enterTransitionInProgress;
    private bool bonusActive;
    private bool used;

    public bool BonusActive => bonusActive;

    void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        if (bonusTimer != null)
            bonusTimer.AddExpiredListener(HandleTimerExpired);
    }

    void OnDisable()
    {
        if (bonusTimer != null)
            bonusTimer.RemoveExpiredListener(HandleTimerExpired);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (triggerOnce && used)
            return;

        UseEnterDoor(other);
    }

    public void CompleteFromGoal(Collider2D player)
    {
        if (!bonusActive || !IsActivePlayer(player))
            return;

        Transform returnPoint = successReturnPoint != null ? successReturnPoint : timeoutReturnPoint;
        FinishBonus(success: true, player, teleportTargetOverride: returnPoint);
    }

    public void FailActiveBonus(Collider2D player)
    {
        if (!bonusActive || !IsActivePlayer(player))
            return;

        FinishBonus(success: false, player, timeoutReturnPoint);
    }

    public bool TryHandleActivePlayerDeath()
    {
        if (!bonusActive || activePlayer == null)
            return false;

        Collider2D player = activePlayer;
        Transform returnPoint = timeoutReturnPoint;

        FinishBonus(success: false, player, returnPoint);

        PlayerScript playerScript = player.GetComponent<PlayerScript>();
        if (playerScript == null)
            playerScript = player.GetComponentInParent<PlayerScript>();

        if (playerScript != null && returnPoint != null)
            playerScript.RespawnAt(returnPoint.position);
        else
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health == null)
                health = player.GetComponentInParent<PlayerHealth>();

            if (health != null)
                health.ResetToFull();
        }

        return true;
    }

    public bool IsActivePlayer(Collider2D player)
    {
        if (activePlayer == null || player == null)
            return false;

        Transform activeRoot = activePlayer.attachedRigidbody != null
            ? activePlayer.attachedRigidbody.transform
            : activePlayer.transform.root;

        Transform otherRoot = player.attachedRigidbody != null
            ? player.attachedRigidbody.transform
            : player.transform.root;

        return activeRoot == otherRoot;
    }

    private void UseEnterDoor(Collider2D player)
    {
        if (teleportTarget == null)
            return;

        if (bonusActive || enterTransitionInProgress)
            return;

        used = true;
        activePlayer = player;
        onDoorUsed?.Invoke();

        if (triggerOnce)
            DisableDoorCollision();

        if (enterDelaySeconds > 0f)
        {
            enterTransitionInProgress = true;
            StartCoroutine(BeginBonusAfterDelay());
            return;
        }

        StartBonusNow();
    }

    private void HandleTimerExpired()
    {
        if (!bonusActive)
            return;

        FinishBonus(success: false, activePlayer, timeoutReturnPoint);
    }

    private void FinishBonus(bool success, Collider2D player, Transform teleportTargetOverride)
    {
        enterTransitionInProgress = false;
        bonusActive = false;

        if (bonusTimer != null)
        {
            bonusTimer.StopTimer();
            bonusTimer.HideTimerUI();
        }

        if (player != null)
        {
            Transform target = teleportTargetOverride;
            if (target != null)
                TeleportPlayer(player, target);
        }

        if (success)
        {
            onBonusCompleted?.Invoke();
            if (hideDoorVisualsAfterUse)
                HideDoorVisuals();
        }
        else
        {
            onBonusFailed?.Invoke();
        }

        activePlayer = null;
    }

    private IEnumerator BeginBonusAfterDelay()
    {
        Collider2D player = activePlayer;
        if (player != null && player.attachedRigidbody != null)
        {
            player.attachedRigidbody.linearVelocity = Vector2.zero;
            player.attachedRigidbody.angularVelocity = 0f;
        }

        yield return new WaitForSeconds(enterDelaySeconds);

        enterTransitionInProgress = false;

        if (player == null)
            yield break;

        StartBonusNow();
    }

    private void StartBonusNow()
    {
        if (activePlayer == null || teleportTarget == null)
            return;

        bonusActive = true;

        TeleportPlayer(activePlayer, teleportTarget);

        if (bonusTimer != null)
            bonusTimer.StartCountdown(countdownSeconds);

        onBonusStarted?.Invoke();
    }

    private void TeleportPlayer(Collider2D player, Transform target)
    {
        if (player == null || target == null)
            return;

        Rigidbody2D rb = player.attachedRigidbody;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = target.position;
            rb.rotation = target.eulerAngles.z;
            return;
        }

        player.transform.SetPositionAndRotation(target.position, target.rotation);
    }

    private void DisableDoorCollision()
    {
        if (cachedCollider != null)
            cachedCollider.enabled = false;
    }

    private void HideDoorVisuals()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }
}
