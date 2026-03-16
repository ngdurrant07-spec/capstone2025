using UnityEngine;

public class CreditsBackBtn : MonoBehaviour
{
    [SerializeField] private string sceneName = "StartScene";

    public void CreditsButtonClick()
    {
        SceneController.LoadScene(sceneName);
    }
}
