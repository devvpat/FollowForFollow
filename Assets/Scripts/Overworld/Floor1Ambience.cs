using UnityEngine;

public class Floor1Ambience : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MusicManager.Instance.PlayMusic(MusicManager.Instance.overworldTheme);
    }
}
