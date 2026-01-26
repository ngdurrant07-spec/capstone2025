using UnityEngine;

public class PlayerStomp : MonoBehaviour
{
    public Rigidbody2D rb;
    public float bounceForce = 12f;

  private void OnTriggerEnter2D(Collider2D collision)
{
    if (!collision.CompareTag("Enemy")) return;

    Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
    if (rb == null || rb.linearVelocity.y >= -0.1f)
        return;

    IStompable stompable = collision.GetComponent<IStompable>();
    if (stompable != null)
    {
        stompable.OnStomp();

        // Bounce player
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f); // your bounce
    }
}
}



