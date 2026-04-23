using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Dialogue
{
    public string name;
    public string dialogueText;
}

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private List<Dialogue> dialogues;
}
