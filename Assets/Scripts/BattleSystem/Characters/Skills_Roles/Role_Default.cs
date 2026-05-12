// Default: Has all default skills.
public class DefaultRole : IRole
{
    public string RoleName => "Default";

    public ISkill[] GetSkills() => new ISkill[]
    {
        new SlashSkill(),                   // 0: unchanged
        new HeavyStrikeSkill(),             // 1: unchanged
        new DrainSkill(),                   // 2: unchanged
        new FocusStrikeSkill(),             // 3: unchanged
    };
}