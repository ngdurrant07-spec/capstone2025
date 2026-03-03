using UnityEngine;

public class AirDashPad : MonoBehaviour
{
    [Header("Boost")]
    public float forwardBoost = 18f;
    public float upwardBoost = 5f;

    [Header("Glide")]
    public float liftRestore = 0.6f;
    public float gravityLockTime = 0.12f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerScript player = other.GetComponent<PlayerScript>();
        if (player == null)
            player = other.GetComponentInParent<PlayerScript>();
        if (player == null) return;
        if (!player.IsGlidingActive) return;

        float dir = Mathf.Sign(player.linearVelocity.x);
        if (Mathf.Approximately(dir, 0f))
            dir = Mathf.Sign(player.FacingDirection);
        if (Mathf.Approximately(dir, 0f))
            dir = 1f;

        Vector2 boost = new Vector2(dir * forwardBoost, upwardBoost);

        player.AirBoost(boost, liftRestore, gravityLockTime);
    }
}
