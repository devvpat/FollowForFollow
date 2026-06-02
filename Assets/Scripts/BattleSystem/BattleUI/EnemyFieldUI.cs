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

    private static MimicSpriteLibrary _mimicLib;
    private static bool _mimicLibLoaded;

    public Enemy BoundEnemy => _enemy;
    public BattleCharacter Char => _enemy;

    public void Bind(Enemy enemy, Action<BattleCharacter> onClick)
    {
        _enemy = enemy;
        _onClick = onClick;
        if (nameText != null) nameText.text = enemy.Name;
        TintHpBar();
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
        RefreshMimicSprite();
        RefreshStatusIcons();
    }

    // If this enemy is a mimic that has copied an ally, show the matching mimic portrait.
    // Updates live because Refresh() runs on every battle state change (incl. the copy turn).
    private void RefreshMimicSprite()
    {
        if (enemySprite == null) return;
        var copied = _enemy.CopiedAlly;
        if (copied == null) return;

        if (!_mimicLibLoaded)
        {
            _mimicLib = Resources.Load<MimicSpriteLibrary>("MimicSpriteLibrary");
            _mimicLibLoaded = true;
        }
        if (_mimicLib == null) return;

        var sprite = _mimicLib.Get(copied.CharSkillSet);
        if (sprite != null)
        {
            enemySprite.sprite = sprite;
            enemySprite.preserveAspect = true;
        }
    }

    // Color the HP bar fill with the shared health color (same as the ally cards).
    private void TintHpBar()
    {
        if (hpBar == null || hpBar.fillRect == null) return;
        var fill = hpBar.fillRect.GetComponent<Image>();
        if (fill != null) fill.color = BattleUIColors.HPBarFill;
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

    // Quick "hop" on the enemy sprite when this enemy acts.
    public void PlayHop()
    {
        if (enemySprite == null || !isActiveAndEnabled) return;
        BattleSpriteFx.Hop(this, enemySprite.rectTransform);
    }

    // Shake + red flash when this enemy takes damage.
    public void PlayHurt()
    {
        if (enemySprite == null || !isActiveAndEnabled) return;
        BattleSpriteFx.Shake(this, enemySprite.rectTransform);
        BattleSpriteFx.Flash(this, enemySprite);
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
