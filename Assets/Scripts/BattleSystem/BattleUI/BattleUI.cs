using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Handles all functionality related to the battle UI:
public class BattleUI : MonoBehaviour
{
    // ----- REFERENCES -----

    [Header("Panels")]
    public Transform allyPanel;
    public Transform enemyPanel;
    public GameObject actionPanel;
    public GameObject resultOverlay;

    [Header("Action Buttons")]
    public Button attackButton;
    public Button skillButton;
    public Button defendButton;

    [Header("Skill Submenu")]
    public GameObject skillPanel;
    public Button[] skillButtons; // should have 4
    public TMP_Text[] skillButtonsTexts; // should have 4
    public Button backButton;

    [Header("Result Overlay")]
    public TMP_Text resultTitleText;
    public Button   resultButton;
    public TMP_Text resultButtonLabel;

    [Header("Battle Log")]
    public TMP_Text battleLogText;

    [Header("Prefabs")]
    public GameObject allyCardPrefab;
    public GameObject enemyCardPrefab;

    // ----- INTERNAL STATE -----

    private List<AllyCharUI> _allyCharsUI = new();
    private List<EnemyCharUI> _enemyCharsUI = new();

    private AllyActionType _selectedAction;
    private ISkill _selectedSkill;
    private BattleCharacter _selectedTarget;
    private bool _targetingMode; // true when player must click an enemy

    // ----- EVENT SUBSCRIPTIONS -----

    private void OnEnable()
    {
        BattleManager.Instance.OnBattleStart += HandleBattleStart;
        BattleManager.Instance.OnLogMessage += AppendLog;
        BattleManager.Instance.OnStateChanged += Refresh;
        BattleManager.Instance.OnAllyTurnStart += HandleAllyTurnStart;
        BattleManager.Instance.OnEnemyTurnStart += HandleEnemyTurnStart;
        BattleManager.Instance.OnBattleEnd += HandleBattleEnd;

        attackButton.onClick.AddListener(OnAttackPressed);
        skillButton.onClick.AddListener(OnSkillMenuPressed);
        defendButton.onClick.AddListener(OnDefendPressed);
        backButton.onClick.AddListener(OnBackPressed);
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int index = i;
            skillButtons[i].onClick.AddListener(() => OnSkillPressed(index));
        }
    }

    private void OnDisable()
    {
        if (BattleManager.Instance == null) return;
        BattleManager.Instance.OnBattleStart -= HandleBattleStart;
        BattleManager.Instance.OnLogMessage -= AppendLog;
        BattleManager.Instance.OnStateChanged -= Refresh;
        BattleManager.Instance.OnAllyTurnStart -= HandleAllyTurnStart;
        BattleManager.Instance.OnEnemyTurnStart -= HandleEnemyTurnStart;
        BattleManager.Instance.OnBattleEnd -= HandleBattleEnd;

        attackButton.onClick.RemoveListener(OnAttackPressed);
        skillButton.onClick.RemoveListener(OnSkillMenuPressed);
        defendButton.onClick.RemoveListener(OnDefendPressed);
        backButton.onClick.RemoveListener(OnBackPressed);
        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].onClick.RemoveListener(() => OnSkillPressed(i));
        }

    }

    // ----- EVENT HANDLERS -----

    private void HandleBattleStart()
    {
        resultOverlay.SetActive(false);
        battleLogText.text = "";
        BuildAllyCharsUI();
        BuildEnemyCharsUI();
        actionPanel.SetActive(false);
    }

    private void HandleAllyTurnStart(Ally ally)
    {
        _selectedAction = AllyActionType.Attack;
        _selectedTarget = null;
        _targetingMode = false;

        // Highlight active ally card
        for (int i = 0; i < _allyCharsUI.Count; i++)
            _allyCharsUI[i].SetActive(BattleManager.Instance.Allies[i] == ally);

        // Update action area
        actionPanel.SetActive(true);
        skillPanel.SetActive(false);

        // Clear enemy targeting highlights
        foreach (var card in _enemyCharsUI)
            card.SetHighlighted(false);
    }

    private void HandleEnemyTurnStart(BattleCharacter enemy)
    {
        // Disable action panel + set all ally cards to inactive and all enemy cards to non-highlighted
        actionPanel.SetActive(false);
        foreach (var card in _allyCharsUI) card.SetActive(false);
        foreach (var card in _enemyCharsUI) card.SetHighlighted(false);
    }

    private void HandleBattleEnd(bool playerWon)
    {
        actionPanel.SetActive(false);
        resultOverlay.SetActive(true);

        if (playerWon)
        {
            // Show victory message and set button to start next fight
            resultTitleText.text = "Victory!";
            resultButtonLabel.text = "Continue";
            resultButton.onClick.RemoveAllListeners();
            resultButton.onClick.AddListener(() => gameObject.SetActive(false)); // Hide the UI
        }
        else
        {
            // Show defeat message and set button to restart
            resultTitleText.text = "Defeat!";
            resultButtonLabel.text = "Replay";
            resultButton.onClick.RemoveAllListeners();
            resultButton.onClick.AddListener(OnRestart);
        }
    }

    // ----- UI BUILDING METHODS -----

    private void BuildAllyCharsUI()
    {
        foreach (Transform t in allyPanel) Destroy(t.gameObject);
        _allyCharsUI.Clear();

        // Create an ally char ui for each ally
        foreach (var ally in BattleManager.Instance.Allies)
        {
            var go = Instantiate(allyCardPrefab, allyPanel);
            var card = go.GetComponent<AllyCharUI>();
            card.Bind(ally);
            _allyCharsUI.Add(card);
        }
    }

    private void BuildEnemyCharsUI()
    {
        foreach (Transform t in enemyPanel) Destroy(t.gameObject);
        _enemyCharsUI.Clear();

        // Create an enemy char ui for each enemy
        foreach (var enemy in BattleManager.Instance.Enemies)
        {
            var go = Instantiate(enemyCardPrefab, enemyPanel);
            var card = go.GetComponent<EnemyCharUI>();
            card.Bind(enemy, OnEnemyCardClicked);
            _enemyCharsUI.Add(card);
        }
    }

    private void PopulateSkillPanel()
    {
        Ally ally = BattleManager.Instance.CurrentAlly;
        if (ally == null) return;

        for (int i = 0; i < skillButtons.Length; i++)
        {
            ISkill skill = ally.Skills[i];
            skillButtonsTexts[i].text = $"{skill.Name}\n({skill.ManaCost} MP)";
            skillButtons[i].interactable = ally.CanAffordSkill(skill);
        }
    }

    // ----- UI UPDATE METHOD -----

    private void Refresh()
    {
        foreach (var card in _allyCharsUI) card.Refresh();
        foreach (var card in _enemyCharsUI) card.Refresh();
    }

    // ----- ACTION BUTTON HANDLERS -----

    private void OnAttackPressed()
    {
        _selectedAction = AllyActionType.Attack;
        EnterTargetingMode();
    }

    private void OnDefendPressed()
    {
        // No need for a target when defending
        BattleManager.Instance.SubmitAllyAction(PendingAllyAction.MakeDefend());
        actionPanel.SetActive(false);
        _targetingMode = false;
    }

    private void EnterTargetingMode()
    {
        _targetingMode = true;
        AppendLog("[*] Select a target…");
        foreach (var card in _enemyCharsUI)
            card.SetHighlighted(card.Enemy.IsAlive);
    }

    private void OnEnemyCardClicked(BattleCharacter enemy)
    {
        if (!_targetingMode || !enemy.IsAlive) return;

        _selectedTarget = enemy;
        _targetingMode  = false;

        foreach (var card in _enemyCharsUI) card.SetHighlighted(false);

        // Create appropriate action based on selected action type and submit to battle manager
        PendingAllyAction action = _selectedAction switch
        {
            AllyActionType.Attack => PendingAllyAction.MakeAttack(_selectedTarget),
            AllyActionType.Skill => PendingAllyAction.MakeSkill(_selectedTarget, _selectedSkill),
            _ => PendingAllyAction.MakeAttack(_selectedTarget)
        };

        BattleManager.Instance.SubmitAllyAction(action);
        actionPanel.SetActive(false);
    }

    private void OnSkillMenuPressed()
    {
        skillPanel.SetActive(true);
        PopulateSkillPanel();
    }

    private void OnSkillPressed(int index)
    {
        Ally ally = BattleManager.Instance.CurrentAlly;
        if (ally == null) return;

        ISkill skill = ally.Skills[index];
        if (!ally.CanAffordSkill(skill)) return;

        _selectedAction = AllyActionType.Skill;
        _selectedSkill = skill;

        skillPanel.SetActive(false);

        if (skill.NeedsTarget)
        {
            EnterTargetingMode();
        } else
        {
            // If skill doesn't need target, submit action immediately with ally as target
            BattleManager.Instance.SubmitAllyAction(PendingAllyAction.MakeSkill(ally, skill));
            actionPanel.SetActive(false);
        }
    }

    private void OnBackPressed()
    {
        skillPanel.SetActive(false);
    }

    // ----- LOG -----

    private void AppendLog(string msg)
    {
        battleLogText.text += msg + "\n";
    }

    // ----- RESTART (DEFEAT) -----

    private void OnRestart()
    {
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}