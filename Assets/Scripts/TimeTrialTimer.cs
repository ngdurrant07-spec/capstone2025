using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TimeTrialTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool hideTextUntilStart = true;

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool startOnce = true;
    [SerializeField] private bool hideTriggerOnStart = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onTrialStarted;
    [SerializeField] private UnityEvent onTrialStopped;

    private float elapsedSeconds;
    private bool running;
    private Collider2D triggerCollider;
    private Renderer[] cachedRenderers;

    public float ElapsedSeconds => elapsedSeconds;
    public bool IsRunning => running;

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
    }

    void Start()
    {
        if (hideTextUntilStart && timerText != null)
            timerText.gameObject.SetActive(false);

        UpdateText();
    }

    void OnEnable()
    {
        PlayerHealth.OnPlayerDied += HandlePlayerDied;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDied -= HandlePlayerDied;
    }

    void Update()
    {
        if (!running) return;

        elapsedSeconds += Time.deltaTime;
        UpdateText();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        bool wasRunning = running;
        running = true;
        if (timerText != null)
            timerText.gameObject.SetActive(true);

        if (!wasRunning)
        {
            onTrialStarted?.Invoke();
            SoundEffectManager.Play("CollectTimeTrial");
        }

        if (startOnce)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        if (hideTriggerOnStart)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }
    }

    public void ResetTimer()
    {
        running = false;
        elapsedSeconds = 0f;

        if (hideTextUntilStart && timerText != null)
            timerText.gameObject.SetActive(false);

        if (startOnce && triggerCollider != null)
            triggerCollider.enabled = true;

        if (hideTriggerOnStart && cachedRenderers != null)
        {
            foreach (Renderer cachedRenderer in cachedRenderers)
            {
                if (cachedRenderer != null)
                    cachedRenderer.enabled = true;
            }
        }

        UpdateText();
    }

    public void StopTimer()
    {
        bool wasRunning = running;
        running = false;

        if (wasRunning)
            onTrialStopped?.Invoke();
    }

    private void UpdateText()
    {
        if (timerText == null) return;

        timerText.SetText(TimeTrialProgress.FormatTime(elapsedSeconds));
    }

    private void HandlePlayerDied()
    {
        if (!running)
            return;

        ResetTimer();
    }
}
