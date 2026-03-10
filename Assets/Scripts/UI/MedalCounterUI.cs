using TMPro;
using UnityEngine;

public class MedalCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private string label = "Medals: ";
    [SerializeField] private TMP_FontAsset fontOverride;

    [Header("Level Target")]
    [SerializeField] private int targetMedals = 8;
    [SerializeField] private bool resetCountOnAwake = true;

    private static MedalCounterUI instance;
    private static int currentMedals;

    void Awake()
    {
        instance = this;

        if (resetCountOnAwake)
            currentMedals = 0;

        ApplyFontOverride();
        UpdateText();
    }

    public static void AddMedals(int amount = 1)
    {
        currentMedals += Mathf.Max(0, amount);

        if (instance != null)
            instance.UpdateText();
    }

    public static int GetCurrentMedals()
    {
        return currentMedals;
    }

    public void SetTargetMedals(int target)
    {
        targetMedals = Mathf.Max(0, target);
        UpdateText();
    }

    public static void ResetMedals()
    {
        currentMedals = 0;

        if (instance != null)
            instance.UpdateText();
    }

    private void UpdateText()
    {
        if (counterText == null)
            return;

        counterText.SetText($"{label}{currentMedals}/{Mathf.Max(0, targetMedals)}");
    }

    private void ApplyFontOverride()
    {
        if (counterText == null || fontOverride == null)
            return;

        counterText.font = fontOverride;
    }
}
