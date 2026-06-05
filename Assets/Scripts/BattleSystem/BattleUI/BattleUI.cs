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

    // Per-character turn-start SFX (CharacterSkillSet -> AudioClip), loaded once from Resources like
    // EnemyFieldUI does for its MimicSpriteLibrary. Played through a 2D AudioSource on this object.
    private static CharacterAudioLibrary _audioLib;
    private AudioSource _sfxSource;

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
        BattleManager.Instance.OnHeal += HandleHeal;
        BattleManager.Instance.OnMiss += HandleMiss;
        BattleManager.Instance.OnStatusApplied += HandleStatusApplied;
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
        BattleManager.Instance.OnHeal -= HandleHeal;
        BattleManager.Instance.OnMiss -= HandleMiss;
        BattleManager.Instance.OnStatusApplied -= HandleStatusApplied;
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

        // Make each ally field sprite clickable as a target (e.g. for ally-targeted buffs/heals),
        // routing to the same OnTargetClicked the ally card uses.
        var allies = BattleManager.Instance != null ? BattleManager.Instance.Allies : null;
        if (allies != null)
        {
            foreach (var kv in _allyFieldSprites)
            {
                var ally = allies.Find(a => a.CharSkillSet == kv.Key);
                if (ally == null) continue;
                var img = kv.Value;
                img.raycastTarget = true;
                var btn = img.GetComponent<Button>();
                if (btn == null) btn = img.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.RemoveAllListeners();
                var captured = ally;
                btn.onClick.AddListener(() => OnTargetClicked(captured));
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
        if (BattleFxSettings.PanelEaseIn) BattleSpriteFx.ScalePunch(this, actionPanel.transform, 0.85f);
        skillPanelUI.Hide();
        skillButton.interactable = !ally.IsSilenced && !ally.IsForceSilenced;

        ClearAllHighlights();
        turnOrderUI.Refresh();
    }

    // Plays the character-specific sound when an ally attacks / uses a skill, if enabled and mapped.
    private void PlayAttackSfx(Ally ally)
    {
        if (!BattleFxSettings.AttackSfx) return;
        if (_audioLib == null) _audioLib = Resources.Load<CharacterAudioLibrary>("CharacterAudioLibrary");
        var clip = _audioLib != null ? _audioLib.Get(ally.CharSkillSet) : null;
        if (clip == null) return;
        if (_sfxSource == null)
        {
            if (!TryGetComponent(out _sfxSource)) _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f; // 2D UI sound
        }
        _sfxSource.PlayOneShot(clip);
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
        if (BattleFxSettings.ResultPunch && resultTitleText != null)
            BattleSpriteFx.ScalePunch(this, resultTitleText.transform, 1.6f);

        if (playerWon)
        {
            resultTitleText.text = "Victory!";
            resultButtonLabel.text = "Continue";
            resultButton.onClick.RemoveAllListeners();
            resultButton.onClick.AddListener(() => transform.parent.gameObject.SetActive(false));
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
        if (BattleFxSettings.DeathFade) RefreshDeathFades();
    }

    // Idle breathing on field sprites, swapped for a stronger pulse on valid targets while targeting.
    private void Update()
    {
        bool pulsing = BattleFxSettings.TargetingPulse && _targetingMode;
        if (!BattleFxSettings.IdleBreathing && !pulsing) return;

        int i = 0;
        var allies = BattleManager.Instance != null ? BattleManager.Instance.Allies : null;
        foreach (var kv in _allyFieldSprites)
        {
            bool alive = allies != null && (allies.Find(a => a.CharSkillSet == kv.Key)?.IsAlive ?? false);
            ApplyIdleScale(kv.Value, i++, true, pulsing, alive);
        }
        for (int e = 0; e < _enemyFields.Count; e++)
        {
            bool alive = _enemyFields[e].BoundEnemy != null && _enemyFields[e].BoundEnemy.IsAlive;
            ApplyIdleScale(_enemyFields[e].enemySprite, i++, false, pulsing, alive);
        }
    }

    private void ApplyIdleScale(Image img, int index, bool allySide, bool pulsing, bool alive)
    {
        if (img == null) return;
        if (!alive) { img.transform.localScale = Vector3.one; return; } // dead: no pulse/breathe (fade stays)
        bool validTarget = pulsing && (
            _currentTargetType == SkillTargetType.Any ||
            (allySide && (_currentTargetType == SkillTargetType.Ally || _currentTargetType == SkillTargetType.Self)) ||
            (!allySide && _currentTargetType == SkillTargetType.Enemy));

        if (validTarget)
        {
            float s = 1f + Mathf.Sin(Time.time * 6f) * 0.08f;
            img.transform.localScale = new Vector3(s, s, 1f);
        }
        else if (BattleFxSettings.IdleBreathing)
        {
            BattleSpriteFx.Breathe(img.transform, index * 0.7f);
        }
        else
        {
            img.transform.localScale = Vector3.one;
        }
    }

    private readonly HashSet<BattleCharacter> _faded = new();
    private void RefreshDeathFades()
    {
        if (BattleManager.Instance == null) return;
        foreach (var a in BattleManager.Instance.Allies)
            if (!a.IsAlive && _faded.Add(a)) FadeSprite(SpriteFor(a));
        // Dead enemies remove their whole field prefab (EnemyFieldUI.HideOnDeath), so no sprite dim here.
    }
    private void FadeSprite(Image img) { if (img != null) StartCoroutine(FadeRoutine(img)); }
    private System.Collections.IEnumerator FadeRoutine(Image img)
    {
        float t = 0f; const float dur = 0.5f;
        Color start = img.color;
        Color end = new Color(start.r, start.g, start.b, 0.15f);
        while (t < dur && img != null)
        {
            t += Time.deltaTime;
            img.color = Color.Lerp(start, end, Mathf.Clamp01(t / dur));
            yield return null;
        }
        if (img != null) img.color = end;
    }

    // The on-field sprite Image for any combatant (ally field sprite or enemy field sprite), or null.
    private Image SpriteFor(BattleCharacter c)
    {
        if (c is Ally ally)
            return _allyFieldSprites.TryGetValue(ally.CharSkillSet, out var img) ? img : null;
        foreach (var field in _enemyFields)
            if (field.BoundEnemy == c) return field.enemySprite;
        return null;
    }

    // A character just attacked / used a skill — lunge (or hop) its field sprite toward foes.
    private void HandleActionPerformed(BattleCharacter actor)
    {
        if (actor is Ally ally) PlayAttackSfx(ally);

        var img = SpriteFor(actor);
        if (img == null) return;
        if (actor is Enemy)
            _enemyFields.Find(f => f.BoundEnemy == actor)?.PlayAttack();
        if (BattleFxSettings.AttackLunge)
            BattleSpriteFx.Lunge(this, img.rectTransform, actor is Ally ? 1f : -1f);
        else
            BattleSpriteFx.Hop(this, img.rectTransform);
    }

    // A character just took damage — shake + flash, damage number (crit-styled), and a hit-stop.
    private void HandleDamageTaken(BattleCharacter victim, float amount, bool isCrit)
    {
        var img = SpriteFor(victim);
        if (img != null)
        {
            BattleSpriteFx.Shake(this, img.rectTransform);
            BattleSpriteFx.Flash(this, img);

            if (BattleFxSettings.DamageNumbers)
            {
                string txt = Mathf.RoundToInt(amount).ToString();
                if (isCrit && BattleFxSettings.CritNumbers)
                    BattleFloatingText.Spawn(img.rectTransform, txt + "!", new Color(1f, 0.85f, 0.2f, 1f), 1.5f, true);
                else
                    BattleFloatingText.Spawn(img.rectTransform, txt, new Color(1f, 0.3f, 0.3f, 1f));
            }
        }

        if (BattleFxSettings.HitStop && !_hitStopActive)
            StartCoroutine(HitStopRoutine(isCrit ? 0.12f : 0.05f));
    }

    private void HandleHeal(BattleCharacter target, float amount)
    {
        if (!BattleFxSettings.HealNumbers) return;
        var img = SpriteFor(target);
        if (img == null) return;
        var green = new Color(0.35f, 1f, 0.45f, 1f);
        BattleFloatingText.Spawn(img.rectTransform, "+" + Mathf.RoundToInt(amount), green);
        BattleSpriteFx.Flash(this, img, green);
    }

    private void HandleMiss(BattleCharacter target, bool dodged)
    {
        if (!BattleFxSettings.MissText) return;
        var img = SpriteFor(target);
        if (img == null) return;
        BattleFloatingText.Spawn(img.rectTransform, dodged ? "Dodge!" : "Miss!", new Color(0.82f, 0.82f, 0.88f, 1f));
    }

    private void HandleStatusApplied(BattleCharacter target, BaseStatusEffect effect)
    {
        if (!BattleFxSettings.StatusApplyFlash) return;
        var img = SpriteFor(target);
        if (img == null) return;
        Color c = (effect != null && effect.EffectType == StatusEffectType.Buff)
            ? new Color(0.4f, 1f, 0.5f, 1f)
            : new Color(1f, 0.45f, 0.45f, 1f);
        BattleSpriteFx.Flash(this, img, c);
    }

    private bool _hitStopActive;
    private System.Collections.IEnumerator HitStopRoutine(float seconds)
    {
        _hitStopActive = true;
        float prev = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(seconds);
        Time.timeScale = prev;
        _hitStopActive = false;
    }

    private void FocusEnemy(Enemy enemy)
    {
        foreach (var field in _enemyFields)
            field.SetSelected(field.BoundEnemy == enemy);
        enemyDetailPanel.Show(enemy);
    }

    private void OnAttackPressed()
    {
        if (BattleFxSettings.ButtonPop) BattleSpriteFx.ScalePunch(this, attackButton.transform, 0.9f);
        _selectedAction = AllyActionType.Attack;
        _currentTargetType = SkillTargetType.Enemy;
        skillPanelUI.Hide();
        EnterTargetingMode(SkillTargetType.Enemy);
    }

    private void OnDefendPressed()
    {
        if (BattleFxSettings.ButtonPop) BattleSpriteFx.ScalePunch(this, defendButton.transform, 0.9f);
        BattleManager.Instance.SubmitAllyAction(PendingAllyAction.MakeDefend());
        actionPanel.SetActive(false);
        skillPanelUI.Hide();
        _targetingMode = false;
    }

    private void OnSkillMenuPressed()
    {
        if (BattleFxSettings.ButtonPop) BattleSpriteFx.ScalePunch(this, skillButton.transform, 0.9f);
        Ally ally = BattleManager.Instance.CurrentAlly;
        if (ally == null) return;
        actionPanel.SetActive(false);
        skillPanelUI.Populate(ally);
        skillPanelUI.Show();
        if (BattleFxSettings.PanelEaseIn) BattleSpriteFx.ScalePunch(this, skillPanelUI.transform, 0.85f);
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
        if (BattleFxSettings.PanelEaseIn) BattleSpriteFx.ScalePunch(this, actionPanel.transform, 0.85f);
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
        // GameObject player = GameObject.FindWithTag("Player");
        // player.transform.position = new Vector3(0f, -1f, 0f);
        // AllyParty.Instance.ResetAllAlliesHealthAndMana();
        // transform.parent.gameObject.SetActive(false);
    }

    // Info panel toggle (merged from main). Null-guarded since the classroom scene may not wire InfoText.
    public GameObject InfoText;
    public void ToggleInfoText()
    {
        if (InfoText != null) InfoText.SetActive(!InfoText.activeSelf);
    }
}
