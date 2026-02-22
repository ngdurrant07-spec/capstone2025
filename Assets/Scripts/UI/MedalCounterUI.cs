using TMPro;
using UnityEngine;

public class MedalCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private string label = "Medals: ";

    [Header("Level Target")]
    [SerializeField] private int targetMedals = 1;
    [SerializeField] private bool resetCountOnAwake = true;

    private static MedalCounterUI instance;
    private static int currentMedals;

    void Awake()
    {
        instance = this;

        if (resetCountOnAwake)
            currentMedals = 0;

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

    public void ResetMedals()
    {
        currentMedals = 0;
        UpdateText();
    }

    private void UpdateText()
    {
        if (counterText == null)
            return;

        counterText.SetText($"{label}{currentMedals} / {Mathf.Max(0, targetMedals)}");
    }
}
