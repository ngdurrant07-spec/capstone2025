using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float fallWait = 1.5f;
    public float destroyWait = 2f;

    private bool hasFallen = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Start static
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasFallen && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Fall());
        }
    }

    private IEnumerator Fall()
    {
        hasFallen = true;
        yield return new WaitForSeconds(fallWait);

        rb.bodyType = RigidbodyType2D.Dynamic; // Let gravity drop it
        yield return new WaitForSeconds(destroyWait);

        Destroy(gameObject);
    }
}
