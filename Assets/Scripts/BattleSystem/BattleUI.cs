using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public Button specialButton;
    public Button defendButton;

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
    private Enemy _selectedTarget;
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
        specialButton.onClick.AddListener(OnSpecialPressed);
        defendButton.onClick.AddListener(OnDefendPressed);
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

        // Update action buttons
        actionPanel.SetActive(true);
        specialButton.interactable = ally.CanUseSpecial;

        // Clear enemy targeting highlights
        foreach (var card in _enemyCharsUI)
            card.SetHighlighted(false);
    }

    private void HandleEnemyTurnStart(Enemy enemy)
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
            resultButtonLabel.text = "Next Fight";
            resultButton.onClick.RemoveAllListeners();
            resultButton.onClick.AddListener(() => BattleManager.Instance.StartNewFight());
        }
        else
        {
            // Show defeat message and set button to restart
            resultTitleText.text = "Defeat!";
            resultButtonLabel.text = "Restart";
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

    // ----- UI UPDATE METHOD -----

    private void Refresh()
    {
        foreach (var card in _allyCharsUI) card.Refresh();
        foreach (var card in _enemyCharsUI) card.Refresh();

        // Check if special button needs to be disabled
        if (BattleManager.Instance.WaitingForInput)
        {
            Ally ally = BattleManager.Instance.CurrentAlly;
            if (ally != null)
                specialButton.interactable = ally.CanUseSpecial;
        }
    }

    // ----- ACTION BUTTON HANDLERS -----

    private void OnAttackPressed()
    {
        _selectedAction = AllyActionType.Attack;
        EnterTargetingMode();
    }

    private void OnSpecialPressed()
    {
        _selectedAction = AllyActionType.SpecialAttack;
        EnterTargetingMode();
    }

    private void OnDefendPressed()
    {
        // No need for a target when defending
        BattleManager.Instance.SubmitAllyAction(PendingAllyAction.MakeDefend());
        actionPanel.SetActive(false);
    }

    private void EnterTargetingMode()
    {
        _targetingMode = true;
        AppendLog("[*] Select a target…");
        foreach (var card in _enemyCharsUI)
            card.SetHighlighted(card.Enemy.IsAlive);
    }

    private void OnEnemyCardClicked(Enemy enemy)
    {
        if (!_targetingMode || !enemy.IsAlive) return;

        _selectedTarget = enemy;
        _targetingMode  = false;

        foreach (var card in _enemyCharsUI) card.SetHighlighted(false);

        // Create appropriate action based on selected action type and submit to battle manager
        PendingAllyAction action = _selectedAction switch
        {
            AllyActionType.Attack => PendingAllyAction.MakeAttack(_selectedTarget),
            AllyActionType.SpecialAttack => PendingAllyAction.MakeSpecial(_selectedTarget),
            _ => PendingAllyAction.MakeAttack(_selectedTarget)
        };

        BattleManager.Instance.SubmitAllyAction(action);
        actionPanel.SetActive(false);
    }

    // ----- LOG -----

    private void AppendLog(string msg)
    {
        battleLogText.text += msg + "\n";
    }

    // ----- RESTART (DEFEAT) -----

    private void OnRestart()
    {
        AllyParty.Instance.ResetAllAlliesStats();
        BattleManager.Instance.StartNewFight();
        resultOverlay.SetActive(false);
    }
}