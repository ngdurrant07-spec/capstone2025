using UnityEngine;

public class BonusRoomReturnTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BonusRoomDoor entryDoor;

    [Header("Player Filter")]
    [SerializeField] private string playerTag = "Player";

    [Header("Behavior")]
    [SerializeField] private bool triggerOnEnter = true;
    [SerializeField] private bool triggerOnExit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggerOnEnter)
            return;

        TryReturnPlayer(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!triggerOnExit)
            return;

        TryReturnPlayer(other);
    }

    private void TryReturnPlayer(Collider2D other)
    {
        if (entryDoor == null)
            return;

        if (!other.CompareTag(playerTag))
            return;

        // Uses the bonus fail flow, which returns the player to Timeout Return Point immediately.
        entryDoor.FailActiveBonus(other);
    }
}
