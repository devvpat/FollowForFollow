using UnityEngine;

public class TitleScreenMusic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {     
        MusicManager.Instance.PlayMusic(MusicManager.Instance.titleScreenTheme);
    }
}
