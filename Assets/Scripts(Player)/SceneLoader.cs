using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene loading

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string settingsSceneName = "SettingsScene";
    [SerializeField] private string startSceneName = "StartScene";
    [SerializeField] private string buttonConfigSceneName = "ButtonConfigScene";
    [SerializeField] private string creditsMainMenuSceneName = "CreditsMainMenu";



    // Call this to go to the Settings Scene
    public void LoadSettingsScene()
    {
        TryLoadScene(settingsSceneName);
    }

    // Optional: Go back to title or any other scene
    public void LoadStartScene()
    {
        TryLoadScene(startSceneName);
    }

    public void LoadButtonConfigScene()
    {
        TryLoadScene(buttonConfigSceneName);
    }

    public void LoadCreditsMainMenuScene()
    {
        TryLoadScene(creditsMainMenuSceneName);
    }

    private void TryLoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Scene name is empty on SceneLoader.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Scene '{sceneName}' is not available to load. " +
                "Add it to File -> Build Profiles -> Scene List (or Shared Scene List), " +
                "or load the AssetBundle containing this scene before calling SceneManager.LoadScene.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
