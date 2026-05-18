// Interface for ally character's roles
public interface IRole
{
    string RoleName { get; }
    ISkill[] GetSkills(); // will always have 4 skills
}