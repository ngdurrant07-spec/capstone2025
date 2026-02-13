using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level1";

    public void NextButton_1Click()
    {
        SceneManager.LoadScene(sceneName);
    }
}
