using System;
using UnityEngine;

// Attack Modifier: Changes the attacker's damage output by a percentage
public class AttackModifier : BaseStatusEffect
{
    private float atkMod;

    public AttackModifier(int duration, float modifier) : base(
        name: "Attack Modifier",
        description: $"Increases attack power by {modifier * 100}% for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Buff)
    {
        atkMod = modifier;
    }

    public override void OnApply(BattleCharacter target)
    {
        target.ModifyMultAttack(1 + atkMod);
        Debug.Log($"{target.Name} Attack buff applied: Attack is now {target.AttackModifier}");
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
        target.ModifyMultAttack(1 / (1 + atkMod)); // reverse the attack modifier
        Debug.Log($"{target.Name} Attack buff expired: Attack is now {target.AttackModifier}");
    }
}