using UnityEngine;

public class ThrowableFruitObj : MonoBehaviour
{
    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwUpwardForce = 2f;
    
    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private Vector3 originalPosition;
    
    [Header("Damage")]
    [SerializeField] private int damageAmount = 1;
    
    private Rigidbody2D rb;
    private Collider2D triggerCollider; // For pickup and damage detection
    private Collider2D physicsCollider; // For ground collision
    private bool isPickedUp = false;
    private bool isThrown = false;
    private Vector3 spawnPosition;
    private float respawnTimer = 0f;
    private bool isRespawning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Find trigger and non-trigger colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            if (col.isTrigger)
                triggerCollider = col;
            else
                physicsCollider = col;
        }

        // Prevent the fruit from rotating (rolling) when bumped
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        // If there is a 'Player' layer, ignore collisions between fruit and player
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, playerLayer, true);
        }

        spawnPosition = transform.position;
        originalPosition = transform.position;

        // Make the fruit stationary by default so it doesn't move when bumped
        rb.bodyType = RigidbodyType2D.Static;
    }

    void Update()
    {
        if (isRespawning)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
    }

    /// <summary>
    /// Pick up the fruit object (called by player on collision)
    /// </summary>
    public void PickUp(Transform playerHand)
    {
        if (isPickedUp || isRespawning) return;

        isPickedUp = true;
        isThrown = false;
        
        // Parent to player hand for carrying
        transform.SetParent(playerHand);
        transform.localPosition = Vector3.zero;
        
        // Disable physics while held
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (triggerCollider != null)
            triggerCollider.enabled = false;
        if (physicsCollider != null)
            physicsCollider.enabled = false;
    }

    /// <summary>
    /// Throw the fruit in a direction
    /// </summary>
    public void Throw(Vector2 direction)
    {
        if (!isPickedUp) return;

        isPickedUp = false;
        isThrown = true;
        
        // Unparent from player
        transform.SetParent(null);
        
        // Re-enable physics
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (triggerCollider != null)
            triggerCollider.enabled = true;
        if (physicsCollider != null)
            physicsCollider.enabled = true;
        
        // Apply throw force
        Vector2 throwVelocity = direction.normalized * throwForce;
        throwVelocity.y += throwUpwardForce;
        rb.linearVelocity = throwVelocity;
    }

    /// <summary>
    /// Respawn the fruit at its original location
    /// </summary>
    public void Respawn()
    {
        isRespawning = false;
        isPickedUp = false;
        isThrown = false;
        
        transform.SetParent(null);
        transform.position = spawnPosition;
        
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        // Return to stationary state after respawn
        rb.bodyType = RigidbodyType2D.Static;
        if (triggerCollider != null)
            triggerCollider.enabled = true;
        if (physicsCollider != null)
            physicsCollider.enabled = true;
    }

    /// <summary>
    /// Start the respawn timer (called after fruit goes off-screen or is destroyed)
    /// </summary>
    public void StartRespawn()
    {
        if (isRespawning) return;
        
        isRespawning = true;
        respawnTimer = respawnDelay;
        
        // Disable visuals
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Check if fruit is currently held by player
    /// </summary>
    public bool IsPickedUp() => isPickedUp;

    /// <summary>
    /// Check if fruit is in respawn state
    /// </summary>
    public bool IsRespawning() => isRespawning;

    /// <summary>
    /// Get the damage amount for enemy hits
    /// </summary>
    public int GetDamageAmount() => damageAmount;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Damage enemies when thrown
        if (isThrown && collision.CompareTag("Enemy"))
        {
            // Deal damage to enemy
            IDamageable enemy = collision.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
            }
            
            StartRespawn();
        }
    }
}
