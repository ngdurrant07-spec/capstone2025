using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f);
    public Color activeColor = new Color(0f, 1f, 0f);
    public bool startAsActive;
    public string checkpointSfx = "Enter Checkpoint";

    SpriteRenderer sr;
    Color defaultColor = Color.white;
    static Checkpoint activeCheckpoint;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
            defaultColor = sr.color;
    }

    void Start()
    {
        SetActiveVisual(startAsActive);
        if (startAsActive)
        {
            activeCheckpoint = this;
            CheckpointManager.Instance?.SetCheckpoint(transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Checkpoint] Trigger enter: {other.name} tag={other.tag}");
        if (!other.CompareTag("Player"))
            return;

        if (activeCheckpoint == this)
            return;

        if (activeCheckpoint != null && activeCheckpoint != this)
            activeCheckpoint.SetActiveVisual(false);

        activeCheckpoint = this;
        SetActiveVisual(true);
        CheckpointManager.Instance?.SetCheckpoint(transform.position);
        if (!string.IsNullOrEmpty(checkpointSfx))
            SoundEffectManager.Play(checkpointSfx);
    }

    void SetActiveVisual(bool active)
    {
        if (sr == null)
            return;

        sr.enabled = true;

        Color targetColor = active ? activeColor : inactiveColor;
        targetColor.a = defaultColor.a;
        sr.color = targetColor;
    }
}
