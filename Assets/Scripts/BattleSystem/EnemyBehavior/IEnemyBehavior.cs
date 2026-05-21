using System.Collections.Generic;

// Interface for enemy behavior pattern
public interface IEnemyBehavior
{
    Enemy Self { get; set;}

    // Does an action and returns a string describing it
    string DecideAction(Enemy actor, List<Ally> allies, List<Enemy> enemies);

    void Bind(Enemy enemy) => Self = enemy;
}
