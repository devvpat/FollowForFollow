using System;

// Crit Rate Modifier: Changes the critical hit rate of the target
public class CritRateModifier : BaseStatusEffect
{
    private float critRateMod;

    public CritRateModifier(int duration, float modifier) : base(
        name: "Crit Rate Modifier",
        description: $"Increases critical hit rate by {modifier * 100}% for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Buff)
    {
        critRateMod = modifier;
        Icon = modifier >= 0 ? StatusEffectIcon.CritUp : StatusEffectIcon.CritDown;
    }

    public override void OnApply(BattleCharacter target)
    {
        target.ModifyAddCritRate(critRateMod);
    }

    public override void OnReapply(BattleCharacter target, BaseStatusEffect newEffect)
    {
        RemainingDuration = Math.Max(RemainingDuration, newEffect.RemainingDuration); // refresh duration to the max of the two
    }

    public override void OnTurnStart(BattleCharacter target)
    {
        RemainingDuration--;
    }

    public override void OnExpire(BattleCharacter target)
    {
        target.ModifyAddCritRate(-critRateMod); // reverse the crit rate modifier
    }
}