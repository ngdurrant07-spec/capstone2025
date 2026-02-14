using UnityEngine;

public class SceneStartFade : MonoBehaviour
{
    [SerializeField] private Animator fadeAnimator;

    private void Start()
    {
        if (fadeAnimator != null)
            fadeAnimator.SetTrigger("StartLevel"); // 1 -> 0
    }
}
