// Interface for ally character's skills
public interface ISkill
{
    string Name { get; }
    int ManaCost { get; }
    string Description { get; }
    bool BypassAccuracy => false;

    // Executes the skill - has logic for applying the skill's effects
    // Accuracy check is handled by BattleManager before this is called
    // Returns a SkillResult
    SkillResult Execute(Ally ally, BattleCharacter target);
}