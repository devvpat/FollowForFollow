using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    public void StartGame()
    {
        MusicManager.Instance.StopMusic();
        SceneManager.LoadScene("Classroom");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
