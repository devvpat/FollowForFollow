using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Represents a single ally character in the game UI
public class AllyCharUI : MonoBehaviour
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

    [Header("Active highlight")]
    public Image cardBackground;
    public Color activeColor = new Color(0.3f, 0.6f, 1f, 0.5f);
    public Color inactiveColor = new Color(1f,   1f,   1f, 0.15f);

    private Ally _ally;

    public void Bind(Ally ally)
    {
        _ally = ally;
        nameText.text = ally.Name;
        Refresh();
        SetActive(false);
    }

    // ----- UI UPDATE METHODS -----

    // Update the UI based on current ally stats
    public void Refresh()
    {
        if (_ally == null) return;

        hpBar.value = (float)_ally.CurrentHP / _ally.MaxHP;
        hpText.text = $"{_ally.CurrentHP} / {_ally.MaxHP}";

        manaBar.value = (float)_ally.CurrentMana / _ally.MaxMana;
        manaText.text = $"{_ally.CurrentMana} / {_ally.MaxMana}";

        if (defendingBadge != null)
            defendingBadge.SetActive(_ally.IsDefending);

        if (deadOverlay != null)
            deadOverlay.SetActive(!_ally.IsAlive);
    }

    public void SetActive(bool active)
    {
        if (cardBackground != null)
            cardBackground.color = active ? activeColor : inactiveColor;
    }
}
