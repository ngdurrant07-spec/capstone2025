using UnityEngine;

public class BossBtn : MonoBehaviour
{
    [SerializeField] private string sceneName = "Boss";

    public void BossBtnClick()
    {
        SceneController.LoadScene(sceneName);
    }
}
