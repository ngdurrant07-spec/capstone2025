using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private LevelClearUI levelClearUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MusicAudioManager.PlayLevelClearMusic();
            levelClearUI.Play();
        }
    }
}
