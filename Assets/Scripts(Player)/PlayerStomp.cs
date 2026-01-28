using UnityEngine;

public class StompHitbox : MonoBehaviour
{
    public float bounceForce = 10f;
    private Rigidbody2D playerRb;
    private PlayerScript playerScript;

    private void Start()
    {
        playerRb = GetComponentInParent<Rigidbody2D>();
        playerScript = GetComponentInParent<PlayerScript>();
        if (playerRb == null)
            Debug.LogWarning("Player Rigidbody2D not found on parent!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object implements IStompable
        IStompable stompable = other.GetComponent<IStompable>();
        if (stompable != null)
        {
            // Tell enemy it was stomped
            stompable.OnStomp();

            // Bounce player up
            if (playerRb != null && (playerScript == null || playerScript.currentState != PlayerScript.PlayerState.GroundPounding))
            {
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
            }
        }
    }
}

