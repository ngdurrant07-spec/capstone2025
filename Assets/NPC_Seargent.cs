using UnityEngine;

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
        if (playerInRange && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            DialogueManager.Instance.StartDialogue(player, sentences);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
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
            playerInRange = false;
            player = null;

            if (talkPrompt != null)
                talkPrompt.SetActive(false);
        }
    }
}




