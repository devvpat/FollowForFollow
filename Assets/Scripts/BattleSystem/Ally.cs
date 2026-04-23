using UnityEngine;

// Represents an ally character
public class Ally : BattleCharacter
{
    // ----- STATS -----

    public int MaxMana { get; private set; }
    public int CurrentMana { get; private set; }

    // Special attack numbers
    public const int SpecialManaCost = 30;
    public const float SpecialDamageMultiplier = 2f;

    public bool CanUseSpecial => CurrentMana >= SpecialManaCost && IsAlive;

    public Ally(string name, int maxHP, int attack, int maxMana)
        : base(name, maxHP, attack)
    {
        MaxMana     = maxMana;
        CurrentMana = maxMana;
    }

    // ----- ACTIONS -----

    public int GetAttackDamage() => Attack;

    // Use special attack if enough mana, otherwise do nothing, return damage dealt
    public int UseSpecialAttack()
    {
        if (!CanUseSpecial)
        {
            Debug.LogWarning($"{Name} tried to use special but cannot.");
            return 0;
        }
        CurrentMana -= SpecialManaCost;
        return Mathf.RoundToInt(Attack * SpecialDamageMultiplier);
    }

    // ── Persistence ──────────────────────────────────────────────────────────

    // Saves current HP and mana into a snapshot for inter-fight persistence.
    public AllySnapshot TakeSnapshot() => new AllySnapshot { hp = CurrentHP, mana = CurrentMana };

    // Restores HP and mana from a saved snapshot.
    public void RestoreSnapshot(AllySnapshot snap)
    {
        SetHP(snap.hp);
        CurrentMana = Mathf.Clamp(snap.mana, 0, MaxMana);
    }

    // Full reset to max stats — use only for a true new game, not between fights.
    public void ResetFully()
    {
        SetHP(MaxHP);
        CurrentMana = MaxMana;
    }
}

// Plain data bag used to persist ally stats between scenes/fights.
[System.Serializable]
public class AllySnapshot
{
    public int hp;
    public int mana;
}
