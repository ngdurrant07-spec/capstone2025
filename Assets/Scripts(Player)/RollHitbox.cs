using UnityEngine;

public class RollHitbox : MonoBehaviour
{
    public PlayerScript player;  // reference to PlayerScript

private void OnTriggerEnter2D(Collider2D other)
{
    if (player.currentState != PlayerScript.PlayerState.Rolling)
        return;

    IStompable stompable = other.GetComponent<IStompable>();
    if (stompable == null)
        return;

    // ONLY bounce if falling
    if (player.linearVelocity.y < -0.1f)
    {
        stompable.OnStomp();
        player.linearVelocity = new Vector2(
            player.linearVelocity.x,
            player.stompBounceForce
        );
    }
    else
    {
        // Rolling on ground → speed boost only
        player.hitSpeedBoostTimer = player.hitSpeedBoostDuration;
    }
}


        }


