using UnityEngine;

// Warrior: brute force. HeavyStrike costs less and hits harder.
public class WarriorRole : IRoleDefinition
{
    public string RoleName => "Warrior";

    public ISkill[] GetSkills() => new ISkill[]
    {
        new SlashSkill(),                   // 0: unchanged
        new WarriorHeavyStrikeSkill(),      // 1: modified — cheaper, more damage
        new DrainSkill(),                   // 2: unchanged
        new FocusStrikeSkill(),             // 3: unchanged
    };
}

// Warrior's version of Heavy Strike: 300% ATK, costs 30 mana instead of 40.
public class WarriorHeavyStrikeSkill : ISkill
{
    public string Name => "Heavy Strike";
    public int ManaCost => 30;
    public string Description => "Warrior's brutal blow. Deals 300% ATK damage.";
    public bool BypassAccuracy => false;

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        int raw = Mathf.RoundToInt(ally.Attack * 3.0f);
        int damage = target.TakeDamage(raw);
        return SkillResult.Hit($"[+] {ally.Name} crushes {target.Name} for {damage} damage!", damage);
    }
}