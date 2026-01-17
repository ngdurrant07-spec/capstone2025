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
            stompable.OnStomped();

            // Add momentum boost
            player.hitSpeedBoostTimer = player.hitSpeedBoostDuration;
        }

    }
}

