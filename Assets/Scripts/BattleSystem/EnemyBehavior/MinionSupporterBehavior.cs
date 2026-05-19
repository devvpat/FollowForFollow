using UnityEngine;
using System.Collections.Generic;

public class MinionSupporterBehavior : IEnemyBehavior
{
    public Enemy Self { get; set;}

    public string DecideAction(Enemy actor, List<Ally> allies, List<Enemy> enemies)
    {
        // find the boss
        Enemy boss = enemies.Find(e => e.Type == EnemyType.Boss && e.IsAlive);
        if (boss == null) return $"{Self.Name} has no boss to buff!";

        // randomly choose one of the skills to use on the boss
        float boss_hp_perc = boss.CurrentHP / boss.MaxHP;
        int totalSkills = (boss_hp_perc < 0.3f || boss_hp_perc > 0.5f) ? 3 : 2; // If boss HP is below 30% or above 50%, all 3 skills are available. Otherwise, only the first 2 skills are available.
        int skillChoice = Random.Range(0, totalSkills);
        switch (skillChoice)
        {
            case 0:
                // applies 3 shield and 30% defense to all enemies for 2 turns
                enemies.FindAll(e => e.IsAlive).ForEach(e => e.ApplyStatusEffect(EffectFactory.MakeShield(durability: 3)));
                enemies.FindAll(e => e.IsAlive).ForEach(e => e.ApplyStatusEffect(EffectFactory.MakeDefenseModifier(duration: 2, multiplier: 0.3f)));
                return $"[-] {Self.Name} applies shields and defense boost to all enemies!";
            case 1:
                // heals the boss for 25% of its max hp
                float healAmount = boss.MaxHP * 0.25f;
                boss.RestoreHP(healAmount);
                return $"[-] {Self.Name} heals {boss.Name} for {healAmount} HP!";
            case 2:
                // if boss hp below 30%, heal the boss for 50% of its max hp
                if (boss_hp_perc < 0.3f)
                {
                    float bigHealAmount = boss.MaxHP * 0.5f;
                    boss.RestoreHP(bigHealAmount);
                    return $"[-] {Self.Name} heals {boss.Name} for {bigHealAmount} HP!";
                }
                // else if the boss is above 50% hp, give the boss 5 durability shield
                else if (boss_hp_perc > 0.5f)
                {
                    boss.ApplyStatusEffect(EffectFactory.MakeShield(durability: 5));
                    return $"[-] {Self.Name} gives {boss.Name} a strong shield!";
                }
                else
                {
                    return $"[-] {Self.Name} is confused and does nothing";
                }
            default:
                return $"[-] {Self.Name} is confused and does nothing";
        }
    }
}