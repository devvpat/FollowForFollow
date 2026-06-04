using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

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
    public event System.Action<BattleCharacter> OnActionPerformed; // a character just attacked / used a skill
    public event System.Action<BattleCharacter, float, bool> OnDamageTaken; // (victim, amount, isCrit)
    public event System.Action<BattleCharacter, float> OnHeal;             // (target, amount healed)
    public event System.Action<BattleCharacter, bool> OnMiss;              // (target, dodged)
    public event System.Action<BattleCharacter, BaseStatusEffect> OnStatusApplied; // (target, effect)
    public event System.Action<bool> OnBattleEnd; // true = player won

    // Called from BattleCharacter (events can only be raised by the declaring class).
    public void RaiseHeal(BattleCharacter c, float amount) => OnHeal?.Invoke(c, amount);
    public void RaiseMiss(BattleCharacter c, bool dodged) => OnMiss?.Invoke(c, dodged);
    public void RaiseStatusApplied(BattleCharacter c, BaseStatusEffect e) => OnStatusApplied?.Invoke(c, e);

    private static bool _lastHitWasCrit; // set by the damage calc, read when firing OnDamageTaken

    // ----- BATTLE NUMBERS -----

    public const float BattleTickThreshold = 10000f; // when a character's tick timer reaches this, they can act
    public const float DefenseConstant = 100f;

    // Enemy-turn pacing (seconds): a beat after an enemy is highlighted before it acts, and a longer
    // beat after the action so the result is readable. Tune to taste.
    private const float EnemyTurnStartDelay = 0.6f;
    private const float EnemyActionDelay = 1.25f;

    // ----- BATTLE STATE -----

    public List<Ally> Allies { get; private set; }
    public List<Enemy> Enemies { get; private set; }
    public Ally CurrentAlly => _currentAlly;
    public bool WaitingForInput => _waitingForPlayerInput;

    private Ally _currentAlly;
    private bool _battleActive;
    private bool _waitingForPlayerInput;
    private PendingAllyAction _pendingAction;

    private Comparer<float> tickComparer; // comparer to sort floats in descending order
    private PriorityQueue<BattleCharacter, float> charactersTakingTurn;

    // ----- SETUP -----

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        tickComparer = Comparer<float>.Create((x, y) => y.CompareTo(x));
        charactersTakingTurn = new PriorityQueue<BattleCharacter, float>(tickComparer);
        charactersTakingTurn.Clear();
    }

    // ----- PUBLIC API -----

    // Starts a new fight
    public void StartNewFight(List<Enemy> enemies)
    {
        if (_battleActive)
        {
            Debug.LogWarning("Battle already active! Cannot start a new fight.");
            return;
        }
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("No enemies provided! Cannot start a fight.");
            return;
        }

        Allies = AllyParty.Instance.Allies;
        Enemies = enemies;
        foreach (var a in Allies) a.ResetTickTimer();
        foreach (var e in Enemies) e.ResetTickTimer();

        _battleActive = true;
        _currentAlly = null;
        _waitingForPlayerInput = false;
        _pendingAction = null;

        OnBattleStart?.Invoke();
        Log("[*] A new battle begins!");
        foreach (var a in Allies) a.OnBattleStart();
        foreach (var e in Enemies) e.OnBattleStart();
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
            charactersTakingTurn.Clear();
            foreach (var a in Allies) if (a.Tick()) charactersTakingTurn.Enqueue(a, a.TickTimer);
            foreach (var e in Enemies) if (e.Tick()) charactersTakingTurn.Enqueue(e, e.TickTimer);

            // Let each character take their turn in order
            while (charactersTakingTurn.TryDequeue(out BattleCharacter currChar, out float _)) {
                // Check battle state
                if (!currChar.IsAlive) continue; // skip if character is dead
                if (currChar.TickTimer < BattleTickThreshold) continue; // skip if character is not ready to take a turn
                if (!IsAnyEnemyAlive() || !IsAnyAllyAlive()) break; // end battle if no one is alive

                // End current character's defend state
                currChar.EndDefend();

                // Process all status effects for start of turn
                currChar.ProcessStatusEffectsOnTurnStart();
                currChar.FindAndRemoveExpiredStatusEffects(); // check for expired effects after processing turn start effects in case any effects expire at the start of the turn
                currChar.OnTurnStart();

                // Process ally character turn
                if (currChar is Ally ally && !ally.IsStunned)
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
                else if (currChar is Enemy enemy && !enemy.IsStunned)
                {
                    // Log turn start, execute enemy action, and log result
                    OnEnemyTurnStart?.Invoke(enemy);
                    Log($"[*] {enemy.Name}'s turn.");
                    yield return new WaitForSeconds(EnemyTurnStartDelay); // beat so the focused enemy is visible before it acts
                    OnActionPerformed?.Invoke(enemy);
                    string result = enemy.TakeTurn(GetLivingAllies(), GetLivingEnemies());
                    Log(result);

                    yield return new WaitForSeconds(EnemyActionDelay); // delay after enemy action for log readability
                }

                // Process all status effects for end of turn (after action but before checking for expired effects)
                currChar.OnTurnEnd();
                currChar.ProcessStatusEffectsOnTurnEnd();
                currChar.FindAndRemoveExpiredStatusEffects(); // check for expired effects after processing turn end effects

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

                // check to requeue character if they have excess tick time (e.g. from haste)
                if (currChar.TickTimer >= BattleTickThreshold)
                    charactersTakingTurn.Enqueue(currChar, currChar.TickTimer);
            }

            // Wait until next frame to continue battle loop
            yield return null;
        }
    }

    public void AddToTakingTurnQueue(BattleCharacter character, float tickTimer)
    {
        // since the battle loop checks the TickTimer each time a character takes a turn,
        // there isnt a need to remove and requeue
        charactersTakingTurn.Enqueue(character, tickTimer);
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
                if (!ally.PerformAccuracyCheck()) { Log($"[+] {ally.Name} tried to attack {action.Target.Name} but missed"); return; }
                if (action.Target.PerformDodgeCheck()) { Log($"[+] {ally.Name} tried to attack {action.Target.Name} but {action.Target.Name} dodged"); return; }
                ally.OnNormalAttack();
                OnActionPerformed?.Invoke(ally);
                // Attack enemy target
                float atkDmg = CalculateAndApplyDamage(ally, action.Target);
                Log($"[+] {ally.Name} attacks {action.Target.Name} for {atkDmg} damage!");
                break;

            case AllyActionType.Skill:
                // Check target validity and mana availability
                if (action.Target == null) { Log("[*] No target!"); return; }
                ISkill skill = action.SkillUsed;
                if (!ally.CanAffordSkill(skill)) { Log($"[*] {ally.Name} does not have enough mana!"); return; }
                // Spend mana and perform accuracy check in skill then execute and log result
                ally.SpendMana(skill.ManaCost);
                ally.OnSkilluse();
                OnActionPerformed?.Invoke(ally);
                SkillResult result = skill.Execute(ally, action.Target);
                Log(result.LogMessage);
                break;

            case AllyActionType.Defend:
                ally.OnDefend();
                ally.StartDefend();
                Log($"[+] {ally.Name} defends");
                break;
        }
    }

    // Ends the battle
    private void EndBattle(bool playerWon)
    {
        _battleActive = false;
        foreach (var a in Allies) a.OnBattleEnd();
        foreach (var e in Enemies) e.OnBattleEnd();
        string result = playerWon ? "Victory!" : "Defeat!";
        Log($"[*] {result}");
        AllyParty.Instance.RemoveAllStatusEffectsFromAllAllies();
        OnBattleEnd?.Invoke(playerWon);
    }

    // ----- UTILITY METHODS -----

    // Calculates damage from attacker to defender using the specified skill / normal attack
    // Applies the calculated damage to the defender
    // Returns the final damage dealt
    public static float CalculateAndApplyDamage(BattleCharacter attacker, BattleCharacter defender, ISkill skill = null, bool isGuaranteedCrit = false, bool bypassDefense = false, bool isPercentHealthDamage = false, float maxHealthPercentDamage = 0f)
    {
        float damage = CalculateDamage(attacker, defender, skill, isGuaranteedCrit, bypassDefense, isPercentHealthDamage, maxHealthPercentDamage);
        defender.TakeDamage(damage);
        if (damage > 0f && Instance != null) Instance.OnDamageTaken?.Invoke(defender, damage, _lastHitWasCrit);
        if (attacker.HasPoisonedWeapon)
        {
            var weapon = attacker.StatusEffects.First(e => e is PoisonWeapon) as PoisonWeapon;
            defender.ApplyStatusEffect(EffectFactory.MakePoison(weapon.StacksToApply));
        }
        return damage;
    }

    // Calculates and returns the final (post-mitigation) damage of a normal attack/skill
    // If skill is not null, applies the skill's power to the damage calculation
    public static float CalculateDamage(BattleCharacter attacker, BattleCharacter defender, ISkill skill = null, bool isGuaranteedCrit = false, bool bypassDefense = false, bool isPercentHealthDamage = false, float maxHealthPercentDamage = 0f)
    {
        float rawDamage;
        _lastHitWasCrit = false;
        if (isPercentHealthDamage)
            rawDamage = CalculatePreMitigationPercentHealthDamage(defender, maxHealthPercentDamage);
        else
            rawDamage = skill == null
                ? CalculatePreMitigationAttackDamage(attacker, isGuaranteedCrit)
                : CalculatePreMitigationSkillDamage(attacker, skill, isGuaranteedCrit);
        float finalDamage = CalculatePostMitigationDamage(rawDamage, defender, bypassDefense);
        return finalDamage;
    }

    // Calculates raw damage for normal attacks
    // RawDamage = (Attack * AttackModifier) * (isCrit ? CritDamage : 1)
    public static float CalculatePreMitigationAttackDamage(BattleCharacter attacker, bool isGuaranteedCrit = false)
    {
        float dmg = attacker.Attack * attacker.AttackModifier + 750;
        if (isGuaranteedCrit || Random.Range(0, 100)/100f < attacker.CritChance)
        {
            dmg *= attacker.CritDamage;
            _lastHitWasCrit = true;
        }
        return dmg;
    }

    // Calculates raw damage for skills
    // RawDamage = (SkillPower + (Attack * AttackModifier)) * (isCrit ? CritDamage : 1)
    public static float CalculatePreMitigationSkillDamage(BattleCharacter attacker, ISkill skill, bool isGuaranteedCrit = false)
    {
        float dmg = skill.Power * Mathf.Pow(ISkill.SkillPowerScale, AllyParty.Instance.LevelScale) * (attacker is Ally ? (attacker as Ally).SkillPowerMod : 1f) + (attacker.Attack * attacker.AttackModifier);
        if (isGuaranteedCrit || Random.Range(0, 100)/100f < attacker.CritChance)
        {
            dmg *= attacker.CritDamage;
            _lastHitWasCrit = true;
        }
        return dmg;
    }

    // Calculates raw damage for percent max health attacks
    // RawDamage = TargetMaxHealth * PercentHealthDamage
    public static float CalculatePreMitigationPercentHealthDamage(BattleCharacter defender, float percentHealthDamage)
    {
        return defender.MaxHP * percentHealthDamage;
    }

    // Calculates final damage after applying defense mitigation
    // MitigatedDamage = RawDamage * (DefenseConstant / (DefenseConstant + Defense * DefenseModifier))
    public static float CalculatePostMitigationDamage(float rawDamage, BattleCharacter defender, bool bypassDefense)
    {
        if (bypassDefense) return rawDamage;
        float mitigatedDamage = rawDamage * (DefenseConstant / (DefenseConstant + defender.Defense * defender.DefenseModifier));
        return mitigatedDamage;
    }

    public List<BattleCharacter> GetPredictedTurnOrder(int maxSlots = 8)
    {
        var result = new List<BattleCharacter>(maxSlots);
        var allChars = new List<BattleCharacter>();
        foreach (var a in Allies) if (a.IsAlive) allChars.Add(a);
        foreach (var e in Enemies) if (e.IsAlive) allChars.Add(e);
        if (allChars.Count == 0) return result;

        var simTimers = new float[allChars.Count];
        for (int i = 0; i < allChars.Count; i++)
            simTimers[i] = allChars[i].TickTimer;

        int safety = 0;
        while (result.Count < maxSlots && safety < 10000)
        {
            safety++;
            int bestIdx = -1;
            float bestTimer = -1f;
            for (int i = 0; i < allChars.Count; i++)
            {
                simTimers[i] += allChars[i].Speed;
                if (simTimers[i] >= BattleTickThreshold && simTimers[i] > bestTimer)
                {
                    bestTimer = simTimers[i];
                    bestIdx = i;
                }
            }
            if (bestIdx >= 0)
            {
                result.Add(allChars[bestIdx]);
                simTimers[bestIdx] -= BattleTickThreshold;
            }
        }
        return result;
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

    public List<BattleCharacter> GetLivingTeamOf(BattleCharacter character)
    {
        if (character is Ally)
        {
            return GetLivingAllies().Cast<BattleCharacter>().ToList();
        }

        if (character is Enemy)
        {
            return GetLivingEnemies().Cast<BattleCharacter>().ToList();
        }

        return new List<BattleCharacter>();
    }

    public List<BattleCharacter> GetLivingOpponentsOf(BattleCharacter character)
    {
        if (character is Ally)
        {
            return GetLivingEnemies().Cast<BattleCharacter>().ToList();
        }

        if (character is Enemy)
        {
            return GetLivingAllies().Cast<BattleCharacter>().ToList();
        }

        return new List<BattleCharacter>();
    }
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
