using UnityEngine;

public enum AllyRole { GlassCannon, Bruiser, Tank, BurstDPS, BattleMage, Caster, AllRounder }
public enum AllyCharacter { Bookwyrm, Karaage, JohnDreamblade, ApolloPhoebe }

// Represents an ally character
public class Ally : BattleCharacter
{
    // ----- STATS -----
    
    public AllyRole Role { get; private set; }
    public CharacterSkillSet CharSkillSet { get; private set;}
    public ISkill[] Skills { get; private set; }

    public float MaxMana { get; private set; }
    public float CurrentMana { get; private set; }

    public float SkillPowerMod { get; private set; } = 1f; // Multiplier for skill power in dmg formula

    private const float levelScaleMod = 1.15f; // Each level scale increases stats by 15%

    private readonly AllyCharacter characterName;

    public Ally(string name, float maxHP, float attack, float defense, float speed, float accuracy, float critChance, float critDamage, float maxMana, AllyRole role, int level, CharacterSkillSet skillSet, AllyCharacter characterName)
        : base(name, maxHP, attack, defense, speed, accuracy, critChance, critDamage, level)
    {
        MaxMana = maxMana;
        this.characterName = characterName;
        Role = role;
        CharSkillSet = skillSet;
        Skills = CharSkillSetFactory.GetSkills(skillSet);
        ApplyRoleBuff(role);

        // refresh health and mana in case stats have changed from roles
        CurrentMana = MaxMana;
        CurrentHP = MaxHP;
    }

    public static Ally CreateFromData(AllyData data)
    {
        return new Ally(data.Name, data.MaxHP, data.Attack, data.Defense, data.Speed, data.Accuracy, data.CritChance, data.CritDamage, data.MaxMana, data.Role, data.Level, data.charSkillSet, data.CharacterName);
    }

    private void ApplyRoleBuff(AllyRole role)
    {
        switch (role)
        {
            // -30% max health, +30% skillpowermod
            case AllyRole.GlassCannon:
                ModifyMultMaxHP(0.70f);
                ModifyMultSkillPower(1.30f);
                break;
            // +15% max health, +15% skillpowermod
            case AllyRole.Bruiser:
                ModifyMultMaxHP(1.15f);
                ModifyMultSkillPower(1.15f);
                break;
            // +30% max health, -30% skillpowermod
            case AllyRole.Tank:
                ModifyMultMaxHP(1.30f);
                ModifyMultSkillPower(0.70f);
                break;
            // -30% max mana, +30% skillpowermod
            case AllyRole.BurstDPS:
                ModifyMultMaxMana(0.70f);
                ModifyMultSkillPower(1.30f);
                break;
            // +15% max mana, +15% skillpowermod
            case AllyRole.BattleMage:
                ModifyMultMaxMana(1.15f);
                ModifyMultSkillPower(1.15f);
                break;
            // +30% max mana, -30% skillpowermod
            case AllyRole.Caster:
                ModifyMultMaxMana(1.30f);
                ModifyMultSkillPower(0.70f);
                break;
            // +10% max health, +10% max mana, +10% skillpowermod
            case AllyRole.AllRounder:
                ModifyMultMaxHP(1.10f);
                ModifyMultMaxMana(1.10f);
                ModifyMultSkillPower(1.10f);
                break;
        }
    }

    // undo the role buff above
    private void RemoveRoleBuff(AllyRole role)
    {
        switch (role)
        {
            // +30% max health, -30% skillpowermod
            case AllyRole.GlassCannon:
                ModifyMultMaxHP(1.30f);
                ModifyMultSkillPower(0.70f);
                break;
            // -15% max health, -15% skillpowermod
            case AllyRole.Bruiser:
                ModifyMultMaxHP(0.85f);
                ModifyMultSkillPower(0.85f);
                break;
            // -30% max health, +30% skillpowermod
            case AllyRole.Tank:
                ModifyMultMaxHP(0.70f);
                ModifyMultSkillPower(1.30f);
                break;
            // +30% max mana, -30% skillpowermod
            case AllyRole.BurstDPS:
                ModifyMultMaxMana(1.30f);
                ModifyMultSkillPower(0.70f);
                break;
            // -15% max mana, -15% skillpowermod
            case AllyRole.BattleMage:
                ModifyMultMaxMana(0.85f);
                ModifyMultSkillPower(0.85f);
                break;
            // -30% max mana, +30% skillpowermod
            case AllyRole.Caster:
                ModifyMultMaxMana(0.70f);
                ModifyMultSkillPower(1.30f);
                break;
            // -10% max health, -10% max mana, -10% skillpowermod
            case AllyRole.AllRounder:
                ModifyMultMaxHP(0.9f);
                ModifyMultMaxMana(0.9f);
                ModifyMultSkillPower(0.9f);
                break;
        }
    }

    public override void OnNormalAttack()
    {
        base.OnNormalAttack();
        // Bookwyrm restore 15% max mana on normal attack
        if (characterName == AllyCharacter.Bookwyrm)
        {
            RestoreMana(MaxMana * 0.15f);
        }
    }


    // ----- ACTIONS -----

    // Full reset to max stats
    public void ResetFully()
    {
        CurrentHP = MaxHP;
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
        RemoveRoleBuff(Role);
        ApplyRoleBuff(newRole);
        Role = newRole;
    }

    public void ModifyMultSkillPower(float amount)
    {
        SkillPowerMod *= amount;
    }

    public void ModifyMultMaxMana(float amount)
    {
        MaxMana *= amount;
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
