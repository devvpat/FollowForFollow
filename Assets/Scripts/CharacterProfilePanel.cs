using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterProfilePanel : MonoBehaviour
{
    [Header("UI References")]
    public Image portraitImage;
    public TMP_Text realNameText;
    public TMP_Text ignText;
    public TMP_Dropdown roleDropdown;

    [Header("Default")]
    public CharacterProfile defaultProfile;

    CharacterProfile current;

    void Start()
    {
        if (roleDropdown != null)
        {
            roleDropdown.ClearOptions();
            roleDropdown.AddOptions(new List<string> { "Support", "Attacker", "Defender", "Observer" });
            roleDropdown.onValueChanged.AddListener(OnRoleChanged);
        }

        Show(defaultProfile);
    }

    public void Show(CharacterProfile profile)
    {
        current = profile;
        if (profile == null)
            return;

        if (realNameText != null)
            realNameText.text = string.IsNullOrEmpty(profile.realName) ? "" : profile.realName;

        if (ignText != null)
            ignText.text = string.IsNullOrEmpty(profile.ign) ? "" : profile.ign;

        if (portraitImage != null)
        {
            portraitImage.sprite = profile.portraitSprite;
            portraitImage.enabled = profile.portraitSprite != null;
        }

        if (roleDropdown != null)
            roleDropdown.SetValueWithoutNotify((int)profile.partyRole);
    }

    void OnRoleChanged(int index)
    {
        if (current != null)
            current.partyRole = (PartyRole)index;
    }
}
