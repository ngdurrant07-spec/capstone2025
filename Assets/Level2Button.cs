using UnityEngine;

public class Level2Button : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level2";

    public void Level2BtnClick()
    {
        SceneController.LoadScene(sceneName);
    }
}

