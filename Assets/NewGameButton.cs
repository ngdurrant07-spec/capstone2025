using UnityEngine;

public class NewGameButton : MonoBehaviour
{
    [SerializeField] private string tutorialSceneName = "Tutorial";
    [SerializeField] private string[] scenesToReset =
    {
        "Tutorial",
        "Level1",
        "Level2",
        "Level3",
        "GametestScene"
    };

    public void NewGameButtonClick()
    {
        MedalProgress.ClearSavedProgress(scenesToReset);
        TimeTrialAreaLock.ClearSavedUnlocks(scenesToReset);

        PlayerPrefs.DeleteKey("LastGameScene");
        PlayerPrefs.Save();

        MedalCounterUI.ResetMedals();
        SceneController.LoadScene(tutorialSceneName);
    }
}
