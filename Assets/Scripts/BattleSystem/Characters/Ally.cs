using UnityEngine;

public enum AllyRole { DPS, Support, Tank, Toxic }

// Represents an ally character
public class Ally : BattleCharacter
{
    // ----- STATS -----
    
    public AllyRole Role { get; private set; }
    public CharacterSkillSet charSkillSet { get; private set;}
    public ISkill[] Skills { get; private set; }

    public float MaxMana { get; private set; }
    public float CurrentMana { get; private set; }

    private float levelScaleMod = 1.15f; // Each level scale increases stats by 15%

    public Ally(string name, float maxHP, float attack, float defense, float speed, float accuracy, float critChance, float critDamage, float maxMana, AllyRole role, int level, CharacterSkillSet skillSet)
        : base(name, maxHP, attack, defense, speed, accuracy, critChance, critDamage, level)
    {
        MaxMana = maxMana;
        CurrentMana = maxMana;
        Role = role;
        charSkillSet = skillSet;
        Skills = CharSkillSetFactory.GetSkills(skillSet);
    }

    public static Ally CreateFromData(AllyData data)
    {
        return new Ally(data.Name, data.MaxHP, data.Attack, data.Defense, data.Speed, data.Accuracy, data.CritChance, data.CritDamage, data.MaxMana, data.Role, data.Level, data.charSkillSet);
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

    public void UpdateRole(AllyRole newRole)
    {
        Role = newRole;
    }

    public void IncreaseLevel()
    {
        Level += 1;
        MaxHP *= levelScaleMod;
        Attack *= levelScaleMod;
        Defense *= levelScaleMod;
        Speed *= levelScaleMod;
        // Accuracy *= levelScaleMod;
        // CritChance *= levelScaleMod;
        // CritDamage *= levelScaleMod;
        MaxMana *= levelScaleMod;
    }

}
