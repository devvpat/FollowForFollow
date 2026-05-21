using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public CharacterProfile sender;

    [TextArea(2, 4)]
    public string text;

    [Tooltip("Delay before this line starts (ms). 0 = use DialoguePlayer default.")]
    public float delayBeforeMs;

    [Tooltip("How long the typing animation takes (ms). 0 = auto-calc from char count.")]
    public float typingDurationMs;
}
