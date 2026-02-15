using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearMenuButton : MonoBehaviour
{

 [SerializeField] private string sceneName = "Levels";

     public void ClearMenuButtonClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}
