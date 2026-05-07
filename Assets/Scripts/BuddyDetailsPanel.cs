using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuddyDetailsPanel : MonoBehaviour
{
    [Header("Header")]
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text statusText;

    [Header("Stats")]
    public TMP_Text statsText;

    [Header("Items")]
    public TMP_Text itemsText;

    [Header("Controls")]
    public Button closeButton;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        gameObject.SetActive(false);
    }

    public void Show(CharacterProfile profile)
    {
        if (profile == null) return;

        if (portraitImage != null && profile.portraitSprite != null)
            portraitImage.sprite = profile.portraitSprite;

        if (nameText != null)
            nameText.text = string.IsNullOrEmpty(profile.realName)
                ? profile.ign
                : $"{profile.ign} ({profile.realName})";

        if (statusText != null)
            statusText.text = profile.statusMessage;

        if (statsText != null)
        {
            statsText.text =
                $"HP  {profile.maxHP}\n" +
                $"ATK {profile.attack}\n" +
                $"DEF {profile.defense}\n" +
                $"SPD {profile.speed}";
        }

        if (itemsText != null)
        {
            if (profile.items == null || profile.items.Count == 0)
            {
                itemsText.text = "(no items)";
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var item in profile.items)
                {
                    if (item == null) continue;
                    sb.Append("• ").Append(item.itemName);
                    if (!string.IsNullOrEmpty(item.description))
                        sb.Append(" — ").Append(item.description);
                    sb.Append('\n');
                }
                itemsText.text = sb.ToString();
            }
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}