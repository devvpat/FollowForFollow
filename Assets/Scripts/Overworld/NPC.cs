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
    public Image nameBackground;

    [Tooltip("Play the speaker's voice blip every Nth typed character (skipping whitespace).")]
    public int charsPerSound = 3;

    private int dialogueIndex ;
    private bool isTyping, isDialogueActive;
    private AudioSource audioSource;

    public event System.Action OnDialogueEnded;

    void Awake()
    {
        if (!TryGetComponent(out audioSource))
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

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

        string nm = speaker != null ? speaker.name : "";
        nameText.SetText(nm);
        portraitImage.sprite = speaker != null ? speaker.portrait : null;

        Color tint = speaker != null ? speaker.themeColor : Color.white;
        if (nameBackground != null) nameBackground.color = tint;

        float preferredW = nameText.GetPreferredValues(nm).x;
        const float bgPadding = 24f;
        if (nameText.rectTransform != null)
        {
            Vector2 s = nameText.rectTransform.sizeDelta;
            nameText.rectTransform.sizeDelta = new Vector2(preferredW, s.y);
        }
        if (nameBackground != null && nameBackground.rectTransform != null)
        {
            Vector2 s = nameBackground.rectTransform.sizeDelta;
            nameBackground.rectTransform.sizeDelta = new Vector2(preferredW + bgPadding, s.y);
        }

        int step = Mathf.Max(1, charsPerSound);
        int shown = 0;
        foreach(char letter in CurrentLine.text)
        {
            dialogueText.text += letter;
            shown++;

            if (!char.IsWhiteSpace(letter)
                && speaker != null && speaker.voiceBlip != null
                && audioSource != null
                && shown % step == 0)
            {
                audioSource.PlayOneShot(speaker.voiceBlip);
            }

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

        OnDialogueEnded?.Invoke();
    }
}
