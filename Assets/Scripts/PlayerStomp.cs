using UnityEngine;

public class PlayerStomp : MonoBehaviour
{
    public Rigidbody2D rb;
    public float bounceForce = 12f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("EnemyType1"))
            return;

        // Check if we hit the enemy from above
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && rb.linearVelocity.y <= 0f)
            {
                // Bounce player up
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);

                // Kill enemy
                Destroy(collision.gameObject);
                return;
            }
        }
    }
}

