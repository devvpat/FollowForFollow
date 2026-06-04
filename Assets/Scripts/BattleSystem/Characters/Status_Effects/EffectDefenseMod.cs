using System;
using UnityEngine;

// Defense Modifier: Changes the defender's damage output by a percentage
public class DefenseModifier : BaseStatusEffect
{
    private float defMod;

    public DefenseModifier(int duration, float modifier) : base(
        name: "Defense Modifier",
        description: $"Increases defense power by {modifier * 100}% for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Buff)
    {
        defMod = modifier;
    }

    public override void OnApply(BattleCharacter target)
    {
        target.ModifyMultDefense(1 + defMod);
        Debug.Log($"{target.Name} Defense buff applied: Defense is now {target.Defense}");
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
        target.ModifyMultDefense(1 / (1 + defMod)); // reverse the defense modifier
        Debug.Log($"{target.Name} Defense buff expired: Defense is now {target.Defense}");
    }
}