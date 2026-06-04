public static class KaraageSkills
{
    public class Yabai : ISkill
    {
        public string Name => "Yabai!";
        public string Type => "Multi-Hit";
        public string Description => "Strike an enemy 4 times in rapid succession.";
        public int ManaCost => 70;
        public float Power => 750;
        public SkillTargetType TargetType => SkillTargetType.Enemy;
        
        // Hit an enemy 4 times
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            if (target.PerformDodgeCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but {target.Name} dodged");
            
            float total = 0;
            for (int i = 0; i < 4; ++i)
            {
                total += BattleManager.CalculateAndApplyDamage(caster, target, skill: this);
                if (caster is Ally ally)
                {
                    ally.IncreaseHitCount(1);
                }
            }
            return SkillResult.Success($"[+] {caster.Name} used Yabai! on {target.Name} for a total of {total} damage", total);
        }
    }

    public class WingClipper : ISkill
    {
        public string Name => "Wing Clipper";
        public string Type => "Utility/Burst";
        public string Description => "Deal heavy damage and slow the target for 1 turn.";
        public int ManaCost => 90;
        public float Power => 3200;
        public SkillTargetType TargetType => SkillTargetType.Enemy;
        
        // Deals high damage to one target and slows them for [1 turn]
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            if (target.PerformDodgeCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but {target.Name} dodged");

            float total = BattleManager.CalculateAndApplyDamage(caster, target, skill: this);
            target.ApplyStatusEffect(EffectFactory.MakeSlow(2));
            if (caster is Ally ally)
            {
                ally.IncreaseHitCount(1);
            }
            return SkillResult.Success($"[+] {caster.Name} used Wing Clipper on {target.Name} for {total} damage and slowed them for 1 turn", total);
        }
    }

    public class FeatherStitch : ISkill
    {
        public string Name => "Feather Stitch";
        public string Type => "Single Target Heal";
        public string Description => "Restore 25% of an ally's max HP.";
        public int ManaCost => 100;
        public float Power => 0;
        public SkillTargetType TargetType => SkillTargetType.Ally;
        
        // Heals for [25%] Max HP
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            if (target.PerformDodgeCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but {target.Name} dodged");

            float heal = target.MaxHP * 0.25f;
            target.RestoreHP(heal);
            return SkillResult.Success($"[+] {caster.Name} used Feather Stitch on {target.Name} to heal them {heal} HP", heal);
        }
    }

    public class ClipFarm : ISkill
    {
        public string Name => "Clip Farm";
        public string Type => "RNG Multi-Hit";
        public string Description => "Hit an enemy 1-8 times at random.";
        public int ManaCost => 130;
        public float Power => 1000;
        public SkillTargetType TargetType => SkillTargetType.Enemy;
        
        // Hits the character 1-8 times
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            if (target.PerformDodgeCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but {target.Name} dodged");

            float total = 0;
            int hits = UnityEngine.Random.Range(1, 9);
            for (int i = 0; i < hits; ++i)
            {
                total += BattleManager.CalculateAndApplyDamage(caster, target, skill: this);
                if (caster is Ally ally)
                {
                    ally.IncreaseHitCount(1);
                }
            }
            return SkillResult.Success($"[+] {caster.Name} used Clip Farm on {target.Name} for a total of {total} damage", total);
        }
    }
}