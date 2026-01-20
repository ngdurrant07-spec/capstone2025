using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Movement")]
    public float flySpeed = 3f;
    public float stopDistance = 0.5f;

    [Header("Hover")]
    public float bobAmplitude = 0.3f;
    public float bobSpeed = 3f;

    private Rigidbody2D rb;
    private Vector2 startOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startOffset = transform.position;
    }

    void FixedUpdate()
{
    if (target == null) return;

    Vector2 targetPos = target.position;
    Vector2 desired = (targetPos - rb.position).normalized * flySpeed;
    rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desired, 5f * Time.fixedDeltaTime);
}

}
