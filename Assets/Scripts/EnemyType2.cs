using UnityEngine;
using System;

public class EnemyType2 : MonoBehaviour, IGroundPoundable
{
    public event Action OnEnemyDefeated; // Event triggered when enemy dies

    [Header("Movement")]
    public float moveSpeed = 1.5f;

    [Header("Defeat")]
    public float knockbackForce = 10f;
    public float fallGravity = 3f;
    public float destroyDelay = 1f;

    [Header("Ground Pound Hitbox")]
    public BoxCollider2D topCollider; // assign in inspector (child collider on top of enemy)

    private Rigidbody2D rb;
    private bool defeated;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (defeated) return;

        // Simple horizontal movement
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
    }

    // Called when player ground pounds on the enemy's topCollider
    public void OnGroundPounded()
    {
        if (defeated) return;
        Defeat(Vector2.up);
    }

    private void Defeat(Vector2 hitDirection)
    {
        if (defeated) return;
        defeated = true;

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        // Enable gravity for fall effect
        rb.gravityScale = fallGravity;

        // Optional knockback
        rb.AddForce(hitDirection * knockbackForce, ForceMode2D.Impulse);

        // Trigger any event listeners
        OnEnemyDefeated?.Invoke();

        // Destroy after delay
        Destroy(gameObject, destroyDelay);
    }

#if UNITY_EDITOR
    // Draw topCollider in editor for debugging
    private void OnDrawGizmosSelected()
    {
        if (topCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(topCollider.bounds.center, topCollider.bounds.size);
        }
    }
#endif
}
