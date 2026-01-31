using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseSettings : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Scene to return to when leaving settings (e.g., Level1, GametestScene).")]
    public string gameSceneName;

    [Header("Options")]
    public bool resumeTimeOnBack = true;

    public void BackToGame()
    {
        if (resumeTimeOnBack)
            Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        if (PlayerPrefs.HasKey("LastGameScene"))
        {
            SceneManager.LoadScene(PlayerPrefs.GetString("LastGameScene"));
            return;
        }

        Debug.LogWarning("[PauseSettings] No game scene configured. Set gameSceneName in the Inspector.");
    }

    public static void SaveReturnScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            PlayerPrefs.SetString("LastGameScene", sceneName);
    }
}
