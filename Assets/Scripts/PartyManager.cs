using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    public readonly List<CharacterProfile> selected = new List<CharacterProfile>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsSelected(CharacterProfile profile) => profile != null && selected.Contains(profile);

    public void SetSelected(CharacterProfile profile, bool isSelected)
    {
        if (profile == null) return;
        if (isSelected)
        {
            if (!selected.Contains(profile)) selected.Add(profile);
        }
        else
        {
            selected.Remove(profile);
        }
    }
}