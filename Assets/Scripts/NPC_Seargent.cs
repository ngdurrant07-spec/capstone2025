using UnityEngine;
using UnityEngine.InputSystem;

public class NPC_Seargent : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea]
    public string[] sentences;

    [Header("Optional")]
    public GameObject talkPrompt; // small arrow or text above NPC

    private bool playerInRange = false;
    private Transform player;

    private void Update()
    {
        bool interactPressed = Keyboard.current != null &&
                               (Keyboard.current.upArrowKey.wasPressedThisFrame ||
                                Keyboard.current.wKey.wasPressedThisFrame);

        if (interactPressed)
            Debug.Log($"[NPC_Seargent] Input detected. playerInRange={playerInRange}, dialogueActive={(DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())}");

        if (playerInRange && !DialogueManager.Instance.IsDialogueActive() && interactPressed)
        {
            Debug.Log("[NPC_Seargent] Starting dialogue.");
            DialogueManager.Instance.StartDialogue(player, sentences);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[NPC_Seargent] Player entered range.");
            playerInRange = true;
            player = collision.transform;

            if (talkPrompt != null)
                talkPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[NPC_Seargent] Player exited range.");
            playerInRange = false;
            player = null;

            if (talkPrompt != null)
                talkPrompt.SetActive(false);
        }
    }
}
