using UnityEngine;

// Represents an enemy character
public class Enemy : BattleCharacter
{
    // ----- STATS -----

    private IEnemyBehavior _behavior;

    public Enemy(string name, int maxHP, int attack, IEnemyBehavior behavior)
        : base(name, maxHP, attack)
    {
        _behavior = behavior;
    }

    public void SetBehavior(IEnemyBehavior behavior)
    {
        _behavior = behavior;
    }

    // ----- ACTIONS -----

    public string TakeTurn(System.Collections.Generic.List<Ally> allies,
                           System.Collections.Generic.List<Enemy> enemies)
    {
        EndDefend();  // reset defend state at the start of each turn
        return _behavior.DecideAction(this, allies, enemies);
    }

    public int GetAttackDamage() => Attack;
}
