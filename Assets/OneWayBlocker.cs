using UnityEngine;

public class OneWayBlocker : MonoBehaviour
{
    private bool enabledBlock = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabledBlock && other.CompareTag("Player"))
        {
            // enable the collider so player can't go back
            GetComponent<Collider2D>().isTrigger = false;
            enabledBlock = true;
        }
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }
}
