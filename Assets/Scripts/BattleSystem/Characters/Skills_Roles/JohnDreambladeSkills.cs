using System;
using System.Linq;

public static class JohnDreambladeSkills
{
    public class AllKill : ISkill
    {
        public string Name => "All-Kill";
        public string Type => "AOE";
        public int ManaCost => 110;
        public float Power => 3250;
        public SkillTargetType TargetType => SkillTargetType.None;

        // Does damage to every enemy
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");

            var alive_enemies = BattleManager.Instance.GetLivingOpponentsOf(caster);
            float total = 0;
            foreach (BattleCharacter e in alive_enemies)
            {
                if (e.PerformDodgeCheck()) continue;
                total += BattleManager.CalculateAndApplyDamage(caster, e, skill: this);
                // 75% chance to apply stun for 1 turn
                if (UnityEngine.Random.value < 0.75) e.ApplyStatusEffect(EffectFactory.MakeStun(2));
            }
            return SkillResult.Success($"[+] {caster.Name} used All-Kill on all enemies for a total of {total} damage", total);
        }
    }

    public class BiasWrecker : ISkill
    {
        public string Name => "Bias Wrecker";
        public string Type => "Single Target Burst";
        public int ManaCost => 90;
        public float Power => 4500;
        public SkillTargetType TargetType => SkillTargetType.Enemy;

        // Deals high damage to one target
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            if (target.PerformDodgeCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but {target.Name} dodged");

            float total = BattleManager.CalculateAndApplyDamage(caster, target, skill: this);
            return SkillResult.Success($"[+] {caster.Name} used Bias Wrecker on {target.Name} for {total} damage", total);
        }
    }

    public class Aegyo : ISkill
    {
        public string Name => "Aegyo";
        public string Type => "Party Shield";
        public int ManaCost => 150;
        public float Power => 2500;
        public SkillTargetType TargetType => SkillTargetType.None;

        // Gives all allies a 3 turn shield
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");

            foreach (BattleCharacter ally in BattleManager.Instance.GetLivingTeamOf(caster))
            {
                ally.ApplyStatusEffect(EffectFactory.MakeShield(3));
            }
            return SkillResult.Success($"[+] {caster.Name} used Aegyo on all allies", 0);
        }
    }

    public class VacantVoid : ISkill
    {
        public string Name => "Vacant Void";
        public string Type => "Buff Stripper";
        public int ManaCost => 120;
        public float Power => 3000;
        public SkillTargetType TargetType => SkillTargetType.Enemy;

        // Removes [2] random buffs from the enemy
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            if (target.PerformDodgeCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but {target.Name} dodged");

            var buffIndicies = target.StatusEffects.FindAll(e => e.EffectType == StatusEffectType.Buff).Select(e => target.StatusEffects.IndexOf(e)).ToList();
            buffIndicies.Shuffle();
            for (int i = 0; i < Math.Min(2, buffIndicies.Count); i++)
            {
                target.RemoveStatusEffect(buffIndicies[i]);
            }
            return SkillResult.Success($"[+] {caster.Name} used Vacant Void on {target.Name} and removed {Math.Min(2, buffIndicies.Count)} buffs", Math.Min(2, buffIndicies.Count));
        }
    }
}
