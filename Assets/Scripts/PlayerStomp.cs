using UnityEngine;

public class StompHitbox : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D playerRb;       // Drag in Player Rigidbody2D
    public float bounceForce = 12f;    // How high player bounces after stomping

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger if player is falling
        if (playerRb.linearVelocity.y > 0f)
            return;

        // Check if the object is stompable
        IStompable stompable = other.GetComponent<IStompable>();
        if (stompable != null)
        {
            // Optional: ensure player is above enemy
            if (playerRb.transform.position.y > other.bounds.max.y)
            {
                // Bounce the player up
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);

                // Tell enemy it was stomped
                stompable.OnStomp();
            }
        }
    }
}


