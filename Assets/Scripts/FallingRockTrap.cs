using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FallingRockTrap : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private string playerTag = "Player";

    [Header("Timing")]
    [SerializeField] private float initialDelay = 0f;
    [SerializeField] private float timeBeforeDrop = 1.5f;
    [SerializeField] private float maxFallTime = 3f;
    [SerializeField] private float respawnDelay = 1f;

    [Header("Reset")]
    [SerializeField] private float resetFallDistance = 20f;

    private Rigidbody2D rb;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private bool isDropping;
    private bool hitSomethingThisDrop;
    private bool damagedPlayerThisDrop;
    private Coroutine cycleRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    private void OnEnable()
    {
        cycleRoutine = StartCoroutine(DropCycle());
    }

    private void OnDisable()
    {
        if (cycleRoutine != null)
            StopCoroutine(cycleRoutine);

        cycleRoutine = null;
        ResetRockToSpawn();
    }

    private IEnumerator DropCycle()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            ResetRockToSpawn();

            if (timeBeforeDrop > 0f)
                yield return new WaitForSeconds(timeBeforeDrop);

            StartDrop();

            float elapsed = 0f;
            while (isDropping && elapsed < maxFallTime)
            {
                if (transform.position.y <= spawnPosition.y - resetFallDistance)
                    hitSomethingThisDrop = true;

                if (hitSomethingThisDrop)
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            StopDrop();

            if (respawnDelay > 0f)
                yield return new WaitForSeconds(respawnDelay);
        }
    }

    private void StartDrop()
    {
        isDropping = true;
        hitSomethingThisDrop = false;
        damagedPlayerThisDrop = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void StopDrop()
    {
        isDropping = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void ResetRockToSpawn()
    {
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        isDropping = false;
        hitSomethingThisDrop = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider, stopDropOnHit: true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other, stopDropOnHit: false);
    }

    private void HandleHit(Collider2D other, bool stopDropOnHit)
    {
        if (!isDropping)
            return;

        if (other.CompareTag(playerTag))
        {
            if (!damagedPlayerThisDrop)
            {
                IDamageable damageable = other.GetComponentInParent<IDamageable>();
                if (damageable != null)
                    damageable.TakeDamage(damage);

                damagedPlayerThisDrop = true;
            }

            return;
        }

        if (stopDropOnHit)
            hitSomethingThisDrop = true;
    }
}
