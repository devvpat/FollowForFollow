using System;
using System.Collections.Generic;
using UnityEngine;

// Singleton that holds all allies in the player's party, including their stats
public class EnemyParty : MonoBehaviour
{
    // ----- SETUP -----
    [Header("Enemy Start Data")]
    [SerializeField] private List<EnemyData> EnemyDataList;

    // Should be initialized at the start of the game. Persists across scene, keeping track of current enemy stats.
    public List<Enemy> Enemies { get; private set; }

    private void Awake()
    {
        InitializeEnemies();
    }

    private void InitializeEnemies()
    {
        Enemies = new List<Enemy>();
        foreach (var data in EnemyDataList)
        {
            Enemies.Add(Enemy.CreateFromData(data));
        }
    }

    public void ReinitializeEnemies()
    {
        InitializeEnemies();
    }

    // ----- PUBLIC API -----

    // Returns a list of all currently living enemies
    public List<Enemy> GetLivingEnemies()
    {
        var living = new List<Enemy>();
        foreach (var e in Enemies)
            if (e.IsAlive) living.Add(e);
        return living;
    }

    // Returns true if at least one enemy is alive
    public bool IsAnyEnemyAlive() => GetLivingEnemies().Count > 0;
}