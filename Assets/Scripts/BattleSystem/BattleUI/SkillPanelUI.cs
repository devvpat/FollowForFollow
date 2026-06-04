using UnityEngine;
using UnityEngine.UI;
using System;

public class SkillPanelUI : MonoBehaviour
{
    public SkillSlotUI[] skillSlots;
    public Button backButton;

    public event Action<int> OnSkillSelected;
    public event Action OnBackPressed;

    private void Awake()
    {
        backButton.onClick.AddListener(() => OnBackPressed?.Invoke());
    }

    public void Populate(Ally ally)
    {
        bool silenced = ally.IsSilenced || ally.IsForceSilenced;
        for (int i = 0; i < skillSlots.Length && i < ally.Skills.Length; i++)
        {
            ISkill skill = ally.Skills[i];
            skillSlots[i].Bind(i, skill, ally.CanAffordSkill(skill), silenced, idx => OnSkillSelected?.Invoke(idx));
        }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
