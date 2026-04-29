using UnityEngine;

// Mage: Slash becomes a magic bolt (ignore target defense), Drain restores more mana.
public class MageRole : IRoleDefinition
{
    public string RoleName => "Mage";

    public ISkill[] GetSkills() => new ISkill[]
    {
        new MageBoltSkill(),                // 0: modified — replaces Slash
        new HeavyStrikeSkill(),             // 1: unchanged
        new MageDrainSkill(),               // 2: modified — more mana restored
        new FocusStrikeSkill(),             // 3: unchanged
    };
}

// Mage's Slash replacement: magic damage that ignores target defense.
public class MageBoltSkill : ISkill
{
    public string Name => "Magic Bolt";
    public int ManaCost => 15;
    public string Description => "A bolt of magic. Deals 120% ATK, ignores defense.";
    public bool BypassAccuracy => false;

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        // Bypass TakeDamage (which applies defense) — apply directly to HP
        int damage = Mathf.RoundToInt(ally.Attack * 1.2f);
        target.TakeDamageRaw(damage);
        return SkillResult.Hit($"[+] {ally.Name} blasts {target.Name} with magic for {damage} damage!", damage);
    }
}

// Mage's Drain: restores 40 mana instead of 20.
public class MageDrainSkill : ISkill
{
    public string Name => "Drain";
    public int ManaCost => 25;
    public string Description => "Drains energy. Deals 80% ATK damage and restores 40 mana.";
    public bool BypassAccuracy => false;

    private const int ManaRestore = 40;

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        int raw = UnityEngine.Mathf.RoundToInt(ally.Attack * 0.8f);
        int damage = target.TakeDamage(raw);
        ally.RestoreMana(ManaRestore);
        return SkillResult.Hit($"[+] {ally.Name} drains {target.Name} for {damage} damage and restores {ManaRestore} mana!", damage);
    }
}