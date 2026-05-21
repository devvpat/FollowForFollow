using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    public TMP_Text bodyText;
    public TMP_Text nameText;
    public Image portraitImage;

    Sprite defaultPortraitSprite;

    void Awake()
    {
        if (portraitImage != null)
            defaultPortraitSprite = portraitImage.sprite;
    }

    public void Setup(CharacterProfile sender, string initialText)
    {
        if (nameText != null)
            nameText.text = sender != null ? sender.ign : "";

        if (portraitImage != null)
        {
            bool hasPortrait = sender != null && sender.portraitSprite != null;
            portraitImage.sprite = hasPortrait ? sender.portraitSprite : defaultPortraitSprite;
            portraitImage.enabled = true;
            portraitImage.color = hasPortrait
                ? Color.white
                : (sender != null ? sender.themeColor : Color.white);
        }

        SetText(initialText);
    }

    public void SetText(string text)
    {
        if (bodyText != null)
            bodyText.text = text ?? "";
    }
}
