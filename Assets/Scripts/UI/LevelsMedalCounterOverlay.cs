using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsMedalCounterOverlay : MonoBehaviour
{
    private const string LevelsSceneName = "Levels";

    [Header("UI")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_FontAsset fontOverride;
    [SerializeField] private string label = "Total Medals: ";

    [Header("Display")]
    [SerializeField] private int medalLimit = 8;
    [SerializeField] private bool showLastClear = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryCreateOverlay(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreateOverlay(scene);
    }

    private static void TryCreateOverlay(Scene scene)
    {
        if (scene.name != LevelsSceneName)
            return;

        LevelsMedalCounterOverlay existingOverlay = FindFirstObjectByType<LevelsMedalCounterOverlay>();
        if (existingOverlay != null)
        {
            existingOverlay.ApplyBottomCenterLayout();
            existingOverlay.Refresh();
            return;
        }

        Canvas canvas = FindTargetCanvas(scene);
        if (canvas == null)
            return;

        GameObject overlayObject = new GameObject("LevelsMedalCounterOverlay");
        overlayObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = overlayObject.AddComponent<RectTransform>();

        TextMeshProUGUI text = overlayObject.AddComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset != null
            ? TMP_Settings.defaultFontAsset
            : Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts & Materials/LiberationSans SDF");
        text.fontSize = 28f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        LevelsMedalCounterOverlay overlay = overlayObject.AddComponent<LevelsMedalCounterOverlay>();
        overlay.counterText = text;
        overlay.ApplyBottomCenterLayout();
        overlay.Refresh();
    }

    private static Canvas FindTargetCanvas(Scene scene)
    {
        Canvas fallback = null;

        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas == null || canvas.gameObject.scene != scene || !canvas.isRootCanvas)
                continue;

            if (canvas.name.Contains("LevelSelect"))
                return canvas;

            if (fallback == null)
                fallback = canvas;
        }

        return fallback;
    }

    private void OnEnable()
    {
        ApplyBottomCenterLayout();
        Refresh();
    }

    private void ApplyBottomCenterLayout()
    {
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, 40f);
        rectTransform.sizeDelta = new Vector2(600f, 90f);
    }

    public void Refresh()
    {
        if (counterText == null)
            counterText = GetComponent<TMP_Text>();

        if (counterText == null)
            return;

        if (fontOverride != null)
            counterText.font = fontOverride;

        string lastScene = MedalProgress.GetLastCompletedSceneName();
        int lastCount = MedalProgress.GetLastCompletedMedalCount();
        int total = MedalProgress.GetSavedTotalMedals();
        int clampedLimit = Mathf.Max(0, medalLimit);

        if (!showLastClear || string.IsNullOrEmpty(lastScene))
        {
            counterText.SetText($"{label}{total}/{clampedLimit}");
            return;
        }

        counterText.SetText($"{label}{total}/{clampedLimit}\nLast Clear: {lastScene} ({lastCount})");
    }
}
