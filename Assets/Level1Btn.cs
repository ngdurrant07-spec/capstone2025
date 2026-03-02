using UnityEngine;

public class Level1Btn : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level1";

    public void Level1BtnClick()
    {
        SceneController.LoadScene(sceneName);
    }
}
