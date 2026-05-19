using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    public TMP_Text bodyText;
    public TMP_Text nameText;
    public Image portraitImage;

    public void Setup(CharacterProfile sender, string initialText)
    {
        if (nameText != null)
            nameText.text = sender != null ? sender.ign : "";

        if (portraitImage != null)
        {
            if (sender != null && sender.portraitSprite != null)
            {
                portraitImage.sprite = sender.portraitSprite;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }

        SetText(initialText);
    }

    public void SetText(string text)
    {
        if (bodyText != null)
            bodyText.text = text ?? "";
    }
}
