using System;

// Shielded: Each shield stack absorbs one instance of damage, no matter how high.
public class Shield : BaseStatusEffect
{
    private int durability; // Number of attacks the shield can block before expiring

    public Shield(int durability) : base(
        name: "Shield",
        description: $"Grants damage immunity for {durability} attacks.",
        totalDuration: 1, // durability is the main factor determining how long the shield lasts, so duration is set to 1 turn and will be refreshed on each application
        effectType: StatusEffectType.Buff)
    {
        this.durability = durability;
        Icon = StatusEffectIcon.Shield;
    }

    public override void OnReapply(BattleCharacter target, BaseStatusEffect newEffect)
    {
        // add durabilities
        if (newEffect is Shield newShield)
        {
            durability += newShield.durability;
        }
    }

    public void ReduceDurability()
    {
        durability--;
        if (durability <= 0)
        {
            RemainingDuration = 0; // Set duration to 0 to trigger expiration
        }
    }

}