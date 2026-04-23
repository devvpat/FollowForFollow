using UnityEngine;

// Base character class for allies and enemies
public class BattleCharacter
{
    // ----- STATS -----

    public string Name { get; protected set; }

    public int MaxHP { get; protected set; }
    public int CurrentHP { get; protected set; }

    public int Attack { get; protected set; }

    public bool IsDefending { get; private set; }
    public bool IsAlive => CurrentHP > 0;

    // NewDmg = OldDmg * (1 - DefendDamageReduction%)
    public const float DefendDamageReduction = 0.5f;

    public BattleCharacter(string name, int maxHP, int attack)
    {
        Name = name;
        MaxHP = maxHP;
        CurrentHP = maxHP;
        Attack = attack;
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
            ? Mathf.RoundToInt(rawDamage * (1f - DefendDamageReduction))
            : rawDamage;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        return damage;
    }

    // Set HP directly (clamps between 0 and MaxHP)
    public void SetHP(int hp)
    {
        CurrentHP = Mathf.Clamp(hp, 0, MaxHP);
    }
}
