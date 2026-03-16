using UnityEngine;

public class WinBackButton : MonoBehaviour
{
    [SerializeField] private string sceneName = "Levels";

    public void WinBackButtonClick()
    {
        MusicAudioManager.StopMusic();
        SceneController.LoadScene(sceneName);
    }
}
