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
    public TMP_Text statsText;
    public TMP_Text profileInfoText;

    [Header("Default")]
    public CharacterProfile defaultProfile;

    CharacterProfile current;

    void Start()
    {
        if (roleDropdown != null)
        {
            roleDropdown.ClearOptions();
            // Options are generated from the AllyRole enum (in declaration order) so dropdown index ==
            // (AllyRole)index, which OnRoleChanged relies on. We only prettify the display label.
            roleDropdown.AddOptions(BuildRoleLabels());
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
        {
            if (profile.allyData != null)
            {
                roleDropdown.interactable = true;
                roleDropdown.SetValueWithoutNotify((int)profile.allyData.Role);
            }
            else
            {
                roleDropdown.interactable = false;
                Debug.LogWarning($"CharacterProfile '{profile.name}' has no AllyData assigned; role dropdown disabled.");
            }
        }

        if (statsText != null)
        {
            if (profile.allyData != null)
            {
                AllyData d = profile.allyData;
                statsText.text =
                    $"HP: {d.MaxHP}\n" +
                    $"Mana: {d.MaxMana}\n" +
                    $"Atk: {d.Attack}\n" +
                    $"Def: {d.Defense}\n" +
                    $"Spd: {d.Speed}";
            }
            else
            {
                statsText.text = "";
            }
        }

        if (profileInfoText != null)
        {
            profileInfoText.text =
                $"Name: {Shorten(profile.realName)}\n" +
                $"Likes: {Shorten(profile.likes)}\n" +
                $"Dislikes: {Shorten(profile.dislikes)}";
        }
    }

    const int ShortenMaxChars = 40;
    static string OrDash(string s) => string.IsNullOrEmpty(s) ? "—" : s;
    static string Shorten(string s)
    {
        if (string.IsNullOrEmpty(s)) return "—";
        return s.Length <= ShortenMaxChars ? s : s.Substring(0, ShortenMaxChars - 1) + "…";
    }

    void OnRoleChanged(int index)
    {
        if (current == null || current.allyData == null) return;

        AllyRole newRole = (AllyRole)index;
        current.allyData.Role = newRole;

        if (AllyParty.Instance != null)
            AllyParty.Instance.UpdateAllyRole(current.allyData.Name, newRole);
    }

    // Enum names in AllyRole declaration order, with camelCase split into words for display
    // (e.g. "GlassCannon" -> "Glass Cannon", "BurstDPS" -> "Burst DPS"). Order is preserved so the
    // option index still equals the enum value.
    static List<string> BuildRoleLabels()
    {
        string[] names = System.Enum.GetNames(typeof(AllyRole));
        var labels = new List<string>(names.Length);
        foreach (string n in names)
            labels.Add(Spacify(n));
        return labels;
    }

    static string Spacify(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        // Split lower/digit -> Upper boundaries ("BurstDPS" -> "Burst DPS"),
        // then acronym -> Word boundaries ("DPSReport" -> "DPS Report"). Keeps acronyms intact.
        string s = System.Text.RegularExpressions.Regex.Replace(name, "([a-z0-9])([A-Z])", "$1 $2");
        s = System.Text.RegularExpressions.Regex.Replace(s, "([A-Z]+)([A-Z][a-z])", "$1 $2");
        return s;
    }
}
