// Factory for getting enemy behavior definitions

public enum EnemyBehaviorType { Random, Minion_Debuffer, Minion_Buffer, Minion_Supporter, Boss_Mimic }

public static class EnemyBehaviorFactory
{
    public static IEnemyBehavior GetBehavior(EnemyBehaviorType type)
    {
        return type switch
        {
            EnemyBehaviorType.Random => new RandomEnemyBehavior(),
            EnemyBehaviorType.Minion_Debuffer => new MinionDebufferBehavior(),
            EnemyBehaviorType.Minion_Buffer => new MinionBufferBehavior(),
            EnemyBehaviorType.Minion_Supporter => new MinionSupporterBehavior(),
            EnemyBehaviorType.Boss_Mimic => new BossMimicBehavior(),
            _                        => new RandomEnemyBehavior()
        };
    }
}