using System;

// Poison: Deals damage to the target at the start of their turn. It stacks infinite times but decreases by X each turn.
public class PoisonWeapon : BaseStatusEffect
{
    public int StacksToApply { get; private set; }
    
    public PoisonWeapon(int duration, int stacksToApply) : base(
        name: "Poison Weapon",
        description: $"Deals damage at the start of the target's turn based on number stacks",
        totalDuration: duration, // stacks represent the true duration, this is so the effect is not removed prematurely
        effectType: StatusEffectType.Debuff)
    {
        StacksToApply = stacksToApply;
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