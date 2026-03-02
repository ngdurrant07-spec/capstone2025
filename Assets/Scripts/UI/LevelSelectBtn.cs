using UnityEngine;

public class LevelSelectBtn : MonoBehaviour
{
    [SerializeField] private string sceneName = "Levels";

    public void LevelSelectButtonClick()
    {
        SceneController.LoadScene(sceneName);
    }
}
