using UnityEngine;

public class TimeTrialGoal : MonoBehaviour
{
    [SerializeField] private TimeTrialTimer timer;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool hideGoalOnFinish = true;

    void Awake()
    {
        if (timer == null)
            timer = FindFirstObjectByType<TimeTrialTimer>(FindObjectsInactive.Include);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (timer != null)
            timer.StopTimer();

        if (hideGoalOnFinish)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }
    }
}
