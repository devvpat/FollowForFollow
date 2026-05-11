// Cleric: Drain becomes a heal targeting an ally instead, FocusStrike buffs defense.
public class ClericRole : IRole
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
    public bool NeedsTarget => false;
    public SkillPower Power => SkillPower.None;

    private const int HealAmount = 60;

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        ally.RestoreHP(HealAmount);
        return SkillResult.Hit($"[+] {ally.Name} heals for {HealAmount} HP!", 0);
    }
}

// Cleric's FocusStrike replacement: raises caster defense by 5.
public class ClericFortifySkill : ISkill
{
    public string Name => "Fortify";
    public int ManaCost => 50;
    public string Description => "Raises your defense by 2%.";
    public bool BypassAccuracy => true;
    public bool NeedsTarget => false;
    public SkillPower Power => SkillPower.None;

    private const float DefenseBonus = 0.02f; // 2% defense boost

    public SkillResult Execute(Ally ally, BattleCharacter target)
    {
        ally.ModifyAddDefense(DefenseBonus);
        return SkillResult.Hit($"[+] {ally.Name} fortifies, gaining +{DefenseBonus}% defense!", 0);
    }
}