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
            return SkillResult.Success($"[+] {caster.Name} poked {target.Name} for 1 damage", 1);
        }
    }

    public class LoreDump : ISkill
    {
        public string Name => "Lore Dump";
        public string Type => "Group Buff";
        public int ManaCost => 120;
        public float Power => 0;
        public SkillTargetType TargetType => SkillTargetType.None;


        // [10% attack buff, 10% defense buff, 10% crit rate buff] for 3 turns
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");

            foreach (var ally in BattleManager.Instance.GetLivingTeamOf(caster))
            {
                ally.ApplyStatusEffect(EffectFactory.MakeAttackModifier(4, 1.10f));
                ally.ApplyStatusEffect(EffectFactory.MakeDefenseModifier(4, 1.10f));
                ally.ApplyStatusEffect(EffectFactory.MakeCritRateModifier(4, 0.10f));
                ally.RestoreHP(ally.MaxHP * 0.15f);
                // ally.ApplyStatusEffect(EffectFactory.MakeHaste(3));
            }
            return SkillResult.Success($"[+] {caster.Name} shared some lore, buffing the party's attack, defense, crit rate, and speed for 3 turns!", 0);
        }
    }

    public class Demonization : ISkill
    {
        public string Name => "Demonitization";
        public string Type => "Single Target Debuff";
        public int ManaCost => 80;
        public float Power => 1200;
        public SkillTargetType TargetType => SkillTargetType.Enemy;

        // Lowers target Defense by [100%] for [2 turns].
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            if (target.PerformDodgeCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but {target.Name} dodged");

            target.ApplyStatusEffect(EffectFactory.MakeDefenseModifier(3, -1.00f));
            return SkillResult.Success($"[+] {caster.Name} demonized {target.Name}, lowering their defense by 100% for 2 turns!", 1200);
        }
    }

    public class FrameManipulation : ISkill
    {
        public string Name => "Frame Manipulation";
        public string Type => "Turn Manipulation";
        public int ManaCost => 200;
        public float Power => 0;
        public SkillTargetType TargetType => SkillTargetType.Ally;

        // Target ally gains [10,000] Speed Bucket progress and 50% more attack for 1 turn.
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            if (target.PerformDodgeCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but {target.Name} dodged");

            target.ApplyStatusEffect(EffectFactory.MakeAttackModifier(2, 0.50f));

            target.AddToTickTimer(10000);
            if (target.TickTimer >= BattleManager.BattleTickThreshold)
                BattleManager.Instance.AddToTakingTurnQueue(target, target.TickTimer);
            return SkillResult.Success($"[+] {caster.Name} gave {target.Name} a boost in speed!", 0);
        }
    }

    public class MouseTrap : ISkill
    {
        public string Name => "Mouse Trap";
        public string Type => "Trap";
        public int ManaCost => 150;
        public float Power => 1800;
        public SkillTargetType TargetType => SkillTargetType.Enemy;

        // Target is Blinded for [2 turns].
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            if (target.PerformDodgeCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but {target.Name} dodged");

            target.ApplyStatusEffect(EffectFactory.MakeBlind(3));
            return SkillResult.Success($"[+] {caster.Name} set a mouse trap, blinding {target.Name} for 2 turns!", 1800);
        }
    }
}
