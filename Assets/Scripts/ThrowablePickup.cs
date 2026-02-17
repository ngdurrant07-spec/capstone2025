using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowablePickup : MonoBehaviour
{
    public bool respawnOnThrow = true;
    public float respawnDelay = 2f;
    public GameObject throwablePrefab;

    Collider2D col;
    SpriteRenderer sr;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Keyboard.current == null || !Keyboard.current.eKey.isPressed)
            return;

        PlayerScript player = other.GetComponentInParent<PlayerScript>();
        if (player == null)
            return;

        if (!player.TryPickupThrowable(throwablePrefab))
            return;

        if (respawnOnThrow)
        {
            if (col != null) col.enabled = false;
            if (sr != null) sr.enabled = false;
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Respawn()
    {
        if (col != null) col.enabled = true;
        if (sr != null) sr.enabled = true;
    }
}
