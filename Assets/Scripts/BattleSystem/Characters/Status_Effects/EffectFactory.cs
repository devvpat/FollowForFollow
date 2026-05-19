public static class EffectFactory
{
    public static Blind MakeBlind(int duration)
    {
        return new Blind(duration);
    }

    public static Blur MakeBlur(int duration)
    {
        return new Blur(duration);
    }

    public static Haste MakeHaste(int duration)
    {
        return new Haste(duration);
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