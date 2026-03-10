using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Progress Unlock")]
    [SerializeField] private bool persistUnlockByScene = false;
    [SerializeField] private string saveKeyOverride = "";

    void Awake()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = GetComponentsInChildren<Collider2D>(true);

        bool locked = startLocked;
        if (persistUnlockByScene && IsSavedUnlocked())
            locked = false;

        SetLocked(locked);
    }

    public void LockArea()
    {
        SetLocked(true);
    }

    public void UnlockArea()
    {
        SetLocked(false);
    }

    public void UnlockAreaAndSave()
    {
        UnlockArea();

        if (!persistUnlockByScene)
            return;

        string key = GetSaveKey();
        if (string.IsNullOrEmpty(key))
            return;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public void LockAreaAndClearSave()
    {
        LockArea();

        if (!persistUnlockByScene)
            return;

        string key = GetSaveKey();
        if (string.IsNullOrEmpty(key))
            return;

        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
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

    private bool IsSavedUnlocked()
    {
        string key = GetSaveKey();
        if (string.IsNullOrEmpty(key))
            return false;

        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    private string GetSaveKey()
    {
        if (!string.IsNullOrWhiteSpace(saveKeyOverride))
            return saveKeyOverride.Trim();

        Scene scene = gameObject.scene;
        if (!scene.IsValid() || string.IsNullOrEmpty(scene.name))
            return string.Empty;

        return $"TimeTrialUnlocked_{scene.name}";
    }

    public static void ClearSavedUnlocks(params string[] sceneNames)
    {
        if (sceneNames == null)
            return;

        foreach (string sceneName in sceneNames)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                continue;

            PlayerPrefs.DeleteKey($"TimeTrialUnlocked_{sceneName.Trim()}");
        }
    }
}
