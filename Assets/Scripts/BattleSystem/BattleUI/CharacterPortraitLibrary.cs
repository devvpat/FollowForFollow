using System;
using System.Collections.Generic;
using UnityEngine;

// Maps an ally character (CharacterSkillSet) to the portrait shown on their AllyCard in the battle UI.
// Lives as a single asset under Resources/ so AllyCardUI can load it at runtime without a serialized
// reference on the card prefab/scene object (mirrors MimicSpriteLibrary).
[CreateAssetMenu(fileName = "CharacterPortraitLibrary", menuName = "F4F/Character Portrait Library")]
public class CharacterPortraitLibrary : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public CharacterSkillSet skillSet;
        public Sprite sprite;
    }

    public List<Entry> entries = new();

    // Used when an ally has no entry (falls back to the AllyCard's solid color instead if also null).
    public Sprite defaultSprite;

    public Sprite Get(CharacterSkillSet skillSet)
    {
        foreach (var e in entries)
            if (e.skillSet == skillSet)
                return e.sprite;
        return defaultSprite;
    }
}
