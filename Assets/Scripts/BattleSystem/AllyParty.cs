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
            //         name    maxHP  atk  def  spd  acc  mana  role
            new Ally("Aria",   120,   20,  20,  65,  90,  100,  AllyRole.Mage),
            new Ally("Brom",   160,   18,  50,  40,  80,  60,   AllyRole.Warrior),
            new Ally("Celia",  100,   25,  10,  75,  85,  120,  AllyRole.Rogue),
            new Ally("Doric",  140,   15,  65,  30,  75,  80,   AllyRole.Cleric),
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