using UnityEngine;

public class EnemyType2 : MonoBehaviour, IStompable
{
    [Header("Movement")]
    public float flySpeed = 2f;
    public float directionChangeTime = 2f;

    private float timer;
    private int direction = 1; // 1 = right, -1 = left
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    [Header("Death")]
    public float deathPopForce = 8f;
    public float deathFallGravity = 3f;
    public float deathLifetime = 2f;

    bool isDead;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;     // flying enemy
        rb.freezeRotation = true;

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        anim.ResetTrigger("Fly");
    }

    void Start()
    {
        timer = directionChangeTime;
    }

    void Update()
    {
        Fly();
    }

    void Fly()
    {
        timer -= Time.deltaTime;

        rb.linearVelocity = new Vector2(direction * flySpeed, 0f);

        if (timer <= 0f)
        {
            ChangeDirection();
        }
    }

    void ChangeDirection()
    {
        direction *= -1;
        timer = directionChangeTime;

        // Flip sprite
        sprite.flipX = direction > 0;
    }

    // -------------------------
    // STOMPED BY PLAYER
    // -------------------------
    public void OnStomp()
{
    if (isDead) return;
    isDead = true;
    SoundEffectManager.Play("Hit_Stomp");

    // Disable AI logic
    enabled = false;

    // Stop movement
    rb.linearVelocity = Vector2.zero;

    // Physics fall
    rb.bodyType = RigidbodyType2D.Dynamic;
    rb.gravityScale = deathFallGravity;

    // Pop upward
    rb.AddForce(Vector2.up * deathPopForce, ForceMode2D.Impulse);

    // Disable colliders so it can't hurt or block player
    foreach (Collider2D col in GetComponents<Collider2D>())
        col.enabled = false;

        Animator anim = GetComponent<Animator>();
    if (anim != null)
        anim.SetTrigger("Die");

    Destroy(gameObject, deathLifetime);
}

}

