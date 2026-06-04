// Base class for a status effect
// Each status effect is responsible for implementing behavior + handling its own duration and expiration (only the methods are called appropriately by the BattleCharacter class, the rest is handled by class implementations)
public abstract class BaseStatusEffect
{
    public string Name { get; protected set; }
    public string Description { get; protected set; } // Description of the status effect, can be used for UI tooltips
    public int TotalDuration { get; protected set; } // Total duration in turns
    public int RemainingDuration { get; protected set; } // Remaining duration in turns
    public bool HasExpired => RemainingDuration <= 0;
    public StatusEffectType EffectType { get; protected set; } // Type of the status effect (buff, debuff, etc.)
    public StatusEffectIcon Icon { get; protected set; } // Which icon the UI displays for this effect

    public BaseStatusEffect(string name, string description, int totalDuration, StatusEffectType effectType)
    {
        Name = name;
        Description = description;
        TotalDuration = totalDuration;
        RemainingDuration = totalDuration;
        EffectType = effectType;
    }

    public virtual void OnApply(BattleCharacter target) {} // Called when the status effect is applied for the first time
    public virtual void OnReapply(BattleCharacter target, BaseStatusEffect newEffect) {} // Called when the status effect is reapplied (if ReapplyType is ApplyAgain, this will be called for each application)
    public virtual void OnTurnStart(BattleCharacter target) {} // Called at the start of the target's turn
    public virtual void OnTurnEnd(BattleCharacter target) {} // Called at the end of the target's turn
    public virtual void OnExpire(BattleCharacter target) {} // Called when the status effect expires
}

public enum StatusEffectType
{
    Buff,
    Debuff,
    Other,
}

// Identifies which icon the UI shows for a status effect. Each effect sets its own value;
// the StatusEffectIconLibrary maps these to sprites. None = fall back to the letter label.
public enum StatusEffectIcon
{
    None,
    AttackUp,
    AttackDown,
    DefenseUp,
    DefenseDown,
    CritUp,
    CritDown,
    Blind,
    Blurry,
    Haste,
    Slow,
    Poison,
    PoisonWeapon,
    Shield,
    Silence,
    Stun,
    HealBuff,       // no effect class yet — reserved slot
    HealReduction,  // no effect class yet — reserved slot
    Lifesteal,      // no effect class yet — reserved slot
}