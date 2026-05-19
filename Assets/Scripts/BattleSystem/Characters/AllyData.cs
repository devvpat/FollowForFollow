using UnityEngine;

[CreateAssetMenu(fileName = "NewAllyData", menuName = "Character/Ally Data")]
public class AllyData : ScriptableObject
{
    public string Name = "Test";
    public float MaxHP = 100f;
    public float Attack = 15f;
    public float Defense = 0.25f; // % damage reduction
    public float Speed = 5000f; // Higher speed means earlier turn order
    public float Accuracy = 0.75f; // % chance to hit
    public float CritChance = 0.25f; // % chance to deal critical hit
    public float CritDamage = 1.5f; // % damage multiplier for critical hits
    public int Level = 95; // Character level, used for scaling

    public float MaxMana = 100f;
    public CharacterSkillSet charSkillSet = CharacterSkillSet.Bookwyrm;
    public AllyRole Role = AllyRole.AllRounder;
}
