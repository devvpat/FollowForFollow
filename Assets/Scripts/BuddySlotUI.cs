using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; 

public class BuddySlotUI : MonoBehaviour
{
    [Header("UI Connections")]
    public Image portraitImage;
    public TMP_Text ignText;
    public TMP_Dropdown roleDropdown;

    [Header("Data Connection")]
    public CharacterProfile assignedProfile;

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
    }
}