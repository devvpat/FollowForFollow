using UnityEngine;
using System.Collections.Generic;

public class MinionBufferBehavior : IEnemyBehavior
{
    public Enemy Self { get; set;}

    public string DecideAction(Enemy actor, List<Ally> allies, List<Enemy> enemies)
    {
        // find the boss
        Enemy boss = enemies.Find(e => e.Type == EnemyType.Boss && e.IsAlive);
        if (boss == null) return $"{Self.Name} has no boss to buff!";

        // randomly choose one of the skills to use on the boss
        int skillChoice = Random.Range(0, 3);
        switch (skillChoice)
        {
            case 0:
                return Skill_HasteAttack(boss);
            case 1:
                // give all alive enemies crit chance
                enemies.FindAll(e => e.IsAlive).ForEach(e => e.ApplyStatusEffect(EffectFactory.MakeCritRateModifier(duration: 3, amount: 0.2f))); // Crit Chance +20% for 3 turns
                return $"[-] {Self.Name} boosts crit chance of all enemies!";
            case 2:
                return Skill_Random_Buff(boss);
            default:
                return $"[-] {Self.Name} is confused and does nothing";
        }
    }

    private string Skill_HasteAttack(BattleCharacter target)
    {
        target.ApplyStatusEffect(EffectFactory.MakeSlow(duration: 2)); // Slow for 2 turns
        target.ApplyStatusEffect(EffectFactory.MakePoison(stacks: 3)); // 3 stacks of Poison
        return $"[-] {Self.Name} boosts Haste and Attack on {target.Name}";
    }

    private string Skill_Random_Buff(BattleCharacter target)
    {
        int debuffChoice = Random.Range(0, 3);
        switch (debuffChoice)
        {
            case 0:
                target.ApplyStatusEffect(EffectFactory.MakeBlur(duration: 3)); // 3 stacks of Blur
                return $"[-] {Self.Name} applies Blur on {target.Name}";
            case 1:
                target.ApplyStatusEffect(EffectFactory.MakeAttackModifier(duration: 3, multiplier: 0.2f)); // Attack 20% for 3 turns
                return $"[-] {Self.Name} boosts attack of {target.Name}";
            case 2:
                target.ApplyStatusEffect(EffectFactory.MakeDefenseModifier(duration: 3, multiplier: 0.2f)); // Defense 20% for 3 turns
                return $"[-] {Self.Name} boosts defense of {target.Name}";
            default:
                return $"[-] {Self.Name} is confused and does nothing";
        }
    }
}