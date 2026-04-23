using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Represents a single enemy character in the game UI
public class EnemyCharUI : MonoBehaviour
{
    // ----- REFERENCES & CONFIGURATION -----

    [Header("References")]
    public TMP_Text nameText;
    public Slider hpBar;
    public TMP_Text hpText;
    public GameObject defendingBadge;
    public GameObject deadOverlay;
    public Image highlightRing;
    public Button bodyButton;

    [Header("Targeting colors")]
    public Color highlightedColor = new Color(1f, 0.85f, 0.1f, 1f);
    public Color normalColor = new Color(1f, 1f,    1f,   0f);

    private Enemy _enemy;
    private Action<Enemy> _onClickCallback;
    private Button _button;

    public Enemy Enemy => _enemy;

    public void Bind(Enemy enemy, Action<Enemy> onClickCallback)
    {
        _enemy = enemy;
        _onClickCallback = onClickCallback;

        _button = bodyButton;
        _button.onClick.AddListener(OnClicked);

        nameText.text = enemy.Name;
        Refresh();
        SetHighlighted(false);
    }

    // ----- UI UPDATE METHODS -----

    // Update the UI based on current enemy stats
    public void Refresh()
    {
        if (_enemy == null) return;

        hpBar.value = (float)_enemy.CurrentHP / _enemy.MaxHP;
        hpText.text = $"{_enemy.CurrentHP} / {_enemy.MaxHP}";

        if (defendingBadge != null)
            defendingBadge.SetActive(_enemy.IsDefending);

        if (deadOverlay != null)
            deadOverlay.SetActive(!_enemy.IsAlive);

        // Disable button if enemy is dead
        if (_button != null)
            _button.interactable = _enemy.IsAlive;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightRing != null)
            highlightRing.color = highlighted ? highlightedColor : normalColor;
    }

    private void OnClicked()
    {
        _onClickCallback?.Invoke(_enemy);
    }
}
