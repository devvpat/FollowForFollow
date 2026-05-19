using System;

public class Silence : BaseStatusEffect
{
    public Silence(int duration) : base(
        name: "Silence",
        description: $"Prevents skill usage for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Debuff)
    {
    }

    public override void OnReapply(BattleCharacter target, BaseStatusEffect newEffect)
    {
        // If the same silence effect is reapplied, refresh the duration to the new effect's duration
        RemainingDuration = Math.Max(RemainingDuration, newEffect.RemainingDuration);
    }

    public override void OnTurnStart(BattleCharacter target)
    {
        RemainingDuration--;
    }
}