using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

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

    private float _targetHp;
    private bool _barInitialized;

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
        if (!_enemy.IsAlive)
        {
            if (!_hidden) { _hidden = true; HideOnDeath(); }
            return;
        }
        float newHp = _enemy.CurrentHP / _enemy.MaxHP;
        if (BattleFxSettings.HpChip && _barInitialized && newHp < _targetHp - 0.001f && hpBar != null && hpBar.fillRect != null)
        {
            var f = hpBar.fillRect.GetComponent<Image>();
            if (f != null) BattleSpriteFx.Flash(this, f, Color.white);
        }
        _targetHp = newHp;
        if (hpBar != null && (!BattleFxSettings.SmoothBars || !_barInitialized))
        {
            hpBar.value = _targetHp;
            _barInitialized = true;
        }
        if (deadOverlay != null) deadOverlay.SetActive(!_enemy.IsAlive);
        if (clickArea != null) clickArea.interactable = _enemy.IsAlive;
        RefreshSprite();
        RefreshStatusIcons();
    }

    private void Update()
    {
        if (BattleFxSettings.SmoothBars && _enemy != null && hpBar != null)
            hpBar.value = Mathf.MoveTowards(hpBar.value, _targetHp, Time.deltaTime * 1.5f);
    }

    private bool _attacking;
    private const float AttackFrameDuration = 0.4f;

    private bool _hidden;
    private const float DeathFadeDuration = 0.5f;

    // When the enemy dies, remove its whole field prefab from the battlefield (fade out if enabled).
    private void HideOnDeath()
    {
        if (clickArea != null) clickArea.interactable = false;
        if (!isActiveAndEnabled || !BattleFxSettings.DeathFade)
        {
            gameObject.SetActive(false);
            return;
        }
        StartCoroutine(FadeOutAndHide());
    }

    private IEnumerator FadeOutAndHide()
    {
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
        float t = 0f;
        float start = cg.alpha;
        while (t < DeathFadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, 0f, t / DeathFadeDuration);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    // Decides which sprite the enemy shows: mimics keep their copy-driven path; everyone else shows
    // their idle sprite (when they have one). Skipped while an attack-frame swap is mid-flight.
    private void RefreshSprite()
    {
        if (enemySprite == null || _enemy == null) return;
        if (_enemy.IsMimic) { RefreshMimicSprite(); return; }
        if (_attacking) return;
        if (_enemy.IdleSprite != null)
        {
            enemySprite.sprite = _enemy.IdleSprite;
            enemySprite.preserveAspect = true;
        }
    }

    // Briefly swap to the attack sprite while this enemy acts, then revert to idle.
    public void PlayAttack()
    {
        if (!isActiveAndEnabled || enemySprite == null || _enemy == null || _enemy.IsMimic) return;
        if (!BattleFxSettings.AttackSpriteSwap) return;
        if (_enemy.AttackSprite == null || _enemy.IdleSprite == null) return;
        StartCoroutine(AttackSwap());
    }

    private IEnumerator AttackSwap()
    {
        _attacking = true;
        enemySprite.sprite = _enemy.AttackSprite;
        enemySprite.preserveAspect = true;
        yield return new WaitForSeconds(AttackFrameDuration);
        _attacking = false;
        if (_enemy != null && _enemy.IdleSprite != null) enemySprite.sprite = _enemy.IdleSprite;
    }

    // Mimic visuals: show the base mimic sprite until it copies someone, then the copied ally's
    // mimic portrait. Updates live because Refresh() runs on every battle state change (incl. copy).
    private void RefreshMimicSprite()
    {
        if (enemySprite == null || !_enemy.IsMimic) return;

        if (!_mimicLibLoaded)
        {
            _mimicLib = Resources.Load<MimicSpriteLibrary>("MimicSpriteLibrary");
            _mimicLibLoaded = true;
        }
        if (_mimicLib == null) return;

        var copied = _enemy.CopiedAlly;
        var sprite = copied != null ? _mimicLib.Get(copied.CharSkillSet) : _mimicLib.defaultSprite;
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
