using UnityEngine;

public class RollHitbox : MonoBehaviour
{
    public PlayerScript player;  // reference to PlayerScript

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(player.currentState != PlayerScript.PlayerState.Rolling)
            return;

        IStompable stompable = other.GetComponent<IStompable>();
        if(stompable != null)
        {
            // Only bounce if falling (y < 0)
            if (player.linearVelocity.y < 0f)
            {
                stompable.OnStomp();
                player.linearVelocity = new Vector2(player.linearVelocity.x, player.stompBounceForce); // optional bounce
            }
            else
            {
                // Just give speed boost, stay grounded
                player.hitSpeedBoostTimer = player.hitSpeedBoostDuration;
            }
        }
    }
}


