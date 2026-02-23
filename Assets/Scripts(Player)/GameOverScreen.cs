using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public Button retryButton;
    [Header("Animation")]
    public Animator gameOverAnimator;
    public string gameOverTrigger = "GameOverAnimation";
    public string gameOverStateName = "GameOverAnimation";

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
        // Bonus-room deaths should return through the bonus door flow instead of normal checkpoint/game over.
        foreach (BonusRoomDoor bonusDoor in FindObjectsByType<BonusRoomDoor>(FindObjectsSortMode.None))
        {
            if (bonusDoor != null && bonusDoor.TryHandleActivePlayerDeath())
                return;
        }

        // Show the game over screen
        gameObject.SetActive(true);

        PlayGameOverAnimation();

        // Pause the game
        Time.timeScale = 0f;
    }

    private void PlayGameOverAnimation()
    {
        if (gameOverAnimator == null)
            return;

        // Trigger path: use when Animator Controller defines a trigger parameter.
        if (!string.IsNullOrEmpty(gameOverTrigger) && HasTriggerParameter(gameOverAnimator, gameOverTrigger))
        {
            gameOverAnimator.SetTrigger(gameOverTrigger);
            return;
        }

        // Fallback path: directly play a state for controllers without parameters.
        if (!string.IsNullOrEmpty(gameOverStateName))
            gameOverAnimator.Play(gameOverStateName, 0, 0f);
    }

    private static bool HasTriggerParameter(Animator animator, string parameterName)
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Trigger)
                return true;
        }
        return false;
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
