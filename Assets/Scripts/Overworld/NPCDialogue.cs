using UnityEngine;

[System.Serializable]
public class Speaker
{
    public string name;
    public Sprite portrait;
}

[System.Serializable]
public class NPCDialogueLine
{
    [Min(0)]
    public int speakerIndex;

    [TextArea(2, 5)]
    public string text;

    public bool autoProgress;
}

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public Speaker[] speakers;
    public NPCDialogueLine[] dialogueLines;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;

    public Speaker GetSpeaker(int speakerIndex)
    {
        if (speakers == null || speakerIndex < 0 || speakerIndex >= speakers.Length)
        {
            return null;
        }

        return speakers[speakerIndex];
    }
}
