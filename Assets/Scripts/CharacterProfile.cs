using UnityEngine;

public enum MoralAlignment { LawfulGood, TrueNeutral, ChaoticGood, ChaoticNeutral }
public enum EnergyLevel { Low, Medium, High, Varies }
public enum PartyRole { Support, Attacker, Defender, Observer }

[CreateAssetMenu(fileName = "New Buddy Profile", menuName = "F4F/Buddy Profile")]
public class CharacterProfile : ScriptableObject
{
    [Header("Identity")]
    public string realName;
    public string ign;
    public Sprite portraitSprite;
    public Color themeColor = new Color(0.726f, 0.257f, 0.257f, 1f);

    [Header("Personality & Traits")]
    public MoralAlignment alignment;
    public EnergyLevel energyLevel;
    [TextArea(2, 4)] public string likes;
    [TextArea(2, 4)] public string dislikes;

    [Header("Retro Vibe")]
    [TextArea(2, 3)] public string statusMessage;

    [Header("Combat Setup")]
    public string preferredRole;
    public PartyRole partyRole;
    public AllyData allyData;

    [Header("Audio")]
    [Tooltip("Short blip played as each character of their dialogue types out (Undertale style).")]
    public AudioClip textSound;
}
