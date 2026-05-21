using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    private int dialogueIndex ;
    private bool isTyping, isDialogueActive;

    private NPCDialogueLine CurrentLine => dialogueData.dialogueLines[dialogueIndex];

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive)) return;

        if (isDialogueActive)
        {
            NextLine();
        } else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        if (dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            return;
        }

        isDialogueActive = true;
        dialogueIndex = 0;

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(CurrentLine.text);
            isTyping = false;
        } else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        } else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        Speaker speaker = dialogueData.GetSpeaker(CurrentLine.speakerIndex);

        nameText.SetText(speaker != null ? speaker.name : "");
        portraitImage.sprite = speaker != null ? speaker.portrait : null;

        foreach(char letter in CurrentLine.text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if(CurrentLine.autoProgress)
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
    }
}
