using UnityEngine;

public class ThrowableItem : MonoBehaviour
{
    public float lifetime = 4f;
    public bool useGravityOnThrow = true;
    public float gravityScaleOnThrow = 1f;
    public bool forceSolidCollider = true;
    public LayerMask wallLayers;

    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (forceSolidCollider && col != null)
            col.isTrigger = false;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction, float speed)
    {
        if (rb == null) return;
        if (useGravityOnThrow)
            rb.gravityScale = gravityScaleOnThrow;
        rb.linearVelocity = direction.normalized * speed;
    }

    void HandleHit(Collider2D other)
    {
        if (other == null) return;
        if (other.CompareTag("Player")) return;

        IStompable stompable = other.GetComponentInParent<IStompable>();
        if (stompable != null)
        {
            stompable.OnStomp();
            Destroy(gameObject);
            return;
        }

        BreakableWall wall = other.GetComponentInParent<BreakableWall>();
        if (wall != null)
        {
            wall.Break();
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger && (wallLayers == 0 || IsInLayerMask(other.gameObject.layer, wallLayers)))
        {
            Destroy(gameObject);
        }
    }

    static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }
}
