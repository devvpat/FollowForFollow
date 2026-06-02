# Battle UI — Unity Editor Setup Guide

This guide walks you through wiring the battle UI in `Assets/Scenes/BattleTest.unity`. All the
scripts are written; the remaining work is building GameObjects, creating 4 prefabs, and
assigning serialized references in the Inspector.

## Progress — last updated 2026-05-29 (resume here)

**Done:**
- Step 0 — managers verified (`BattleManager` / `AllyParty` / `EnemyParty` already prefab-instanced).
- Step 0.5 — old `BattleUI Canvas` prefab deleted; `AllyCard` salvaged onto the plain `Canvas`.
- Step 1a — `StatusIcon.prefab` built (icon library left empty → colored-letter fallback).
- Step 1b — `AllyCard.prefab` finished + saved.
- Step 1c — `EnemyField.prefab` built + saved.
- Step 1d — `TurnOrderUI.prefab` (was already built; no work).
- Step 2a / 2b / 2c — `TurnOrderBar`, `AllyCardContainer`, `BattlefieldArea` built.
- Step 2d — `EnemyDetailPanel` built + wired (6 fields).
- Step 2e — `ActionPanel` built (Attack / Skill / Defend buttons; listeners wired by `BattleUI`).
- Step 2f — `SkillPanel` built (4 `SkillSlotUI` rows + BACK; 4 fields on each row, 2 on panel).
- Step 2g — `BattleLogPanel` built (toggle button + collapsible "Info view" scroll; 5 fields).
- Step 2h — `ResultOverlay` built (full-screen dim + title + button; may start inactive).
- Step 4 — `BattleUI` orchestrator added + all 16 fields wired.
- Step 5 (in progress) — fixed three bugs found in Play mode:
  - `ResultOverlay` was active at load → full-screen raycast blocker over `TestButton`. Set inactive
    (scene YAML, line ~1756 `m_IsActive: 0`).
  - `BattleLogPanel.panelContainer` pointed at the panel ROOT (hid the toggle too) → repointed at
    the scroll-view content GameObject (fileID 1599644430) in scene YAML.
  - Battle log appeared blank: the log text doubled as ScrollRect content but was hand-sized for the
    old "Info view" skill-list dump (1843px tall, -921 offset), so short battle lines rendered
    off-screen; and it started collapsed. Fixed in `BattleLogPanel.cs` (NOT scene YAML, because Unity
    was open and would overwrite scene edits): added `NormalizeContentLayout()` (top-stretch anchor +
    ContentSizeFitter), starts open (`IsOpen = true`), wipes stale text, auto-scrolls to bottom,
    null-guards everything.

**▶ NEXT (tomorrow): finish Step 5 verification.**

⚠ TWO SCENE-YAML FIXES AT RISK: because Unity was open this session, the `ResultOverlay` inactive
fix and the `panelContainer` repoint may have been reverted when Unity last saved the scene. After
Unity recompiles `BattleLogPanel.cs`, FIRST verify in the Hierarchy:
  1. `ResultOverlay` checkbox is UNTICKED at scene load (else TestButton is unclickable again).
  2. `BattleLogPanel`'s `panelContainer` field → the scroll-view ("Log") object, NOT the panel root.
If either reverted, re-apply via the Inspector (Unity is authoritative now), then Play → click
TestButton → confirm the log streams "A new battle begins!" + each turn/attack live and always-visible.

Then resume the rest of the Step 5 checklist (ally cards, turn order, enemy panel, action/skill flow,
Victory/Defeat overlay). Nothing in the battle UI work is committed yet — whole working tree is dirty.

---

Layout target is the mockup in `Downloads/Screenshot 2026-05-26 114425.png`:

```
[turn-order strip]                              [ENEMY DETAIL window (X)]
[ally card]
[ally card]                 (enemy sprite + HP)
[ally card]
[ally card]
                  [BACK]                    [ skill 1 | 10 MP ]
                                            [ skill 2 | 10 MP ]
                  [Attack][Skill][Defend]   [ skill 3 | 10 MP ]
                                            [ skill 4 | 10 MP ]
```

> ## ⚠ Read this first: it's all-or-nothing
> `BattleUI.OnEnable` and `HandleBattleStart` dereference **almost every** serialized field the
> moment a battle starts (all buttons, all panels, both containers, both prefabs, turn order).
> If any one reference is left empty you'll get a `NullReferenceException` and nothing renders.
> Wire **everything** in Step 4 before pressing Play.

---

## Step 0 — Prerequisites (managers + battle trigger) — already in the scene, just VERIFY

The battle does **not** start automatically. It begins only when `BattleManager.StartNewFight()`
is called, which the scene's **TestButton** does on click. The required managers are **already
present** in `BattleTest.unity` as prefab instances (each is its own top-level root in the
Hierarchy). You don't add them — confirm them:

- [x] **`BattleManager`** — instance of `Assets/Prefabs/Singletons/BattleManager.prefab`. Sets
  the `Instance` singleton in `Awake`. ✓ present.
- [x] **`AllyParty`** — instance of `Assets/Prefabs/Singletons/AllyParty.prefab`. Its
  **`AllyDataList`** is already filled with the **4** ally `AllyData` assets; it builds the party
  in `Awake` and persists via `DontDestroyOnLoad`. ✓ present (verify the 4 entries are non-empty).
- [x] **`EnemyParty`** — instance of `Assets/Prefabs/Character/EnemyParty.prefab`, with its
  **`EnemyDataList`** filled with **2** enemy `EnemyData` assets. Provides `.Enemies`. ✓ present.
- [ ] **TestButton** — the existing `TestButton` object already has `BattleTestButton` on it (it
  lives under the plain `Canvas`, not the old battle UI). Confirm its `EnemyPartyRef` points at
  the `EnemyParty` above (✓ already wired), and its Button `OnClick` calls
  `BattleTestButton.OnClickTest`.

> If any of the three manager roots is missing from your Hierarchy, drag the matching prefab from
> `Assets/Prefabs/Singletons/` (or `Character/`) back into the scene.

**Execution order is already handled:** `BattleUI` runs at script execution order `100`, so
`BattleManager.Awake`/`AllyParty.Awake` (order 0) always run before `BattleUI.OnEnable` and
`Instance` is ready. You don't need to change anything.

---

## Step 0.5 — Remove the old battle UI (do this BEFORE building)

The scene still contains a **stale** `BattleUI Canvas` root — an instance of the old
`BattleUI Canvas.prefab` (last edited before the script rewrite). It carries the **old** `BattleUI`
orchestrator and old panels, none of which match the current scripts. We're building fresh, so it
has to go. **It is a separate root from the plain `Canvas`** that holds `TestButton` and `Info view`
— do not delete that one.

- [ ] **Salvage the AllyCard first (recommended).** The half-wired `AllyCard` (290×90, with
  portrait/name/HP/MP/highlight) currently lives as a child **inside** `BattleUI Canvas`. In the
  Hierarchy, **drag `AllyCard` out** of `BattleUI Canvas` and drop it onto the plain **`Canvas`**
  so its layout + 7 wired fields survive. (Skip this only if you'd rather rebuild it from scratch
  in Step 1b.)
- [ ] **Delete the `BattleUI Canvas` root** entirely (right-click → Delete). This removes the old
  orchestrator and old panels. `BattleManager` / `AllyParty` / `EnemyParty` / `TestButton` /
  `Info view` are all on **other** roots and are unaffected.
- [ ] Confirm the Hierarchy now has **one** Canvas-type root (the plain `Canvas`). All new UI in
  the steps below goes **under that `Canvas`**.

---

## Step 1 — Build the 4 prefabs

Make a folder `Assets/Prefabs/BattleUI/` if needed. Build each root, add the component, then
assign every listed field by dragging the child object/component onto the slot.

### 1a. `StatusEffectIconUI` prefab (`statusIconPrefab`)
Small icon shown on ally/enemy cards and the enemy panel. Tiny — ~32×32.
- Root: UI `Image`. `Add Component → StatusEffectIconUI`.
- Children: one `Text (TMP)` for the duration number, one `Text (TMP)` for the single-letter label.

| Field | Assign to |
|---|---|
| `iconImage` | the root `Image` |
| `durationText` | the duration `Text (TMP)` |
| `labelText` | the label `Text (TMP)` — now a **fallback**, only shown when an effect has no icon assigned |
| `iconLibrary` | the `StatusEffectIconLibrary` asset (see the status-effect-icons setup) |

When `iconLibrary` is assigned and the effect's icon slot has a sprite, the icon shows the real
sprite and the letter label is hidden. Effects whose slot is empty fall back to the colored
square + first letter.

Drag into `Assets/Prefabs/BattleUI/` to save as a prefab, then delete the scene copy.

### 1b. `AllyCardUI` prefab (`allyCardPrefab`) — finish the salvaged `AllyCard`
After Step 0.5 the `AllyCard` (290×90, with portrait/name/HP/MP/highlight) now lives under the
plain `Canvas`. It's already wired for the top 7 fields but **missing the rest** — finish them.
(If you skipped salvage in Step 0.5, build a new `AllyCard` from scratch matching this table.)

| Field | Status | Assign to |
|---|---|---|
| `portraitImage` | ✅ done | circular portrait `Image` |
| `nameText` | ✅ done | Name `Text (TMP)` |
| `hpBar` | ✅ done | HP `Slider` |
| `hpText` | ✅ done | HP `Text (TMP)` |
| `mpBar` | ✅ done | MP `Slider` |
| `mpText` | ✅ done | MP `Text (TMP)` |
| `activeHighlight` | ✅ done | active `Image` (shown on this ally's turn) |
| `targetHighlight` | ➕ add | a second overlay `Image` (shown during targeting) |
| `statusIconContainer` | ➕ add | an empty child with `HorizontalLayoutGroup` for status icons |
| `statusIconPrefab` | ➕ add | the **1a** prefab |
| `defendingBadge` | ➕ add | a small "shield" child GameObject (toggled when defending) |
| `deadOverlay` | ➕ add | a dark full-card `Image` GameObject (toggled when dead) |
| `cardButton` | ➕ add | the **Button on the card root** (already exists) — needed so you can click an ally as a skill target |

Then drag `AllyCard` into `Assets/Prefabs/BattleUI/` to make it a prefab. You can delete the
scene instance — `BattleUI` spawns 4 copies at runtime into the ally-card container.

### 1c. `EnemyFieldUI` prefab (`enemyFieldPrefab`)
A single on-field enemy: sprite + floating HP bar + status icons. Build new:
- Root: UI `Image` (the enemy sprite). `Add Component → EnemyFieldUI` and `Add Component → Button`.
- Children: `Text (TMP)` name, a `Slider` HP bar floating above, an empty `statusIconContainer`
  (HorizontalLayoutGroup), a `selectionIndicator` `Image` (glow), a `deadOverlay` GameObject.

| Field | Assign to |
|---|---|
| `enemySprite` | the root `Image` |
| `nameText` | name `Text (TMP)` |
| `hpBar` | the floating `Slider` |
| `statusIconContainer` | the status-icon child |
| `selectionIndicator` | the glow `Image` |
| `clickArea` | the `Button` on the root |
| `deadOverlay` | the dead-overlay GameObject |
| `statusIconPrefab` | the **1a** prefab |

> Note: `BattleUI` overwrites this prefab's anchors at runtime to position enemies, so its
> on-canvas position doesn't matter — just give it a sensible size.

Save to `Assets/Prefabs/BattleUI/`.

### 1d. `turnIconPrefab` — reuse the existing prefab
`Assets/Prefabs/BattleUI/TurnOrderUI.prefab` is **already built and wired** with `TurnOrderIconUI`
(portrait, border, initial label). Despite its name it is the **single turn-order icon**, not the
container. Use it as-is for `turnIconPrefab` in Step 2. (No work needed.)

---

## Step 2 — Build the scene hierarchy (under `Canvas`)

Create these as children of the plain **`Canvas`** — the one that has `TestButton` + `Info view`
(it already has Canvas + CanvasScaler + GraphicRaycaster). The old `BattleUI Canvas` is gone after
Step 0.5, so there's only one Canvas to build under now. Position to match the screenshot.

### 2a. `TurnOrderBar` (top-left)
- Empty UI object anchored top-left, stretched horizontally a bit. `Add Component →
  HorizontalLayoutGroup`. `Add Component → TurnOrderUI`.
- Set `container` = its own `RectTransform` (or an inner "Content" child if you prefer).
- Set `turnIconPrefab` = `TurnOrderUI.prefab` (the **1d** icon).

### 2b. `AllyCardContainer` (left column)
- Empty UI object anchored to the left edge, tall. `Add Component → VerticalLayoutGroup`
  (spacing ~10, child alignment upper-left). Leave it **empty** — `BattleUI` spawns the 4 ally
  cards into it at runtime.

### 2c. `BattlefieldArea` (center)
- Empty `RectTransform` covering the center area where enemies appear. Leave it empty — enemy
  fields spawn here and are positioned by normalized anchors, so make this a **stretched** rect.

### 2d. `EnemyDetailPanel` (top-right — the "ERROR" window)
- Panel styled like the mockup's OS error window. `Add Component → EnemyDetailPanelUI`.
- Children: `nameText` (TMP), `hpBar` (Slider), `hpText` (TMP), `statusEffectContainer` (empty,
  HorizontalLayoutGroup), `closeButton` (the X `Button`).

| Field | Assign to |
|---|---|
| `nameText` | name `Text (TMP)` |
| `hpBar` | HP `Slider` |
| `hpText` | HP `Text (TMP)` |
| `statusEffectContainer` | the status-icon child |
| `closeButton` | the X `Button` |
| `statusIconPrefab` | the **1a** prefab |

### 2e. `ActionPanel` (bottom — Attack / Skill / Defend)
- A container with three `Button`s. (`BattleUI` shows/hides this panel automatically.)

### 2f. `SkillPanel` (bottom-right — 4 slots + BACK)
- Container + `Add Component → SkillPanelUI`.
- Build **one** `SkillSlotUI` row, then duplicate to **4**. Each row:
  - `Add Component → SkillSlotUI` on the row root (which has a `background` Image + a `Button`).
  - Children: name `Text (TMP)`, description `Text (TMP)`, MP-cost `Text (TMP)`.

  | SkillSlotUI field | Assign to |
  |---|---|
  | `nameText` | name TMP |
  | `descriptionText` | description TMP |
  | `mpCostText` | MP-cost TMP |
  | `background` | the row's background `Image` |
  | `button` | the row's `Button` |

  (The 3 color fields have sensible defaults — leave them.)
- Add a **BACK** `Button`.

| SkillPanelUI field | Assign to |
|---|---|
| `skillSlots` | size 4 → the 4 `SkillSlotUI` rows |
| `backButton` | the BACK `Button` |

### 2g. `BattleLogPanel` (corner, collapsible)
- A small always-visible **toggle button** plus a collapsible content panel.
- `Add Component → BattleLogPanel`.
- You can repurpose the existing **"Info view"** ScrollRect as the collapsible content.

| Field | Assign to |
|---|---|
| `panelContainer` | the collapsible content GameObject (the scroll view) |
| `scrollRect` | its `ScrollRect` |
| `logText` | the `Text (TMP)` inside the scroll content |
| `toggleButton` | the always-visible toggle `Button` |
| `toggleButtonLabel` | the toggle button's label `Text (TMP)` |

### 2h. `ResultOverlay` (full-screen victory/defeat)
- A full-screen dim panel with a big title and a button. `BattleUI` toggles it (starts hidden).
- Children: title `Text (TMP)`, a `Button`, and its label `Text (TMP)`.

---

## Step 3 — Keep the right roots ACTIVE (important Awake gotcha)

`SkillPanelUI`, `EnemyDetailPanelUI`, and `BattleLogPanel` wire their button listeners in
`Awake`, which **only runs if the GameObject is active when the scene loads**. So in the Editor:

- [ ] Leave **`BattleUI`, `ActionPanel`, `SkillPanel`, `EnemyDetailPanel`, `BattleLogPanel`**
  root GameObjects **active (checkbox ticked)** at scene load. `BattleUI` will hide the ones that
  should be hidden the instant a battle starts.
- [ ] `ResultOverlay` may start inactive (it has no `Awake` logic).

---

## Step 4 — Add the `BattleUI` orchestrator and wire everything

Create a `BattleUI` GameObject under the Canvas (or reuse the Canvas) and `Add Component →
BattleUI`. Assign **every** field from the objects above:

| BattleUI field | Assign to |
|---|---|
| `allyCardContainer` | `AllyCardContainer` (2b) |
| `allyCardPrefab` | the `AllyCard` prefab (1b) |
| `battlefieldArea` | `BattlefieldArea` (2c) |
| `enemyFieldPrefab` | the EnemyField prefab (1c) |
| `enemyDetailPanel` | `EnemyDetailPanel` (2d) |
| `turnOrderUI` | `TurnOrderBar` (2a) |
| `actionPanel` | `ActionPanel` (2e) |
| `attackButton` | Attack button (2e) |
| `skillButton` | Skill button (2e) |
| `defendButton` | Defend button (2e) |
| `skillPanelUI` | `SkillPanel` (2f) |
| `battleLogPanel` | `BattleLogPanel` (2g) |
| `resultOverlay` | `ResultOverlay` (2h) |
| `resultTitleText` | result title TMP (2h) |
| `resultButton` | result `Button` (2h) |
| `resultButtonLabel` | result button label TMP (2h) |
| `allyColors` | leave the 4 defaults |

---

## Step 5 — Verify in Play mode

> Reminder: nothing appears until you click **TestButton** — that's what starts the fight.

- [ ] Press **Play**. No `NullReferenceException` on load (means all managers + refs are present).
- [ ] Click **TestButton**. The battle starts and the log prints "A new battle begins!".
- [ ] **4 ally cards** appear on the left with names and filled HP/MP bars.
- [ ] The **turn-order strip** fills top-left with ally/enemy icons (current actor glows gold).
- [ ] The **enemy** appears center with a floating HP bar; the **enemy detail** window shows it.
- [ ] On an **ally turn** the action panel appears.
  - [ ] **Attack** → enemies highlight → click an enemy → damage resolves, log updates.
  - [ ] **Skill** → skill panel opens with the ally's skills (name / MP / description), one
    highlighted; pick one (it enters targeting if needed); **BACK** closes the panel.
  - [ ] **Defend** → submits and ends the turn.
- [ ] Kill all enemies → **ResultOverlay** shows "Victory!"; lose all allies → "Defeat!" with a
  Replay button.

---

## Reference notes / gotchas

- **`statusIconPrefab` is assigned in three places** — the same 1a prefab goes on `AllyCardUI`,
  `EnemyFieldUI`, and `EnemyDetailPanelUI`.
- **`CharUI.cs` is unused** by the current `BattleUI` — ignore it.
- **`TurnOrderUI.prefab` is the icon, not the container** — the container is the `TurnOrderUI`
  component you add to `TurnOrderBar` in 2a.
- On the ally card, `defendingBadge` / `deadOverlay` / `cardButton` / `targetHighlight` /
  status-icon fields are null-guarded in code, so a partially-wired card won't crash — but those
  features (targeting an ally, dead overlay, status icons) won't work until wired.
