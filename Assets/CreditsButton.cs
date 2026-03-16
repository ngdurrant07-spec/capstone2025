using UnityEngine;

public class CreditsButton : MonoBehaviour
{
    [SerializeField] private string sceneName = "Credits";

    public void CreditsButtonClick()
    {
        SceneController.LoadScene(sceneName);
    }
}
