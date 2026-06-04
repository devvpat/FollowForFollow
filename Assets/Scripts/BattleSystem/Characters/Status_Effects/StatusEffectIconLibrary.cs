using System.Collections.Generic;
using UnityEngine;

// Maps each StatusEffectIcon to a sprite. Create one asset and assign it on the StatusIcon prefab.
[CreateAssetMenu(fileName = "StatusEffectIconLibrary", menuName = "F4F/Status Effect Icon Library")]
public class StatusEffectIconLibrary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public StatusEffectIcon id;
        public Sprite sprite;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<StatusEffectIcon, Sprite> _lookup;

    // Returns the sprite for the given icon, or null if none is assigned.
    public Sprite Get(StatusEffectIcon id)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<StatusEffectIcon, Sprite>();
            foreach (var e in entries)
                if (e.sprite != null) _lookup[e.id] = e.sprite;
        }
        return _lookup.TryGetValue(id, out var sprite) ? sprite : null;
    }
}
