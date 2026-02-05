using TMPro;
using UnityEngine;

public class TimeTrialTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool hideTextUntilStart = true;

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool startOnce = true;
    [SerializeField] private bool hideTriggerOnStart = true;

    private float elapsedSeconds;
    private bool running;

    void Start()
    {
        if (hideTextUntilStart && timerText != null)
            timerText.gameObject.SetActive(false);

        UpdateText();
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

        running = true;
        if (timerText != null)
            timerText.gameObject.SetActive(true);

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
        elapsedSeconds = 0f;
        UpdateText();
    }

    public void StopTimer()
    {
        running = false;
    }

    private void UpdateText()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
        float seconds = elapsedSeconds % 60f;
        timerText.SetText($"{minutes:00}:{seconds:00.00}");
    }
}
