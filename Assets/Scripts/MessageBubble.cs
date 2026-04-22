using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FollowForFollow.Chat
{
    /// <summary>
    /// Attach to the root of your MessageBubble prefab.
    ///
    /// Prefab children expected:
    ///   SenderLabel  (TMP_Text)  — username
    ///   BodyLabel    (TMP_Text)  — message text
    ///   Background   (Image)    — colored bubble background
    ///
    /// Colors and alignment are set per-sender so each party member feels distinct.
    /// The "dreamcore / liminal but whimsical" aesthetic should come from your
    /// Sprite assets and font choices in the prefab — this script just wires the data.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MessageBubble : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text senderLabel;
        [SerializeField] private TMP_Text bodyLabel;
        [SerializeField] private Image    background;

        [Header("Sender Color Palette")]
        [SerializeField] private Color playerColor   = new(0.55f, 0.85f, 1f);    // cool blue
        [SerializeField] private Color streamerColor = new(1f,   0.45f, 0.45f);  // streamer red
        [SerializeField] private Color idolColor     = new(1f,   0.75f, 0.95f);  // idol pink
        [SerializeField] private Color youtuberColor = new(0.45f, 1f,   0.65f);  // vlog green
        [SerializeField] private Color systemColor   = new(0.6f,  0.6f,  0.6f);  // muted grey

        [Header("Alignment")]
        [SerializeField] private RectTransform bubbleRect;
        [SerializeField] private float         playerRightPadding = 80f;
        [SerializeField] private float         npcLeftPadding     = 80f;

        // ── Public API ───────────────────────────────────────────────────────

        public void Populate(ChatMessage msg)
        {
            senderLabel.text = msg.Sender == ChatMessage.SenderType.System
                ? string.Empty
                : msg.DisplayName;

            bodyLabel.text = msg.Text;

            Color col = msg.Sender switch
            {
                ChatMessage.SenderType.Player    => playerColor,
                ChatMessage.SenderType.Streamer  => streamerColor,
                ChatMessage.SenderType.Idol      => idolColor,
                ChatMessage.SenderType.YouTuber  => youtuberColor,
                _                                => systemColor
            };

            if (background != null)
                background.color = col;

            AlignBubble(msg.Sender);
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void AlignBubble(ChatMessage.SenderType sender)
        {
            if (bubbleRect == null) return;

            if (sender == ChatMessage.SenderType.Player)
            {
                // Right-align player messages
                bubbleRect.anchorMin = new Vector2(1, 0.5f);
                bubbleRect.anchorMax = new Vector2(1, 0.5f);
                bubbleRect.pivot     = new Vector2(1, 0.5f);
                var off = bubbleRect.offsetMin;
                bubbleRect.offsetMin = new Vector2(-playerRightPadding, off.y);
            }
            else
            {
                // Left-align NPC messages
                bubbleRect.anchorMin = new Vector2(0, 0.5f);
                bubbleRect.anchorMax = new Vector2(0, 0.5f);
                bubbleRect.pivot     = new Vector2(0, 0.5f);
                var off = bubbleRect.offsetMax;
                bubbleRect.offsetMax = new Vector2(npcLeftPadding, off.y);
            }
        }
    }
}
