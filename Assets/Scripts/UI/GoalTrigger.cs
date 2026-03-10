using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private LevelClearUI levelClearUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MedalProgress.SaveLevelResult(SceneManager.GetActiveScene().name, MedalCounterUI.GetCurrentMedals());
            MusicAudioManager.PlayLevelClearMusic();
            levelClearUI.Play();
        }
    }
}
