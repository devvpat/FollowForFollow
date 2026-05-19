public static class BookwyrmSkills
{
    public class TrueTest : ISkill
    {
        public string Name => "True Test";
        public string Type => "Single Target Damage";
        public int ManaCost => 100;
        public float Power => 1;
        
        public SkillResult Execute(Ally ally, BattleCharacter target)
        {
            target.TakeDamage(1);
            return SkillResult.Hit($"[+] {ally.Name} poked {target.Name} for 1 damage", 1);
        }
    }
}