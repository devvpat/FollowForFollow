// Cleric: Drain becomes a heal targeting an ally instead, FocusStrike buffs defense.
public class ClericRole : IRoleDefinition
{
    public string RoleName => "Cleric";

    public ISkill[] GetSkills() => new ISkill[]
    {
        new SlashSkill(),                   // 0: unchanged
        new HeavyStrikeSkill(),             // 1: unchanged
        new ClericHealSkill(),              // 2: modified — heals instead of damages
        new ClericFortifySkill(),           // 3: modified — raises target defense
    };
}

// Cleric's Drain replacement: heals itself.
public class ClericHealSkill : ISkill
{
    public string Name => "Self-Heal";
    public int ManaCost => 25;
    public string Description => "Restores 60 HP to self.";
    public bool BypassAccuracy => true;

    private const int HealAmount = 60;

    public SkillResult Execute(Ally caster, BattleCharacter target)
    {
        caster.RestoreHP(HealAmount);
        return SkillResult.Hit($"[+] {caster.Name} heals for {HealAmount} HP!", 0);
    }
}

// Cleric's FocusStrike replacement: raises caster defense by 5.
public class ClericFortifySkill : ISkill
{
    public string Name => "Fortify";
    public int ManaCost => 50;
    public string Description => "Raises your defense by 5%.";
    public bool BypassAccuracy => true;

    private const int DefenseBonus = 5;

    public SkillResult Execute(Ally caster, BattleCharacter target)
    {
        caster.ModifyDefense(DefenseBonus);
        return SkillResult.Hit($"🛡 {caster.Name} fortifies, gaining +{DefenseBonus}% defense!", 0);
    }
}