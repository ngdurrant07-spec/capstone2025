using UnityEngine;

public class CreditsNextButton : MonoBehaviour
{
    [SerializeField] private string sceneName = "Credits_2";

    public void CreditsNextButtonClick()
    {
        SceneController.LoadScene(sceneName);
    }

}
