using System;
using System.Collections.Generic;
using UnityEngine;

// Maps an ally character (CharacterSkillSet) to the sound effect played at the start of their turn.
// Lives as a single asset under Resources/ so BattleUI can load it at runtime without a serialized
// reference on a scene object (mirrors CharacterPortraitLibrary / MimicSpriteLibrary).
[CreateAssetMenu(fileName = "CharacterAudioLibrary", menuName = "F4F/Character Audio Library")]
public class CharacterAudioLibrary : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public CharacterSkillSet skillSet;
        public AudioClip clip;
    }

    public List<Entry> entries = new();

    public AudioClip Get(CharacterSkillSet skillSet)
    {
        foreach (var e in entries)
            if (e.skillSet == skillSet)
                return e.clip;
        return null;
    }
}
