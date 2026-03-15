using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class WinSceneMedalText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text targetText;

    [Header("Text")]
    [SerializeField] private string label = "Total Medals: ";
    [SerializeField] private bool showMedalLimit = false;
    [SerializeField] private int medalLimit = 8;

    [Header("Style")]
    [SerializeField] private TMP_FontAsset fontOverride;
    [SerializeField] private Color textColor = Color.white;

    private void Reset()
    {
        targetText = GetComponent<TMP_Text>();
    }

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();

        ApplyStyle();
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnValidate()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();

        ApplyStyle();
        Refresh();
    }

    public void Refresh()
    {
        if (targetText == null)
            return;

        int totalMedals = MedalProgress.GetSavedTotalMedals();
        if (showMedalLimit)
        {
            targetText.SetText($"{label}{totalMedals}/{Mathf.Max(0, medalLimit)}");
            return;
        }

        targetText.SetText($"{label}{totalMedals}");
    }

    private void ApplyStyle()
    {
        if (targetText == null)
            return;

        if (fontOverride != null)
            targetText.font = fontOverride;

        targetText.color = textColor;
    }
}
