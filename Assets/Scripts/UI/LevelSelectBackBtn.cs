using UnityEngine;

public class LevelSelectBackBtn : MonoBehaviour
{
    [SerializeField] private string sceneName = "SelectGameScene";

    public void LevelSelectBackBtnClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
