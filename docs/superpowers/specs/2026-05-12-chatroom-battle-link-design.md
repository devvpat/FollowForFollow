# Chatroom ↔ Battle System Linkage — Design

**Date:** 2026-05-12
**Branch:** chatroomV2
**Status:** Approved, awaiting implementation plan

## Goal

Let the player pick an ally's battle role from the chatroom's profile panel, and have that choice take effect in the next battle. The battle system (`AllyData` / `AllyRole`) is the source of truth; the chatroom adapts to it.

## Scope

In scope:
- One-way write from chatroom UI → battle ally state.
- Persisting the chosen role on the `AllyData` asset (Editor) and the runtime `Ally` (battle).
- Replacing the chatroom dropdown options to match `AllyRole`.

Out of scope:
- Refactoring `AllyData` or `CharacterProfile` into a single asset.
- Reverse data flow (battle events updating the chatroom).
- UI for switching role *during* a battle.
- Automated tests (no test framework set up in this project).

## Source of truth

- `AllyData` (ScriptableObject) and `AllyRole` (enum) remain canonical.
- `CharacterProfile.partyRole` and the `PartyRole` enum stay on disk but are no longer written to by the UI. They become dead fields the code can ignore. Removing them is a separate cleanup task.

## Identity link

Each `CharacterProfile` gains a direct reference to its corresponding `AllyData`:

```csharp
// CharacterProfile.cs
public AllyData allyData;
```

The reference is set per-profile in the Inspector. There are four `CharacterProfile` assets in `Assets/Profiles/` (`MainPlayer_Profile`, `Park_Profile`, `Tsuruha_Profile`, `Winston_Profile`) and four `AllyData` assets in `Assets/Data/Ally Party/` (`Ally 1` through `Ally 4`). The developer pairs them in the Inspector — one drag per profile. The pairing (which profile maps to which `AllyData`) is a content decision left to the developer and not prescribed by this spec.

Note that names do not match across the two sets (`MainPlayer_Profile.ign = "You"`, `Ally 1.Name = "Aria"`), which is why a string-match approach would not have worked.

Rationale: explicit, type-safe, editor-clickable, refactor-safe. Alternatives considered (name-string matching, shared ID field, merging the two ScriptableObjects, inverse reference from AllyData → Profile) were rejected for fragility, naming-dependency, or scope creep.

## Data flow

```
Player picks dropdown value in CharacterProfilePanel
        ↓
OnRoleChanged(index)
        ↓
   newRole = (AllyRole)index
        ↓
   ├─ profile.allyData.Role = newRole                      // persist (Editor)
   └─ AllyParty.Instance?.UpdateAllyRole(
          profile.allyData.Name, newRole)                  // runtime Ally
```

Both writes are required:
- Writing only to the `AllyData` SO would not affect the runtime `Ally` instance that `BattleManager` reads from — `AllyParty.InitializeAllies` runs once in `Awake()` and the result is `DontDestroyOnLoad`.
- Writing only to the runtime `Ally` would not survive an Editor stop/restart that re-runs `AllyParty.Awake()`.

In a built game, SO mutations don't persist to disk between launches; this is acceptable for the current development phase. A save system is a separate future concern.

## UI changes

`CharacterProfilePanel.cs:23-25`: replace the hard-coded option list.

Before:
```csharp
roleDropdown.AddOptions(new List<string> { "Support", "Attacker", "Defender", "Observer" });
```

After:
```csharp
roleDropdown.AddOptions(new List<string> { "Warrior", "Mage", "Rogue", "Cleric" });
```

Order matches `AllyRole` (`Warrior=0, Mage=1, Rogue=2, Cleric=3`) so `(AllyRole)index` lines up. `AllyRole.Default` is intentionally omitted from the dropdown — it is a fallback, not a player-selectable role.

`CharacterProfilePanel.cs:50`: `SetValueWithoutNotify((int)profile.partyRole)` becomes `SetValueWithoutNotify((int)profile.allyData.Role)`.

`OnRoleChanged` (line 53) replaces its current body with the two-write flow above.

## Edge cases

| Case | Handling |
|---|---|
| `profile.allyData == null` | `Show()` adds a null-check; if missing, disable the dropdown and `Debug.LogWarning` once per profile. Do not crash. |
| `AllyParty.Instance == null` (chatroom loaded without battle prefab) | Wrap the `UpdateAllyRole` call in a null-check. The SO write still happens, so battle picks up the new role the next time `AllyParty.Awake()` runs. |
| Player changes role mid-battle | `Ally.UpdateRole` rebuilds `Skills[]` immediately. `BattleUI`'s skill-button labels refresh on the next skill-menu open. Acceptable; chatroom is not normally reachable during a fight. |
| `AllyData.Name` and `CharacterProfile.ign` differ | Lookup uses `profile.allyData.Name`. Chatroom display text continues to use `ign`. The two never need to agree. |

## Files touched

- `Assets/Scripts/CharacterProfile.cs` — add `public AllyData allyData;` field.
- `Assets/Scripts/CharacterProfilePanel.cs` — replace dropdown options, replace `OnRoleChanged` body, read initial value from `profile.allyData.Role`.
- `Assets/Profiles/*.asset` (the four `CharacterProfile` assets) — Inspector-only change: drag matching `AllyData` (from `Assets/Data/Ally Party/`) into each profile's new `allyData` slot.

## Files explicitly NOT touched

- `Assets/Scripts/BattleSystem/**` — entire battle system unchanged.
- `Assets/Scripts/BuddySlotUI.cs` — slot UI unaffected.
- `AllyData.cs`, `Ally.cs`, `AllyParty.cs` — already expose the API we need.

## Verification

Manual, in-Editor:
1. Open `Chatroom.unity`. Click each buddy slot. Profile panel shows the role from `AllyData.Role`.
2. Change dropdown to a different role. No console errors. Reopening the same profile shows the new value.
3. Open `BattleTest.unity`. Start a fight. The changed ally's skill-menu reflects the new role's skill set.
4. Stop play, restart, repeat step 3 — role should still match because the `AllyData` asset was mutated (Editor-only persistence).

## Open follow-ups (out of this spec)

- Remove `PartyRole` enum and `CharacterProfile.partyRole` field once nothing reads them.
- Decide later whether to merge `CharacterProfile` and `AllyData` into one asset.
- Consider a real save system before shipping (SO mutation doesn't persist in builds).
