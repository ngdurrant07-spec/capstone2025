using UnityEngine;

public class TutorialBtn : MonoBehaviour
{
    [SerializeField] private string sceneName = "Tutorial";

    public void TutorialBtnClick()
    {
        SceneController.LoadScene(sceneName);
    }
}
