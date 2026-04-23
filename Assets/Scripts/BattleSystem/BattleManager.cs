using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;

// Singleton class that handles all battle logic
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // ----- BATTLE EVENTS -----

    public event System.Action OnBattleStart;
    public event System.Action<string> OnLogMessage; // battle log entry
    public event System.Action OnStateChanged; // redraw UI
    public event System.Action<int> OnAllyTurnStart; // ally index
    public event System.Action OnEnemyPhaseStart;
    public event System.Action<bool> OnBattleEnd; // true = player won

    // ----- BATTLE STATE -----

    public List<Ally> Allies { get; private set; }
    public List<Enemy> Enemies { get; private set; }

    private int  _currentAllyIndex;
    private bool _battleActive;
    private bool _waitingForPlayerInput;

    private PendingAllyAction _pendingAction;

    [Header("Enemy Count")]
    [Range(1, 4)]
    public int minEnemies = 1;
    [Range(1, 4)]
    public int maxEnemies = 4;

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
        Allies = AllyParty.Instance.Allies;        // live references — stats persist
        Enemies = SpawnEnemies();
        _battleActive = true;
        _currentAllyIndex = 0;

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
            // ALLY PHASE
            for (int i = 0; i < Allies.Count; i++)
            {
                // Check if ally is alive and if there are still enemies to fight
                Ally ally = Allies[i];
                if (!ally.IsAlive) continue;
                if (!IsAnyEnemyAlive()) break;

                ally.EndDefend();
                _currentAllyIndex = i;
                OnAllyTurnStart?.Invoke(i);
                Log($"[+] {ally.Name}'s turn.");

                // Wait for player input
                _waitingForPlayerInput = true;
                _pendingAction = null;
                OnStateChanged?.Invoke();

                yield return new WaitUntil(() => !_waitingForPlayerInput);

                // Execute the chosen action
                if (_pendingAction != null)
                    ExecuteAllyAction(ally, _pendingAction);

                OnStateChanged?.Invoke();

                // Check if all enemies are defeated
                if (!IsAnyEnemyAlive())
                {
                    EndBattle(playerWon: true);
                    yield break;
                }

                yield return new WaitForSeconds(0.4f);
            }

            // ENEMY PHASE
            OnEnemyPhaseStart?.Invoke();
            Log("[-] Enemies act!");

            foreach (var enemy in Enemies)
            {
                // Check if enemy is alive and if there are still allies to fight
                if (!enemy.IsAlive) continue;
                if (!IsAnyAllyAlive()) break;

                enemy.EndDefend();

                // Enemy takes its turn via its designated behavior
                var livingAllies = GetLivingAllies();
                string result = enemy.TakeTurn(livingAllies, GetLivingEnemies());
                Log(result);

                OnStateChanged?.Invoke();
                yield return new WaitForSeconds(0.6f);
            }

            // Check if all allies are defeated
            if (!IsAnyAllyAlive())
            {
                EndBattle(playerWon: false);
                yield break;
            }

            OnStateChanged?.Invoke();
        }
    }


    // Executes the given ally action, applying its effects to the target enemy if applicable.
    private void ExecuteAllyAction(Ally ally, PendingAllyAction action)
    {
        switch (action.ActionType)
        {
            case AllyActionType.Attack:
                if (action.EnemyTarget == null) { Log("[*]No target!"); return; }
                int atkDmg = action.EnemyTarget.TakeDamage(ally.GetAttackDamage());
                Log($"[+] {ally.Name} attacks {action.EnemyTarget.Name} for {atkDmg} damage!");
                break;

            case AllyActionType.SpecialAttack:
                if (action.EnemyTarget == null) { Log("[*]No target!"); return; }
                if (!ally.CanUseSpecial) { Log($"[*] {ally.Name} has no mana!"); return; }
                int spDmg = ally.UseSpecialAttack();
                int dealt = action.EnemyTarget.TakeDamage(spDmg);
                Log($"[+] {ally.Name} uses a special attack on {action.EnemyTarget.Name} for {dealt} damage!");
                break;

            case AllyActionType.Defend:
                ally.StartDefend();
                Log($"[+] {ally.Name} defends — damage reduced by {(int)(BattleCharacter.DefendDamageReduction * 100)}% this round.");
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

    private List<Enemy> SpawnEnemies()
    {
        int count = Random.Range(minEnemies, maxEnemies + 1);
        var list  = new List<Enemy>();

        string[] names   = { "Goblin", "Orc", "Troll", "Bandit" };
        int[]    hpPool  = { 60,  80,  120, 70  };
        int[]    atkPool = { 12,  16,  10,  14  };

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, names.Length);
            string enemyName = count > 1 ? $"{names[idx]} {i + 1}" : names[idx];
            list.Add(new Enemy(enemyName, hpPool[idx], atkPool[idx], new RandomEnemyBehavior()));
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

    public int CurrentAllyIndex => _currentAllyIndex;
    public bool WaitingForInput => _waitingForPlayerInput;

    // ----- TEMP -----
    public GameObject TestStartButton;
    public void OnClickTest()
    {
        TestStartButton.SetActive(false);
        StartNewFight();
    }
    // ----- TEMP -----
}

// ----- ALLY ACTION STRUCTURE -----

public enum AllyActionType { Attack, SpecialAttack, Defend }

public class PendingAllyAction
{
    public AllyActionType ActionType;
    public Enemy          EnemyTarget;   // null for Defend

    public static PendingAllyAction MakeAttack(Enemy target)   =>
        new PendingAllyAction { ActionType = AllyActionType.Attack,        EnemyTarget = target };
    public static PendingAllyAction MakeSpecial(Enemy target)  =>
        new PendingAllyAction { ActionType = AllyActionType.SpecialAttack, EnemyTarget = target };
    public static PendingAllyAction MakeDefend()               =>
        new PendingAllyAction { ActionType = AllyActionType.Defend,        EnemyTarget = null   };
}
