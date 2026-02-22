using UnityEngine;
using UnityEngine.Events;

public class BonusRoomCoin : MonoBehaviour
{
    [Header("Collect")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private BonusRoomDoor entryDoor;
    [SerializeField] private int coinAmount = 1;
    [SerializeField] private bool collectOnce = true;
    [SerializeField] private bool hideOnCollect = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onCollected;

    private Collider2D cachedCollider;
    private bool collected;

    void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (collectOnce && collected)
            return;

        if (entryDoor == null)
            return;

        if (!entryDoor.BonusActive || !entryDoor.IsActivePlayer(other))
            return;

        collected = true;

        BonusCoinCounter.AddCoin(coinAmount);
        MedalCounterUI.AddMedals(coinAmount);
        onCollected?.Invoke();

        if (hideOnCollect)
            HideVisuals();

        if (cachedCollider != null)
            cachedCollider.enabled = false;

        entryDoor.CompleteFromGoal(other);
    }

    private void HideVisuals()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }
}
