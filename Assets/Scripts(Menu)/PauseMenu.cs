using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;   // Assign your Pause Panel here
    public GameObject settingsMenuUI; // Assign your Settings Panel here
    public GameObject controlConfigUI; // Assign PauseButtonConfiguration here

    private bool isPaused;

    private FlightSchooledPlayerControls controls;

    void Awake()
    {
        controls = new FlightSchooledPlayerControls();
        InputBindingOverrides.ApplySavedOverrides(controls.asset);

        // Toggle pause with the Pause action
        controls.UI.Pause.performed += _ => TogglePause();
    }

    void OnEnable()
    {
        if (controls == null)
        {
            controls = new FlightSchooledPlayerControls();
            InputBindingOverrides.ApplySavedOverrides(controls.asset);
        }
        controls.Enable();
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (firstSelectedButton != null)
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        isPaused = false;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);
        if (controlConfigUI != null)
            controlConfigUI.SetActive(false);
        Time.timeScale = 1f;
    }

    // -------------------------
    // PAUSE LOGIC
    // -------------------------
    void TogglePause()
    {
        if (!isPaused)
            PauseGame();
        else
            ResumeGame();
    }

    void PauseGame()
    {
        isPaused = true;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);
        if (controlConfigUI != null)
            controlConfigUI.SetActive(false);
        Time.timeScale = 1f;

        // Reset Pause Input to avoid missed triggers
        controls.UI.Pause.Disable();
        controls.UI.Pause.Enable();
    }

    public void RestartLevel()
    {
        Debug.Log("[PauseMenu] RestartLevel called");
        isPaused = false;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);
        if (controlConfigUI != null)
            controlConfigUI.SetActive(false);
        Time.timeScale = 1f;

        // Reset Pause Input to avoid missed triggers
        controls.UI.Pause.Disable();
        controls.UI.Pause.Enable();

        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }

    // -------------------------
    // BUTTON FUNCTIONS
    // -------------------------
    public void ExitToSelectGame()
    {
        Time.timeScale = 1f; // unfreeze time
        MusicAudioManager.StopMusic();
        SceneManager.LoadScene("SelectgameScene"); // exact scene name
    }

    public GameObject firstSelectedButton;

    // -------------------------
    // SETTINGS (SAME SCENE)
    // -------------------------
    public void OpenSettings()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        if (controlConfigUI != null)
            controlConfigUI.SetActive(false);
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    public void OpenControlConfig()
    {
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);

        if (controlConfigUI != null)
        {
            EnsureControlConfigVisible();
            controlConfigUI.SetActive(true);
            controlConfigUI.transform.SetAsLastSibling();
        }
    }

    public void CloseControlConfig()
    {
        if (controlConfigUI != null)
            controlConfigUI.SetActive(false);

        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(true);
    }

    private void EnsureControlConfigVisible()
    {
        RectTransform configRect = controlConfigUI.GetComponent<RectTransform>();
        if (configRect != null)
            NormalizeRect(configRect, true);

        Transform panelTransform = controlConfigUI.transform.Find("Panel");
        if (panelTransform is RectTransform panelRect)
            NormalizeRect(panelRect, false);
    }

    private static void NormalizeRect(RectTransform rect, bool stretchFullParent)
    {
        if (rect == null)
            return;

        // Recover from accidentally zeroed scale.
        if (Mathf.Abs(rect.localScale.x) < 0.01f ||
            Mathf.Abs(rect.localScale.y) < 0.01f ||
            Mathf.Abs(rect.localScale.z) < 0.01f)
        {
            rect.localScale = Vector3.one;
        }

        if (stretchFullParent)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
    }

    public void QuitGame()
    {
        MusicAudioManager.StopMusic();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
