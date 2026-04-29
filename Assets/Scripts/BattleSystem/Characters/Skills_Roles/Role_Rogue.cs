using UnityEngine;

// Rogue: FocusStrike becomes a guaranteed crit, Slash hits twice.
public class RogueRole : IRoleDefinition
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

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        int raw1 = Mathf.RoundToInt(ally.Attack * 0.8f);
        int raw2 = Mathf.RoundToInt(ally.Attack * 0.8f);
        int d1 = target.TakeDamage(raw1);
        int d2 = target.IsAlive ? target.TakeDamage(raw2) : 0;
        int total = d1 + d2;
        return SkillResult.Hit($"[+] {ally.Name} double slashes {target.Name} for {d1}+{d2} ({total}) damage!", total);
    }
}

// Rogue's FocusStrike: guaranteed hit, 200% ATK.
public class RogueCritStrikeSkill : ISkill
{
    public string Name => "Crit Strike";
    public int ManaCost => 20;
    public string Description => "A precise crit. Guaranteed hit, deals 200% ATK damage.";
    public bool BypassAccuracy => true;

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        int raw = Mathf.RoundToInt(ally.Attack * 2.0f);
        int damage = target.TakeDamage(raw);
        return SkillResult.Hit($"[+] {ally.Name} crits {target.Name} for {damage} damage!", damage);
    }
}