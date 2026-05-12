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

    public bool IsDefending { get; private set; }
    public bool IsAlive => CurrentHP > 0;

    public BattleCharacter(string name, float maxHP, float attack, float defense, float speed, float accuracy, float critChance, float critDamage)
    {
        Name = name;
        MaxHP = maxHP;
        CurrentHP = maxHP;
        Attack = attack;
        AttackModifier = 1f;
        Defense = Mathf.Clamp(defense, 0, 100);
        DefenseModifier = 1f;
        Speed = Mathf.Clamp(speed, 0, 100);
        Accuracy = Mathf.Clamp(accuracy, 0, 100);
        CritChance = Mathf.Clamp(critChance, 0, 100);
        CritDamage = Mathf.Clamp(critDamage, 0, 100);
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

    // ----- MODIFIERS -----

    public void StartDefend()
    {
        IsDefending = true;
        DefenseModifier += 0.5f; // Example: defending increases defense effectiveness by 50%
    }

    public void EndDefend()
    {
        IsDefending = false;
        DefenseModifier -= 0.5f; // Example: stopping defense decreases defense effectiveness by 50%
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

    // Modifies attack (additive) by the specified amount (e.g. amount = 0.2f -> 20% increase)
    public void ModifyAddAttack(float amount)
    {
        AttackModifier += amount;
    }

    // Modifies attack (multiplicative) by the specified amount (e.g. amount = 1.2f -> 20% increase)
    public void ModifyMultAttack(float amount)
    {
        AttackModifier *= amount;
    }

    // Modifies defense (additive) by the specified amount (e.g. amount = 0.2f -> 20% increase)
    public void ModifyAddDefense(float amount)
    {
        DefenseModifier += amount;
    }

    // Modifies defense (multiplicative) by the specified amount (e.g. amount = 1.2f -> 20% increase)
    public void ModifyMultDefense(float amount)
    {
        DefenseModifier *= amount;
    }

    // Set HP directly (clamps between 0 and MaxHP)
    public void SetHP(float hp)
    {
        CurrentHP = Mathf.Clamp(hp, 0, MaxHP);
    }
}
