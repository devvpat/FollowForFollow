using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("Ally Panel")]
    public Transform allyCardContainer;
    public GameObject allyCardPrefab;

    [Header("Battlefield")]
    public RectTransform battlefieldArea;
    public GameObject enemyFieldPrefab;

    [Header("Enemy Detail")]
    public EnemyDetailPanelUI enemyDetailPanel;

    [Header("Turn Order")]
    public TurnOrderUI turnOrderUI;

    [Header("Action Panel")]
    public GameObject actionPanel;
    public Button attackButton;
    public Button skillButton;
    public Button defendButton;

    [Header("Skill Panel")]
    public SkillPanelUI skillPanelUI;

    [Header("Battle Log")]
    public BattleLogPanel battleLogPanel;

    [Header("Result Overlay")]
    public GameObject resultOverlay;
    public TMP_Text resultTitleText;
    public Button resultButton;
    public TMP_Text resultButtonLabel;

    [Header("Ally Portrait Colors")]
    public Color[] allyColors = {
        new Color(0.60f, 0.35f, 0.85f, 1f),
        new Color(0.85f, 0.45f, 0.25f, 1f),
        new Color(0.25f, 0.70f, 0.85f, 1f),
        new Color(0.85f, 0.25f, 0.45f, 1f)
    };

    private List<AllyCardUI> _allyCards = new();
    private List<EnemyFieldUI> _enemyFields = new();

    // Ally battlefield character sprites (static Canvas objects). Found by their art (sprite) name
    // and mapped to the ally they represent, so they're robust to GameObject renames. Drive the
    // hop (on action) and shake/flash (on damage) effects.
    private readonly Dictionary<CharacterSkillSet, Image> _allyFieldSprites = new();
    private static readonly Dictionary<string, CharacterSkillSet> FieldSpriteArt = new()
    {
        { "Minseong_Attack", CharacterSkillSet.JohnDreamblade },
        { "Phoebe_Attack",   CharacterSkillSet.ApolloPhoebe },
        { "Hatori_Attack",   CharacterSkillSet.Karaage },
        { "Winston_Attack",  CharacterSkillSet.Bookwyrm },
    };

    private AllyActionType _selectedAction;
    private ISkill _selectedSkill;
    private bool _targetingMode;
    private SkillTargetType _currentTargetType;

    private static readonly Vector2[][] EnemyPositions = {
        new[] { new Vector2(0.5f, 0.6f) },
        new[] { new Vector2(0.40f, 0.6f), new Vector2(0.60f, 0.6f) },
        new[] { new Vector2(0.35f, 0.55f), new Vector2(0.5f, 0.65f), new Vector2(0.65f, 0.55f) },
        new[] { new Vector2(0.32f, 0.55f), new Vector2(0.44f, 0.65f), new Vector2(0.56f, 0.65f), new Vector2(0.68f, 0.55f) }
    };

    private void OnEnable()
    {
        BattleManager.Instance.OnBattleStart += HandleBattleStart;
        BattleManager.Instance.OnLogMessage += HandleLogMessage;
        BattleManager.Instance.OnStateChanged += Refresh;
        BattleManager.Instance.OnAllyTurnStart += HandleAllyTurnStart;
        BattleManager.Instance.OnEnemyTurnStart += HandleEnemyTurnStart;
        BattleManager.Instance.OnActionPerformed += HandleActionPerformed;
        BattleManager.Instance.OnDamageTaken += HandleDamageTaken;
        BattleManager.Instance.OnBattleEnd += HandleBattleEnd;

        attackButton.onClick.AddListener(OnAttackPressed);
        skillButton.onClick.AddListener(OnSkillMenuPressed);
        defendButton.onClick.AddListener(OnDefendPressed);

        skillPanelUI.OnSkillSelected += OnSkillSelected;
        skillPanelUI.OnBackPressed += OnSkillBackPressed;
    }

    private void OnDisable()
    {
        if (BattleManager.Instance == null) return;
        BattleManager.Instance.OnBattleStart -= HandleBattleStart;
        BattleManager.Instance.OnLogMessage -= HandleLogMessage;
        BattleManager.Instance.OnStateChanged -= Refresh;
        BattleManager.Instance.OnAllyTurnStart -= HandleAllyTurnStart;
        BattleManager.Instance.OnEnemyTurnStart -= HandleEnemyTurnStart;
        BattleManager.Instance.OnActionPerformed -= HandleActionPerformed;
        BattleManager.Instance.OnDamageTaken -= HandleDamageTaken;
        BattleManager.Instance.OnBattleEnd -= HandleBattleEnd;

        attackButton.onClick.RemoveListener(OnAttackPressed);
        skillButton.onClick.RemoveListener(OnSkillMenuPressed);
        defendButton.onClick.RemoveListener(OnDefendPressed);

        skillPanelUI.OnSkillSelected -= OnSkillSelected;
        skillPanelUI.OnBackPressed -= OnSkillBackPressed;
    }

    private void HandleBattleStart()
    {
        resultOverlay.SetActive(false);
        battleLogPanel.Clear();
        actionPanel.SetActive(false);
        skillPanelUI.Hide();
        enemyDetailPanel.Hide();

        BuildAllyCards();
        BuildEnemyField();
        BuildFieldSpriteMap();
        turnOrderUI.Refresh();

        if (_enemyFields.Count > 0)
            FocusEnemy(_enemyFields[0].BoundEnemy);
    }

    // Locate the static ally battlefield sprites by their art name (clobber-proof vs renames).
    private void BuildFieldSpriteMap()
    {
        _allyFieldSprites.Clear();
        foreach (var img in FindObjectsOfType<Image>(true))
        {
            if (img.sprite == null) continue;
            // Sprite sub-asset names carry a "_0" suffix (multi-sprite import), so match by prefix.
            foreach (var kv in FieldSpriteArt)
            {
                if (img.sprite.name.StartsWith(kv.Key))
                {
                    _allyFieldSprites[kv.Value] = img;
                    break;
                }
            }
        }
    }

    private void HandleAllyTurnStart(Ally ally)
    {
        _selectedAction = AllyActionType.Attack;
        _selectedSkill = null;
        _targetingMode = false;
        _currentTargetType = SkillTargetType.None;

        for (int i = 0; i < _allyCards.Count; i++)
            _allyCards[i].SetActive(BattleManager.Instance.Allies[i] == ally);

        actionPanel.SetActive(true);
        skillPanelUI.Hide();
        skillButton.interactable = !ally.IsSilenced && !ally.IsForceSilenced;

        ClearAllHighlights();
        turnOrderUI.Refresh();
    }

    private void HandleEnemyTurnStart(Enemy enemy)
    {
        actionPanel.SetActive(false);
        skillPanelUI.Hide();
        foreach (var card in _allyCards) card.SetActive(false);
        ClearAllHighlights();
        FocusEnemy(enemy);
        turnOrderUI.Refresh();
    }

    private void HandleBattleEnd(bool playerWon)
    {
        actionPanel.SetActive(false);
        skillPanelUI.Hide();
        resultOverlay.SetActive(true);

        if (playerWon)
        {
            resultTitleText.text = "Victory!";
            resultButtonLabel.text = "Continue";
            resultButton.onClick.RemoveAllListeners();
            resultButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
        else
        {
            resultTitleText.text = "Defeat!";
            resultButtonLabel.text = "Replay";
            resultButton.onClick.RemoveAllListeners();
            resultButton.onClick.AddListener(OnRestart);
        }
    }

    private void BuildAllyCards()
    {
        foreach (Transform t in allyCardContainer) Destroy(t.gameObject);
        _allyCards.Clear();

        var allies = BattleManager.Instance.Allies;
        for (int i = 0; i < allies.Count; i++)
        {
            var go = Instantiate(allyCardPrefab, allyCardContainer);
            var card = go.GetComponent<AllyCardUI>();
            Color color = i < allyColors.Length ? allyColors[i] : Color.gray;
            card.Bind(allies[i], color, OnTargetClicked);
            _allyCards.Add(card);
        }
    }

    private void BuildEnemyField()
    {
        foreach (Transform t in battlefieldArea) Destroy(t.gameObject);
        _enemyFields.Clear();

        var enemies = BattleManager.Instance.Enemies;
        int count = Mathf.Min(enemies.Count, 4);
        Vector2[] positions = count > 0 ? EnemyPositions[count - 1] : new Vector2[0];

        for (int i = 0; i < enemies.Count; i++)
        {
            var go = Instantiate(enemyFieldPrefab, battlefieldArea);
            var field = go.GetComponent<EnemyFieldUI>();
            field.Bind(enemies[i], OnTargetClicked);

            if (i < positions.Length)
            {
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = positions[i];
                rt.anchorMax = positions[i];
                rt.anchoredPosition = Vector2.zero;
            }

            _enemyFields.Add(field);
        }
    }

    private void Refresh()
    {
        foreach (var card in _allyCards) card.Refresh();
        foreach (var field in _enemyFields) field.Refresh();
        enemyDetailPanel.Refresh();
        turnOrderUI.Refresh();
    }

    // A character just attacked / used a skill — hop its field sprite.
    private void HandleActionPerformed(BattleCharacter actor)
    {
        if (actor is Ally ally)
        {
            if (_allyFieldSprites.TryGetValue(ally.CharSkillSet, out var img) && img != null)
                BattleSpriteFx.Hop(this, img.rectTransform);
            return;
        }
        foreach (var field in _enemyFields)
            if (field.BoundEnemy == actor) { field.PlayHop(); return; }
    }

    // A character just took damage — shake + flash red its field sprite.
    private void HandleDamageTaken(BattleCharacter victim)
    {
        if (victim is Ally ally)
        {
            if (_allyFieldSprites.TryGetValue(ally.CharSkillSet, out var img) && img != null)
            {
                BattleSpriteFx.Shake(this, img.rectTransform);
                BattleSpriteFx.Flash(this, img);
            }
            return;
        }
        foreach (var field in _enemyFields)
            if (field.BoundEnemy == victim) { field.PlayHurt(); return; }
    }

    private void FocusEnemy(Enemy enemy)
    {
        foreach (var field in _enemyFields)
            field.SetSelected(field.BoundEnemy == enemy);
        enemyDetailPanel.Show(enemy);
    }

    private void OnAttackPressed()
    {
        _selectedAction = AllyActionType.Attack;
        _currentTargetType = SkillTargetType.Enemy;
        skillPanelUI.Hide();
        EnterTargetingMode(SkillTargetType.Enemy);
    }

    private void OnDefendPressed()
    {
        BattleManager.Instance.SubmitAllyAction(PendingAllyAction.MakeDefend());
        actionPanel.SetActive(false);
        skillPanelUI.Hide();
        _targetingMode = false;
    }

    private void OnSkillMenuPressed()
    {
        Ally ally = BattleManager.Instance.CurrentAlly;
        if (ally == null) return;
        actionPanel.SetActive(false);
        skillPanelUI.Populate(ally);
        skillPanelUI.Show();
    }

    private void OnSkillSelected(int index)
    {
        Ally ally = BattleManager.Instance.CurrentAlly;
        if (ally == null) return;

        ISkill skill = ally.Skills[index];
        if (!ally.CanAffordSkill(skill)) return;

        _selectedAction = AllyActionType.Skill;
        _selectedSkill = skill;
        _currentTargetType = skill.TargetType;
        skillPanelUI.Hide();

        ClearAllHighlights();

        if (skill.TargetType != SkillTargetType.None && skill.TargetType != SkillTargetType.Self)
        {
            EnterTargetingMode(skill.TargetType);
        }
        else
        {
            BattleManager.Instance.SubmitAllyAction(PendingAllyAction.MakeSkill(ally, skill));
            actionPanel.SetActive(false);
        }
    }

    private void OnSkillBackPressed()
    {
        skillPanelUI.Hide();
        actionPanel.SetActive(true);
    }

    private void EnterTargetingMode(SkillTargetType targetType)
    {
        _targetingMode = true;

        Dictionary<SkillTargetType, string> targetTypeNames = new()
        {
            { SkillTargetType.Enemy, "an Enemy" },
            { SkillTargetType.Ally, "an Ally" },
            { SkillTargetType.Self, "Self" },
            { SkillTargetType.Any, "an Enemy or Ally" }
        };
        HandleLogMessage($"[*] Select {targetTypeNames[targetType]} as the target…");

        ClearAllHighlights();

        if (targetType == SkillTargetType.Enemy || targetType == SkillTargetType.Any)
        {
            foreach (var field in _enemyFields)
                field.SetHighlighted(field.BoundEnemy.IsAlive);
        }
        if (targetType == SkillTargetType.Ally || targetType == SkillTargetType.Any)
        {
            foreach (var card in _allyCards)
                card.SetHighlighted(card.BoundAlly.IsAlive);
        }
    }

    private void OnTargetClicked(BattleCharacter character)
    {
        if (character is Enemy enemy)
            FocusEnemy(enemy);

        if (!_targetingMode || !character.IsAlive) return;

        if (_currentTargetType == SkillTargetType.Enemy && character is not Enemy
            || _currentTargetType == SkillTargetType.Ally && character is not Ally)
        {
            HandleLogMessage("[!] Invalid target. Please select a valid target.");
            return;
        }

        HandleLogMessage($"[*] Selected {character.Name} as target.");
        _targetingMode = false;
        ClearAllHighlights();

        PendingAllyAction action = _selectedAction switch
        {
            AllyActionType.Attack => PendingAllyAction.MakeAttack(character),
            AllyActionType.Skill => PendingAllyAction.MakeSkill(character, _selectedSkill),
            _ => PendingAllyAction.MakeAttack(character)
        };

        BattleManager.Instance.SubmitAllyAction(action);
        actionPanel.SetActive(false);
    }

    private void ClearAllHighlights()
    {
        foreach (var card in _allyCards) card.SetHighlighted(false);
        foreach (var field in _enemyFields) field.SetHighlighted(false);
    }

    private void HandleLogMessage(string msg)
    {
        battleLogPanel.AppendLog(msg);
    }

    private void OnRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
