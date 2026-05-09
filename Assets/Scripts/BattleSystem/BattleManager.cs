using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Singleton class that handles all battle logic
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // ----- BATTLE EVENTS -----

    public event System.Action OnBattleStart; // start of battle
    public event System.Action<string> OnLogMessage; // battle log entry
    public event System.Action OnStateChanged; // redraw UI
    public event System.Action<Ally> OnAllyTurnStart; // ally taking turn
    public event System.Action<Enemy> OnEnemyTurnStart; // enemy taking turn
    public event System.Action<bool> OnBattleEnd; // true = player won

    // ----- BATTLE NUMBERS -----

    public const float BattleTickThreshold = 10000f; // when a character's tick timer reaches this, they can act
    public const float DefenseConstant = 100f;

    [Header("Enemy Count")]
    [Range(1, 4)]
    [SerializeField] private int minEnemies = 1;
    [Range(1, 4)]
    [SerializeField] private int maxEnemies = 4;

    // ----- BATTLE STATE -----

    public List<Ally> Allies { get; private set; }
    public List<Enemy> Enemies { get; private set; }
    public Ally CurrentAlly => _currentAlly;
    public bool WaitingForInput => _waitingForPlayerInput;

    private Ally _currentAlly;
    private bool _battleActive;
    private bool _waitingForPlayerInput;
    private PendingAllyAction _pendingAction;

    // ----- SETUP -----

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ----- PUBLIC API -----

    // Starts a new fight
    public void StartNewFight()
    {
        Allies = AllyParty.Instance.Allies;
        Enemies = SpawnEnemies();
        foreach (var a in Allies) a.ResetTickTimer();
        foreach (var e in Enemies) e.ResetTickTimer();

        _battleActive = true;
        _currentAlly = null;

        OnBattleStart?.Invoke();
        Log("[*] A new battle begins!");
        OnStateChanged?.Invoke();

        StartCoroutine(RunBattle());
    }

    // Called by BattleUI when player chooses an action
    public void SubmitAllyAction(PendingAllyAction action)
    {
        if (!_waitingForPlayerInput) return;
        _pendingAction = action;
        _waitingForPlayerInput = false;
    }

    // ----- BATTLE LOOP -----

    // Main battle loop coroutine. Alternates between ally and enemy turn phases until win/loss.
    private IEnumerator RunBattle()
    {
        while (_battleActive)
        {
            // Pass a tick for all characters and collect all characters that are ready to take an action
            List<BattleCharacter> charactersTakingTurn = new List<BattleCharacter>();
            foreach (var a in Allies) if (a.Tick()) charactersTakingTurn.Add(a);
            foreach (var e in Enemies) if (e.Tick()) charactersTakingTurn.Add(e);

            // Sort by TickTimer value so highest goes first
            charactersTakingTurn.Sort((char1, char2) => char2.TickTimer.CompareTo(char1.TickTimer));
            
            // debug: list all characters taking a turn this round
            if (charactersTakingTurn.Count > 0)
            {
                string charNames = string.Join(", ", charactersTakingTurn.ConvertAll(c => c.Name));
                Debug.Log($"[*] Taking turns this round: {charNames}");
            }

            // Let each character take their turn in order
            foreach (var currChar in charactersTakingTurn) {
                // Check battle state
                if (!currChar.IsAlive) continue; // skip if character is dead
                if (!IsAnyEnemyAlive() || !IsAnyAllyAlive()) break; // end battle if no one is alive

                // End current character's defend state
                currChar.EndDefend();

                // Process ally character turn
                if (currChar is Ally ally)
                {
                    // Log turn start and set waiting for player input
                    _currentAlly = ally;
                    OnAllyTurnStart?.Invoke(ally);
                    Log($"[*] {ally.Name}'s turn.");
                    _waitingForPlayerInput = true;
                    _pendingAction = null;
                    OnStateChanged?.Invoke();

                    // Wait until player submits an action
                    yield return new WaitUntil(() => !_waitingForPlayerInput);

                    // Execute the submitted action
                    if (_pendingAction != null)
                        ExecuteAllyAction(ally, _pendingAction);
                }
                // Process enemy character turn
                else if (currChar is Enemy enemy)
                {
                    // Log turn start, execute enemy action, and log result
                    OnEnemyTurnStart?.Invoke(enemy);
                    Log($"[*] {enemy.Name}'s turn.");
                    string result = enemy.TakeTurn(GetLivingAllies(), GetLivingEnemies());
                    Log(result);

                    yield return new WaitForSeconds(1f); // small delay after enemy action for log readability
                }

                // Consume current character's ticks and update UI
                currChar.ConsumeTickTurn();
                OnStateChanged?.Invoke();

                // Check if battle ended after current character finished their action
                if (!IsAnyEnemyAlive())
                {
                    EndBattle(true);
                    yield break;
                }
                else if (!IsAnyAllyAlive())
                {
                    EndBattle(false);
                    yield break;
                }
            }

            // Wait until next frame to continue battle loop
            yield return null;
        }
    }

    // Executes the given ally action, applying its effects to the target enemy if applicable.
    private void ExecuteAllyAction(Ally ally, PendingAllyAction action)
    {
        float accuracyRoll = Random.Range(0, 100) / 100f; // [0.0f, 1.0f) for accuracy checks

        switch (action.ActionType)
        {
            case AllyActionType.Attack:
                // Check target validity and perform accuracy check
                if (action.Target == null) { Log("[*] No target!"); return; }
                if (accuracyRoll >= ally.Accuracy)
                {
                    Log($"[*] {ally.Name} attacks {action.Target.Name} but misses!");
                    break;
                }
                // Attack enemy target
                float atkDmg = CalculateAndApplyDamage(ally, action.Target);
                Log($"[+] {ally.Name} attacks {action.Target.Name} for {atkDmg} damage!");
                break;

            case AllyActionType.Skill:
                // Check target validity and mana availability
                if (action.Target == null) { Log("[*] No target!"); return; }
                ISkill skill = action.SkillUsed;
                if (!ally.CanAffordSkill(skill)) { Log($"[*] {ally.Name} does not have enough mana!"); return; }
                // Spend mana and perform accuracy check if applicable
                ally.SpendMana(skill.ManaCost);
                bool skillHits = skill.BypassAccuracy || accuracyRoll < ally.Accuracy;
                if (!skillHits)
                {
                    Log($"[*] {ally.Name} tried to use {skill.Name} on {action.Target.Name} but misses!");
                    break;
                }
                // Execute skill and log result
                SkillResult result = skill.Execute(ally, action.Target);
                Log(result.LogMessage);
                break;

            case AllyActionType.Defend:
                ally.StartDefend();
                Log($"[+] {ally.Name} defends — damage reduced by {ally.Defense}% this round.");
                break;
        }
    }

    // Ends the battle
    private void EndBattle(bool playerWon)
    {
        _battleActive = false;
        string result = playerWon ? "Victory!" : "Defeat!";
        Log($"[*] {result}");
        OnBattleEnd?.Invoke(playerWon);
    }

    // ----- UTILITY METHODS -----

    // Calculates damage from attacker to defender using the specified skill / normal attack
    // Applies the calculated damage to the defender
    // Returns the final damage dealt
    public static float CalculateAndApplyDamage(BattleCharacter attacker, BattleCharacter defender, ISkill skill = null, bool isGuaranteedCrit = false, bool bypassDefense = false)
    {
        float damage = CalculateDamage(attacker, defender, skill, isGuaranteedCrit, bypassDefense);
        defender.TakeDamage(damage);
        return damage;
    }

    // Calculates and returns the final (post-mitigation) damage of a normal attack/skill
    // If skill is not null, applies the skill's power to the damage calculation
    public static float CalculateDamage(BattleCharacter attacker, BattleCharacter defender, ISkill skill = null, bool isGuaranteedCrit = false, bool bypassDefense = false)
    {
        float rawDamage = skill == null
            ? CalculatePreMitigationAttackDamage(attacker, isGuaranteedCrit)
            : CalculatePreMitigationSkillDamage(attacker, skill, isGuaranteedCrit);
        float finalDamage = CalculatePostMitigationDamage(rawDamage, defender, bypassDefense);
        return finalDamage;
    }

    // Calculates raw damage for normal attacks
    // RawDamage = (Attack * AttackModifier) * (isCrit ? CritDamage : 1)
    public static float CalculatePreMitigationAttackDamage(BattleCharacter attacker, bool isGuaranteedCrit = false)
    {
        float dmg = attacker.Attack * attacker.AttackModifier;
        if (isGuaranteedCrit || Random.Range(0, 100)/100f < attacker.CritChance)
            dmg *= attacker.CritDamage;
        return dmg;
    }

    // Calculates raw damage for skills
    // RawDamage = (SkillPower + (Attack * AttackModifier)) * (isCrit ? CritDamage : 1)
    public static float CalculatePreMitigationSkillDamage(BattleCharacter attacker, ISkill skill, bool isGuaranteedCrit = false)
    {
        float dmg = (int)skill.Power + (attacker.Attack * attacker.AttackModifier);
        if (isGuaranteedCrit || Random.Range(0, 100)/100f < attacker.CritChance)
            dmg *= attacker.CritDamage;
        return dmg;
    }

    // Calculates final damage after applying defense mitigation
    // MitigatedDamage = RawDamage * (DefenseConstant / (DefenseConstant + Defense * DefenseModifier))
    public static float CalculatePostMitigationDamage(float rawDamage, BattleCharacter defender, bool bypassDefense)
    {
        if (bypassDefense) return rawDamage;
        float mitigatedDamage = rawDamage * (DefenseConstant / (DefenseConstant + defender.Defense * defender.DefenseModifier));
        return mitigatedDamage;
    }

    private List<Enemy> SpawnEnemies()
    {
        int count = Random.Range(minEnemies, maxEnemies + 1);
        var list  = new List<Enemy>();

        string[] names   = { "Goblin", "Orc", "Troll", "Bandit" };
        float[]    hpPool  = { 60,  80,  120,  70 };
        float[]    atkPool = { 12,  16,  10,   14 };
        float[]    defPool = { 0.10f,  0.20f,  0.35f,   0.15f };
        float[]    spdPool = { 5000,  3500,  2500,   5500 };
        float[]    accPool = { 0.80f,  0.70f,  0.65f,   0.85f };
        float[]    crPool  = { 0.05f, 0.10f, 0.15f, 0.40f };
        float[]    cdPool  = { 1.50f, 1.50f, 1.50f, 1.50f };


        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, names.Length);
            string enemyName = count > 1 ? $"{names[idx]} {i + 1}" : names[idx];
            list.Add(new Enemy(enemyName, hpPool[idx], atkPool[idx], defPool[idx], spdPool[idx], accPool[idx], crPool[idx], cdPool[idx], EnemyBehaviorType.Random));
        }
        return list;
    }

    private void Log(string msg) => OnLogMessage?.Invoke(msg);

    public bool IsAnyEnemyAlive()
    {
        foreach (var e in Enemies) if (e.IsAlive) return true;
        return false;
    }

    public bool IsAnyAllyAlive()
    {
        foreach (var a in Allies) if (a.IsAlive) return true;
        return false;
    }

    public List<Ally> GetLivingAllies()
    {
        var list = new List<Ally>();
        foreach (var a in Allies) if (a.IsAlive) list.Add(a);
        return list;
    }

    public List<Enemy> GetLivingEnemies()
    {
        var list = new List<Enemy>();
        foreach (var e in Enemies) if (e.IsAlive) list.Add(e);
        return list;
    }

    // ----- TEMP -----
    [Header("TEMP")]
    public GameObject TestStartButton;
    public void OnClickTest()
    {
        TestStartButton.SetActive(false);
        StartNewFight();
    }
    // ----- TEMP -----
}

// ----- ALLY ACTION STRUCTURE -----

public enum AllyActionType { Attack, Skill, Defend }

public class PendingAllyAction
{
    public AllyActionType ActionType;
    public BattleCharacter Target;
    public ISkill SkillUsed;

    public static PendingAllyAction MakeAttack(BattleCharacter target) =>
        new PendingAllyAction { ActionType = AllyActionType.Attack, Target = target };
    public static PendingAllyAction MakeSkill(BattleCharacter target, ISkill skill) =>
        new PendingAllyAction { ActionType = AllyActionType.Skill, Target = target, SkillUsed = skill };
    public static PendingAllyAction MakeDefend() =>
        new PendingAllyAction { ActionType = AllyActionType.Defend, Target = null };
}
