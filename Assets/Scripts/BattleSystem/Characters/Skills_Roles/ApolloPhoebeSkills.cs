using UnityEngine;

public static class ApolloPhoebeSkills
{
    public class WorldCuttingSlash : ISkill
    {
        public string Name => "World Cutting Slash";
        public string Type => "Burst";
        public string Description => "Sacrifice 15% HP to deal massive damage to all enemies.";
        public int ManaCost => 0;
        public float Power => 5000;
        public SkillTargetType TargetType => SkillTargetType.None;

        // Deals high damage to all targets
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");

            // cost 15% of max health
            caster.SetHP(caster.CurrentHP - caster.MaxHP * 0.15f);
            if (caster.CurrentHP <= 0) return SkillResult.Fail($"[+] {caster.Name} used World Cutting Slash but died from the cost");

            float total = 0;
            foreach (BattleCharacter enemy in BattleManager.Instance.GetLivingOpponentsOf(caster))
            {
                if (enemy.PerformDodgeCheck()) continue;
                total += BattleManager.CalculateAndApplyDamage(caster, enemy, skill: this);
            }
            return SkillResult.Success($"[+] {caster.Name} used World Cutting Slash on all enemies for a total of {total} damage", total);
        }
    }

    public class Riposte : ISkill
    {
        public string Name => "Riposte";
        public string Type => "Parry";
        public string Description => "Gain a shield that blocks 2 hits.";
        public int ManaCost => 120;
        public float Power => 3500;
        public SkillTargetType TargetType => SkillTargetType.None;

        // Gains a 2 durability shield
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            caster.ApplyStatusEffect(EffectFactory.MakeShield(2));
            return SkillResult.Success($"[+] {caster.Name} used Riposte and gained a 2 durability shield", 2);
        }
    }

    public class Instigation : ISkill
    {
        public string Name => "Instigation";
        public string Type => "Taunt";
        public string Description => "Boost own defense by 75% for 2 turns.";
        public int ManaCost => 100;
        public float Power => 0;
        public SkillTargetType TargetType => SkillTargetType.None;

        // Gains high defense for 2 turns
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");
            caster.ApplyStatusEffect(EffectFactory.MakeDefenseModifier(3, 1.75f));
            return SkillResult.Success($"[+] {caster.Name} used Instigation and gained high defense for 2 turns", 2);
        }
    }

    public class Blackout : ISkill
    {
        public string Name => "Blackout";
        public string Type => "Execute";
        public string Description => "Hit all enemies harder the lower their HP. Up to 3x damage.";
        public int ManaCost => 250;
        public float Power => 4000;
        public SkillTargetType TargetType => SkillTargetType.None;

        // Deal high damage to all targets
        public SkillResult Execute(BattleCharacter caster, BattleCharacter target)
        {
            if (!caster.PerformAccuracyCheck()) return SkillResult.Fail($"[+] {caster.Name} tried to use {Name} but missed");

            float total = 0;
            foreach (BattleCharacter enemy in BattleManager.Instance.GetLivingOpponentsOf(caster))
            {
                if (enemy.PerformDodgeCheck()) continue;
                // +1% skillpowermod for every 0.5% missing health on target, max 3x damage
                float missingHealthPercentage = (enemy.MaxHP - enemy.CurrentHP) / enemy.MaxHP;
                float powerModifier = Mathf.Min(missingHealthPercentage / 0.5f, 3f);
                if (caster is Ally ally)
                {
                    ally.ModifyMultSkillPower(1 + powerModifier);
                }
                total += BattleManager.CalculateAndApplyDamage(caster, enemy, skill: this);
                if (caster is Ally a)
                {
                    a.ModifyMultSkillPower(1 - powerModifier);
                }
            }
            return SkillResult.Success($"[+] {caster.Name} used Blackout and dealt a total of {total} damage to all enemies", total);
        }
    }
}
