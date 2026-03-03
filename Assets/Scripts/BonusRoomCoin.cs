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

        if (entryDoor == null)
            entryDoor = GetComponentInParent<BonusRoomDoor>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (collectOnce && collected)

            return;

        BonusRoomDoor door = ResolveDoorForPlayer(other);
        if (door == null)
            return;

        collected = true;

        BonusCoinCounter.AddCoin(coinAmount);
        MedalCounterUI.AddMedals(coinAmount);
        SoundEffectManager.Play("CollectMedal");
        onCollected?.Invoke();

        if (hideOnCollect)
            HideVisuals();

        if (cachedCollider != null)
            cachedCollider.enabled = false;

        door.CompleteActiveBonusSuccess();

        StopAllBonusTimers();
    }

    private BonusRoomDoor ResolveDoorForPlayer(Collider2D player)
    {
        if (entryDoor != null && entryDoor.BonusActive && entryDoor.IsActivePlayer(player))
            return entryDoor;

        foreach (BonusRoomDoor door in FindObjectsByType<BonusRoomDoor>(FindObjectsSortMode.None))
        {
            if (door != null && door.BonusActive && door.IsActivePlayer(player))
                return door;
        }

        return null;
    }

    private void StopAllBonusTimers()
    {
        foreach (BonusRoomTimer timer in FindObjectsByType<BonusRoomTimer>(FindObjectsSortMode.None))
        {
            if (timer == null || !timer.IsRunning)
                continue;

            timer.StopTimer();
            timer.HideTimerUI();
        }
    }

    private void HideVisuals()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }
}
