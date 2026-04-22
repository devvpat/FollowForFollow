using System;

namespace FollowForFollow.Chat
{
    /// <summary>
    /// Represents a single message in the chatroom.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        public enum SenderType
        {
            Player,
            Streamer,   // e.g. "XxGamerxX"
            Idol,       // e.g. "StarLight"
            YouTuber,   // e.g. "VlogBoy"
            System      // narrative / server messages
        }

        public SenderType Sender;
        public string DisplayName;
        public string Text;
        public float Timestamp; // Time.time when sent

        public ChatMessage(SenderType sender, string displayName, string text, float timestamp)
        {
            Sender = sender;
            DisplayName = displayName;
            Text = text;
            Timestamp = timestamp;
        }
    }
}
