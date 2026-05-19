# Technical Design Document — godot1

> Living document — architecture will evolve as systems are built and playtested.

## Architecture Overview

Godot 4.6, C#, Forward Plus renderer. Scene composition over inheritance — each system is a self-contained scene or node that communicates via signals. Two save layers: a persistent save file (meta) and an in-memory run session (discarded on run end).

---

## Scene Layout

```
Main (Node)
├── World (Node2D)
│   ├── TileMap
│   ├── EnemySpawner (Node)
│   ├── Enemies (Node)          ← spawned enemies parented here
│   └── Pickups (Node)          ← XP gems, coins, health drops
├── Player (CharacterBody2D)
├── UI (CanvasLayer)
│   ├── HUD                     ← health, XP bar, timer, coin count
│   ├── UpgradePicker           ← level-up pause overlay
│   ├── PauseMenu
│   └── RunResults
└── RunSession (Node)           ← tracks time, XP, level, run state
```

> Provisional — update as scenes are created.

---

## Core Systems

| System            | Responsibility                                               | Path                      |
|-------------------|--------------------------------------------------------------|---------------------------|
| Player            | Input, movement, stat sheet, taking damage                   | `res://src/player/`       |
| Weapon            | Auto-attack, targeting nearest enemy, firing on cooldown     | `res://src/weapon/`       |
| EnemySpawner      | Time-based wave scaling, spawning enemy scenes               | `res://src/enemies/`      |
| Enemy             | AI (chase/ranged), taking damage, death + drop spawning      | `res://src/enemies/`      |
| Pickup            | XP gems, coins, health — auto-collected on contact           | `res://src/pickups/`      |
| RunSession        | Tracks elapsed time, XP, current level, run state           | `res://src/run/`          |
| UpgradePicker     | Pause game, present N choices, apply selected upgrade        | `res://src/ui/`           |
| SaveManager       | Read/write persistent save file (meta layer)                 | `res://src/save/`         |
| MetaProgression   | Gear slots, permanent upgrades, unlocks, coin bank           | `res://src/meta/`         |

---

## Data / Resource Types

Godot `Resource` subclasses used as data containers (no logic).

| Resource            | Fields                                                        |
|---------------------|---------------------------------------------------------------|
| `CharacterData`     | Name, base stats, starting weapon reference, unlock condition |
| `WeaponData`        | Name, base damage, cooldown, upgrade path (array of `WeaponUpgradeData`) |
| `WeaponUpgradeData` | Damage delta, cooldown delta, new behaviour flags            |
| `GearData`          | Name, slot (Weapon/Armour/Accessory), stat modifiers, ability |
| `UpgradeOptionData` | Display name, description, effect type + value               |
| `EnemyData`         | HP, speed, damage, XP value, drop table weights              |

---

## Save Layers

### Persistent Save (between runs)
Serialised to disk. Contains:
- Equipped gear (3 slots)
- Permanent upgrade levels
- Coin bank balance
- Unlocked characters
- Unlocked starting builds

### Run Session (in-memory only)
Lives on the `RunSession` node. Discarded when the scene unloads.
- Elapsed time
- Current XP + level
- Upgrades chosen this run
- Coins earned this run → flushed to coin bank on run end

---

## Weapon Upgrade Path

Weapon is a single entity that evolves. On level-up, one upgrade choice may advance the weapon along its path.

```
WeaponData
  └── UpgradePath: WeaponUpgradeData[]
        [0] → Stage 1 (base)
        [1] → Stage 2 (faster fire)
        [2] → Stage 3 (piercing)
        [3] → Stage 4 (AoE explosion)
```

Current stage index stored on the player/weapon instance during the run.

---

## Enemy Spawner — Wave Scaling

Time-driven, no fixed waves. Every N seconds the spawner recalculates:
- **Spawn rate** — increases with time
- **Enemy pool** — harder variants unlock at time thresholds
- **Horde size** — group spawns grow larger over time

Final boss spawns when the run timer expires.

---

## Drop System

Each enemy holds a weighted drop table from its `EnemyData`.

| Drop          | Default weight |
|---------------|---------------|
| Nothing       | High           |
| XP gem (small)| Medium         |
| XP gem (large)| Low            |
| Coin          | Low            |
| Health pickup | Very low       |

---

## Class Conventions (C#)

- **Namespaces:** `Godot1.<System>` (e.g. `Godot1.Player`, `Godot1.Combat`)
- **Node classes:** PascalCase — `PlayerController`, `EnemyBase`, `WeaponController`
- **Resource classes:** suffix `Data` — `EnemyData`, `WeaponData`, `GearData`
- **Private fields:** `_camelCase`; public properties: `PascalCase`
- **Signals:** `[Signal]` delegate, past-tense — `HealthChanged`, `EnemyDied`, `LeveledUp`
- **Folder layout:** `src/<system>/` mirrors namespace

---

## Signals & Events

Systems communicate via signals only — no direct cross-system method calls.

| Signal                  | Emitter        | Receivers                        |
|-------------------------|----------------|----------------------------------|
| `HealthChanged(int)`    | Player         | HUD, GameManager                 |
| `PlayerDied`            | Player         | RunSession (end run)             |
| `LeveledUp(int)`        | RunSession     | UpgradePicker (show choices)     |
| `UpgradeChosen(data)`   | UpgradePicker  | Player, WeaponController         |
| `EnemyDied(position)`   | Enemy          | DropSpawner, RunSession (XP)     |
| `XpCollected(int)`      | Pickup         | RunSession                       |
| `CoinCollected(int)`    | Pickup         | RunSession                       |
| `RunTimerExpired`       | RunSession     | EnemySpawner (spawn boss)        |
| `RunEnded(result)`      | RunSession     | SaveManager (flush coins/rewards)|

---

## Third-party / Tools

| Tool           | Purpose                               |
|----------------|---------------------------------------|
| Godot MCP Pro  | AI-assisted editor control via Claude |
