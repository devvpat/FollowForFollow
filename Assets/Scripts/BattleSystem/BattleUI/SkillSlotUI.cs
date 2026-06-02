using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text mpCostText;
    public Image background;
    public Button button;

    // Slot colors are hardcoded constants (not serialized fields) so they ALWAYS apply at runtime.
    // The scene's serialized SkillSlotUI color values kept reverting to dark whenever Unity re-saved
    // the open scene, so driving them from code is the only reliable way to match the action panel.
    static readonly Color SlotNormal = new Color(1f, 1f, 1f, 1f);
    static readonly Color SlotHighlighted = new Color(0.72f, 0.85f, 1f, 1f);
    static readonly Color SlotDisabled = new Color(0.7f, 0.7f, 0.7f, 0.6f);

    private int _index;
    private System.Action<int> _onClicked;

    public void Bind(int index, ISkill skill, bool canAfford, bool isSilenced, System.Action<int> onClicked)
    {
        _index = index;
        _onClicked = onClicked;
        nameText.text = skill.Name;
        descriptionText.text = skill.Description;
        mpCostText.text = $"{skill.ManaCost} MP";
        ApplyTextFormatting();

        bool interactable = canAfford && !isSilenced;
        // The Button's targetGraphic is this same background Image, so its ColorTint transition
        // would multiply our slot colors by the Pressed/Selected/Disabled tints and drive the
        // slot to near-black. Disable the transition so our explicit colors are the only source.
        button.transition = Selectable.Transition.None;
        button.interactable = interactable;
        background.color = interactable ? SlotNormal : SlotDisabled;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClicked?.Invoke(_index));
    }

    // Readable, consistent text formatting for the three slot labels. Applied in code so all four
    // slots match regardless of their scene-authored font sizes; the slot's RectTransform bands
    // (set in the scene) place name/cost/description top→bottom without overlap.
    private void ApplyTextFormatting()
    {
        var textColor = new Color(0.196f, 0.196f, 0.196f); // dark, matches the action-button labels
        if (nameText != null)
        {
            nameText.color = textColor;
            nameText.fontSize = 26;
            nameText.alignment = TextAlignmentOptions.Top;
        }
        if (mpCostText != null)
        {
            mpCostText.color = textColor;
            mpCostText.fontSize = 18;
            mpCostText.alignment = TextAlignmentOptions.Center;
        }
        if (descriptionText != null)
        {
            descriptionText.color = textColor;
            descriptionText.fontSize = 15;
            descriptionText.alignment = TextAlignmentOptions.Top;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (button.interactable)
            background.color = highlighted ? SlotHighlighted : SlotNormal;
    }
}
