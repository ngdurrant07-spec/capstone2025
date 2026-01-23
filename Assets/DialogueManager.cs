using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance; // Singleton

    [Header("UI Elements")]
    public GameObject dialoguePanel;       // Panel background
    public TextMeshProUGUI dialogueText;   // Text inside panel (TMP)

    [Header("Settings")]
    public float typingSpeed = 0.02f;      // Letters per second
    public Vector3 panelOffset = new Vector3(0, -4f, 0); // Offset from player

    private string[] sentences;
    private int index;
    private Transform player;

    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Hide panel by default
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        // Make panel follow the player
        if (player != null && dialoguePanel != null && dialoguePanel.activeSelf)
        {
            Vector3 worldPos = player.position + panelOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            dialoguePanel.transform.position = screenPos;
        }

        // Advance dialogue
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                ShowNextSentence();
            }
        }
    }

    public void StartDialogue(Transform playerTransform, string[] newSentences)
    {
        if (playerTransform == null || newSentences == null || newSentences.Length == 0)
            return;

        player = playerTransform;
        sentences = newSentences;
        index = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowNextSentence();
    }

    private void ShowNextSentence()
    {
        if (sentences == null || sentences.Length == 0)
        {
            EndDialogue();
            return;
        }

        if (index < sentences.Length)
        {
            if (dialogueText != null)
            {
                StopAllCoroutines();
                StartCoroutine(TypeSentence(sentences[index]));
            }
            index++;
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText == null) yield break;

        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        player = null;
        sentences = null;
        index = 0;
    }
}
