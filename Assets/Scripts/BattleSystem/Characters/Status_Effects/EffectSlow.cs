using System;

public class Slow : BaseStatusEffect
{
    private float speedModifier;

    public Slow(int duration, float speedModifier) : base(
        name: "Slow",
        description: $"Reduces speed by {speedModifier * 100}% for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Debuff)
    {
        this.speedModifier = speedModifier;
    }

    public override void OnApply(BattleCharacter target)
    {
        target.ModifyMultSpeed(1 - speedModifier);
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
        target.ModifyMultSpeed(1 / (1 - speedModifier)); // reverse the speed decrease
    }
}