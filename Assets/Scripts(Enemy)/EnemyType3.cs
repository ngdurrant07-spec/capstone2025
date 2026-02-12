using UnityEngine;

public class EnemyType3 : MonoBehaviour, IStompable
{
    [Header("Behavior")]
    public float detectionRadius = 2.5f;
    public float popUpHeight = 1.2f;
    public float popUpSpeed = 6f;
    public float hideSpeed = 6f;
    public float stayUpTime = 1.0f;
    public float cooldownTime = 1.5f;

    [Header("Damage")]
    public int damage = 1;
    public Collider2D damageTrigger; // trigger collider for damage

    [Header("References")]
    public Transform player;

    Vector3 hiddenPos;
    Vector3 upPos;
    float stateTimer;
    enum State { Hidden, Rising, Up, Hiding, Cooldown }
    State state = State.Hidden;
    bool isDead;

    void Start()
    {
        hiddenPos = transform.position;
        upPos = hiddenPos + Vector3.up * popUpHeight;
        if (damageTrigger != null)
            damageTrigger.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        switch (state)
        {
            case State.Hidden:
                if (Vector2.Distance(transform.position, player.position) <= detectionRadius)
                    state = State.Rising;
                break;

            case State.Rising:
                MoveTowards(upPos, popUpSpeed);
                if (Vector3.Distance(transform.position, upPos) <= 0.01f)
                {
                    state = State.Up;
                    stateTimer = stayUpTime;
                    if (damageTrigger != null)
                        damageTrigger.enabled = true;
                }
                break;

            case State.Up:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    state = State.Hiding;
                break;

            case State.Hiding:
                MoveTowards(hiddenPos, hideSpeed);
                if (Vector3.Distance(transform.position, hiddenPos) <= 0.01f)
                {
                    state = State.Cooldown;
                    stateTimer = cooldownTime;
                    if (damageTrigger != null)
                        damageTrigger.enabled = false;
                }
                break;

            case State.Cooldown:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    if (Vector2.Distance(transform.position, player.position) <= detectionRadius)
                        state = State.Rising;
                    else
                        state = State.Hidden;
                }
                break;
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null)
            return;

        PlayerScript playerScript = other.GetComponentInParent<PlayerScript>();
        if (playerScript != null && !playerScript.CanTakeDamage())
            return;

        health.TakeDamage(damage);
    }

    // -------------------------
    // DEFEATED BY THROWABLE
    // -------------------------
    public void OnStomp()
    {
        if (isDead) return;
        isDead = true;

        SoundEffectManager.Play("Hit_Stomp");

        // Disable behavior and collisions
        enabled = false;
        if (damageTrigger != null)
            damageTrigger.enabled = false;
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>(true))
        col.enabled = false;

        Destroy(gameObject, 0.1f);
    }
}
