// Interface for ally character's skills
public interface ISkill
{
    const float SkillPowerScale = 1.15f; // total power = power * (skillPowerScale ^ AllyParty.LevelScale)

    string Name { get; }
    string Type { get; }
    int ManaCost { get; }
    float Power { get; }
    
    bool BypassAccuracy => false;
    bool NeedsTarget => true;
    

    // Executes the skill - has logic for applying the skill's effects
    // Accuracy check is handled by BattleManager before this is called
    // Returns a SkillResult
    SkillResult Execute(Ally ally, BattleCharacter target);
}