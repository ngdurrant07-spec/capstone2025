using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("References")]
    public Transform player;
    public PlayerScript playerScript;
    public PlayerHealth playerHealth;

    Vector3 currentCheckpoint;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (playerScript == null && player != null)
            playerScript = player.GetComponent<PlayerScript>();
        if (playerHealth == null && player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (player != null)
            currentCheckpoint = player.position;
    }

    public void SetCheckpoint(Vector3 position)
    {
        currentCheckpoint = position;
    }

    public void RespawnPlayer()
    {
        if (player == null)
            return;

        if (playerScript != null)
        {
            playerScript.RespawnAt(currentCheckpoint);
        }
        else
        {
            player.position = currentCheckpoint;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        if (playerHealth != null)
            playerHealth.ResetToFull();
    }
}
