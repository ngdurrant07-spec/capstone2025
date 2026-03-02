using UnityEngine;
public class StartMenuController : MonoBehaviour
{
    public void OnStartClick()
    {
       SceneController.LoadScene("SelectGameScene");
    }

    public void OnExitClick()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
