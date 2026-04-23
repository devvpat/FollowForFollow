using System.Collections.Generic;
using UnityEngine;

// Singleton that holds all allies in the player's party, including their stats
public class AllyParty : MonoBehaviour
{
    // ----- SETUP -----
    public static AllyParty Instance { get; private set; }

    // Should be initialized at the start of the game. Persists across scene, keeping track of current ally stats.
    public List<Ally> Allies { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAllies();
    }

    private void InitializeAllies()
    {
        Allies = new List<Ally>
        {
            //       name     maxHP   attack  maxMana
            new Ally("Lau",   120,    20,     100),
            new Ally("Ren",   160,    18,      60),
            new Ally("Lar",   100,    25,     120),
            new Ally("Ral",   140,    15,      80),
        };
    }

    // ----- PUBLIC API -----

    // Returns a list of all currently living allies
    public List<Ally> GetLivingAllies()
    {
        var living = new List<Ally>();
        foreach (var a in Allies)
            if (a.IsAlive) living.Add(a);
        return living;
    }

    // Returns true if at least one ally is alive
    public bool IsAnyAllyAlive() => GetLivingAllies().Count > 0;

    // Resets stats of all allies to their initial values
    public void ResetAllAlliesStats()
    {
        foreach (var a in Allies)
            a.ResetFully();
    }
}