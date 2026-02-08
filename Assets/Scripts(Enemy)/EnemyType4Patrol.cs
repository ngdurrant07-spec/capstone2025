using UnityEngine;

public class EnemyType4Patrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform leftPoint;
    public Transform rightPoint;
    public float patrolSpeed = 2f;
    public float arriveThreshold = 0.05f;
    public float waitTime = 0f;

    [Header("Edge Check")]
    public bool preventLedgeFall = true;
    public Transform edgeCheck;
    public float edgeCheckForwardOffset = 0.3f;
    public float edgeCheckDistance = 0.6f;
    public LayerMask groundLayer;

    [Header("Rush")]
    public Transform player;
    public float rushDistance = 2f;
    public float rushSpeed = 6f;

    Rigidbody2D rb;
    SpriteRenderer sprite;
    float waitTimer;
    int direction = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (leftPoint == null || rightPoint == null)
            return;

        if (ShouldRush())
        {
            waitTimer = 0f;
            RushTowardPlayer();
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (preventLedgeFall && edgeCheck != null)
        {
            if (!IsGroundAhead(direction))
                direction *= -1;
        }

        Vector3 target = direction > 0 ? rightPoint.position : leftPoint.position;
        Vector2 current = transform.position;
        Vector2 next = Vector2.MoveTowards(current, target, patrolSpeed * Time.fixedDeltaTime);

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = next;

        if (sprite != null)
            sprite.flipX = direction < 0;

        if (Vector2.Distance(next, target) <= arriveThreshold)
        {
            direction *= -1;
            waitTimer = waitTime;
        }
    }

    bool ShouldRush()
    {
        if (player == null)
            return false;

        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= rushDistance;
    }

    void RushTowardPlayer()
    {
        Vector2 current = transform.position;
        float playerX = player.position.x;
        direction = playerX >= current.x ? 1 : -1;

        if (preventLedgeFall && edgeCheck != null)
        {
            if (!IsGroundAhead(direction))
            {
                if (rb != null)
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
        }

        Vector2 rushTarget = new Vector2(playerX, current.y);
        Vector2 next = Vector2.MoveTowards(current, rushTarget, rushSpeed * Time.fixedDeltaTime);

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = next;

        if (sprite != null)
            sprite.flipX = direction < 0;
    }

    bool IsGroundAhead(float moveDir)
    {
        Vector2 origin = (Vector2)edgeCheck.position + Vector2.right * edgeCheckForwardOffset * moveDir;
        return Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (edgeCheck == null) return;
        Gizmos.color = Color.yellow;
        Vector2 origin = (Vector2)edgeCheck.position + Vector2.right * edgeCheckForwardOffset * direction;
        Gizmos.DrawLine(origin, origin + Vector2.down * edgeCheckDistance);
    }
}
