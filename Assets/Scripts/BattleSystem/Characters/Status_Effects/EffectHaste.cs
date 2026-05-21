using System;

// Haste: Changes the characters speed stat by x% for x turns.
public class Haste : BaseStatusEffect
{
    private const float speedModifier = 0.3f; // 30% speed increase

    public Haste(int duration) : base(
        name: "Haste",
        description: $"Increases speed by {speedModifier * 100}% for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Buff)
    {}

    public override void OnApply(BattleCharacter target)
    {
        target.ModifyMultSpeed(1 + speedModifier);
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
        target.ModifyMultSpeed(1 / (1 + speedModifier)); // reverse the speed increase
    }
}