using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;   // Assign your Pause Panel here

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
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        // Reset Pause Input to avoid missed triggers
        controls.UI.Pause.Disable();
        controls.UI.Pause.Enable();
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
}
