using UnityEngine;

public class Level1Btn : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level1";

    public void Level1BtnClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
