using UnityEngine;

public class EnemyType4Patrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform leftPoint;
    public Transform rightPoint;
    public float patrolSpeed = 2f;
    public float arriveThreshold = 0.05f;
    public float waitTime = 0f;
    public bool lockPatrolPointsInWorld = true;

    [Header("Edge Check")]
    public bool preventLedgeFall = true;
    public Transform edgeCheck;
    public float edgeCheckForwardOffset = 0.3f;
    public float edgeCheckDistance = 0.6f;
    public LayerMask groundLayer;

    [Header("Chase")]
    public Transform player;
    public float chaseSpeed = 6f;

    Rigidbody2D rb;
    SpriteRenderer sprite;
    float waitTimer;
    int direction = 1;
    bool isChasing;
    Vector2 leftPatrolTarget;
    Vector2 rightPatrolTarget;
    bool hasCachedPatrolTargets;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        CachePatrolTargets();
    }

    void Start()
    {
        CachePatrolTargets();
        InitializePatrolDirection();

        if (rb != null)
            rb.WakeUp();
    }

    void CachePatrolTargets()
    {
        if (leftPoint == null || rightPoint == null)
        {
            hasCachedPatrolTargets = false;
            return;
        }

        leftPatrolTarget = leftPoint.position;
        rightPatrolTarget = rightPoint.position;
        hasCachedPatrolTargets = true;
    }

    void InitializePatrolDirection()
    {
        if (!hasCachedPatrolTargets)
            return;

        Vector2 current = transform.position;
        float leftDistance = Vector2.Distance(current, leftPatrolTarget);
        float rightDistance = Vector2.Distance(current, rightPatrolTarget);

        // If spawned close to one end point, start by moving away from it.
        if (rightDistance <= arriveThreshold && leftDistance > arriveThreshold)
            direction = -1;
        else if (leftDistance <= arriveThreshold && rightDistance > arriveThreshold)
            direction = 1;
    }

    public void ActivateChase(Transform target)
    {
        isChasing = true;
        if (target != null)
            player = target;
    }

    void FixedUpdate()
    {
        if (isChasing)
        {
            if (player == null)
                FindPlayerByTag();

            if (player != null)
            {
                waitTimer = 0f;
                ChaseTowardPlayer();
                return;
            }

            // If chase was activated before a player exists, resume patrol instead of freezing.
            isChasing = false;
        }

        if (!hasCachedPatrolTargets && (leftPoint == null || rightPoint == null))
            return;

        if (!hasCachedPatrolTargets)
            CachePatrolTargets();

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

        Vector2 target;
        if (lockPatrolPointsInWorld && hasCachedPatrolTargets)
            target = direction > 0 ? rightPatrolTarget : leftPatrolTarget;
        else
            target = direction > 0 ? (Vector2)rightPoint.position : (Vector2)leftPoint.position;

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

    void FindPlayerByTag()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    void ChaseTowardPlayer()
    {
        if (player == null)
            return;

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
        Vector2 next = Vector2.MoveTowards(current, rushTarget, chaseSpeed * Time.fixedDeltaTime);

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
