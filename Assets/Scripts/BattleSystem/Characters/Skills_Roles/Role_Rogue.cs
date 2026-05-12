using UnityEngine;

// Rogue: FocusStrike becomes a guaranteed crit, Slash hits twice.
public class RogueRole : IRole
{
    public string RoleName => "Rogue";

    public ISkill[] GetSkills() => new ISkill[]
    {
        new RogueDoubleSlashSkill(),        // 0: modified — hits twice
        new HeavyStrikeSkill(),             // 1: unchanged
        new DrainSkill(),                   // 2: unchanged
        new RogueCritStrikeSkill(),         // 3: modified — guaranteed crit
    };
}

// Rogue's Slash: hits twice for 80% ATK each.
public class RogueDoubleSlashSkill : ISkill
{
    public string Name => "Double Slash";
    public int ManaCost => 15;
    public string Description => "Two quick strikes. Deals 80% ATK twice.";
    public bool BypassAccuracy => false;
    public SkillPower Power => SkillPower.Low;

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        ally.ModifyMultAttack(0.8f);
        float d1 = BattleManager.CalculateDamage(ally, target, skill: this);
        float d2 = BattleManager.CalculateDamage(ally, target, skill: this);
        ally.ModifyMultAttack(1/0.8f);
        float total = d1 + d2;
        target.TakeDamage(total);
        return SkillResult.Hit($"[+] {ally.Name} double slashes {target.Name} for {d1}+{d2} ({total}) damage!", total);
    }
}

// Rogue's FocusStrike: guaranteed hit, 200% ATK.
public class RogueCritStrikeSkill : ISkill
{
    public string Name => "Big Strike";
    public int ManaCost => 20;
    public string Description => "A big, guaranteed hit, deals 200% ATK damage.";
    public bool BypassAccuracy => true;
    public SkillPower Power => SkillPower.High;

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        ally.ModifyMultAttack(2f);
        float damage = BattleManager.CalculateAndApplyDamage(ally, target, skill: this);
        ally.ModifyMultAttack(1/2f);
        return SkillResult.Hit($"[+] {ally.Name} slams {target.Name} for {damage} damage!", damage);
    }
}