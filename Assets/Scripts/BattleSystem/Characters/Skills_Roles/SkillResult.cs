// Class that represents the result of executing a skill
// Includes a Log message, the damage dealt, and if the skill hit the target
public class SkillResult
{
    public string LogMessage;
    public int DamageDealt;
    public bool HitTarget;

    public static SkillResult Miss(string msg) =>
        new SkillResult { LogMessage = msg, HitTarget = false, DamageDealt = 0 };

    public static SkillResult Hit(string msg, int damage) =>
        new SkillResult { LogMessage = msg, HitTarget = true, DamageDealt = damage };
}