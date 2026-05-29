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

    [Header("Colors")]
    public Color normalColor = new Color(0.22f, 0.18f, 0.30f, 0.85f);
    public Color highlightedColor = new Color(0.40f, 0.30f, 0.55f, 0.95f);
    public Color disabledColor = new Color(0.15f, 0.13f, 0.18f, 0.6f);

    private int _index;
    private System.Action<int> _onClicked;

    public void Bind(int index, ISkill skill, bool canAfford, bool isSilenced, System.Action<int> onClicked)
    {
        _index = index;
        _onClicked = onClicked;
        nameText.text = skill.Name;
        descriptionText.text = skill.Description;
        mpCostText.text = $"{skill.ManaCost} MP";

        bool interactable = canAfford && !isSilenced;
        button.interactable = interactable;
        background.color = interactable ? normalColor : disabledColor;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClicked?.Invoke(_index));
    }

    public void SetHighlighted(bool highlighted)
    {
        if (button.interactable)
            background.color = highlighted ? highlightedColor : normalColor;
    }
}
