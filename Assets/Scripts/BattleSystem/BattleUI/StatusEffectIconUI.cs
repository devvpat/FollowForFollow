using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectIconUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text durationText;
    public TMP_Text labelText;

    [Header("Icons")]
    public StatusEffectIconLibrary iconLibrary;

    // Fallback library loaded from Resources when none is assigned in the Inspector. Lets the icons
    // work without a serialized reference on the prefab (which Unity keeps clobbering while open).
    private static StatusEffectIconLibrary _fallbackLibrary;
    private static bool _fallbackLoaded;

    private StatusEffectIconLibrary Library
    {
        get
        {
            if (iconLibrary != null) return iconLibrary;
            if (!_fallbackLoaded)
            {
                _fallbackLibrary = Resources.Load<StatusEffectIconLibrary>("StatusEffectIconLibrary");
                _fallbackLoaded = true;
            }
            return _fallbackLibrary;
        }
    }

    private static readonly Color BuffColor = new Color(0.3f, 0.7f, 0.4f, 1f);
    private static readonly Color DebuffColor = new Color(0.8f, 0.25f, 0.25f, 1f);
    private static readonly Color OtherColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    public void Bind(BaseStatusEffect effect)
    {
        var library = Library;
        Sprite sprite = library != null ? library.Get(effect.Icon) : null;
        if (sprite != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                iconImage.color = Color.white;
            }
            if (labelText != null)
                labelText.gameObject.SetActive(false);
        }
        else
        {
            // Fallback: tint by category and show the effect's first letter
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.color = effect.EffectType switch
                {
                    StatusEffectType.Buff => BuffColor,
                    StatusEffectType.Debuff => DebuffColor,
                    _ => OtherColor
                };
            }
            if (labelText != null)
            {
                labelText.gameObject.SetActive(true);
                labelText.text = effect.Name.Length > 0 ? effect.Name[0].ToString() : "?";
            }
        }
        Refresh(effect);

        if (BattleFxSettings.StatusIconPop)
            BattleSpriteFx.ScalePunch(this, transform, 0f);
    }

    public void Refresh(BaseStatusEffect effect)
    {
        if (durationText != null)
            durationText.text = effect.RemainingDuration.ToString();
    }
}
