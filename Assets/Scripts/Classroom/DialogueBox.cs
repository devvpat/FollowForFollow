using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    public TMP_Text bodyText;
    public TMP_Text nameText;
    public Image portraitImage;
    public Image nameBackground;

    Sprite defaultPortraitSprite;

    void Awake()
    {
        if (portraitImage != null)
            defaultPortraitSprite = portraitImage.sprite;
    }

    public void Setup(CharacterProfile sender, string initialText)
    {
        if (nameText != null)
            nameText.text = sender != null
                ? (!string.IsNullOrEmpty(sender.realName) ? sender.realName : sender.ign)
                : "";

        if (portraitImage != null)
        {
            portraitImage.sprite = defaultPortraitSprite;
            portraitImage.enabled = true;
            portraitImage.color = sender != null ? sender.themeColor : Color.white;
        }

        if (nameBackground != null)
            nameBackground.color = sender != null ? sender.themeColor : Color.white;

        SetText(initialText);
    }

    public void SetText(string text)
    {
        if (bodyText != null)
            bodyText.text = text ?? "";
    }
}
