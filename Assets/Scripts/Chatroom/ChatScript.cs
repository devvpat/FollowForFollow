using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chat Script", menuName = "F4F/Group Chat Script")]
public class ChatScript : ScriptableObject
{
    public List<ChatMessage> messages = new List<ChatMessage>();
}
