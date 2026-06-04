using TMPro;
using UnityEngine;

// A floating combat-text popup (e.g. damage numbers). Spawn() builds a TMP label at a UI anchor's
// position under its Canvas; the popup rises and fades, then destroys itself. Fully self-contained,
// so callers just invoke BattleFloatingText.Spawn(anchorRect, "12", Color.red).
public class BattleFloatingText : MonoBehaviour
{
    private const float Duration = 0.8f;
    private const float Rise = 70f;

    // The font the rest of the UI uses (PixelifySans), loaded once from TMP's Resources folder.
    private static TMP_FontAsset _font;
    private static bool _fontLoaded;
    private static TMP_FontAsset GameFont
    {
        get
        {
            if (!_fontLoaded)
            {
                _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/PixelifySans-Regular SDF");
                _fontLoaded = true;
            }
            return _font != null ? _font : TMP_Settings.defaultFontAsset;
        }
    }

    private RectTransform _rt;
    private TMP_Text _tmp;
    private Vector3 _startWorld;
    private float _t;

    private bool _shake;

    public static void Spawn(RectTransform anchor, string text, Color color, float scale = 1f, bool shake = false)
    {
        if (anchor == null) return;
        var canvas = anchor.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("FloatingText", typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(canvas.transform, false);
        rt.sizeDelta = new Vector2(220f, 64f);
        rt.position = anchor.position;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = 44f * scale;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
        if (GameFont != null) tmp.font = GameFont;

        var ft = go.AddComponent<BattleFloatingText>();
        ft._rt = rt;
        ft._tmp = tmp;
        ft._startWorld = rt.position;
        ft._shake = shake;
    }

    private void Update()
    {
        _t += Time.deltaTime;
        float p = Mathf.Clamp01(_t / Duration);
        if (_rt != null)
        {
            // Shake adds a quick horizontal jitter that decays over the first third (crit emphasis).
            float jitter = _shake ? Mathf.Sin(_t * 80f) * 6f * Mathf.Clamp01(1f - p * 3f) : 0f;
            _rt.position = _startWorld + new Vector3(jitter, p * Rise, 0f);
        }
        if (_tmp != null)
        {
            var c = _tmp.color;
            c.a = 1f - p;
            _tmp.color = c;
        }
        if (_t >= Duration) Destroy(gameObject);
    }
}
