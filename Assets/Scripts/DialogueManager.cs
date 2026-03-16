using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject continueIndicator;

    [Header("Settings")]
    public float typingSpeed = 0.02f;
    public UnityEngine.Vector3 panelOffset = new UnityEngine.Vector3(0, 1f, 0); // offset above NPC

    private string[] sentences;
    private int index;
    private Transform currentNPC; // follow the NPC talking, not player
    private Coroutine typingCoroutine;
    private string currentSentence;
    private bool isTyping;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        SetContinueIndicatorVisible(false);
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
        bool advancePressed = DialogueInputResolver.WasDialogueAdvancePressedThisFrame();

        if (dialoguePanel.activeSelf && advancePressed)
        {
            if (isTyping)
            {
                FinishCurrentSentence();
                return;
            }

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
        currentSentence = null;
        isTyping = false;

        dialoguePanel.SetActive(true);
        SetContinueIndicatorVisible(false);

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
            currentSentence = DialogueInputResolver.ResolvePlaceholders(sentences[index]);
            typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
        }
        index++;
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        SetContinueIndicatorVisible(false);
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
        SetContinueIndicatorVisible(index < sentences.Length);
    }

    private void FinishCurrentSentence()
    {
        if (dialogueText == null)
            return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentSentence ?? string.Empty;
        isTyping = false;
        typingCoroutine = null;
        SetContinueIndicatorVisible(index < sentences.Length);
    }

    private void SetContinueIndicatorVisible(bool visible)
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(visible);
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        SetContinueIndicatorVisible(false);
        sentences = null;
        currentNPC = null;
        index = 0;
        currentSentence = null;
        isTyping = false;
        typingCoroutine = null;
    }
}
