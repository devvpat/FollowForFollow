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
            // Choose random target from allies
            Ally target = allies[Random.Range(0, allies.Count)];

            // Accuracy check
            bool hit = Random.Range(0, 100) < actor.Accuracy;
            if (!hit) return $"[-] {actor.Name} attacks {target.Name} but misses!";

            // Attack enemy target
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
