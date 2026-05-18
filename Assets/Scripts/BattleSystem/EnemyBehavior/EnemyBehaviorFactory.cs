// Factory for getting enemy behavior definitions

public enum EnemyBehaviorType { Random }

public static class EnemyBehaviorFactory
{
    public static IEnemyBehavior GetBehavior(EnemyBehaviorType type)
    {
        return type switch
        {
            EnemyBehaviorType.Random => new RandomEnemyBehavior(),
            _                        => new RandomEnemyBehavior()
        };
    }
}