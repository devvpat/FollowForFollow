using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyDetailPanelUI : MonoBehaviour
{
    [Header("Display")]
    public TMP_Text nameText;
    public Slider hpBar;
    public TMP_Text hpText;
    public Transform statusEffectContainer;
    public Button closeButton;

    [Header("Status Icons")]
    public GameObject statusIconPrefab;

    private Enemy _enemy;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public void Show(Enemy enemy)
    {
        _enemy = enemy;
        gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _enemy = null;
    }

    public void Refresh()
    {
        if (_enemy == null) return;

        nameText.text = _enemy.Name;
        hpBar.value = _enemy.CurrentHP / _enemy.MaxHP;
        if (hpText != null)
            hpText.text = $"{_enemy.CurrentHP:F0} / {_enemy.MaxHP:F0}";

        RefreshStatusIcons();
    }

    private void RefreshStatusIcons()
    {
        if (statusEffectContainer == null || statusIconPrefab == null) return;

        foreach (Transform child in statusEffectContainer)
            Destroy(child.gameObject);

        foreach (var effect in _enemy.StatusEffects)
        {
            var go = Instantiate(statusIconPrefab, statusEffectContainer);
            var iconUI = go.GetComponent<StatusEffectIconUI>();
            if (iconUI != null) iconUI.Bind(effect);
        }
    }

    public bool HasEnemy => _enemy != null;
    public Enemy CurrentEnemy => _enemy;
}
