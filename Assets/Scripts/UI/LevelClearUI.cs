using UnityEngine;

public class LevelClearUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool disableInteractionOnStart = true;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (disableInteractionOnStart && canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Play()
    {
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
}
