using UnityEngine;

public class RollHitbox : MonoBehaviour
{
    public PlayerScript player;  // reference to PlayerScript

    void Awake()
    {
        if (player == null)
            player = GetComponentInParent<PlayerScript>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (player == null)
            return;

        if (player.currentState != PlayerScript.PlayerState.Rolling)
            return;

        IStompable stompable = other.GetComponentInParent<IStompable>();
        if (stompable == null)
            return;

        // Always damage enemy while rolling
        stompable.OnStomp();

        // EnemyType4 always bounces the player off on roll impact.
        if (stompable is EnemyType4)
        {
            float rollDir = Mathf.Sign(player.linearVelocity.x);
            if (rollDir == 0f)
                rollDir = Mathf.Sign(player.transform.localScale.x);
            if (rollDir == 0f)
                rollDir = 1f;

            player.linearVelocity = new Vector2(
                -rollDir * player.rollSpeed,
                player.stompBounceForce
            );
            return;
        }

        // Optional bounce only if falling
        if (player.linearVelocity.y < -0.1f)
        {
            player.linearVelocity = new Vector2(
                player.linearVelocity.x,
                player.stompBounceForce
            );
        }
        else
        {
            // Rolling on ground -> speed boost only
            player.hitSpeedBoostTimer = player.hitSpeedBoostDuration;
        }
    }
}
