using UnityEngine;

public class LevelClearUI : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Play()
    {
        if (animator != null)
        {
            animator.SetTrigger("Show");
        }
    }
}
