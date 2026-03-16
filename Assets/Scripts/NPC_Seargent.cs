using UnityEngine;

public class NPC_Seargent : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea]
    public string[] sentences;

    [Header("Optional")]
    public GameObject talkPrompt; // small arrow or text above NPC

    private bool playerInRange = false;

    private void Update()
    {
        bool interactPressed = DialogueInputResolver.WasDialogueAdvancePressedThisFrame();

        if (interactPressed)
            Debug.Log($"[NPC_Seargent] Input detected. playerInRange={playerInRange}, dialogueActive={(DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())}");

        if (playerInRange && !DialogueManager.Instance.IsDialogueActive() && interactPressed)
        {
            Debug.Log("[NPC_Seargent] Starting dialogue.");
            DialogueManager.Instance.StartDialogue(transform, sentences);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[NPC_Seargent] Player entered range.");
            playerInRange = true;

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

            if (talkPrompt != null)
                talkPrompt.SetActive(false);
        }
    }
}
