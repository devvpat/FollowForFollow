using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleLogPanel : MonoBehaviour
{
    public GameObject panelContainer;
    public ScrollRect scrollRect;
    public TMP_Text logText;
    public Button toggleButton;
    public TMP_Text toggleButtonLabel;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        NormalizeContentLayout(); // the log text doubles as the scroll content; it was hand-sized for
                                  // the old skill-list dump, so make it auto-fit and grow top-down
        if (logText != null) logText.text = ""; // wipe stale design-time text (old Info-view skill list)
        if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);
        IsOpen = true;                                  // start open — log streams live during combat
        if (panelContainer != null) panelContainer.SetActive(true);
        if (toggleButtonLabel != null) toggleButtonLabel.text = "Log";
    }

    // The logText's RectTransform is the ScrollRect content. Anchor it top-stretch with a top pivot
    // and add a vertical ContentSizeFitter so it sizes to the text and new lines push downward.
    private void NormalizeContentLayout()
    {
        if (logText == null) return;
        logText.alignment = TMPro.TextAlignmentOptions.TopLeft;

        var rt = logText.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y);

        var fitter = logText.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = logText.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    public void AppendLog(string message)
    {
        if (logText == null) return;
        logText.text += message + "\n";
        ScrollToBottom();

        if (BattleFxSettings.LogFlash && isActiveAndEnabled)
        {
            if (!_baseCaptured) { _logBaseColor = logText.color; _baseCaptured = true; }
            if (_flashCo != null) StopCoroutine(_flashCo);
            logText.color = _logBaseColor;
            _flashCo = StartCoroutine(FlashLog());
        }
    }

    private Color _logBaseColor = Color.white;
    private bool _baseCaptured;
    private Coroutine _flashCo;
    private System.Collections.IEnumerator FlashLog()
    {
        float t = 0f; const float dur = 0.25f;
        while (t < dur && logText != null)
        {
            t += Time.deltaTime;
            logText.color = Color.Lerp(Color.white, _logBaseColor, t / dur);
            yield return null;
        }
        if (logText != null) logText.color = _logBaseColor;
        _flashCo = null;
    }

    private void ScrollToBottom()
    {
        if (scrollRect == null) return;
        Canvas.ForceUpdateCanvases();           // make the layout reflect the new line before scrolling
        scrollRect.verticalNormalizedPosition = 0f; // 0 = bottom
    }

    public void Clear()
    {
        if (logText != null) logText.text = "";
    }

    public void Toggle()
    {
        IsOpen = !IsOpen;
        if (panelContainer != null) panelContainer.SetActive(IsOpen);
        if (IsOpen) ScrollToBottom();
    }
}
