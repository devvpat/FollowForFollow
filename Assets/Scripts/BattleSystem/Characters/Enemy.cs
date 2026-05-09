using UnityEngine;
using System.Collections.Generic;

// Represents an enemy character
public class Enemy : BattleCharacter
{
    // ----- STATS -----

    private IEnemyBehavior behavior;

    public Enemy(string name, float maxHP, float attack, float defense, float speed, float accuracy, float critChance, float critDamage, EnemyBehaviorType behaviorType)
        : base(name, maxHP, attack, defense, speed, accuracy, critChance, critDamage)
    {
        behavior = EnemyBehaviorFactory.GetBehavior(behaviorType);
    }

    public void SetBehavior(EnemyBehaviorType behaviorType)
    {
        behavior = EnemyBehaviorFactory.GetBehavior(behaviorType);
    }

    // ----- ACTIONS -----

    public string TakeTurn(List<Ally> allies, List<Enemy> enemies)
    {
        return behavior.DecideAction(this, allies, enemies);
    }
}
