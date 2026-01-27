using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f);
    public Color activeColor = new Color(0f, 1f, 0f);
    public bool startAsActive;

    SpriteRenderer sr;
    static Checkpoint activeCheckpoint;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
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

        if (activeCheckpoint != null && activeCheckpoint != this)
            activeCheckpoint.SetActiveVisual(false);

        activeCheckpoint = this;
        SetActiveVisual(true);
        CheckpointManager.Instance?.SetCheckpoint(transform.position);
    }

    void SetActiveVisual(bool active)
    {
        if (sr != null)
            sr.color = active ? activeColor : inactiveColor;
    }
}
