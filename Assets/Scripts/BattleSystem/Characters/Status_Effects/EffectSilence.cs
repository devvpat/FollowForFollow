using System;

// Silence: Prevents the affected character from using mana based skills, forcing them to use basic attacks.
public class Silence : BaseStatusEffect
{
    public Silence(int duration) : base(
        name: "Silence",
        description: $"Prevents skill usage for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Debuff)
    {
        Icon = StatusEffectIcon.Silence;
    }

    public override void OnReapply(BattleCharacter target, BaseStatusEffect newEffect)
    {
        RemainingDuration = Math.Max(RemainingDuration, newEffect.RemainingDuration); // refresh duration to the max of the two
    }

    public override void OnTurnStart(BattleCharacter target)
    {
        RemainingDuration--;
    }
}