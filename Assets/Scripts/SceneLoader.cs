using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene loading

public class SceneLoader : MonoBehaviour
{
    // Call this to go to the Settings Scene
    public void LoadSettingsScene()
    {
        SceneManager.LoadScene("SettingsScene"); // Replace with your scene's name
    }

    // Optional: Go back to title or any other scene
    public void LoadStartScene()
    {
        SceneManager.LoadScene("StartScene"); // Replace with your title scene name
    }
}

