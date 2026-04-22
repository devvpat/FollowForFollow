using System;
using System.Collections.Generic;
using UnityEngine;

namespace FollowForFollow.Chat
{
    /// <summary>
    /// A scripted beat is a pre-authored sequence of NPC messages that plays
    /// automatically (with optional delays), optionally followed by a set of
    /// player reply choices.
    /// </summary>
    [CreateAssetMenu(fileName = "NewChatBeat", menuName = "FollowForFollow/Chat Beat")]
    public class ChatBeat : ScriptableObject
    {
        [Serializable]
        public class NPCLine
        {
            public ChatMessage.SenderType Sender;
            public string DisplayName;
            [TextArea(2, 6)]
            public string Text;
            [Tooltip("Seconds to wait before this line appears.")]
            public float DelayBefore = 0.8f;
        }

        [Serializable]
        public class PlayerChoice
        {
            [TextArea(1, 3)]
            public string ChoiceLabel;          // shown in the choice buttons
            [TextArea(1, 3)]
            public string PlayerMessageText;    // what appears in chat after selection

            [Tooltip("Beat to trigger after this choice is selected. Leave null to end.")]
            public ChatBeat NextBeat;

            [Tooltip("Affection/perception delta for each party member (Streamer, Idol, YouTuber).")]
            public int StreamerAffectionDelta;
            public int IdolAffectionDelta;
            public int YouTuberAffectionDelta;
        }

        [Header("NPC Lines (plays in order)")]
        public List<NPCLine> Lines = new();

        [Header("Player Reply Choices (shown after all lines)")]
        [Tooltip("Leave empty to auto-advance with no player input.")]
        public List<PlayerChoice> Choices = new();

        [Tooltip("Beat to auto-advance to if no choices are defined.")]
        public ChatBeat AutoNextBeat;
    }
}
