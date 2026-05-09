using UnityEngine;
using System.Collections.Generic;

// Represents an enemy character
public class Enemy : BattleCharacter
{
    // ----- STATS -----

    private IEnemyBehavior _behavior;

    public Enemy(string name, int maxHP, int attack, int defense, int speed, int accuracy, int critChance, int critDamage, IEnemyBehavior behavior)
        : base(name, maxHP, attack, defense, speed, accuracy, critChance, critDamage)
    {
        _behavior = behavior;
    }

    public void SetBehavior(IEnemyBehavior behavior)
    {
        _behavior = behavior;
    }

    // ----- ACTIONS -----

    public string TakeTurn(List<Ally> allies, List<Enemy> enemies)
    {
        EndDefend();  // reset defend state at the start of each turn
        return _behavior.DecideAction(this, allies, enemies);
    }
}
