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
    public StatusEffectReapplyType ReapplyType { get; protected set; } // Determines how the status effect behaves when reapplied

    public BaseStatusEffect(string name, string description, int totalDuration, StatusEffectType effectType, StatusEffectReapplyType reapplyType)
    {
        Name = name;
        Description = description;
        TotalDuration = totalDuration;
        RemainingDuration = totalDuration;
        EffectType = effectType;
        ReapplyType = reapplyType;
    }

    public virtual void OnApply(BattleCharacter target) {} // Called when the status effect is applied for the first time
    public virtual void OnReset(BattleCharacter target) {} // Called when the status effect is reapplied (if ReapplyType is Reset)
    public virtual void OnStack(BattleCharacter target) {} // Called when the status effect is reapplied (if ReapplyType is Stack)
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

public enum StatusEffectReapplyType
{
    Reset,
    Stack,
    IgnoreNew,
    ApplyAgain,
}