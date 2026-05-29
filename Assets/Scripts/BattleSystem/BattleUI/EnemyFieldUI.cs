using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class EnemyFieldUI : MonoBehaviour
{
    [Header("Sprite")]
    public Image enemySprite;

    [Header("Floating Info")]
    public TMP_Text nameText;
    public Slider hpBar;
    public Transform statusIconContainer;

    [Header("Selection")]
    public Image selectionIndicator;
    public Button clickArea;

    [Header("State")]
    public GameObject deadOverlay;

    [Header("Status Icons")]
    public GameObject statusIconPrefab;

    private Enemy _enemy;
    private Action<BattleCharacter> _onClick;

    public Enemy BoundEnemy => _enemy;
    public BattleCharacter Char => _enemy;

    public void Bind(Enemy enemy, Action<BattleCharacter> onClick)
    {
        _enemy = enemy;
        _onClick = onClick;
        nameText.text = enemy.Name;
        clickArea.onClick.AddListener(() => _onClick?.Invoke(_enemy));
        SetHighlighted(false);
        SetSelected(false);
        Refresh();
    }

    public void Refresh()
    {
        if (_enemy == null) return;
        hpBar.value = _enemy.CurrentHP / _enemy.MaxHP;
        if (deadOverlay != null) deadOverlay.SetActive(!_enemy.IsAlive);
        if (clickArea != null) clickArea.interactable = _enemy.IsAlive;
        RefreshStatusIcons();
    }

    private void RefreshStatusIcons()
    {
        if (statusIconContainer == null || statusIconPrefab == null) return;
        foreach (Transform child in statusIconContainer)
            Destroy(child.gameObject);
        foreach (var effect in _enemy.StatusEffects)
        {
            var icon = Instantiate(statusIconPrefab, statusIconContainer);
            var iconUI = icon.GetComponent<StatusEffectIconUI>();
            if (iconUI != null) iconUI.Bind(effect);
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (selectionIndicator != null)
            selectionIndicator.enabled = highlighted;
    }

    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
            selectionIndicator.color = selected
                ? new Color(1f, 0.6f, 0.2f, 0.8f)
                : new Color(1f, 0.85f, 0.1f, 0.6f);
    }
}
