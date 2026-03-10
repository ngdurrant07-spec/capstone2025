using UnityEngine;

public class Level3Button : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level3";

    public void Level3BtnClick()
    {
        SceneController.LoadScene(sceneName);
    }
}
