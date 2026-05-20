public static class BookwyrmSkills
{
    public class TrueTest : ISkill
    {
        public string Name => "True Test";
        public string Type => "Single Target Damage";
        public int ManaCost => 100;
        public float Power => 1;
        public SkillTargetType TargetType => SkillTargetType.Any;
        
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            target.TakeDamage(1);
            return SkillResult.Hit($"[+] {caster.Name} poked {target.Name} for 1 damage", 1);
        }
    }

    public class LoreDump : ISkill
    {
        public string Name => "Lore Dump";
        public string Type => "Group Buff";
        public int ManaCost => 120;
        public float Power => 0;
        public SkillTargetType TargetType => SkillTargetType.None;


        // [10% attack buff, 10% defense buff, 10% crit rate buff, haste] for 3 turns
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            foreach (var ally in BattleManager.Instance.GetLivingAllies())
            {
                ally.ApplyStatusEffect(EffectFactory.MakeAttackModifier(3, 1.10f));
                ally.ApplyStatusEffect(EffectFactory.MakeDefenseModifier(3, 1.10f));
                ally.ApplyStatusEffect(EffectFactory.MakeCritRateModifier(3, 0.10f));
                ally.ApplyStatusEffect(EffectFactory.MakeHaste(3));
            }
            return SkillResult.Hit($"[+] {caster.Name} shared some lore, buffing the party's attack, defense, crit rate, and speed for 3 turns!", 0);
        }
    }
}