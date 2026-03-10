using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    [SerializeField] private string sceneName = "Levels";
    [SerializeField] private string[] progressionOrder =
    {
        "Tutorial",
        "Level1",
        "Level2",
        "Level3",
        "Boss"
    };

    public void NextButton_1Click()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        string currentSceneName = activeScene.name;
        int currentOrderIndex = System.Array.IndexOf(progressionOrder, currentSceneName);
        if (currentOrderIndex >= 0 && currentOrderIndex + 1 < progressionOrder.Length)
        {
            SceneManager.LoadScene(progressionOrder[currentOrderIndex + 1]);
            return;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Debug.LogWarning($"NextLevel: no next scene configured after '{currentSceneName}' and no fallback scene is set.");
    }
}
