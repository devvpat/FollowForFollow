using UnityEngine;

// Represents an ally character
public class Ally : BattleCharacter
{
    // ----- STATS -----
    
    public AllyRole Role { get; private set; }
    public ISkill[] Skills { get; private set; }

    public int MaxMana { get; private set; }
    public int CurrentMana { get; private set; }

    public Ally(string name, int maxHP, int attack, int defense, int speed, int accuracy, int maxMana, AllyRole role)
        : base(name, maxHP, attack, defense, speed, accuracy)
    {
        MaxMana = maxMana;
        CurrentMana = maxMana;
        Role = role;
        Skills = RoleFactory.GetRole(role).GetSkills();
    }

    // ----- ACTIONS -----

    public int GetAttackDamage() => Attack;

    // Full reset to max stats
    public void ResetFully()
    {
        SetHP(MaxHP);
        CurrentMana = MaxMana;
    }

    // Check if the ally can afford to use a skill based on current mana
    public bool CanAffordSkill(ISkill skill) => CurrentMana >= skill.ManaCost && IsAlive;

    // Spend mana for using a skill
    public void SpendMana(int amount)
    {
        CurrentMana = Mathf.Max(CurrentMana - amount, 0);
    }

    // Restore mana by a certain amount, without exceeding max
    public void RestoreMana(int amount)
    {
        CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
    }
}
