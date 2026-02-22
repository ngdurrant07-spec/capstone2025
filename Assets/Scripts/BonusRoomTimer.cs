using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BonusRoomTimer : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float defaultDurationSeconds = 20f;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool hideTextWhenIdle = true;
    [SerializeField] private string textPrefix = "";

    [Header("Events")]
    [SerializeField] private UnityEvent onStarted;
    [SerializeField] private UnityEvent onStopped;
    [SerializeField] private UnityEvent onExpired;

    private float remainingSeconds;
    private bool running;
    private bool expired;

    public bool IsRunning => running;
    public bool HasExpired => expired;
    public float RemainingSeconds => remainingSeconds;

    void Start()
    {
        remainingSeconds = Mathf.Max(0f, defaultDurationSeconds);

        if (hideTextWhenIdle && timerText != null)
            timerText.gameObject.SetActive(false);

        UpdateText();
    }

    void Update()
    {
        if (!running)
            return;

        float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        remainingSeconds = Mathf.Max(0f, remainingSeconds - delta);
        UpdateText();

        if (remainingSeconds > 0f)
            return;

        running = false;
        expired = true;
        onExpired?.Invoke();
    }

    public void StartCountdown()
    {
        StartCountdown(defaultDurationSeconds);
    }

    public void StartCountdown(float durationSeconds)
    {
        remainingSeconds = Mathf.Max(0f, durationSeconds);
        running = true;
        expired = false;

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        UpdateText();
        onStarted?.Invoke();
    }

    public void StopTimer()
    {
        if (!running)
            return;

        running = false;
        onStopped?.Invoke();
    }

    public void ResetTimer()
    {
        running = false;
        expired = false;
        remainingSeconds = Mathf.Max(0f, defaultDurationSeconds);

        if (hideTextWhenIdle && timerText != null)
            timerText.gameObject.SetActive(false);

        UpdateText();
    }

    public void HideTimerUI()
    {
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    public void AddExpiredListener(UnityAction listener)
    {
        onExpired.AddListener(listener);
    }

    public void RemoveExpiredListener(UnityAction listener)
    {
        onExpired.RemoveListener(listener);
    }

    private void UpdateText()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
        float seconds = remainingSeconds % 60f;
        timerText.SetText($"{textPrefix}{minutes:00}:{seconds:00.00}");
    }
}
