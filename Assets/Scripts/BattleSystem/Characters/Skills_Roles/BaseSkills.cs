using UnityEngine;

// Has all 4 base skills that every role can use

// Skill 0 - Slash: A quick strike. Deals 120% ATK damage.
public class SlashSkill : ISkill
{
    public string Name => "Slash";
    public int ManaCost => 15;
    public string Description => "A quick strike. Deals 120% ATK damage.";

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        int raw = Mathf.RoundToInt(ally.Attack * 1.2f);
        int damage = target.TakeDamage(raw);
        return SkillResult.Hit($"[+] {ally.Name} slashes {target.Name} for {damage} damage!", damage);
    }
}

// Skill 1 - Heavy Strike: A powerful blow. Deals 250% ATK damage.
public class HeavyStrikeSkill : ISkill
{
    public string Name => "Heavy Strike";
    public int ManaCost => 40;
    public string Description => "A powerful blow. Deals 250% ATK damage.";

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        int raw = Mathf.RoundToInt(ally.Attack * 2.5f);
        int damage = target.TakeDamage(raw);
        return SkillResult.Hit($"[+] {ally.Name} lands a heavy strike on {target.Name} for {damage} damage!", damage);
    }
}

// Skill 2 - Drain: Drains energy. Deals 80% ATK damage and restores 20 mana.
public class DrainSkill : ISkill
{
    public string Name => "Drain";
    public int ManaCost => 25;
    public string Description => "Drains energy. Deals 80% ATK damage and restores 20 mana.";

    private const int ManaRestore = 20;

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        int raw = Mathf.RoundToInt(ally.Attack * 0.8f);
        int damage = target.TakeDamage(raw);
        ally.RestoreMana(ManaRestore);
        return SkillResult.Hit($"[+] {ally.Name} drains {target.Name} for {damage} damage and restores {ManaRestore} mana!", damage);
    }
}

// Skill 3 - Focus Strike: Attack with full concentration. Guaranteed to hit. Deals 100% ATK damage.
public class FocusStrikeSkill : ISkill
{
    public string Name => "Focus Strike";
    public int ManaCost => 20;
    public string Description => "Attack with full concentration. Guaranteed to hit. Deals 100% ATK damage.";
    public bool BypassAccuracy => true;

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        int damage = target.TakeDamage(ally.Attack);
        return SkillResult.Hit($"[+] {ally.Name} focus strikes {target.Name} for {damage} damage!", damage);
    }
}