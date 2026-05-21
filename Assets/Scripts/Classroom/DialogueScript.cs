using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Script", menuName = "F4F/Dialogue Script")]
public class DialogueScript : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}
