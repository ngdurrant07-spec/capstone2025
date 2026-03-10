using TMPro;
using UnityEngine;

public class LevelClearUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool disableInteractionOnStart = true;

    [Header("Medals")]
    [SerializeField] private TMP_Text medalText;
    [SerializeField] private string medalLabel = "Medals: ";
    [SerializeField] private TMP_FontAsset medalFontOverride;
    [SerializeField] private int medalTarget = 8;
    [SerializeField] private bool hideMedalTextUntilThreshold = false;
    [SerializeField] private int medalsNeededToShow = 1;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (disableInteractionOnStart && canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        ApplyFontOverride();
    }

    public void Play()
    {
        RefreshMedalText();

        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (animator != null)
        {
            animator.SetTrigger("Show");
        }
    }

    public void SetMedalTarget(int target)
    {
        medalTarget = Mathf.Max(0, target);
        RefreshMedalText();
    }

    private void RefreshMedalText()
    {
        if (medalText == null)
            return;

        int current = MedalCounterUI.GetCurrentMedals();
        bool shouldShow = !hideMedalTextUntilThreshold || current >= Mathf.Max(0, medalsNeededToShow);
        medalText.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        medalText.SetText($"{medalLabel}{current}/{Mathf.Max(0, medalTarget)}");
    }

    private void ApplyFontOverride()
    {
        if (medalText == null || medalFontOverride == null)
            return;

        medalText.font = medalFontOverride;
    }
}
