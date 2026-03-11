using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GoalArea : MonoBehaviour
{
    [Header("Trigger")]
    public bool triggerOnce = true;
    public string playerTag = "Player";

    [Header("Scene Load")]
    public bool loadScene = true;
    public bool loadNextSceneByIndex = true;
    public string sceneName;
    public float loadDelay = 0.5f;

    [Header("UI")]
    public LevelClearUI levelClearUI;

    [Header("Player Stop")]
    public bool stopPlayerOnGoal = true;
    public bool disablePlayerController = true;
    public bool freezePlayerRigidbody = true;

    [Header("Events")]
    public UnityEvent onGoalReached;

    bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnce && triggered)
            return;
        if (!other.CompareTag(playerTag))
            return;

        triggered = true;
        if (stopPlayerOnGoal)
            StopPlayer(other);

        MedalProgress.SaveLevelResult(SceneManager.GetActiveScene().name, MedalCounterUI.GetCurrentMedals());
        MusicAudioManager.PlayLevelClearMusic();



        onGoalReached?.Invoke();
        if (levelClearUI != null)
            levelClearUI.Play();

        if (loadScene)
            StartCoroutine(LoadSceneAfterDelay());
    }

    IEnumerator LoadSceneAfterDelay()
    {
        if (loadDelay > 0f)
            yield return new WaitForSeconds(loadDelay);

        if (loadNextSceneByIndex)
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextIndex);
            yield break;
        }

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    void StopPlayer(Collider2D playerCollider)
    {
        Rigidbody2D playerRb = playerCollider.attachedRigidbody;
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            if (freezePlayerRigidbody)
                playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (!disablePlayerController)
            return;

        PlayerScript playerController = playerCollider.GetComponent<PlayerScript>();
        if (playerController == null)
            playerController = playerCollider.GetComponentInParent<PlayerScript>();

        if (playerController != null)
            playerController.enabled = false;
    }
}
