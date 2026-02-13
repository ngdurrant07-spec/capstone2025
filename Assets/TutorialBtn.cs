using UnityEngine;

public class TutorialBtn : MonoBehaviour
{
    [SerializeField] private string sceneName = "Tutorial";

    public void TutorialBtnClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
