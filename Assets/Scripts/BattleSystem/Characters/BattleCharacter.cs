using System.Collections.Generic;
using UnityEngine;

// Base character class for allies and enemies
public class BattleCharacter
{
    // ----- STATS -----

    public string Name { get; protected set; }
    public float MaxHP { get; protected set; } = 100f;
    public float CurrentHP { get; protected set; }
    public float Attack { get; protected set; } = 15f;
    public float AttackModifier { get; protected set; } = 1f; // Multiplier for attack damage
    public float Defense { get; protected set; } = 0.25f; // % damage reduction
    public float DefenseModifier { get; protected set; } = 1f; // Multiplier for defense effectiveness
    public float Speed { get; protected set; } = 5000f; // Higher speed means earlier turn order
    public float Accuracy { get; protected set; } = 0.75f; // % chance to hit
    public float CritChance { get; protected set; } = 0.1f; // % chance to deal critical hit
    public float CritDamage { get; protected set; } = 1.5f; // % damage multiplier for critical hits

    public float TickTimer { get; private set; } // Accumulates over time based on Speed, upon reaching the threshold the character can act
    public List<BaseStatusEffect> StatusEffects { get; private set; } // List of current status effects on the character

    public bool IsDefending { get; private set; }
    public bool IsAlive => CurrentHP > 0;
    
    public bool IsStunned => StatusEffects.Exists(e => e.Name == "Stun");
    public bool IsSilenced => StatusEffects.Exists(e => e.Name == "Silence");
    public bool IsShielded => StatusEffects.Exists(e => e.Name == "Shield");

    public BattleCharacter(string name, float maxHP, float attack, float defense, float speed, float accuracy, float critChance, float critDamage)
    {
        Name = name;
        MaxHP = maxHP;
        CurrentHP = maxHP;
        Attack = attack;
        AttackModifier = 1f;
        Defense = defense;
        DefenseModifier = 1f;
        Speed = speed;
        Accuracy = accuracy; // 0.75 = 75% chance to hit
        CritChance = critChance; // 0.1 = 10%
        CritDamage = critDamage; // 1.5 = 150% damage

        TickTimer = 0;
        StatusEffects = new List<BaseStatusEffect>();
    }

    // ----- TICK & TURN ORDER -----

    // Adds current Speed to the TickTimer. Returns true if the TickTimer has reached the TickThreshold
    // (which means the character can take an action), otherwise false
    public bool Tick()
    {
        if (!IsAlive) return false;
        TickTimer += Speed;
        return TickTimer >= BattleManager.BattleTickThreshold;
    }

    // Minuses the TickThreshold from the TickTimer. Should be called after character takes an action
    // and keeps excess tick time
    public void ConsumeTickTurn()
    {
        TickTimer -= BattleManager.BattleTickThreshold;
    }

    // Sets TickTimer to 0. Should be called at the start of a battle
    public void ResetTickTimer()
    {
        TickTimer = 0;
    }

    public void SetTickTimer(int val)
    {
        TickTimer = val;
    }

    // ----- STATUS EFFECTS -----

    // Adds a status effect to the character. If the character already has the same status effect, handles it based on the effect's ReapplyType
    public void AddStatusEffect(BaseStatusEffect effect)
    {
        var existingEffect = StatusEffects.Find(e => e.Name == effect.Name);
        // status effect already exists
        if (existingEffect != null)
        {
            switch (effect.ReapplyType)
            {
                case StatusEffectReapplyType.Reset:
                    existingEffect.OnReset(this); // reset the existing effect
                    break;
                case StatusEffectReapplyType.Stack:
                    existingEffect.OnStack(this); // stack new effect on existing effect
                    break;
                case StatusEffectReapplyType.IgnoreNew:
                    // do nothing
                    break;
                case StatusEffectReapplyType.ApplyAgain:
                    effect.OnApply(this);
                    StatusEffects.Add(effect); // apply new effect again (allows duplicates)
                    break;
            }
        }
        // status effect does not exist
        else
        {
            effect.OnApply(this);
            StatusEffects.Add(effect);
        }
    }

    // Gets all expired status effects, calls their OnExpire method, and removes them from the character's status effect list
    public void FindAndRemoveExpiredStatusEffects()
    {
        var expiredEffects = StatusEffects.FindAll(e => e.HasExpired);
        foreach (var effect in expiredEffects)
        {
            effect.OnExpire(this);
            StatusEffects.Remove(effect);
        }
    }

    // Calls OnTurnStart for all status effects
    public void ProcessStatusEffectsOnTurnStart()
    {
        foreach (var effect in StatusEffects)
        {
            effect.OnTurnStart(this);
        }
    }

    // Calls OnTurnEnd for all status effects
    public void ProcessStatusEffectsOnTurnEnd()
    {
        foreach (var effect in StatusEffects)
        {
            effect.OnTurnEnd(this);
        }
    }

    public void RemoveAllStatusEffects()
    {
        foreach (var effect in StatusEffects)
        {
            effect.OnExpire(this);
        }
        StatusEffects.Clear();
    }
    
    // ----- MODIFIERS -----

    public void StartDefend()
    {
        IsDefending = true;
        ModifyMultDefense(1.5f); // defending increases defense effectiveness by 50%
    }

    public void EndDefend()
    {
        IsDefending = false;
        ModifyMultDefense(0.5f); // Example: stopping defense decreases defense effectiveness by 50%
    }

    // Reduces HP by specified damage amount (HP clamped to 0)
    public void TakeDamage(float damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
    }

    // Restores HP by the specified amount (HP clamped to MaxHP)
    public void RestoreHP(float amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
    }

    // Modifies attack (multiplicative) by the specified amount (e.g. amount = 1.2f -> 20% increase)
    public void ModifyMultAttack(float amount)
    {
        AttackModifier *= amount;
    }

    // Modifies defense (multiplicative) by the specified amount (e.g. amount = 1.2f -> 20% increase)
    public void ModifyMultDefense(float amount)
    {
        DefenseModifier *= amount;
    }

    // Modifies crit rate by the specified amount (e.g. amount = 0.1f -> 10% increase)
    public void ModifyAddCritRate(float amount)
    {
        CritChance += amount;
        CritChance = Mathf.Max(0, CritChance); // clamp to 0% minimum
    }

    // Modifies speed (multiplicative) by the specified amount (e.g. amounnt = 1.2f -> 20% increase)
    public void ModifyMultSpeed(float amount)
    {
        Speed *= amount;
    }

    // Set HP directly (clamps between 0 and MaxHP)
    public void SetHP(float hp)
    {
        CurrentHP = Mathf.Clamp(hp, 0, MaxHP);
    }
}
