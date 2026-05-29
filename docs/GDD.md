# Game Design Document — godot1

> Living document — details will evolve as the game is playtested.

## Overview

A top-down horde survival game (Vampire Survivors / Diablo style). The player takes a persistent character into timed runs against escalating enemy waves. Skills fire automatically on cooldown — survival is about positioning and progression, not twitch reflexes.

Every run makes the character permanently stronger: level and XP carry over, stat bonuses stack, and coins and crafting materials earned go into a shared account pool. Between runs, players spend coins on meta upgrades and craft gear from materials — building both their character and their item collection over time.

The game has two intertwined goals: grow your character through runs, and build your gear through crafting.

---

## Core Mechanics

### Movement
- Top-down, 8-directional
- [TBD] Speed, acceleration values

### Combat

Skills drive all combat. Each skill has a **type**:

| Skill type | Behaviour |
|---|---|
| Active | Fires automatically when its cooldown expires. No player input required during the run. |
| Passive | On/off toggle. Effect is always-on while enabled. |

The **skill bar** on the run HUD shows all slotted skills and their cooldown / toggle state.

**v1:** One active attack skill. Fires on cooldown. Cooldown values TBD.

Character damage scales with character level (via level-up bonuses) and archetype base damage — not from the weapon gear slot.

### Damage Types

Every damage source has a **damage type**. Every entity that can take damage has a **resistance** value per type (percentage reduction).

`effective damage = raw damage × (1 − resistance)`

**v1 damage types:** Physical, Magic

**Future expansion:** Elemental types (Fire, Lightning, Frost, etc.) will be added as the system grows — the formula and resistance model extend naturally.

Resistances are always soft (never total immunity). Exact values are owned by BALANCE.md.

### Interaction
- Collectibles auto-collected on contact (XP gems, coins, health)
- [TBD] Any interactive objects (chests, shrines, etc.)

---

## Characters

Every run requires a character. Characters are created by the player, persist between runs, and grow over time. A player may own multiple characters simultaneously and delete any they no longer want.

### Character Archetypes

| Archetype | Max HP | Speed | Base Damage | Damage Type | Playstyle         |
|-----------|--------|-------|-------------|-------------|-------------------|
| Warrior   | 150    | 170   | 20          | Physical    | Tanky brawler     |
| Rogue     | 80     | 260   | 15          | Physical    | Fast and fragile  |
| Mage      | 100    | 200   | 35          | Magic       | Glass cannon      |

### Character Lifecycle
1. **Create** — player picks a name and archetype
2. **Select** — choose a character from the roster before a run
3. **Run** — character starts at their saved level; every new level gained during the run is permanent
4. **Grow** — level and XP carry over to the character; coins and crafting materials go to the shared account pool; permanent stat bonuses can be purchased between runs
5. **Delete** — player can permanently remove a character (irreversible)

A run cannot start without a selected character.

### Controls
| Input | Action         |
|-------|----------------|
| WASD  | Move           |
| —     | Attack (auto)  |
| ESC   | Pause          |

---

## In-Run Progression

### Level Up
- Killing an enemy grants **`1 XP × map level`** instantly (kill reward, no pickup required)
- Killing enemies also drops **XP gems** — collecting them adds further XP
- Both sources fill the same XP bar
- On level up: automatic permanent bonuses are applied — **+5 Max Health, +1 Weapon Damage**
- Level and XP within the current level persist when the run ends; the character picks up exactly where they left off
- No popup or pause — levelling up is seamless

### Enemy Drops
| Drop               | Effect                                         | Drop chance |
|--------------------|------------------------------------------------|-------------|
| XP gem             | Feeds level-up bar                             | 100%        |
| Coin               | Added to coin bank on run end                  | 25%         |
| Health pickup      | Restores HP instantly                          | 10%         |
| Crafting material (common) | Added to crafting-currency-1 bank on run end | 20% |
| Crafting material (higher tiers) | Added to respective material bank on run end | [TBD — rarer, tied to item tier] |

Drop rarity for crafting materials scales with tier — the more exotic the items a material can produce, the rarer it drops.

---

## Maps

Maps are the arenas where runs take place. Each map has **attributes** that modify the run.

### Map Attributes

| Attribute   | Description                                                                 |
|-------------|-----------------------------------------------------------------------------|
| Map Level   | Scales kill XP reward — killing an enemy grants `1 XP × map level` directly, on top of any XP gem the enemy drops |

More attributes will be added in future (e.g. enemy density modifiers, environmental hazards, drop bonuses).

---

## Run Structure

- **Duration:** Fixed time limit (target ~5 min; currently 5s for testing)
- **Map:** Each run takes place on a map; the map's attributes apply for the full run
- **Difficulty scaling:** Enemy count, speed, and variety increase over time
- **Run end conditions:**
  - Player dies → run over
  - Timer expires → run won [boss mechanic TBD]
- **Run rewards:** Level, XP, coins, and crafting materials earned all persist; player returns to the character screen

---

## Enemies

| Type     | Behavior          | Unlocks | Physical Resist | Magic Resist | Notes                        |
|----------|-------------------|---------|-----------------|--------------|------------------------------|
| Standard | Chase player      | 0:00    | 0%              | 0%           | Balanced — grey sprite       |
| Runner   | Chase player fast | 1:00    | 0%              | 15%          | Fragile, high speed — purple |
| Tank     | Chase player slow | 2:00    | 20%             | 0%           | High HP, high damage — orange|
| [TBD]    | Ranged attacker   | —       | —               | —            | Future type                  |
| [TBD]    | Boss              | Run end | —               | —            | Spawns when timer expires    |

All types scale with elapsed time — speed and HP increase per minute. Spawn rate also accelerates.

---

## Meta-Progression (Between Runs)

### Level Bonuses (automatic)
Each level gained during a run permanently improves the character:

| Per level gained | Effect                  |
|------------------|-------------------------|
| +5 Max Health    | Permanent HP increase   |
| +1 Damage        | Permanent damage increase (applies to attack skill) |

These stack across all runs. A level-10 character has +45 HP and +9 damage above their archetype base.

### Coin-Funded Upgrades (purchased between runs)
Spend coins on the character screen between runs:

| Upgrade      | Cost (per tier) | Max tiers |
|--------------|-----------------|-----------|
| +10 Max HP   | 50 / 100 / 150 / 200 / 250 | 5 |
| +10 Speed    | 50 / 100 / …   | 5         |
| +2 Damage    | 50 / 100 / …   | 5         |

### Gear Slots

Characters can equip up to 3 items, one per slot. Items persist between runs and provide flat stat bonuses on top of archetype base stats and coin upgrades.

| Slot      | Bonus type              |
|-----------|-------------------------|
| Weapon    | HP, Speed               |
| Armor     | HP (primary), Speed     |
| Accessory | Speed, HP, Damage (mixed)|

**Starter items (9 total):**

| Item            | Slot      | HP  | Speed | Damage |
|-----------------|-----------|-----|-------|--------|
| Iron Sword      | Weapon    | +10 | +10   | —      |
| Battle Axe      | Weapon    | +25 | -15   | —      |
| Enchanted Blade | Weapon    | +15 | +20   | —      |
| Leather Vest    | Armor     | +20 | —     | —      |
| Chain Mail      | Armor     | +40 | -10   | —      |
| Mage Robe       | Armor     | +15 | +15   | —      |
| Swift Ring      | Accessory | —   | +20   | —      |
| Vitality Charm  | Accessory | +30 | —     | —      |
| War Band        | Accessory | +10 | —     | +2     |

**Starter gear:** Each character starts with three items, one per slot, chosen to match their archetype:

| Archetype | Weapon          | Armor       | Accessory      |
|-----------|-----------------|-------------|----------------|
| Warrior   | Iron Sword      | Chain Mail  | War Band       |
| Rogue     | Iron Sword      | Leather Vest| Swift Ring     |
| Mage      | Enchanted Blade | Mage Robe   | Vitality Charm |

**Acquisition:** Gear is not dropped by enemies. New items come from crafting — each item has a recipe requiring a combination of materials (see Currencies). Gear is never lost.

**Item identity:** Each item is a fixed, unique definition with its own ID and icon. Progression produces *new* items — a higher-tier crafted weapon is a new item with its own ID, not an upgraded version of an existing one. Icons never change dynamically; an item always looks the same regardless of who owns it or has it equipped.

**Inventory:** All crafted items go into the **account inventory** — a single shared pool accessible by every character. The full inventory is visible on the Character Screen — every owned item is listed with its slot, stat bonuses, and which character (if any) currently has it equipped.

**Equipping:** Click a slot button (Weapon / Armor / Accessory) to open the item picker for that slot. Select any item from the account inventory to equip it; "Unequip" removes it from the slot and returns it to the shared pool.

---

## Currencies

### Coins
Earned during runs (25% enemy drop). **Account-shared** — earned by any character, spendable by any. Spent on permanent meta upgrades (HP / Speed / Damage tiers) on the Character Screen between runs.

### Crafting Materials
Crafting materials are tiered — common through exotic. Each tier drops at a different rate during runs and enables crafting of items at the corresponding tier. Items are crafted from **combinations** of materials, not a single currency spend.

| Tier    | Current name        | Drop rate | Enables                          |
|---------|---------------------|-----------|----------------------------------|
| Common  | crafting-currency-1 | 20%       | Low-tier items                   |
| [TBD]   | —                   | Rarer     | Mid-tier items                   |
| Exotic  | —                   | Very rare | Exotic / high-tier items         |

- All materials are **account-shared** — earned by any character, spendable by any character
- The more exotic the craftable item, the rarer its required materials
- Specific tiers, drop rates, and material combinations will be designed when crafting is fleshed out

---

## UI / HUD

- Health bar
- XP bar + current level
- Coin counter (this run)
- Elapsed time / countdown
- **Skill bar** — shows slotted skills with cooldown state (active) or toggle state (passive)
- [TBD] Minimap

### Menus
- **Main Menu** → title screen, Play button
- **Character Select** → list characters, create new (name + archetype), delete; clicking a character navigates to their screen
- **Character Screen** → character stats, three tabs, Start Run button
  - **Equipment tab** *(default)* — gear slot buttons (Weapon / Armor / Accessory)
  - **Sigils tab** — visible, empty (reserved for future sigil system)
  - **Skills tab** — visible, empty (reserved for future skill tree system)
  - All three tabs are always visible; empty tabs are not locked or greyed out
- Run results overlay → shown at run end; return button goes back to character screen
- **Pause menu** — ESC during a run; second ESC or Resume button closes it; run is paused while open
  - **Resume** button — closes menu, run continues
  - **End Run** button — exits immediately to character screen; all progress from this run is discarded (level, XP, coins, crafting materials). Warning text alongside: *"All progress from this run will be lost."*
  - No confirmation step — warning text is the friction

---

## Win / Lose Conditions

| Condition     | Outcome                                                        |
|---------------|----------------------------------------------------------------|
| Player HP = 0 | Run lost — level, XP, coins, and crafting materials earned still saved |
| Timer expires | Run won — all rewards saved; [boss mechanic TBD]              |

In both cases the player is returned to the character screen. There is no death penalty — every run makes the character stronger regardless of outcome.
