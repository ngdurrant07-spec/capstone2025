using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public Button retryButton;
    [Header("Animation")]
    public Animator gameOverAnimator;
    public string gameOverTrigger = "GameOverAnimation";

    void Start()
    {
        // Subscribe to the player death event
        PlayerHealth.OnPlayerDied += ShowGameOver;

        // Initially hide the game over screen
        gameObject.SetActive(false);

        // Set up button listener
        if (retryButton)
            retryButton.onClick.AddListener(RestartGame);
    }

    private void ShowGameOver()
    {
        // Show the game over screen
        gameObject.SetActive(true);

        if (gameOverAnimator != null && !string.IsNullOrEmpty(gameOverTrigger))
            gameOverAnimator.SetTrigger(gameOverTrigger);

        // Pause the game
        Time.timeScale = 0f;
    }

    private void RestartGame()
    {
        // Resume game time
        Time.timeScale = 1f;

        // Hide game over UI
        gameObject.SetActive(false);

        // Respawn at checkpoint if possible
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer();
            return;
        }

        // Fallback: reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        // Unsubscribe from the event
        PlayerHealth.OnPlayerDied -= ShowGameOver;
    }
}
