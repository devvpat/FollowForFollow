using UnityEngine;
using System.Collections.Generic;

public class MinionDebufferBehavior : IEnemyBehavior
{
    public Enemy Self { get; set;}

    public string DecideAction(Enemy actor, List<Ally> allies, List<Enemy> enemies)
    {
        // prioritize ally DPS or Support
        List<Ally> validTargets = allies.FindAll(a => a.IsAlive);
        var gc_or_bdps = validTargets.FindAll(a => a.Role == AllyRole.GlassCannon || a.Role == AllyRole.BurstDPS);

        Ally target = null;
        if (gc_or_bdps.Count > 0) target = gc_or_bdps[Random.Range(0, gc_or_bdps.Count)];
        else if (validTargets.Count > 0) target = validTargets[Random.Range(0, validTargets.Count)];
        else return $"{Self.Name} has no targets to debuff!";

        // randomly choose one of the skills to use on the target
        int skillChoice = Random.Range(0, 3);
        return skillChoice switch
        {
            0 => Skill_SlowPoison(target),
            1 => Skill_Silence(target),
            2 => Skill_Random_Debuff(target),
            _ => $"[-] {Self.Name} is confused and does nothing",
        };
    }

    private string Skill_SlowPoison(BattleCharacter target)
    {
        target.ApplyStatusEffect(EffectFactory.MakeSlow(duration: 2)); // Slow for 2 turns
        target.ApplyStatusEffect(EffectFactory.MakePoison(stacks: 3)); // 3 stacks of Poison
        return $"[-] {Self.Name} debuffs {target.Name} with Slow and Poison";
    }

    private string Skill_Silence(BattleCharacter target)
    {
        // 30% chance of success
        if (Random.value > 0.3f)
            return $"[-] {Self.Name} tries to use Silence on {target.Name} but fails!";

        target.ApplyStatusEffect(EffectFactory.MakeSilence(duration: 2)); // Silence for 2 turns
        return $"[-] {Self.Name} uses Silence on {target.Name}";
    }

    private string Skill_Random_Debuff(BattleCharacter target)
    {
        int debuffChoice = Random.Range(0, 3);
        switch (debuffChoice)
        {
            case 0:
                target.ApplyStatusEffect(EffectFactory.MakeBlind(duration: 2)); // 2 stacks of Blind
                return $"[-] {Self.Name} applies Blind on {target.Name}";
            case 1:
                target.ApplyStatusEffect(EffectFactory.MakeAttackModifier(duration: 2, multiplier: -0.2f)); // Attack -20% for 2 turns
                return $"[-] {Self.Name} reduces attack of {target.Name}";
            case 2:
                target.ApplyStatusEffect(EffectFactory.MakeDefenseModifier(duration: 2, multiplier: -0.2f)); // Defense -20% for 2 turns
                return $"[-] {Self.Name} reduces defense of {target.Name}";
            default:
                return $"[-] {Self.Name} is confused and does nothing";
        }
    }
}