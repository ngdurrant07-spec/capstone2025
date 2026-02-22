using UnityEngine;

public class TimeTrialAreaLock : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private Collider2D[] collidersToDisable;
    [SerializeField] private MonoBehaviour[] behavioursToDisable;

    [Header("Visuals")]
    [SerializeField] private Color unlockedTint = Color.white;
    [SerializeField] private Color lockedTint = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Startup")]
    [SerializeField] private bool startLocked = false;

    void Awake()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = GetComponentsInChildren<Collider2D>(true);

        SetLocked(startLocked);
    }

    public void LockArea()
    {
        SetLocked(true);
    }

    public void UnlockArea()
    {
        SetLocked(false);
    }

    public void SetLocked(bool locked)
    {
        if (spriteRenderers != null)
        {
            Color tint = locked ? lockedTint : unlockedTint;
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                if (sr != null)
                    sr.color = tint;
            }
        }

        if (collidersToDisable != null)
        {
            foreach (Collider2D col in collidersToDisable)
            {
                if (col != null)
                    col.enabled = !locked;
            }
        }

        if (behavioursToDisable != null)
        {
            foreach (MonoBehaviour behaviour in behavioursToDisable)
            {
                if (behaviour != null)
                    behaviour.enabled = !locked;
            }
        }
    }
}
