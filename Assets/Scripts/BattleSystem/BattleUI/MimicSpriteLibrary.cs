using System;
using System.Collections.Generic;
using UnityEngine;

// Maps a copyable character (CharacterSkillSet) to the mimic portrait shown on the Boss Mimic's
// enemy field once it has copied that ally. Lives as a single asset under Resources/ so the battle
// UI can load it at runtime without a serialized reference on any prefab/scene object.
[CreateAssetMenu(fileName = "MimicSpriteLibrary", menuName = "F4F/Mimic Sprite Library")]
public class MimicSpriteLibrary : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public CharacterSkillSet skillSet;
        public Sprite sprite;
    }

    public List<Entry> entries = new();

    // Shown on a Mimic before it has copied anyone (its base/uncopied form).
    public Sprite defaultSprite;

    public Sprite Get(CharacterSkillSet skillSet)
    {
        foreach (var e in entries)
            if (e.skillSet == skillSet)
                return e.sprite;
        return null;
    }
}
