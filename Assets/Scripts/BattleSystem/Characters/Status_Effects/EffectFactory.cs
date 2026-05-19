public static class EffectFactory
{
    public static AttackModifier MakeAttackModifier(int duration, float multiplier)
    {
        return new AttackModifier(duration, multiplier);
    }

    public static Blind MakeBlind(int duration)
    {
        return new Blind(duration);
    }

    public static Blur MakeBlur(int duration)
    {
        return new Blur(duration);
    }

    public static CritRateModifier MakeCritRateModifier(int duration, float amount)
    {
        return new CritRateModifier(duration, amount);
    }

    public static DefenseModifier MakeDefenseModifier(int duration, float multiplier)
    {
        return new DefenseModifier(duration, multiplier);
    }

    public static Haste MakeHaste(int duration)
    {
        return new Haste(duration);
    }

    public static Poison MakePoison(int stacks)
    {
        return new Poison(stacks);
    }

    public static PoisonWeapon MakePoisonWeapon(int duration, int stacksToApply)
    {
        return new PoisonWeapon(duration, stacksToApply);
    }

    public static Shield MakeShield(int durability)
    {
        return new Shield(durability);
    }

    public static Silence MakeSilence(int duration)
    {
        return new Silence(duration);
    }

    public static Slow MakeSlow(int duration)
    {
        return new Slow(duration);
    }

    public static Stun MakeStun(int duration)
    {
        return new Stun(duration);
    }
}