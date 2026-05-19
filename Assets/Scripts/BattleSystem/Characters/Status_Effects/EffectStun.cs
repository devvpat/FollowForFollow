public class Stun : BaseStatusEffect
{
    public Stun(int duration) : base(
        name: "Stun",
        description: $"Prevents all actions for {duration} turns.",
        totalDuration: duration,
        effectType: StatusEffectType.Debuff,
        reapplyType: StatusEffectReapplyType.ApplyAgain)
    {
    }

    public override void OnTurnStart(BattleCharacter target)
    {
        RemainingDuration--;
    }
}