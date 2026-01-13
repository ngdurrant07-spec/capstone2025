using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SettingsButtonClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SettingsScene");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
