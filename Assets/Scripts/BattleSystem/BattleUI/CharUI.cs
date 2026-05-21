using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Represents a single ally character in the game UI
public class CharUI : MonoBehaviour
{
    // ----- REFERENCES & CONFIGURATION -----

    [Header("References")]
    public TMP_Text nameText;
    public Slider hpBar;
    public TMP_Text hpText;
    public Slider manaBar;
    public TMP_Text manaText;
    public GameObject defendingBadge;
    public GameObject deadOverlay;
    public Image highlightRing;
    public Button bodyButton;

    [Header("Active highlight")]
    public Image cardBackground;
    public Color activeColor = new Color(0.3f, 0.6f, 1f, 0.5f);
    public Color inactiveColor = new Color(1f,   1f,   1f, 0f);

    [Header("Targeting colors")]
    public Color highlightedColor = new Color(1f, 0.85f, 0.1f, 1f);
    public Color normalColor = new Color(1f, 1f,    1f,   0f);

    private BattleCharacter _char;
    private Action<BattleCharacter> _onClickCallback;
    private Button _button;

    public BattleCharacter Char => _char;

    public void Bind(BattleCharacter character, Action<BattleCharacter> onClickCallback)
    {
        _char = character;
        _onClickCallback = onClickCallback;

        _button = bodyButton;
        _button.onClick.AddListener(OnClicked);

        nameText.text = character.Name;
        Refresh();
        SetHighlighted(false);
    }

    // ----- UI UPDATE METHODS -----

    // Update the UI based on current ally stats
    public void Refresh()
    {
        if (_char == null) return;

        // update hp
        hpBar.value = (float)_char.CurrentHP / _char.MaxHP;
        hpText.text = $"{_char.CurrentHP} / {_char.MaxHP}";

        // only show mana and defending badge for allies (hide for enemies)
        if (_char is Ally ally)
        {
            manaBar.value = (float)ally.CurrentMana / ally.MaxMana;
            manaBar.gameObject.SetActive(true);
            manaText.text = $"{ally.CurrentMana} / {ally.MaxMana}";
            manaText.gameObject.SetActive(true);
            defendingBadge.SetActive(_char.IsDefending);
        } else
        {
            manaBar.gameObject.SetActive(false);
            manaText.gameObject.SetActive(false);
            defendingBadge.SetActive(false);
        }

        // update dead overlay
        if (deadOverlay != null)
            deadOverlay.SetActive(!_char.IsAlive);

        // disable button if character is dead
        if (_button != null)
            _button.interactable = _char.IsAlive;
    }

    public void SetActive(bool active)
    {
        if (cardBackground != null)
            cardBackground.color = active ? activeColor : inactiveColor;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightRing != null)
            highlightRing.color = highlighted ? highlightedColor : normalColor;
    }

    private void OnClicked()
    {
        _onClickCallback?.Invoke(_char);
    }
}
