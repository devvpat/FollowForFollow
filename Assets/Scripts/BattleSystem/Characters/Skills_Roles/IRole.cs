// Interface for ally character's roles
public interface IRoleDefinition
{
    string RoleName { get; }
    ISkill[] GetSkills(); // will always have 4 skills
}