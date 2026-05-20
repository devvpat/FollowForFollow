using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SendButtonFlasher : MonoBehaviour
{
    [SerializeField] Color flashColor = new Color(1f, 0.85f, 0.15f, 1f);
    [SerializeField] float periodSeconds = 0.6f;

    Button button;
    Color baseNormalColor;
    Coroutine pulseCoroutine;

    void Awake()
    {
        button = GetComponent<Button>();
        baseNormalColor = button.colors.normalColor;
    }

    public void StartFlashing()
    {
        if (pulseCoroutine != null) return;
        pulseCoroutine = StartCoroutine(Pulse());
    }

    public void StopFlashing()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        ColorBlock cb = button.colors;
        cb.normalColor = baseNormalColor;
        button.colors = cb;
    }

    IEnumerator Pulse()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime / periodSeconds * Mathf.PI * 2f) + 1f) * 0.5f;
            ColorBlock cb = button.colors;
            cb.normalColor = Color.Lerp(baseNormalColor, flashColor, t);
            button.colors = cb;
            yield return null;
        }
    }
}
