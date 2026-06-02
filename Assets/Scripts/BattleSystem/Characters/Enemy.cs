using UnityEngine;
using System.Collections.Generic;
using System;

public enum EnemyType { Minion, Boss }

// Represents an enemy character
public class Enemy : BattleCharacter
{
    // ----- STATS -----

    public EnemyType Type { get; }

    private IEnemyBehavior behavior;

    // The ally this enemy has copied, if it's a mimic that has copied someone; null otherwise.
    public Ally CopiedAlly => (behavior as BossMimicBehavior)?.CopiedAlly;

    private const float MinionLevelScale = 1.15f; // 15%

    public Enemy(string name, float maxHP, float attack, float defense, float speed, float accuracy, float critChance, float critDamage, EnemyBehaviorType behaviorType, int level, EnemyType enemyType)
        : base(name, maxHP, attack, defense, speed, accuracy, critChance, critDamage, level)
    {
        Type = enemyType;
        behavior = EnemyBehaviorFactory.GetBehavior(behaviorType);
        behavior.Bind(this);

        // if minion, scale hp and speed
        if (enemyType == EnemyType.Minion)
        {
            MaxHP *= Mathf.Pow(MinionLevelScale, AllyParty.Instance.LevelScale);
            CurrentHP = MaxHP;
            Speed *= Mathf.Pow(MinionLevelScale, AllyParty.Instance.LevelScale);
        }
    }

    public void SetBehavior(EnemyBehaviorType behaviorType)
    {
        behavior = EnemyBehaviorFactory.GetBehavior(behaviorType);
        behavior.Bind(this);
    }

    public static Enemy CreateFromData(EnemyData data)
    {
        return new Enemy(data.Name, data.MaxHP, data.Attack, data.Defense, data.Speed, data.Accuracy, data.CritChance, data.CritDamage, data.Behavior, data.Level, data.Type);
    }

    // ----- ACTIONS -----

    public string TakeTurn(List<Ally> allies, List<Enemy> enemies)
    {
        return behavior.DecideAction(this, allies, enemies);
    }
}
