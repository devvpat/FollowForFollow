using UnityEngine;

public enum MoralAlignment { LawfulGood, TrueNeutral, ChaoticGood, ChaoticNeutral }
public enum EnergyLevel { Low, Medium, High, Varies }

[CreateAssetMenu(fileName = "New Buddy Profile", menuName = "F4F/Buddy Profile")]
public class CharacterProfile : ScriptableObject
{
    [Header("Identity")]
    public string realName;
    public string ign; 
    public Sprite portraitSprite;
    
    [Header("Personality & Traits")]
    public MoralAlignment alignment;
    public EnergyLevel energyLevel;
    [TextArea(2, 4)] public string likes;
    [TextArea(2, 4)] public string dislikes;

    [Header("Retro Vibe")]
    [TextArea(2, 3)] public string statusMessage;

    [Header("Combat Setup")]
    public string preferredRole; 
}