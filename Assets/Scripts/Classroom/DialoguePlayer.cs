using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class DialoguePlayer : MonoBehaviour
{
    [Header("Content")]
    public DialogueScript dialogue;

    [Header("Scene References")]
    public DialogueBox dialogueBox;

    [Header("Timing")]
    public float defaultDelayBeforeMs = 400f;
    public float defaultCharsPerSecond = 40f;

    [Header("Audio")]
    public AudioClip startBellSound;
    [Tooltip("Play a text-blip sound every Nth typed character (skipping whitespace).")]
    public int charsPerSound = 3;

    [Header("Fade-In")]
    public CanvasGroup fadeOverlay;

    [Header("Fade-Out / Next Scene")]
    public bool fadeOutAtEnd = false;
    public float fadeOutDurationSeconds = 1.5f;
    [Tooltip("If set, loads this scene by name after OnDialogueFinished fires. Leave blank for no transition.")]
    public string nextSceneOnFinish;

    [Header("Playback")]
    public bool playOnStart = true;
    public bool hideBoxAtEnd = true;

    [Header("Events")]
    public UnityEvent OnDialogueFinished;

    AudioSource audioSource;
    bool skipRequested;
    bool awaitingAdvance;
    bool isPlaying;

    void Awake()
    {
        if (!TryGetComponent(out audioSource))
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (dialogueBox != null)
            dialogueBox.gameObject.SetActive(false);

        if (playOnStart) PlayCurrent();
    }

    public void PlayCurrent() => PlayDialogueScript(dialogue);

    public void PlayDialogueScript(DialogueScript script)
    {
        if (isPlaying) return;
        if (script != null) dialogue = script;

        if (dialogue == null || dialogue.lines == null || dialogue.lines.Count == 0)
        {
            Debug.LogWarning("DialoguePlayer: no dialogue script assigned or it has no lines. Firing OnDialogueFinished immediately.");
            OnDialogueFinished?.Invoke();
            return;
        }

        if (dialogueBox == null)
        {
            Debug.LogWarning("DialoguePlayer: dialogueBox reference is null. Cannot play dialogue.");
            return;
        }

        StartCoroutine(PlayDialogue());
    }

    void Update()
    {
        if (!isPlaying) return;

        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool enterKey   = Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame;
        if (!mouseClick && !enterKey) return;

        if (awaitingAdvance)
            awaitingAdvance = false;
        else
            skipRequested = true;
    }

    IEnumerator PlayDialogue()
    {
        isPlaying = true;

        if (fadeOverlay != null) fadeOverlay.alpha = 1f;

        float fadeDuration = (startBellSound != null) ? startBellSound.length : 2.5f;
        if (startBellSound != null && audioSource != null)
            audioSource.PlayOneShot(startBellSound);

        if (fadeOverlay != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(1f - (t / fadeDuration));
                yield return null;
            }
            fadeOverlay.alpha = 0f;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        yield return new WaitForSeconds(0.5f);

        DialogueLine firstLine = dialogue.lines.Count > 0 ? dialogue.lines[0] : null;
        if (firstLine != null)
            dialogueBox.Setup(firstLine.sender, "");

        if (!dialogueBox.gameObject.activeSelf)
            dialogueBox.gameObject.SetActive(true);

        for (int i = 0; i < dialogue.lines.Count; i++)
        {
            DialogueLine line = dialogue.lines[i];
            if (line == null) continue;

            float delaySec = (line.delayBeforeMs > 0f ? line.delayBeforeMs : defaultDelayBeforeMs) / 1000f;
            yield return WaitSkippable(delaySec);

            dialogueBox.Setup(line.sender, "");

            float cps = defaultCharsPerSecond > 0f ? defaultCharsPerSecond : 40f;
            float typingSec = line.typingDurationMs > 0f
                ? line.typingDurationMs / 1000f
                : (line.text != null ? line.text.Length / cps : 0f);

            yield return TypeText(dialogueBox, line.sender, line.text ?? "", typingSec);

            awaitingAdvance = true;
            while (awaitingAdvance) yield return null;
        }

        isPlaying = false;

        if (hideBoxAtEnd && dialogueBox != null)
            dialogueBox.gameObject.SetActive(false);

        if (fadeOutAtEnd && fadeOverlay != null)
        {
            float t = 0f;
            float dur = Mathf.Max(0.01f, fadeOutDurationSeconds);
            while (t < dur)
            {
                t += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(t / dur);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        OnDialogueFinished?.Invoke();

        if (!string.IsNullOrEmpty(nextSceneOnFinish))
            SceneManager.LoadScene(nextSceneOnFinish);
    }

    IEnumerator WaitSkippable(float seconds)
    {
        skipRequested = false;
        float elapsed = 0f;
        while (elapsed < seconds && !skipRequested)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        skipRequested = false;
    }

    IEnumerator TypeText(DialogueBox box, CharacterProfile sender, string fullText, float totalSeconds)
    {
        if (box == null || string.IsNullOrEmpty(fullText))
        {
            if (box != null) box.SetText(fullText ?? "");
            yield break;
        }

        skipRequested = false;
        int step = Mathf.Max(1, charsPerSound);
        float perChar;
        if (sender != null && sender.textSound != null)
            perChar = sender.textSound.length / step;
        else
            perChar = totalSeconds / fullText.Length;
        int shown = 0;

        while (shown < fullText.Length)
        {
            if (skipRequested)
            {
                box.SetText(fullText);
                break;
            }

            char ch = fullText[shown];
            shown++;
            box.SetText(fullText.Substring(0, shown));

            if (!char.IsWhiteSpace(ch)
                && sender != null && sender.textSound != null
                && audioSource != null
                && shown % step == 0)
            {
                audioSource.PlayOneShot(sender.textSound);
            }

            yield return new WaitForSeconds(perChar);
        }

        skipRequested = false;
    }
}
