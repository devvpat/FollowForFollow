using System;

// Poison: Deals damage to the target at the start of their turn. It stacks infinite times but decreases by X each turn.
public class Poison : BaseStatusEffect
{
    private const float baseDamage = 500f;
    private const float damageScale = 1.15f; // Each scale increases by 15%
    
    private int stacks;

    public Poison(int stacks) : base(
        name: "Poison",
        description: $"Deals damage at the start of the target's turn based on number stacks",
        totalDuration: 1, // stacks represent the true duration, this is so the effect is not removed prematurely
        effectType: StatusEffectType.Debuff)
    {
        this.stacks = stacks;
    }

    public override void OnReapply(BattleCharacter target, BaseStatusEffect newEffect)
    {
        if (newEffect is Poison newPoison)
        {
            stacks += newPoison.stacks; // add stacks
        }
    }

    public override void OnTurnStart(BattleCharacter target)
    {
        float realBase = baseDamage * (float)Math.Pow(damageScale, AllyParty.Instance.LevelScale); // scale base damage by level scale
        float rawDamage = realBase * stacks;
        float damage = BattleManager.CalculatePostMitigationDamage(rawDamage, target, false);
        target.TakeDamage(damage);
        ReduceStacks();
    }

    private void ReduceStacks()
    {
        // reduce stacks by 2 or 20% (whichever is higher)
        int stacksToReduce = Math.Max(2, (int)(stacks * 0.2f));
        stacks = Math.Max(0, stacks - stacksToReduce);
        if (stacks <= 0)
        {
            RemainingDuration = 0; // expire the effect if no stacks remain
        }
    }
}