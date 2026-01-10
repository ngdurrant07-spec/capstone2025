using System;
using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public int healAmount = 1;

    public static event Action<int> OnHealthCollect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        OnHealthCollect?.Invoke(healAmount);
        Destroy(gameObject);
    }
}

