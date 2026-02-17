using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image heartPrefab;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    private List<Image> hearts = new List<Image>();

    public void SetMaxHearts(int maxHearts)
    {
        foreach (Image heart in hearts)
        {
            if (heart != null)
                Destroy(heart.gameObject);
        }
        hearts.Clear();

        if (heartPrefab == null)
        {
            Debug.LogWarning("[HealthUI] heartPrefab is not assigned.");
            return;
        }

        for (int i = 0; i < maxHearts; i++)
        {
            Image newHeart;
            try
            {
                newHeart = Instantiate(heartPrefab, transform);
            }
            catch (UnityException ex)
            {
                Debug.LogWarning($"[HealthUI] Could not instantiate heartPrefab: {ex.Message}");
                break;
            }

            if (newHeart == null)
                break;

            newHeart.sprite = fullHeartSprite;
            newHeart.color = Color.red;
            hearts.Add(newHeart);
        }
    }

    public void UpdateHearts(int currentHearts)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentHearts)
            {
                if (hearts[i] != null)
                {
                    hearts[i].sprite = fullHeartSprite;
                    hearts[i].color = Color.red;
                }
            }
            else
            {
                if (hearts[i] != null)
                {
                    hearts[i].sprite = emptyHeartSprite;
                    hearts[i].color = Color.white;
                }
            }
        }
    }
}
