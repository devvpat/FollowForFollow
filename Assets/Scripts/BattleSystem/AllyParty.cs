using System;
using System.Collections.Generic;
using UnityEngine;

// Singleton that holds all allies in the player's party, including their stats
public class AllyParty : MonoBehaviour
{
    // ----- SETUP -----
    public static AllyParty Instance { get; private set; }

    [Header("Ally Start Data")]
    [SerializeField] private List<AllyData> AllyDataList;

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

    // Should only be called once at the start of the game (done in Awake)
    private void InitializeAllies()
    {
        Allies = new List<Ally>();
        foreach (var data in AllyDataList)
        {
            Allies.Add(Ally.CreateFromData(data));
        }
        // Allies = new List<Ally>
        // {
        //     //         name    maxHP  atk  def     spd    acc     critChance  critDamage  mana  role
        //     new Ally("Aria",   120,   20,  0.20f,  6500,  0.90f,  0.05f,      1.50f,      100,  AllyRole.Mage),
        //     new Ally("Brom",   160,   18,  0.50f,  4000,  0.80f,  0.10f,      1.50f,      60,   AllyRole.Warrior),
        //     new Ally("Celia",  100,   25,  0.10f,  7500,  0.85f,  0.15f,	  1.50f,  	  120,  AllyRole.Rogue),
        //     new Ally("Doric",  140,   15,  0.65f,  3000,  0.75f,  0.40f,	  1.50f,	  80,	AllyRole.Cleric),
        // };
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

    public void UpdateAllyRole(string allyName, AllyRole newRole)
    {
        var ally = Allies.Find(a => a.Name == allyName);
        if (ally == null)
        {
            Debug.LogError($"Ally with name {allyName} not found!");
            return;
        } else
        {
            ally.UpdateRole(newRole);
        }
    }
}