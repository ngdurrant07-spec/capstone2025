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
}
