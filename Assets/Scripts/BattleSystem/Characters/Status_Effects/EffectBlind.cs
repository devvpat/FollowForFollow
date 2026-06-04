using System;

// Blinded: Reduces the hit chance of the attacker by X percent.
public class Blind : BaseStatusEffect
{
    private const float blindModifier = 0.25f; // 25% accuracy reduction

    public Blind(int duration) : base(
        name: "Blind",
        description: $"Reduces accuracy by {blindModifier * 100}% for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Debuff)
    {
        Icon = StatusEffectIcon.Blind;
    }

    public override void OnApply(BattleCharacter target)
    {
        target.ModifyMultAccuracy(1 - blindModifier);
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
        target.ModifyMultAccuracy(1 / (1 - blindModifier)); // reverse the accuracy decrease
    }
}