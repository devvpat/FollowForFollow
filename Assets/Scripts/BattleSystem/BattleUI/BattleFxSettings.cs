// Master on/off switches for the optional battle-UI "juice" effects. Each effect checks its flag, so
// you can keep the ones you like by flipping the rest to false here (no scene/prefab changes needed).
// All default ON so you can see them and disable what you don't want.
public static class BattleFxSettings
{
    public static bool DamageNumbers = true;    // floating damage number over a hit combatant
    public static bool SmoothBars = true;        // HP/MP sliders glide to the new value
    public static bool ActiveActorPulse = true;  // active ally's card pulses on its turn
    public static bool PanelEaseIn = true;       // action/skill panels scale-in when shown
    public static bool ResultPunch = true;       // Victory/Defeat title punches in
    public static bool StatusIconPop = true;     // status icons pop (scale 0->1) when applied
    public static bool AttackLunge = true;       // actor's field sprite steps toward foes on action
    public static bool AttackSpriteSwap = true;  // enemy with an attack sprite swaps to it while acting
    public static bool IdleBreathing = false;     // field sprites gently scale-breathe at rest
    public static bool DeathFade = true;         // field sprite fades out on KO
    public static bool LogFlash = true;          // battle log pulses on a new entry
    public static bool ButtonPop = true;         // action buttons pop when pressed
    public static bool StatusApplyFlash = true;  // target flashes buff/debuff color on status apply
    public static bool HpChip = true;            // bar fill flashes white on HP loss
    public static bool HitStop = true;           // brief time freeze on a hit (longer on crit)
    public static bool TargetingPulse = true;    // valid targets pulse during target selection
    public static bool CritNumbers = true;       // crit damage numbers bigger + yellow + shake
    public static bool MissText = true;          // floating "Miss!" / "Dodge!"
    public static bool HealNumbers = true;       // green "+N" + green flash on heal
}
