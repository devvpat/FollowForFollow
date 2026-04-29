using UnityEngine;

// Base character class for allies and enemies
public class BattleCharacter
{
    // ----- STATS -----

    public string Name { get; protected set; }

    public int MaxHP { get; protected set; }
    public int CurrentHP { get; protected set; }

    public int Attack { get; protected set; }
    public int Defense { get; protected set; } // 0-100, % damage reduction
    public int Speed { get; protected set; } // Higher speed means earlier turn order
    public int Accuracy { get; protected set; } // 0-100, % chance to hit

    public float TickTimer { get; private set; } // Accumulates over time based on Speed, upon reaching the threshold the character can act

    public bool IsDefending { get; private set; }
    public bool IsAlive => CurrentHP > 0;

    public BattleCharacter(string name, int maxHP, int attack, int defense, int speed, int accuracy)
    {
        Name = name;
        MaxHP = maxHP;
        CurrentHP = maxHP;
        Attack = attack;
        Defense = Mathf.Clamp(defense, 0, 100);
        Speed = Mathf.Clamp(speed, 0, 100);
        Accuracy = Mathf.Clamp(accuracy, 0, 100);
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

    // ----- ACTIONS -----

    public void StartDefend()
    {
        IsDefending = true;
    }

    public void EndDefend()
    {
        IsDefending = false;
    }

    // Applies and returns the actual damage taken by the character after considering defense (HP clamped to 0)
    public int TakeDamage(int rawDamage)
    {
        int damage = IsDefending
            ? Mathf.RoundToInt(rawDamage * (1f - Defense / 1000f)) // if defending, reduces damage by Defense stat percent
            : rawDamage; // otherwise, take full damage
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        return damage;
    }

    // Set HP directly (clamps between 0 and MaxHP)
    public void SetHP(int hp)
    {
        CurrentHP = Mathf.Clamp(hp, 0, MaxHP);
    }
}
