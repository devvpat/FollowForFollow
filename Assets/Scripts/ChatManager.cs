using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FollowForFollow.Chat
{
    /// <summary>
    /// Central manager for the chatroom phase.
    /// Attach to a persistent GameObject in the Chat scene.
    ///
    /// Usage:
    ///   1. Assign an opening ChatBeat in the Inspector (or call StartBeat() from
    ///      your game-flow manager after loading the scene).
    ///   2. Wire ChatUI events to the public methods below.
    ///   3. Subscribe to OnSessionEnd to trigger the dungeon-entry flow.
    /// </summary>
    public class ChatManager : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Party Display Names")]
        [SerializeField] private string streamerName  = "xX_GamerGod_Xx";
        [SerializeField] private string idolName      = "StarLight✦";
        [SerializeField] private string youtuberName  = "VlogBoy";
        [SerializeField] private string playerName    = "You";

        [Header("Opening Beat")]
        [SerializeField] private ChatBeat openingBeat;

        [Header("Free-type Settings")]
        [Tooltip("Can the player type anything at any time (outside of beat choices)?")]
        [SerializeField] private bool allowFreeInput = true;
        [Tooltip("Max characters per free-type message.")]
        [SerializeField] private int maxInputLength  = 200;

        // ── Public Events ────────────────────────────────────────────────────
        /// <summary>Fired whenever a new message is added to the log.</summary>
        public UnityEvent<ChatMessage> OnMessageAdded = new();

        /// <summary>Fired when NPC typing indicator should show/hide.</summary>
        public UnityEvent<bool> OnTypingIndicator = new();

        /// <summary>Fired with the list of current choices (empty = hide panel).</summary>
        public UnityEvent<List<ChatBeat.PlayerChoice>> OnChoicesUpdated = new();

        /// <summary>Fired when the chatroom session is over and the dungeon should load.</summary>
        public UnityEvent OnSessionEnd = new();

        // ── Public State ─────────────────────────────────────────────────────
        public IReadOnlyList<ChatMessage> MessageLog => _messageLog;

        public int StreamerAffection { get; private set; }
        public int IdolAffection     { get; private set; }
        public int YouTuberAffection { get; private set; }

        // ── Private ──────────────────────────────────────────────────────────
        private readonly List<ChatMessage> _messageLog = new();
        private ChatBeat _currentBeat;
        private bool _beatRunning;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void Start()
        {
            if (openingBeat != null)
                StartBeat(openingBeat);
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>Begin playing a scripted beat.</summary>
        public void StartBeat(ChatBeat beat)
        {
            if (beat == null) return;
            _currentBeat = beat;
            StartCoroutine(PlayBeat(beat));
        }

        /// <summary>Called by ChatUI when the player submits a free-type message.</summary>
        public void SubmitPlayerMessage(string text)
        {
            if (!allowFreeInput) return;
            text = text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            if (text.Length > maxInputLength)
                text = text[..maxInputLength];

            AddMessage(ChatMessage.SenderType.Player, playerName, text);
        }

        /// <summary>Called by ChatUI when the player taps a choice button.</summary>
        public void SelectChoice(ChatBeat.PlayerChoice choice)
        {
            if (choice == null) return;

            // Hide choices immediately
            OnChoicesUpdated.Invoke(new List<ChatBeat.PlayerChoice>());

            // Post player message
            if (!string.IsNullOrWhiteSpace(choice.PlayerMessageText))
                AddMessage(ChatMessage.SenderType.Player, playerName, choice.PlayerMessageText);

            // Apply affection deltas
            StreamerAffection  += choice.StreamerAffectionDelta;
            IdolAffection      += choice.IdolAffectionDelta;
            YouTuberAffection  += choice.YouTuberAffectionDelta;

            // Advance to next beat or end session
            if (choice.NextBeat != null)
                StartBeat(choice.NextBeat);
            else
                EndSession();
        }

        /// <summary>Force-end the session (e.g. player clicks "Enter Dungeon").</summary>
        public void EndSession()
        {
            StopAllCoroutines();
            OnSessionEnd.Invoke();
        }

        // ── Internal ─────────────────────────────────────────────────────────

        private IEnumerator PlayBeat(ChatBeat beat)
        {
            _beatRunning = true;
            OnChoicesUpdated.Invoke(new List<ChatBeat.PlayerChoice>()); // clear choices

            foreach (var line in beat.Lines)
            {
                // Show typing indicator, wait, then show message
                yield return new WaitForSeconds(Mathf.Max(0, line.DelayBefore));
                OnTypingIndicator.Invoke(true);
                float typingTime = Mathf.Clamp(line.Text.Length * 0.03f, 0.4f, 2.5f);
                yield return new WaitForSeconds(typingTime);
                OnTypingIndicator.Invoke(false);

                AddMessage(line.Sender, ResolveDisplayName(line.Sender, line.DisplayName), line.Text);
            }

            _beatRunning = false;

            // Present choices or auto-advance
            if (beat.Choices is { Count: > 0 })
            {
                OnChoicesUpdated.Invoke(beat.Choices);
            }
            else if (beat.AutoNextBeat != null)
            {
                yield return new WaitForSeconds(0.5f);
                StartBeat(beat.AutoNextBeat);
            }
            // else: session idles, player can type freely or click "Enter Dungeon"
        }

        private void AddMessage(ChatMessage.SenderType sender, string displayName, string text)
        {
            var msg = new ChatMessage(sender, displayName, text, Time.time);
            _messageLog.Add(msg);
            OnMessageAdded.Invoke(msg);
        }

        private string ResolveDisplayName(ChatMessage.SenderType sender, string overrideName)
        {
            if (!string.IsNullOrWhiteSpace(overrideName)) return overrideName;
            return sender switch
            {
                ChatMessage.SenderType.Streamer  => streamerName,
                ChatMessage.SenderType.Idol      => idolName,
                ChatMessage.SenderType.YouTuber  => youtuberName,
                ChatMessage.SenderType.Player    => playerName,
                _                                => "SYSTEM"
            };
        }
    }
}
