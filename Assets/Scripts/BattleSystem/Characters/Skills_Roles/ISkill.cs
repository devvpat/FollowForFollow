// Interface for ally character's skills
public interface ISkill
{
    string Name { get; }
    int ManaCost { get; }
    string Description { get; }
    bool BypassAccuracy => false;
    bool NeedsTarget => true;
    SkillPower Power { get; }
    const float SkillPowerScale = 1.15f; // total power scale = skillPowerScale ^ AllyParty.LevelScale

    // Executes the skill - has logic for applying the skill's effects
    // Accuracy check is handled by BattleManager before this is called
    // Returns a SkillResult
    SkillResult Execute(Ally ally, BattleCharacter target);
}

// Enum representing skill power, used for calculating damage
public enum SkillPower
{
    None = 0,
    Low = 5,
    Medium = 10,
    High = 20
}