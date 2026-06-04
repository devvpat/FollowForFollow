using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ChatPlayer : MonoBehaviour
{
    [Header("Content")]
    public ChatScript todaysChat;
    public CharacterProfile selfProfile;

    [Header("Scene References")]
    public RectTransform messageContainer;
    public ScrollRect scrollRect;

    [Header("Prefabs")]
    public GameObject selfBubblePrefab;
    public GameObject otherBubblePrefab;
    public GameObject typingIndicatorPrefab;

    [Header("Timing")]
    public float defaultDelayBeforeMs = 800f;
    public float defaultCharsPerSecond = 30f;

    [Header("Compose Bar")]
    public TMP_Text composeBarText;
    public string composePlaceholder = "Type a message...";

    [Header("Send Control")]
    public Button sendButton;
    public KeyCode sendHotkey = KeyCode.Return;
    public SendButtonFlasher sendButtonFlasher;

    [Header("Audio")]
    [Tooltip("Play a text-blip sound every Nth typed character (skipping whitespace).")]
    public int charsPerSound = 3;

    [Header("Playback")]
    public bool playOnStart = true;

    [Header("Events")]
    public UnityEvent OnChatFinished;

    AudioSource audioSource;
    bool skipRequested;
    bool isPlaying;
    bool waitingForSend;

    void Awake()
    {
        if (!TryGetComponent(out audioSource))
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendPressed);

        if (playOnStart) PlayCurrent();
    }

    public void OnSendPressed()
    {
        if (waitingForSend)
        {
            waitingForSend = false;
            return;
        }
        if (isPlaying)
            skipRequested = true;
    }

    public void PlayCurrent() => PlayChatScript(todaysChat);

    public void PlayChatScript(ChatScript script)
    {
        if (isPlaying) return;
        if (script != null) todaysChat = script;

        if (todaysChat == null || todaysChat.messages == null || todaysChat.messages.Count == 0)
        {
            Debug.LogWarning("ChatPlayer: no chat script assigned or it has no messages. Firing OnChatFinished immediately.");
            OnChatFinished?.Invoke();
            return;
        }

        if (selfProfile == null)
            Debug.LogWarning("ChatPlayer: selfProfile is null. All messages will render as 'other'.");

        StartCoroutine(PlayChat());
    }

    void Update()
    {
        if (!DialogueInput.AdvancePressed()) return;

        if (waitingForSend) { OnSendPressed(); return; }

        if (isPlaying) skipRequested = true;
    }

    IEnumerator PlayChat()
    {
        isPlaying = true;
        ResetComposeBar();

        for (int i = 0; i < todaysChat.messages.Count; i++)
        {
            ChatMessage msg = todaysChat.messages[i];
            if (msg == null) continue;

            bool isSelf = msg.sender != null && selfProfile != null && msg.sender == selfProfile;

            float delaySec = (msg.delayBeforeMs > 0f ? msg.delayBeforeMs : defaultDelayBeforeMs) / 1000f;

            if (isSelf)
            {
                yield return WaitSkippable(delaySec);

                if (composeBarText != null)
                {
                    composeBarText.horizontalAlignment = HorizontalAlignmentOptions.Left;
                    composeBarText.text = msg.text ?? "";
                }

                waitingForSend = true;
                sendButtonFlasher?.StartFlashing();
                while (waitingForSend) yield return null;
                sendButtonFlasher?.StopFlashing();

                SpawnBubble(selfBubblePrefab, msg.sender, msg.text ?? "");
                ResetComposeBar();
                ScrollToBottom();
                continue;
            }

            GameObject typingGO = null;
            if (typingIndicatorPrefab != null)
            {
                typingGO = SpawnBubble(typingIndicatorPrefab, msg.sender, "...");
                ScrollToBottom();
            }

            yield return WaitSkippable(delaySec);

            if (typingGO != null)
                Destroy(typingGO);

            float cps = defaultCharsPerSecond > 0f ? defaultCharsPerSecond : 30f;
            float typingSec = msg.typingDurationMs > 0f
                ? msg.typingDurationMs / 1000f
                : (msg.text != null ? msg.text.Length / cps : 0f);

            GameObject bubbleGO = SpawnBubble(otherBubblePrefab, msg.sender, "");
            ChatBubble bubble = bubbleGO != null ? bubbleGO.GetComponent<ChatBubble>() : null;
            ScrollToBottom();

            yield return TypeText(bubble, msg.sender, msg.text ?? "", typingSec);
            ScrollToBottom();
        }

        isPlaying = false;
        OnChatFinished?.Invoke();
    }

    void ResetComposeBar()
    {
        if (composeBarText == null) return;
        composeBarText.horizontalAlignment = HorizontalAlignmentOptions.Left;
        composeBarText.text = composePlaceholder;
    }

    GameObject SpawnBubble(GameObject prefab, CharacterProfile sender, string initialText)
    {
        if (prefab == null || messageContainer == null) return null;

        GameObject go = Instantiate(prefab, messageContainer);
        ChatBubble bubble = go.GetComponent<ChatBubble>();
        if (bubble != null)
            bubble.Setup(sender, initialText);
        return go;
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

    IEnumerator TypeText(ChatBubble bubble, CharacterProfile sender, string fullText, float totalSeconds)
    {
        if (bubble == null || string.IsNullOrEmpty(fullText))
        {
            if (bubble != null) bubble.SetText(fullText ?? "");
            yield break;
        }

        skipRequested = false;
        float perChar = totalSeconds / fullText.Length;
        int shown = 0;
        int step = Mathf.Max(1, charsPerSound);

        while (shown < fullText.Length)
        {
            if (skipRequested)
            {
                bubble.SetText(fullText);
                break;
            }

            char ch = fullText[shown];
            shown++;
            bubble.SetText(fullText.Substring(0, shown));

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

    void ScrollToBottom()
    {
        if (scrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
