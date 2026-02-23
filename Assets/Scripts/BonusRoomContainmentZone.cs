using UnityEngine;

public class BonusRoomContainmentZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BonusRoomDoor entryDoor;

    [Header("Player Filter")]
    [SerializeField] private string playerTag = "Player";

    void OnTriggerExit2D(Collider2D other)
    {
        if (entryDoor == null)
            return;

        if (!other.CompareTag(playerTag))
            return;

        entryDoor.FailActiveBonus(other);
    }
}
