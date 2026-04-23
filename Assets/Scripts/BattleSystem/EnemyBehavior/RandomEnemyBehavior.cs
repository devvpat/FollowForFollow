using System.Collections.Generic;
using UnityEngine;

// Simple enemy behavior that randomly chooses to attack or defend each turn.
public class RandomEnemyBehavior : IEnemyBehavior
{
    // Attack 60%, Defend 40%
    private const float AttackWeight  = 0.60f;

    public string DecideAction(Enemy actor, List<Ally> allies, List<Enemy> enemies)
    {
        if (allies.Count == 0)
            return $"{actor.Name} looks around confused — no targets!";

        float roll = Random.value;

        if (roll < AttackWeight)
        {
            // Choose random ally and attack
            Ally target = allies[Random.Range(0, allies.Count)];
            int damage  = target.TakeDamage(actor.GetAttackDamage());
            return $"[-] {actor.Name} attacks {target.Name} for {damage} damage!";
        }
        else
        {
            // Enemy defends itself
            actor.StartDefend();
            return $"[-] {actor.Name} takes a defensive stance.";
        }
    }
}
