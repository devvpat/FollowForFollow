using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AllyCardUI : MonoBehaviour
{
    [Header("Display")]
    public Image portraitImage;
    public TMP_Text nameText;
    public Slider hpBar;
    public TMP_Text hpText;
    public Slider mpBar;
    public TMP_Text mpText;

    [Header("Highlights")]
    public Image activeHighlight;
    public Image targetHighlight;

    [Header("Status Effects")]
    public Transform statusIconContainer;
    public GameObject statusIconPrefab;

    [Header("State Overlays")]
    public GameObject defendingBadge;
    public GameObject deadOverlay;

    [Header("Interaction")]
    public Button cardButton;

    private Ally _ally;
    private Action<BattleCharacter> _onClick;

    public Ally BoundAlly => _ally;

    public void Bind(Ally ally, Color portraitColor, Action<BattleCharacter> onClick)
    {
        _ally = ally;
        _onClick = onClick;
        nameText.text = ally.Name;
        if (portraitImage != null) portraitImage.color = portraitColor;
        cardButton.onClick.AddListener(() => _onClick?.Invoke(_ally));
        SetActive(false);
        SetHighlighted(false);
        Refresh();
    }

    public void Refresh()
    {
        if (_ally == null) return;
        hpBar.value = _ally.CurrentHP / _ally.MaxHP;
        hpText.text = $"{_ally.CurrentHP:F0} / {_ally.MaxHP:F0}";
        mpBar.value = (float)_ally.CurrentMana / _ally.MaxMana;
        mpText.text = $"{_ally.CurrentMana} / {_ally.MaxMana}";
        if (defendingBadge != null) defendingBadge.SetActive(_ally.IsDefending);
        if (deadOverlay != null) deadOverlay.SetActive(!_ally.IsAlive);
        if (cardButton != null) cardButton.interactable = _ally.IsAlive;
        RefreshStatusIcons();
    }

    private void RefreshStatusIcons()
    {
        if (statusIconContainer == null || statusIconPrefab == null) return;
        foreach (Transform child in statusIconContainer)
            Destroy(child.gameObject);
        foreach (var effect in _ally.StatusEffects)
        {
            var icon = Instantiate(statusIconPrefab, statusIconContainer);
            var iconUI = icon.GetComponent<StatusEffectIconUI>();
            if (iconUI != null) iconUI.Bind(effect);
        }
    }

    public void SetActive(bool active)
    {
        if (activeHighlight != null)
            activeHighlight.enabled = active;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (targetHighlight != null)
            targetHighlight.enabled = highlighted;
    }
}
