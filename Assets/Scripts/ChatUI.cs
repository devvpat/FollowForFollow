using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FollowForFollow.Chat
{
    /// <summary>
    /// Drives the chatroom UI.
    ///
    /// Hierarchy expected (set in Inspector):
    ///
    ///   [Canvas]
    ///     ChatPanel
    ///       Header          (TMP label — room name / online count)
    ///       ScrollView
    ///         Viewport
    ///           Content     ← messageContainer
    ///       TypingIndicator (GameObject with a TMP label)
    ///       ChoicePanel
    ///         ChoiceButton  (prefab)
    ///       InputRow
    ///         InputField    (TMP_InputField)
    ///         SendButton    (Button)
    ///       EnterDungeonButton (Button)
    ///
    /// All color/font styling is handled via the MessageBubble prefab (see below).
    /// </summary>
    public class ChatUI : MonoBehaviour
    {
        // ── Inspector References ─────────────────────────────────────────────
        [Header("Manager")]
        [SerializeField] private ChatManager chatManager;

        [Header("Message Area")]
        [SerializeField] private Transform          messageContainer;
        [SerializeField] private GameObject         messageBubblePrefab;
        [SerializeField] private ScrollRect         scrollRect;

        [Header("Typing Indicator")]
        [SerializeField] private GameObject         typingIndicatorObject;
        [SerializeField] private TMP_Text           typingIndicatorLabel;

        [Header("Choice Panel")]
        [SerializeField] private GameObject         choicePanel;
        [SerializeField] private Transform          choiceButtonContainer;
        [SerializeField] private GameObject         choiceButtonPrefab;

        [Header("Free Input")]
        [SerializeField] private TMP_InputField     inputField;
        [SerializeField] private Button             sendButton;

        [Header("Flow")]
        [SerializeField] private Button             enterDungeonButton;

        // ── Unity Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            // Validate
            if (chatManager == null)
                chatManager = FindFirstObjectByType<ChatManager>();

            typingIndicatorObject.SetActive(false);
            choicePanel.SetActive(false);
        }

        private void OnEnable()
        {
            chatManager.OnMessageAdded.AddListener(HandleMessageAdded);
            chatManager.OnTypingIndicator.AddListener(HandleTypingIndicator);
            chatManager.OnChoicesUpdated.AddListener(HandleChoicesUpdated);

            sendButton.onClick.AddListener(OnSendClicked);
            inputField.onSubmit.AddListener(_ => OnSendClicked());
            enterDungeonButton.onClick.AddListener(chatManager.EndSession);
        }

        private void OnDisable()
        {
            chatManager.OnMessageAdded.RemoveListener(HandleMessageAdded);
            chatManager.OnTypingIndicator.RemoveListener(HandleTypingIndicator);
            chatManager.OnChoicesUpdated.RemoveListener(HandleChoicesUpdated);

            sendButton.onClick.RemoveListener(OnSendClicked);
            enterDungeonButton.onClick.RemoveListener(chatManager.EndSession);
        }

        // ── Event Handlers ───────────────────────────────────────────────────

        private void HandleMessageAdded(ChatMessage msg)
        {
            SpawnBubble(msg);
            ScrollToBottom();
        }

        private void HandleTypingIndicator(bool show)
        {
            typingIndicatorObject.SetActive(show);
            if (show)
            {
                typingIndicatorLabel.text = "someone is typing...";
                ScrollToBottom();
            }
        }

        private void HandleChoicesUpdated(List<ChatBeat.PlayerChoice> choices)
        {
            // Clear old choice buttons
            foreach (Transform child in choiceButtonContainer)
                Destroy(child.gameObject);

            if (choices == null || choices.Count == 0)
            {
                choicePanel.SetActive(false);
                inputField.interactable = true;
                return;
            }

            choicePanel.SetActive(true);
            inputField.interactable = false; // block free input during choices

            foreach (var choice in choices)
            {
                var btn = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                var label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = choice.ChoiceLabel;

                // Capture for lambda
                var captured = choice;
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    chatManager.SelectChoice(captured);
                });
            }

            ScrollToBottom();
        }

        // ── Input ────────────────────────────────────────────────────────────

        private void OnSendClicked()
        {
            var text = inputField.text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            chatManager.SubmitPlayerMessage(text);
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }

        // ── Bubble Spawning ──────────────────────────────────────────────────

        private void SpawnBubble(ChatMessage msg)
        {
            var bubble = Instantiate(messageBubblePrefab, messageContainer);
            var bubbleCtrl = bubble.GetComponent<MessageBubble>();
            if (bubbleCtrl != null)
                bubbleCtrl.Populate(msg);
        }

        // ── Scroll ───────────────────────────────────────────────────────────

        private void ScrollToBottom()
        {
            // Defer one frame so layout rebuilds first
            StartCoroutine(ScrollNextFrame());
        }

        private System.Collections.IEnumerator ScrollNextFrame()
        {
            yield return null;
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
