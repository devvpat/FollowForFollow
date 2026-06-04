using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Plays a short, auto-timed image cutscene (e.g. the room1 -> room2 -> room3 "arrive home"
/// sequence) as a fullscreen overlay, then hands off via <see cref="OnTransitionFinished"/>.
///
/// It builds its own overlay Canvas at runtime, so it can live on a bare GameObject without
/// any pre-authored UI hierarchy. Each frame fades in (cross-dissolving from the previous one),
/// holds for its configured time, then advances automatically. The shared
/// <see cref="DialogueInput.AdvancePressed"/> helper lets the player skip the current hold,
/// keeping the controls consistent with the dialogue/chat systems.
/// </summary>
public class RoomTransitionPlayer : MonoBehaviour
{
    [Serializable]
    public class Frame
    {
        public Sprite image;

        [TextArea(1, 3)]
        [Tooltip("Optional caption shown along the bottom. Leave blank for no caption.")]
        public string caption;

        [Tooltip("How long this frame stays on screen after fading in (seconds).")]
        public float holdSeconds = 1.5f;
    }

    [Header("Content")]
    public List<Frame> frames = new List<Frame>();

    [Header("Timing")]
    [Tooltip("Cross-dissolve duration between frames, and the initial fade-in from black (seconds).")]
    public float crossfadeSeconds = 0.5f;
    [Tooltip("Fade-to-black duration after the final frame, before handing off (seconds).")]
    public float endFadeSeconds = 0.75f;

    [Header("Playback")]
    public bool playOnStart = true;
    [Tooltip("Allow Space/Enter/click to skip the current frame's remaining hold.")]
    public bool allowClickSkip = true;
    [Tooltip("Sorting order of the overlay canvas. Keep high so it covers the scene.")]
    public int sortingOrder = 1000;

    [Header("Events")]
    [Tooltip("Fired once the cutscene has fully faded to black. Wire this to whatever should run next " +
             "(e.g. DialoguePlayer.PlayCurrent to start the chat intro).")]
    public UnityEvent OnTransitionFinished;

    GameObject overlayRoot;
    Image frontImage;   // currently displayed frame
    Image backImage;    // scratch image used to cross-dissolve the next frame in
    Image fadeBlack;    // top-most black used for the start fade-in and end fade-out
    TextMeshProUGUI captionText;
    bool isPlaying;

    void Start()
    {
        if (playOnStart) Play();
    }

    /// <summary>Begin the cutscene. Safe to call once; ignored while already playing.</summary>
    public void Play()
    {
        if (isPlaying) return;

        if (frames == null || frames.Count == 0)
        {
            // Nothing to show - hand off immediately so the flow isn't left hanging.
            OnTransitionFinished?.Invoke();
            return;
        }

        BuildOverlay();
        StartCoroutine(PlayRoutine());
    }

    void BuildOverlay()
    {
        overlayRoot = new GameObject("RoomTransitionOverlay",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        overlayRoot.transform.SetParent(transform, false);

        var canvas = overlayRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = overlayRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Bottom-to-top draw order: black backdrop, the two frame images, caption, then the fade layer.
        CreateFullscreenImage("Backdrop", Color.black, preserveAspect: false); // fills letterbox gaps
        backImage = CreateFullscreenImage("FrameB", Color.white, preserveAspect: true);
        frontImage = CreateFullscreenImage("FrameA", Color.white, preserveAspect: true);
        captionText = CreateCaption();
        fadeBlack = CreateFullscreenImage("FadeBlack", Color.black, preserveAspect: false);

        SetAlpha(backImage, 0f);
        SetAlpha(frontImage, 0f);
        SetAlpha(fadeBlack, 1f); // start fully black; PlayRoutine fades it out onto frame 0
    }

    Image CreateFullscreenImage(string name, Color color, bool preserveAspect)
    {
        var go = new GameObject(name, typeof(Image));
        go.transform.SetParent(overlayRoot.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.color = color;
        img.preserveAspect = preserveAspect;
        img.raycastTarget = false;
        return img;
    }

    TextMeshProUGUI CreateCaption()
    {
        var go = new GameObject("Caption", typeof(TextMeshProUGUI));
        go.transform.SetParent(overlayRoot.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.06f);
        rt.anchorMax = new Vector2(0.9f, 0.24f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Bottom;
        tmp.fontSize = 42f;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        tmp.text = "";
        return tmp;
    }

    IEnumerator PlayRoutine()
    {
        isPlaying = true;

        // Frame 0: place on the front image and reveal by fading the black layer out.
        ShowOnFront(frames[0]);
        yield return Fade(fadeBlack, 1f, 0f, crossfadeSeconds);
        yield return HoldFrame(frames[0].holdSeconds);

        // Remaining frames: cross-dissolve the new frame in over the current one.
        for (int i = 1; i < frames.Count; i++)
        {
            Frame frame = frames[i];
            backImage.sprite = frame.image;
            SetAlpha(backImage, 0f);

            // Put the incoming image directly above the current one, keep caption + fade on top.
            backImage.transform.SetAsLastSibling();
            captionText.transform.SetAsLastSibling();
            fadeBlack.transform.SetAsLastSibling();

            yield return Fade(backImage, 0f, 1f, crossfadeSeconds);

            // The incoming image is now the visible one; swap roles for the next iteration.
            (frontImage, backImage) = (backImage, frontImage);
            SetAlpha(backImage, 0f);
            SetCaption(frame.caption);

            yield return HoldFrame(frame.holdSeconds);
        }

        // Fade to black, then hand off. The existing DialoguePlayer raises its own black overlay
        // synchronously inside PlayCurrent, so disabling ourselves one frame later avoids any flash.
        yield return Fade(fadeBlack, 0f, 1f, endFadeSeconds);

        OnTransitionFinished?.Invoke();
        yield return null;

        isPlaying = false;
        if (overlayRoot != null) overlayRoot.SetActive(false);
    }

    void ShowOnFront(Frame frame)
    {
        frontImage.sprite = frame.image;
        SetAlpha(frontImage, 1f);
        SetAlpha(backImage, 0f);
        SetCaption(frame.caption);
    }

    IEnumerator HoldFrame(float seconds)
    {
        float elapsed = 0f;
        float target = Mathf.Max(0f, seconds);
        while (elapsed < target)
        {
            if (allowClickSkip && DialogueInput.AdvancePressed()) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Fade(Image img, float from, float to, float seconds)
    {
        if (img == null) yield break;

        float dur = Mathf.Max(0.01f, seconds);
        float t = 0f;
        SetAlpha(img, from);
        while (t < dur)
        {
            t += Time.deltaTime;
            SetAlpha(img, Mathf.Lerp(from, to, t / dur));
            yield return null;
        }
        SetAlpha(img, to);
    }

    void SetCaption(string caption)
    {
        if (captionText == null) return;
        captionText.text = caption ?? "";
        captionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(caption));
    }

    static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color;
        c.a = a;
        g.color = c;
    }
}
