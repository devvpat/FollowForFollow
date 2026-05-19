using UnityEngine;
using System.Collections.Generic;

// Represents an enemy character
public class Enemy : BattleCharacter
{
    // ----- STATS -----

    private IEnemyBehavior behavior;

    public Enemy(string name, float maxHP, float attack, float defense, float speed, float accuracy, float critChance, float critDamage, EnemyBehaviorType behaviorType, int level)
        : base(name, maxHP, attack, defense, speed, accuracy, critChance, critDamage, level)
    {
        behavior = EnemyBehaviorFactory.GetBehavior(behaviorType);
    }

    public void SetBehavior(EnemyBehaviorType behaviorType)
    {
        behavior = EnemyBehaviorFactory.GetBehavior(behaviorType);
    }

    public static Enemy CreateFromData(EnemyData data)
    {
        return new Enemy(data.Name, data.MaxHP, data.Attack, data.Defense, data.Speed, data.Accuracy, data.CritChance, data.CritDamage, data.Behavior, data.Level);
    }

    // ----- ACTIONS -----

    public string TakeTurn(List<Ally> allies, List<Enemy> enemies)
    {
        return behavior.DecideAction(this, allies, enemies);
    }
}
