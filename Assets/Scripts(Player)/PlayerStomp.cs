using UnityEngine;

public class StompHitbox : MonoBehaviour
{
    public float bounceForce = 10f;
    private Rigidbody2D playerRb;

    private void Start()
    {
        playerRb = GetComponentInParent<Rigidbody2D>();
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
            if (playerRb != null)
            {
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
            }
        }
    }
}


