using System;

// Stun: Skips the character turn in the speed bucket and prevents them from taking any action.
public class Stun : BaseStatusEffect
{
    public Stun(int duration) : base(
        name: "Stun",
        description: $"Prevents all actions for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Debuff)
    {
    }

    public override void OnReapply(BattleCharacter target, BaseStatusEffect newEffect)
    {
        RemainingDuration = Math.Max(RemainingDuration, newEffect.RemainingDuration);
    }

    public override void OnTurnStart(BattleCharacter target)
    {
        RemainingDuration--;
    }
}