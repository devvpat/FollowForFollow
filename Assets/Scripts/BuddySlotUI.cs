using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuddySlotUI : MonoBehaviour
{
    [Header("UI Connections")]
    public Image portraitImage;
    public TMP_Text ignText;
    public TMP_Dropdown roleDropdown;
    public Button detailsButton;
    public Toggle selectToggle;

    [Header("Data Connection")]
    public CharacterProfile assignedProfile;
    public BuddyDetailsPanel detailsPanel;

    void Start()
    {
        ignText.text = assignedProfile.ign;
        if (assignedProfile.portraitSprite != null)
        {
            portraitImage.sprite = assignedProfile.portraitSprite;
        }

        roleDropdown.ClearOptions();
        List<string> perceptionRoles = new List<string>
        {
            "Support",
            "Attacker",
            "Defender",
            "Observer"
        };
        roleDropdown.AddOptions(perceptionRoles);

        if (detailsButton != null)
            detailsButton.onClick.AddListener(OpenDetails);

        if (selectToggle != null)
        {
            selectToggle.isOn = PartyManager.Instance != null
                                && PartyManager.Instance.IsSelected(assignedProfile);
            selectToggle.onValueChanged.AddListener(OnSelectChanged);
        }
    }

    void OpenDetails()
    {
        if (detailsPanel != null)
            detailsPanel.Show(assignedProfile);
    }

    void OnSelectChanged(bool isOn)
    {
        if (PartyManager.Instance != null)
            PartyManager.Instance.SetSelected(assignedProfile, isOn);
    }
}