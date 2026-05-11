using UnityEngine;

// Represents an ally character
public class Ally : BattleCharacter
{
    // ----- STATS -----
    
    public AllyRole Role { get; private set; }
    public IRole RoleDefintion { get; private set;}
    public ISkill[] Skills { get; private set; }

    public float MaxMana { get; private set; }
    public float CurrentMana { get; private set; }

    public Ally(string name, float maxHP, float attack, float defense, float speed, float accuracy, float critChance, float critDamage, float maxMana, AllyRole role)
        : base(name, maxHP, attack, defense, speed, accuracy, critChance, critDamage)
    {
        MaxMana = maxMana;
        CurrentMana = maxMana;
        Role = role;
        RoleDefintion = RoleFactory.GetRole(role);
        Skills = RoleDefintion.GetSkills();
    }

    public static Ally CreateFromData(AllyData data)
    {
        return new Ally(data.Name, data.MaxHP, data.Attack, data.Defense, data.Speed, data.Accuracy, data.CritChance, data.CritDamage, data.MaxMana, data.Role);
    }

    // ----- ACTIONS -----

    // Full reset to max stats
    public void ResetFully()
    {
        SetHP(MaxHP);
        CurrentMana = MaxMana;
    }

    // Check if the ally can afford to use a skill based on current mana
    public bool CanAffordSkill(ISkill skill) => CurrentMana >= skill.ManaCost && IsAlive;

    // Spend mana for using a skill
    public void SpendMana(float amount)
    {
        CurrentMana = Mathf.Max(CurrentMana - amount, 0);
    }

    // Restore mana by a certain amount, without exceeding max
    public void RestoreMana(float amount)
    {
        CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
    }
}
