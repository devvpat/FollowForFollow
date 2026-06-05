using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] CinemachineCamera cam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        ResolveSceneReferences();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResolveSceneReferences();
    }

    private void ResolveSceneReferences()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        cam = FindAnyObjectByType<CinemachineCamera>();
    }

    async Task Fade(float targetTransparency)
    {
        ResolveSceneReferences();

        if (canvasGroup == null)
        {
            Debug.LogError("ScreenFader is missing a CanvasGroup reference.");
            return;
        }

        float start = canvasGroup.alpha, t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, targetTransparency, t / fadeDuration);
            if (cam != null)
            {
                cam.PreviousStateIsValid = false;
            }

            await Task.Yield();
        }
        canvasGroup.alpha = targetTransparency;
    }

    public async Task FadeOut()
    {
        await Fade(1);
    }

    public async Task FadeIn()
    {
        await Fade(0);
    }
}
