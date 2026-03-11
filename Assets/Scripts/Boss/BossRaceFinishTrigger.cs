using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossRaceFinishTrigger : MonoBehaviour
{
    [SerializeField] private BossRaceManager raceManager;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
            trigger.isTrigger = true;
    }

    private void Awake()
    {
        if (raceManager == null)
            raceManager = FindFirstObjectByType<BossRaceManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        raceManager?.HandleFinish(other);
    }
}
