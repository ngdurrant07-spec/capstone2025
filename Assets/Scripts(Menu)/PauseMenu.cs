using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;   // Assign your Pause Panel here
    public GameObject settingsMenuUI; // Assign your Settings Panel here

    private bool isPaused;

    private FlightSchooledPlayerControls controls;

    void Awake()
    {
        controls = new FlightSchooledPlayerControls();

        // Toggle pause with the Pause action
        controls.UI.Pause.performed += _ => TogglePause();
    }

    void OnEnable()
    {
        if (controls == null)
            controls = new FlightSchooledPlayerControls();
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
}
