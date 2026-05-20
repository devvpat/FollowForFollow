// Interface for ally character's skills
public enum SkillTargetType { None, Enemy, Ally, Self, Any}

public interface ISkill
{
    const float SkillPowerScale = 1.15f; // total power = power * (skillPowerScale ^ AllyParty.LevelScale)

    string Name { get; }
    string Type { get; }
    int ManaCost { get; }
    float Power { get; }
    SkillTargetType TargetType { get; }

    bool BypassAccuracy => false;

    // Executes the skill - has logic for applying the skill's effects
    // Accuracy check is handled by BattleManager before this is called
    // Returns a SkillResult
    SkillResult Execute(BattleCharacter caster, BattleCharacter target);
}