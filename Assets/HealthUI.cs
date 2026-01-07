using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image healthPrefab;
    public Sprite fullHealthSprite;
    public Sprite emptyHealthSprite;
    
    private List<Image> healthpoints = new List<Image>();

    public void SetMaxHealthpoints(int maxHealth)
    {
        foreach(Image health in healthpoints)
        {
            Destroy(health.gameObject);
        }
        healthpoints.Clear();
        for (int i = 0; i < maxHealth; i++)
        {
            Image newHealthpoint = Instantiate(healthPrefab, transform);
            healthpoints.Add(newHealthpoint);
            newHealthpoint.color = Color.limeGreen;
            healthpoints.Add(newHealthpoint);
            healthpoints.Add(newHealthpoint);
        }
    }

        public void UpdateHealthpoints(int currentHealth)
        {
            for (int i = 0; i < healthpoints.Count; i++)
            {
                if (i < currentHealth)
                {
                    healthpoints[i].sprite = fullHealthSprite;
                    healthpoints[i].color = Color.limeGreen;
                }
                else
                {
                    healthpoints[i].sprite = emptyHealthSprite;
                    healthpoints[i].color = Color.white;
                }
            }
        }
    }
