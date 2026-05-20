# Game Design Document — godot1

> Living document — details will evolve as the game is playtested.

## Overview

A top-down auto-attack horde survival game. The player takes a persistent character into runs against escalating enemy waves. Killing enemies earns XP; levelling up permanently improves the character. The character's level, XP, and stats carry over between runs — each run makes the character meaningfully stronger. Between runs, coins earned fund permanent upgrades. The game is character-driven: you play to grow your character, not to build a single run's loadout.

---

## Core Mechanics

### Movement
- Top-down, 8-directional
- [TBD] Speed, acceleration values

### Combat
- **Auto-attack only** — no manual firing
- Single weapon per character; damage scales with character level
- Weapon targets nearest enemy automatically on a cooldown timer

### Interaction
- Collectibles auto-collected on contact (XP gems, coins, health)
- [TBD] Any interactive objects (chests, shrines, etc.)

---

## Characters

Every run requires a character. Characters are created by the player, persist between runs, and grow over time. A player may own multiple characters simultaneously and delete any they no longer want.

### Character Archetypes

| Archetype | Max HP | Speed | Base Damage | Playstyle         |
|-----------|--------|-------|-------------|-------------------|
| Warrior   | 150    | 170   | 20          | Tanky brawler     |
| Rogue     | 80     | 260   | 15          | Fast and fragile  |
| Mage      | 100    | 200   | 35          | Glass cannon      |

### Character Lifecycle
1. **Create** — player picks a name and archetype
2. **Select** — choose a character from the roster before a run
3. **Run** — character starts at their saved level; every new level gained during the run is permanent
4. **Grow** — level, XP, and coin bank carry over between runs; permanent stat bonuses can be purchased between runs
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
- Killing enemies drops **XP gems**
- Collecting XP gems fills the XP bar
- On level up: automatic permanent bonuses are applied — **+5 Max Health, +1 Weapon Damage**
- Level and XP within the current level persist when the run ends; the character picks up exactly where they left off
- No popup or pause — levelling up is seamless

### Enemy Drops
| Drop        | Effect                              | Drop chance  |
|-------------|-------------------------------------|--------------|
| XP gem      | Feeds level-up bar                  | Common       |
| Coin        | Added to meta currency bank on run end | Uncommon  |
| Health pickup | Restores HP instantly             | Rare         |

---

## Run Structure

- **Duration:** Fixed time limit (target ~5 min; currently 5s for testing)
- **Difficulty scaling:** Enemy count, speed, and variety increase over time
- **Run end conditions:**
  - Player dies → run over
  - Timer expires → run won [boss mechanic TBD]
- **Run rewards:** Level, XP, and coins earned persist to the character; player returns to the character screen

---

## Enemies

| Type     | Behavior          | Unlocks | Notes                        |
|----------|-------------------|---------|------------------------------|
| Standard | Chase player      | 0:00    | Balanced — grey sprite       |
| Runner   | Chase player fast | 1:00    | Fragile, high speed — purple |
| Tank     | Chase player slow | 2:00    | High HP, high damage — orange|
| [TBD]    | Ranged attacker   | —       | Future type                  |
| [TBD]    | Boss              | Run end | Spawns when timer expires    |

All types scale with elapsed time — speed and HP increase per minute. Spawn rate also accelerates.

---

## Meta-Progression (Between Runs)

### Level Bonuses (automatic)
Each level gained during a run permanently improves the character:

| Per level gained | Effect                  |
|------------------|-------------------------|
| +5 Max Health    | Permanent HP increase   |
| +1 Weapon Damage | Permanent damage increase|

These stack across all runs. A level-10 character has +45 HP and +9 damage above their archetype base.

### Coin-Funded Upgrades (purchased between runs)
Spend coins on the character screen between runs:

| Upgrade      | Cost (per tier) | Max tiers |
|--------------|-----------------|-----------|
| +10 Max HP   | 50 / 100 / 150 / 200 / 250 | 5 |
| +10 Speed    | 50 / 100 / …   | 5         |
| +2 Damage    | 50 / 100 / …   | 5         |

### [TBD] Gear Slots
Equipment persisted between runs — weapons, armour, accessories. Not yet implemented.

---

## UI / HUD

- Health bar
- XP bar + current level
- Coin counter (this run)
- Elapsed time / countdown
- [TBD] Minimap

### Menus
- **Main Menu** → title screen, Play button
- **Character Select** → list characters, create new (name + archetype), delete; clicking a character navigates to their screen
- **Character Screen** → character stats, Start Run button, future home of upgrades / gear / crafting
- Run results overlay → shown at run end; return button goes back to character screen
- [TBD] Pause menu

---

## Win / Lose Conditions

| Condition     | Outcome                                                        |
|---------------|----------------------------------------------------------------|
| Player HP = 0 | Run lost — level and XP still saved; coins earned still saved |
| Timer expires | Run won — all rewards saved; [boss mechanic TBD]              |

In both cases the player is returned to the character screen. There is no death penalty — every run makes the character stronger regardless of outcome.
