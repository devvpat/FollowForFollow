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

    void Update()
    {
        // Once a conversation is open, the standard advance buttons progress it.
        // E is excluded here because InteractionDetector already routes E -> Interact() -> NextLine().
        if (isDialogueActive && DialogueInput.AdvancePressed(includeInteractKey: false))
            NextLine();
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

    public bool TryStartDialogue()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive)) return false;

        return StartDialogue();
    }

    private bool StartDialogue()
    {
        if (dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            return false;
        }

        if (!HasRequiredDialogueUI())
        {
            Debug.LogWarning($"{name} cannot start dialogue because its dialogue UI reference was removed or destroyed.", this);
            return false;
        }

        isDialogueActive = true;
        dialogueIndex = 0;

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
        return true;
    }

    private bool HasRequiredDialogueUI()
    {
        ResolveDialogueUIReferences();

        return dialoguePanel != null
            && dialogueText != null
            && nameText != null
            && portraitImage != null;
    }

    private void ResolveDialogueUIReferences()
    {
        if (dialoguePanel != null
            && dialogueText != null
            && nameText != null
            && portraitImage != null)
        {
            return;
        }

        foreach (DialogueBox dialogueBox in FindObjectsByType<DialogueBox>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!dialogueBox.gameObject.scene.IsValid() || !dialogueBox.gameObject.scene.isLoaded)
            {
                continue;
            }

            if (dialogueBox.bodyText == null || dialogueBox.nameText == null || dialogueBox.portraitImage == null)
            {
                continue;
            }

            dialoguePanel = dialogueBox.gameObject;
            dialogueText = dialogueBox.bodyText;
            nameText = dialogueBox.nameText;
            portraitImage = dialogueBox.portraitImage;
            nameBackground = dialogueBox.nameBackground;
            return;
        }
    }

    void NextLine()
    {
        if (!HasRequiredDialogueUI())
        {
            EndDialogue();
            return;
        }

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
        if (!HasRequiredDialogueUI())
        {
            EndDialogue();
            yield break;
        }

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

        // Prefer the shared CharacterProfile voice; fall back to the legacy inline blip
        // for profile-less NPCs (e.g. dungeon NPCs without a CharacterProfile asset).
        AudioClip blip = (speaker != null && speaker.profile != null && speaker.profile.textSound != null)
            ? speaker.profile.textSound
            : (speaker != null ? speaker.voiceBlip : null);

        int step = Mathf.Max(1, charsPerSound);
        int shown = 0;
        foreach(char letter in CurrentLine.text)
        {
            dialogueText.text += letter;
            shown++;

            if (!char.IsWhiteSpace(letter)
                && blip != null
                && audioSource != null
                && shown % step == 0)
            {
                audioSource.PlayOneShot(blip);
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
        isTyping = false;
        if (dialogueText != null) dialogueText.SetText("");
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        PauseController.SetPause(false);

        OnDialogueEnded?.Invoke();
    }
}
