using System;

// Blurry: Grants the defender a X percent chance to dodge incoming attacks
public class Blur : BaseStatusEffect
{
    private const float blurModifier = 0.25f; // 25% evasion chance increase

    public Blur(int duration) : base(
        name: "Blur",
        description: $"Increases evasion by {blurModifier * 100}% for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Buff)
    {
        Icon = StatusEffectIcon.Blurry;
    }

    public override void OnApply(BattleCharacter target)
    {
        target.ModifyAddBlur(blurModifier);
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
        target.ModifyAddBlur(-blurModifier); // reverse the blur increase
    }
}