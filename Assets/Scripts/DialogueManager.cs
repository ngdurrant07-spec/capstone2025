using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public float typingSpeed = 0.02f;
    public UnityEngine.Vector3 panelOffset = new UnityEngine.Vector3(0, 1f, 0); // offset above NPC

    private string[] sentences;
    private int index;
    private Transform currentNPC; // follow the NPC talking, not player
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        // Panel follows the NPC currently talking
        if (currentNPC != null && dialoguePanel != null && dialoguePanel.activeSelf)
        {
            UnityEngine.Vector3 worldPos = currentNPC.position + panelOffset;
            UnityEngine.Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            dialoguePanel.transform.position = screenPos;
        }

        // Advance dialogue
        bool advancePressed = Keyboard.current != null &&
                              (Keyboard.current.upArrowKey.wasPressedThisFrame ||
                               Keyboard.current.wKey.wasPressedThisFrame);

        if (dialoguePanel.activeSelf && advancePressed)
        {
            ShowNextSentence();
        }
    }

    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }

    // Each NPC passes itself
    public void StartDialogue(Transform npcTransform, string[] newSentences)
    {
        if (npcTransform == null || newSentences == null || newSentences.Length == 0) return;

        // Stop previous dialogue
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        currentNPC = npcTransform;
        sentences = newSentences;
        index = 0;

        dialoguePanel.SetActive(true);

        ShowNextSentence();
    }

    private void ShowNextSentence()
    {
        if (sentences == null || index >= sentences.Length)
        {
            EndDialogue();
            return;
        }

        if (dialogueText != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeSentence(sentences[index]));
        }
        index++;
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        sentences = null;
        currentNPC = null;
        index = 0;
        typingCoroutine = null;
    }
}
