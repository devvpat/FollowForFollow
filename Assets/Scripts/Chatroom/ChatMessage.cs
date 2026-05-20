using UnityEngine;

[System.Serializable]
public class ChatMessage
{
    public CharacterProfile sender;

    [TextArea(2, 4)]
    public string text;

    [Tooltip("Delay before this message starts (ms). 0 = use ChatPlayer default.")]
    public float delayBeforeMs;

    [Tooltip("How long the typing animation takes (ms). 0 = auto-calc from char count.")]
    public float typingDurationMs;
}
