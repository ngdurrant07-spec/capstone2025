using UnityEngine;

public class ThrowableItem : MonoBehaviour
{
    public float lifetime = 4f;
    public bool useGravityOnThrow = true;
    public float gravityScaleOnThrow = 1f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
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
