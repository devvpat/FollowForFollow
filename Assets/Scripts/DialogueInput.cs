using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Shared helper for advancing dialogue. Keeps the advance controls consistent
/// across every dialogue system (Classroom, Chatroom, Overworld NPCs).
/// Standard advance inputs: Space, Enter (and numpad Enter), Left-click, and E (interact).
/// </summary>
public static class DialogueInput
{
    /// <summary>
    /// True on the frame the player presses any standard advance input.
    /// Pass includeInteractKey=false where E is already handled elsewhere
    /// (e.g. Overworld NPCs whose interact action also advances), to avoid double-advancing.
    /// </summary>
    public static bool AdvancePressed(bool includeInteractKey = true)
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        bool key = kb != null && (kb.spaceKey.wasPressedThisFrame
            || kb.enterKey.wasPressedThisFrame
            || kb.numpadEnterKey.wasPressedThisFrame
            || (includeInteractKey && kb.eKey.wasPressedThisFrame));
        bool click = mouse != null && mouse.leftButton.wasPressedThisFrame;
        return key || click;
    }
}
