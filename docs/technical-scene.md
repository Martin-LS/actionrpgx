# Technical Design Document — Scene & Architecture

> Part of the technical docs. See also `technical-systems.md` for data types, save format, crafting, combat systems, and more.
> Living document — architecture will evolve as systems are built and playtested.

## Architecture Overview

Godot 4.6, C#, Forward Plus renderer. Game world is 3D (CharacterBody3D, XZ movement plane, Y-up); characters and enemies are rendered as KayKit `.glb` models loaded at runtime as `PackedScene` child nodes. Camera is perspective, fixed ~60° from horizontal (Diablo 4-style), no player rotation, parented to player. UI is 2D (`Control` / `CanvasLayer`) as standard in Godot — unaffected by the 3D world. Scene composition over inheritance — each system is a self-contained scene or node that communicates via signals. Two save layers: a persistent save file (meta) and an in-memory run session (discarded on run end).

---

## Rendering & Camera

| Decision         | Choice                        | Rationale                                                                 |
|------------------|-------------------------------|---------------------------------------------------------------------------|
| World dimensions | 3D, XZ movement plane, Y-up   | Standard for top-down 3D; gravity, navmesh, and lighting all assume Y-up  |
| Camera type      | `Camera3D`, perspective       | Subtle depth like Diablo 4; fixed angle, no player rotation               |
| Camera angle     | Fixed ~60° from horizontal, offset (0, 350, 200) | Closer to overhead than classic 45° isometric; Diablo 4 reference. Set in `CameraFollow.cs` `_Ready()`. |
| Character render | Custom voxel `.glb` loaded as `PackedScene`, instanced as child `Node3D` | Player = `player.glb` (Mixamo stickman rig, scale 9 applied in code). Enemies = `kaykit_enemy_skeleton.glb` (scale 9 applied in code). The 9× scale bridges Blender's meter units to the game's centimeter-scale world — do not remove it. Model child rotates independently via `_model.LookAt()` — CharacterBody3D stays unrotated so camera doesn't spin. |
| Lighting         | Single `DirectionalLight3D` parented to `Camera3D` | Global main light source, moves with camera; one light for now |
| Projectiles      | Physical traveling objects    | Visible projectile travel is core to ARPG feel (not raycasts)              |
| Target aspect ratio | 16:9, PC primary           | All UI scenes must use Godot anchor presets (no absolute offsets) — makes ratio changes free later. Mobile not in scope. |
| Base viewport resolution | 1280×720               | Set in project.godot; Godot stretch mode scales to player's screen. |
| Stretch mode        | `canvas_items`                | Scales UI and world together; crisp at integer multiples of 720p.  |
| UI theme            | Custom Iron & Slate theme (`res://assets/ui/game_theme.tres`) | Hand-built `Theme` resource set via `gui/theme/custom`. Covers PanelContainer/TabContainer/Panel panel styleboxes (gold `#D4A017` border, Iron Black bg), Button states (normal/hover/pressed/disabled/focus), Label/LineEdit/PopupMenu/Tooltip styles. Default font: Exo 2. No per-scene theme overrides — all Control nodes inherit automatically. |
| Fonts               | Exo 2 (UI default), Cinzel (headings/titles), EB Garamond (body/lore), Almendra, Cinzel Decorative, Inter — all at `res://assets/fonts/` | Downloaded from Google Fonts as woff2; imported by Godot. Exo 2 set as `theme.default_font`. Bold variant (`Exo_2_2.woff2`) used for tooltip titles. |
| Floor               | Procedural connector-tile map (`DungeonGenerator.cs`) | 4–6 rooms (400×400 world units each) connected by corridors (90 wide, 160 long). Each room and corridor is a flat `BoxMesh` + matching `CollisionShape3D`. Invisible wall boxes on all room sides with corridor gap openings. Placeholder obstacle props (stumps, boulders, logs) scattered per non-spawn room. Player spawns at room 0 centre. After all geometry is placed, navmesh is baked synchronously using `NavigationServer3D.ParseSourceGeometryData` with DungeonMap as the explicit scan root, then `MapReady` is emitted deferred. |
| Pickup visuals      | Colored `BoxMesh` (10×10×10) | XP Shard = green, coin = yellow, health = red. Opaque to all systems. |

---

## Scene Flow

```
main_menu.tscn  →  account_screen.tscn  →  character_screen.tscn  →  main.tscn
                          ↕
                 character_create.tscn
```

`CharacterManager` (autoload) holds the selected character across scene transitions.

## Scene Layout

### `src/ui/main_menu.tscn`
```
MainMenu (Control)
└── VBox (VBoxContainer)
    ├── Title (Label)
    └── PlayButton (Button)
```

### `src/ui/account_screen.tscn`
Account-level hub. Always the first screen after Main Menu. Contains the character roster; designed to grow with additional account-level info. Character create is inline (no separate scene navigation).
```
AccountScreen (Control)
└── VBox (VBoxContainer)
    ├── HSplit (HSplitContainer)
    │   └── RightPanel
    │       └── TabContainer
    │           └── Characters tab
    │               └── RosterView ← character list + inline create panel
    └── BackButton (Button) → main_menu.tscn
```
On character selected: `CharacterManager.SelectCharacter(id)` → `character_screen.tscn`.

### `src/ui/character_create.tscn`
Dedicated character creation screen. Centred form; Create disabled until a name is entered.
```
CharacterCreate (Control)
└── VBox (VBoxContainer, centred)
    ├── TitleLabel (Label)
    ├── NameInput (LineEdit) ← enables ConfirmBtn when non-empty
    ├── WarriorBtn, RogueBtn, MageBtn (type selection)
    ├── ConfirmBtn (Button) ← creates character → account_screen.tscn
    └── CancelBtn (Button) → account_screen.tscn
```

### `src/ui/character_screen.tscn`
Full management hub for the selected character. Always has a character in context.
```
CharacterScreen (Control)
└── VBox (VBoxContainer)
    ├── BackButton (Button) ← "← Change Character" → account_screen.tscn
    └── TabContainer (expand fill)
        ├── Loadout tab (VBoxContainer, expand fill)
        │   └── LoadoutSplit (HBoxContainer, expand fill)
        │       ├── CharacterView (VBoxContainer, size_flags_h=ExpandFill) ← left column
        │       │   ├── HSplit (HBoxContainer)
        │       │   │   ├── InfoVBox (VBoxContainer, expand)
        │       │   │   │   └── NameLabel, TypeLabel, LevelLabel, StatsLabel
        │       │   │   └── GearPanel (VBoxContainer) ← shrink width
        │       │   │       ├── GearLabel ("— Equipment —")
        │       │   │       ├── WeaponSlot (VBoxContainer) → WeaponLabel + WeaponSlotButton (60×60)
        │       │   │       ├── HatSlot / HatLabel / HatSlotButton (same pattern)
        │       │   │       ├── BodySlot / BodyLabel / BodySlotButton (same pattern)
        │       │   │       └── RingSlot / RingLabel / RingSlotButton (same pattern)
        │       │   ├── SkillBar (HBoxContainer, size_flags_h=SHRINK_CENTER)  ← 5 slots (fully available)
        │       │   │   ├── SkillSlot1 (VBoxContainer) → SkillLabel1 + SkillSlotButton1 (60×60)
        │       │   │   ├── SkillSlot2 / SkillLabel2 / SkillSlotButton2 (same pattern)
        │       │   │   ├── SkillSlot3 / SkillLabel3 / SkillSlotButton3 (same pattern)
        │       │   │   ├── SkillSlot4 / SkillLabel4 / SkillSlotButton4 (same pattern)
        │       │   │   └── SkillSlot5 / SkillLabel5 / SkillSlotButton5 (same pattern)
        │       │   └── Buttons (HBoxContainer)
        │       │       └── StartRunButton (Button, expand fill)
        │       └── InventoryPanel (PanelContainer, min width 280) ← right column
        │           └── InventoryVBox (VBoxContainer)
        │               ├── InventoryTitle (Label) ← "Account Inventory"
        │               ├── InventoryInfo (Label) ← "Gear: N/50  Skills: M/50 ..."
        │               └── InventoryTabs (TabContainer, expand fill)
        │                   ├── Equipment tab
        │                   │   └── InventoryScroll (ScrollContainer)
        │                   │       └── InventoryGrid (GridContainer, 5 cols) ← 50 slots, OwnedGearInstances
        │                   ├── Skills tab
        │                   │   └── SkillsScroll (ScrollContainer)
        │                   │       └── SkillsGrid (GridContainer, 5 cols) ← 50 slots, OwnedSkillInstances
        │                   ├── Skill Augments tab
        │                   │   └── SkillAugmentsScroll (ScrollContainer)
        │                   │       └── SkillAugmentsGrid (GridContainer, 5 cols) ← 50 slots, OwnedSkillAugmentInstances
        │                   └── Equipment Augments tab
        │                       └── EquipmentAugmentsScroll (ScrollContainer)
        │                           └── EquipmentAugmentsGrid (GridContainer, 5 cols) ← 50 slots, OwnedEquipmentAugmentInstances
        └── Sigils tab ← empty; reserved for future sigil system
```
**Slot interaction model (CharacterScreen.cs):**
- **Right-click** a filled inventory item → equip/slot to first empty valid slot
- **Right-click** a filled equipped/slotted slot → un-equip/un-socket; item returns to inventory (blocked if inventory full)
- **Left-click** a filled inventory item → opens Modify panel (no Un-Socket / Un-Equip — item is not attached)
- **Left-click** a filled equipped/slotted slot → opens Modify panel with **Un-Equip** (gear) or **Un-Socket** (skill) button at top
- **Left-click** an empty gear slot → opens Equipment component overlay filtered to that slot type; craft skips type-pick step and jumps to subtype list; crafting or selecting auto-equips and opens Equipment Modify Panel
- **Left-click** an empty skill slot → opens Skill component overlay; craft auto-slots and opens Skill Modify Panel
- **Delete** is only available from the inventory view for **unequipped / unslotted / unsocketed** items — never from inside a Modify panel for an attached item. Equipment, skills, and augments can only be deleted if they are not attached/socketed.
- Modify panels use a two-column layout: left column = augment slot buttons; right panel = context-sensitive (empty slot → pick/craft augment; filled slot → **Upgrade**, **Re-roll**, **Un-Socket** to remove augment)

**Inventory grids:** 50 slots per tab (5 cols, scrollable), all always visible. Empty slots are dimmed. Items in the Skill Augments / Equipment Augments tabs can be permanently deleted only while unslotted/unsocketed — augments are socketed into items via the Modify panel. Capacity: `ProfileData.MaxInventory = 50` — counts only unequipped/unsocketed items. If `SelectedCharacter` is null on `_Ready`, redirects to `account_screen.tscn`.

### `src/ui/item_picker_panel.tscn`
Modal overlay opened from gear slot buttons **when the slot is empty** (left-click). Occupied gear slots open the Equipment Modify Panel directly (with **Un-Equip** button). `ItemPickerPanel` is never opened for an occupied slot.
```
ItemPickerPanel (Control, full-screen)
├── Dim (ColorRect, semi-transparent black)
└── Panel (PanelContainer, centered)
    └── VBox (VBoxContainer)
        ├── TitleLabel (Label)
        ├── Scroll (ScrollContainer)
        │   └── ItemList (VBoxContainer) ← buttons added at runtime, one per owned item in slot
        └── CloseButton (Button)
```

### `src/ui/skill_picker_panel.tscn`
Modal overlay opened from skill slot buttons **when the slot is empty** (left-click). Occupied skill slots open the Skill Modify Panel directly (with **Un-Socket** button).
```
SkillPickerPanel (Control, full-screen)
├── Dim (ColorRect, semi-transparent black)
└── Panel (PanelContainer, centered)
    └── VBox (VBoxContainer)
        ├── TitleLabel (Label) ← "Choose Skill"
        ├── Scroll (ScrollContainer)
        │   └── ItemList (VBoxContainer) ← buttons added at runtime, one per OwnedSkillInstances entry
        └── CloseButton (Button)
```

### `main.tscn` (run scene)
```
Main (Node)
├── Player (CharacterBody3D)   ← stats seeded from CharacterManager.SelectedCharacter
│   ├── CollisionShape3D
│   ├── Camera3D               ← perspective, ~60°, parented to player (follows automatically)
│   │   └── DirectionalLight3D ← global main light, moves with camera
│   └── Weapon (Node)
├── DungeonMap (Node3D)        ← procedural map generator (DungeonGenerator.cs)
│   └── NavigationRegion3D    ← added at runtime after synchronous navmesh bake; baked mesh covers all rooms/corridors/obstacles
├── WorldEnvironment
├── Hud (CanvasLayer)          ← health bar, Focus bar (blue), Focus Shield bar (light blue, all archetypes), XP bar, level, coin counter, run timer, skill bar
├── WorldHud (Node2D)          ← world-space overlay: health bars + floating damage numbers (see Core Systems)
├── RunSession (Node)          ← tracks elapsed time, enemy kill count; emits RunEnded(won, level, elapsed)
├── RunEndOverlay (CanvasLayer)← shown on RunEnded; returns to character_screen.tscn
└── WorldHud (Node2D)          ← world-space overlay: health bars + floating damage numbers (see Core Systems)
```
**Attack range indicator:** `PlayerController._Ready()` creates a `TorusMesh` ring (`MeshInstance3D`) at the player's feet — `OuterRadius` = effective weapon range, cyan `#00CCFF` at 50% alpha, unshaded, no depth test. Hidden by default. Toggled via the Debug Options panel (`OptionsOverlay` → Debug Options → Range Indicator checkbox).

**Target indicator:** Small torus spawned from `res://src/vfx/target_indicator.tscn`, added to scene root (not as a player child). Follows `LockedTarget.GlobalPosition` at Y=1. Visible only while a valid locked target exists.

**Aim reticle:** White torus (`OuterRadius=12, InnerRadius=8`) spawned programmatically, added to scene root. Tracks `TargetPosition` (cursor world projection) at Y=0.5. Visible only when at least one Position-targeting skill is slotted (`WeaponController.HasAnyPositionSkill()`).

**`LockedTarget` selection:** `PlayerController.UpdateLockedTarget()` runs every `_PhysicsProcess`. It selects the enemy whose `GlobalPosition` is closest to `TargetPosition` (the cursor's world-plane projection) — not the enemy closest to the player. This is cursor-driven, not proximity-driven.

**Range buff bonus:** `PlayerController` has `AddRangeBuffBonus(float tiles)` / `RemoveRangeBuffBonus(float tiles)` which accumulate a flat tile bonus (`_rangeBuffBonus`) added into `RecalculateEffectiveRange()`. Intended for active buff skills (e.g. Shout). Added on buff apply; removed on buff expire.

---

## Core Systems

| System            | Responsibility                                               | Path                      | Status |
|-------------------|--------------------------------------------------------------|---------------------------|--------|
| CharacterManager  | Autoload — load/save characters, hold selected character     | `res://src/character/`    | ✅ done |
| Player            | Input, movement, stat sheet, taking damage, Focus pool (CurrentFocus, regen, TrySpendFocus, ReserveFocus/UnreserveFocus, Focus Shield — all archetypes) | `res://src/player/`       | ✅ done |
| Weapon            | Skill firing — targeting nearest enemy, cooldown management; manual activation (Q E R F + Right Click for 5 slots) and per-slot auto-activate toggle | `res://src/weapon/` | ✅ done |
| DungeonGenerator  | Procedural map: 4–6 rooms connected by corridors, wall collision, obstacle scatter, player spawn. Bakes navmesh synchronously via NavigationServer3D with DungeonMap as explicit geometry root; emits `MapReady` (deferred) when done. | `res://src/world/` | ✅ done |
| Enemy             | State machine: Dormant → Idle (on MapReady) → Chasing (on aggro). `NavigationAgent3D` steers via navmesh path updated every 0.25s. Pack aggro via cluster wake-up broadcast. Lost-player threshold: `BalanceConfig.Enemies.LostPlayerDistanceTiles`. Taking damage, death, drops. | `res://src/enemies/` | ✅ done |
| RunSession        | Run timer, kill counter (connects to each enemy's `Died` signal at placement), win/lose detection, emits RunEnded signal | `res://src/run/`          | ✅ done |
| XpShard           | XP Shard pickup — auto-collected on contact                  | `res://src/xp/`           | ✅ done |
| EoT               | Effect over Time — apply, tick, expire on enemies            | `res://src/eot/`          | ✅ done |
| Hud               | Health bar, Focus bar (blue, below health), Focus Shield bar (light blue, below Focus — all archetypes), XP bar, level, coin counter, run timer | `res://src/hud/`          | ✅ done |
| WorldHud          | World-space overlay (`Node2D`). Projects 3D world positions to screen. Renders: (1) health bars above all enemies and the player (50×7px, 2s linger after death); (2) floating damage numbers — Bone White `#E8DCC8` for physical, Ice Shimmer `#B8D8E8` for magic, Gold `#D4A017` for crits — with D3-style scale-pop, rise, and fade animation. Connects to `PlayerController.DamageTaken` and `EnemyController.DamageTaken`. | `res://src/hud/`          | ✅ done |
| UpgradePicker     | (removed from scene — code kept dormant)                     | `res://src/ui/`           | ❌ removed |
| AccountScreen     | Account hub: character roster, crafting tab; navigates to CharacterScreen on select | `res://src/ui/` | ✅ done |
| CharacterCreate   | Dedicated create screen: name input + archetype choice       | `res://src/ui/`           | ✅ done |
| CharacterScreen   | Per-character hub: inventory, gear slots, tabs, Start Run    | `res://src/ui/`           | ✅ done |
| TooltipButton     | `Button` subclass — overrides `_MakeCustomTooltip()` to render a two-section tooltip: title line (gold `#D4A017`, bold Exo 2, 15px) and body (pale slate `#8AA0AE`, regular Exo 2, 13px). Used for all gear/skill/augment slot buttons and all dynamically created inventory buttons in CharacterScreen. | `res://src/ui/TooltipButton.cs` | ✅ done |
| ItemPickerPanel          | Modal picker for equipping/unequipping gear by slot                    | `res://src/ui/`       | ✅ done |
| SkillPickerPanel         | Modal picker for equipping skills into skill slots                     | `res://src/ui/`       | ✅ done |
| BalanceConfig     | Static constants — all tunable numbers (weapons, armour, skills, EoTs, enemies, drops, pickups, archetypes, level-up). Single source of truth; registries/controllers read from here, never hardcode. | `res://src/balance/` | ✅ done |
| ItemRegistry      | Static catalogue of all `ItemData` records                   | `res://src/items/`        | ✅ done |
| SkillRegistry     | Static catalogue of all `SkillData` records                  | `res://src/skills/`       | ✅ done |
| RecipeRegistry    | Static catalogue of all `RecipeData` records                 | `res://src/crafting/`     | ✅ done |
| RunEndOverlay     | Show win/die results, flush run to character, return to character screen | `res://src/ui/` | ✅ done |
| OptionsOverlay    | Autoload — global ESC/options modal; pauses tree when open; context-aware buttons (in-run: Resume/End Run; out-of-run: Close); blocked on main_menu.tscn | `res://src/ui/` | ✅ done |
| CoinPickup        | Coin drop (25% on enemy death) — reports to RunSession       | `res://src/meta/`         | ✅ done |
| MetaProgression   | Level bonuses (automatic +HP/+DMG per level); coin bank accumulates — spend mechanic TBD | `res://src/meta/`, `src/ui/` | ✅ done |
| HealthPickup      | Health drop (10% on enemy death) — heals player on contact   | `res://src/health/`       | ✅ done |

---

## Player Animation (AnimationTree)

All animations are Mixamo clips imported via `player.glb`. The `AnimationTree` is pre-built in the editor (Godot MCP Pro). C# sets the `AnimPlayer` path at runtime and drives state via `Travel()` / `Set()` — never modifies the tree structure.

### Animation Clips

| Clip | Duration | Loop | Use |
|---|---|---|---|
| `breathing_idle` | 298f / 9.97s | Yes | Standing still |
| `run` | 27f / 0.9s | Yes | Moving; root motion stripped in C# (Hips X/Z zeroed each key) |
| `melee_right_atack` | 68f / 2.3s | No | Melee delivery — right arm swing |
| `melee_left_atack` | 60f / 2.03s | No | Ranged delivery — left arm sweep (bow draw) |

### Tree Topology

```
AnimationNodeBlendTree (root)
├── movement (AnimationNodeStateMachine)   ← idle / run locomotion
│   ├── idle → breathing_idle clip
│   └── run  → run clip
├── shot_right (AnimationNodeOneShot) → right_ts (AnimationNodeTimeScale) → melee_right_atack
└── shot_left  (AnimationNodeOneShot) → melee_left_atack
```

Both OneShot nodes use a **partial-body filter** — lower-body bones are excluded so legs continue the movement animation underneath:

Filtered bones: `Foot_L_2`, `Foot_R_2`, `Hips_2`, `LowerLeg_L_2`, `LowerLeg_R_2`, `UpperLeg_L_2`, `UpperLeg_R_2`

(Mixamo FBX import appends `_2` to all bone names to avoid collisions with the scene root.)

### Dynamic Speed Scaling

`shot_right` has a `TimeScale` node (`right_ts`). At fire time, `OnSkillFired` sets:

```
parameters/right_ts/scale = 2.3 / skillCooldown
```

The melee animation always fills exactly one cooldown window. `shot_left` runs at native speed.

### C# Driver Contract

```csharp
// Locomotion — _PhysicsProcess
_moveSm.Travel("run");  // or "idle"
// _moveSm = animTree.Get("parameters/movement/playback").As<AnimationNodeStateMachinePlayback>()

// In-attack check (for facing logic) — _PhysicsProcess
bool inAttack = (bool)animTree.Get("parameters/shot_right/active")
             || (bool)animTree.Get("parameters/shot_left/active");

// Attack — OnSkillFired(int slotIndex, float cooldown, string delivery)
if (delivery == "Ranged")
    animTree.Set("parameters/shot_left/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
else
{
    animTree.Set("parameters/right_ts/scale", 2.3f / cooldown);
    animTree.Set("parameters/shot_right/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
}
```

No manual bool flags — state is always read from tree parameters.

### Facing During Attack

While a OneShot is active: character rotates to face the cursor's world-plane projection (TargetPosition) even while moving. While moving, not attacking: faces movement direction. While idle, not attacking: faces the cursor world position (not nearest enemy — the cursor drives facing at all times when not moving).

### Weapon Attachment

`BoneAttachment3D` on `Skeleton3D` at runtime:
- Bow (`bow_t1`) → `Hand_L` — left hand holds the bow; `shot_left` / left-arm sweep used
- Sword, wand → `Hand_R` — right hand; `shot_right` / right-arm swing used

Helper `FindBone()` checks `name` first, then `name + "_2"` (Mixamo deduplication suffix).

---

## Class Conventions (C#)

- **Namespaces:** `Godot1.<System>` (e.g. `Godot1.Player`, `Godot1.Combat`)
- **Node classes:** PascalCase — `PlayerController`, `EnemyBase`, `WeaponController`
- **Resource classes:** suffix `Data` — `EnemyData`, `ItemData`, `CharacterData`
- **Private fields:** `_camelCase`; public properties: `PascalCase`
- **Signals:** `[Signal]` delegate, past-tense — `HealthChanged`, `EnemyDied`, `LeveledUp`
- **Folder layout:** `src/<system>/` mirrors namespace

---

## Signals & Events

Systems communicate via signals only — no direct cross-system method calls.

| Signal                  | Emitter        | Receivers                        |
|-------------------------|----------------|----------------------------------|
| `HealthChanged(float)`      | Player         | HUD (formats to int for display), GameManager |
| `PlayerDied`                | Player         | RunSession (end run)             |
| `PlayerHit`                 | Player         | Hud — triggers red screen flash (alpha 0.3 → 0 over 0.15s, real-time tween) |
| `DamageTaken(float effectiveDamage, bool isMagic)` | PlayerController | WorldHud (spawns floating damage number above player) |
| `LeveledUp(int)`            | Player         | Hud (level display)              |
| `XpChanged(int currentXp, int xpToNextLevel)` | PlayerController | Hud (XP bar value + max) |
| `SkillFired(int slotIndex, float cooldown, string delivery)` | WeaponController | Hud skill bar (resets cooldown overlay); PlayerController (triggers attack animation via delivery string: "Melee"/"RangeMagic"/other → shot_right, "Ranged" → shot_left) |
| `Died(position)`            | EnemyController | RunSession (kill counter → win check); PlayerController.OnEnemyKilled() (player-side effects e.g. ghost step) |
| `DamageTaken(float effectiveDamage, bool isMagic, bool isCrit)` | EnemyController | WorldHud (spawns floating damage number above enemy; maintains a "dead bar" linger for 2s after death) |
| `CoinChanged(int)`          | RunSession     | Hud (coin counter)               |
| `MapReady`                  | DungeonGenerator | All pre-placed EnemyController instances (Dormant→Idle transition, begin aggro checks) |
| `RunEnded(bool won, int levelReached, float elapsed)` | RunSession | RunEndOverlay (show results, flush run to character) |
| `FocusChanged(float current, float max)` | PlayerController | HUD (Focus bar) |
| `ShieldChanged(float current, float max)` | PlayerController | HUD (Focus Shield bar — all archetypes) |

---

## Enemy Spawning Architecture

All enemies are pre-placed by `DungeonGenerator` at map generation time. There is no wave spawner.

### Map load sequence

```
DungeonGenerator._Ready() builds rooms, corridors, obstacles
→ Places 2–4 enemies per room from MapData.EnemyPool (random floor tiles, min spacing)
→ Computes proximity clusters (BFS over placed enemies)
→ Connects each enemy's Died signal → RunSession.OnEnemyDied()
→ NavigationServer3D.ParseSourceGeometryData(navMesh, data, dungeonMap)
→ NavigationServer3D.BakeFromSourceGeometryData(navMesh, data)  // synchronous
→ NavigationRegion3D added with baked mesh
→ CallDeferred(EmitSignal(MapReady))
→ All enemies transition Dormant → Idle; aggro checks begin
```

`MapReady` is emitted deferred so all `_Ready()` subscribers have connected before the signal fires.

### Proximity cluster system (runtime)

Clusters are not scene nodes — they are an emergent runtime grouping of idle enemies computed once at map load by `DungeonGenerator`.

**State machine per `EnemyController`:**

```
Dormant (all enemies at placement time)
  → MapReady fires → Idle
Idle
  → player enters aggro radius → wake self + connected cluster → Chasing
Chasing
  → player beyond LostPlayerDistanceTiles → Idle (re-scan proximity, rejoin/reform cluster)
```

**Wave-spawned enemies** are created directly in Chasing — they never enter Idle or participate in clustering. See `BalanceConfig.Enemies.LostPlayerDistanceTiles` (current: 30 tiles for wave-spawn; will be split into separate wave/pre-placed constants when pre-placed enemies are implemented).

### Enemy pool (`MapData.EnemyPool`)

Wave spawner draws from a typed pool per map:

```
EnemyPoolEntry { EnemyType, Count, Modifiers { ArmorBonus, HpBonus, SpeedBonus, DamageBonus } }
```

Count drives spawn weighting. Modifiers are applied to the enemy instance at spawn on top of base `EnemyData` values. v1: one entry, `skeleton`, count 1, all modifiers zero.

---

## Global Options Overlay

Unified modal options interface registered globally to handle input, pausing, and options access.

### 1. Autoload Registration
* **Scene Path:** `res://src/ui/options_overlay.tscn` (script: `OptionsOverlay.cs`).
* **Name:** `OptionsOverlay` registered in project autoloads.
* **Process Mode:** Set to `Always` (runs when SceneTree is paused).

### 2. Node & Scene Structure
* `OptionsOverlay` (CanvasLayer — Layer = 100 to draw over all game and HUD UI layers)
  * `GlobalButton` (Button — Small cog icon/text positioned in top-right: `AnchorLeft = 1.0, AnchorRight = 1.0`, offset `(-50, 10)`). Visible only when menu is closed and current scene is not `main_menu.tscn`.
  * `MenuControl` (Control — Stretch full-rect modal mask, mouse filter set to `Stop` to capture background clicks).
    * `PanelContainer` (Centered container, themed stylebox with gold border).
      * `VBoxContainer` (Separation = 8)
        * `Label` (Title: "Options", centered).
        * `HSeparator`
        * `ButtonList` (VBoxContainer — Cleared and rebuilt dynamically on menu opening).
          * In-Run: `"Resume"`, `"End Run"`, HSeparator, `"Debug Options"`.
          * Out-of-Run: `"Close"`, HSeparator, `"Debug Options"`.

### 3. Controller Lifecycle (`OptionsOverlay.cs`)
* Checks `GetTree().CurrentScene.SceneFilePath` to determine if welcome screen (`main_menu.tscn`) is active.
* Intercepts `ui_cancel` (Escape) in `_UnhandledInput(InputEvent @event)`.
* `Toggle()` updates:
  * `MenuControl.Visible = !MenuControl.Visible`
  * `GetTree().Paused = MenuControl.Visible`
  * `GlobalButton.Visible = !MenuControl.Visible && CurrentScene != main_menu.tscn`
  * On open (`MenuControl.Visible == true`): Clears `ButtonList` and builds buttons dynamically based on whether the current scene path is `"res://main.tscn"`.
  * On `"End Run"` press: Sets `GetTree().Paused = false`, calls `GetTree().ChangeSceneToFile("res://src/ui/character_screen.tscn")`, and closes the overlay.
  * On `"Debug Options"` press: Clears `ButtonList` and rebuilds as the debug panel (see §4 below). Only shown when `OS.IsDebugBuild()`.

### 4. Debug Options Panel (`OptionsOverlay.cs` — `RebuildDebugMenu()`)
* `"← Back"` button — calls `RebuildMenu()` to return to the main options list.
* **In-run only** (resolved via `GetTree().GetFirstNodeInGroup("player")` and `player.GetNode<WeaponController>("Weapon")`):
  * Range Indicator `CheckBox` — `player.SetRangeIndicatorVisible(bool)`. Initialised from current state.
  * God Mode `CheckBox` — `player.GodMode = bool`. Initialised from current state.
  * `Label` (`"Skill Auto-cast"`), then `HBoxContainer` of 5 `VBoxContainer` columns each containing a `Label` (`"1"`–`"5"`) and a `CheckBox` → `weapon.SetSlotAutoActivate(i, bool)`. Initialised via `weapon.GetSlotAutoActivate(i)` (getter added to `WeaponController`).
* **Always shown:**
  * `"Add Materials"` button — `CharacterManager.Profile?.AddMaterial("crafting_common", 500)`.
* The `"Debug Options"` button itself is hidden when `!OS.IsDebugBuild()`.
* **Replaces `DevOverlay`:** `DevOverlay.cs` and the `WorldHud` CanvasLayer node in `main.tscn` (which was the DevOverlay) are deleted as part of this change.

---

## Future Systems

*(No pending future systems — Focus was promoted to v1. See `technical-systems.md § Focus (Skill Resource)` for the complete runtime spec. This section will grow as new post-v1 systems are identified.)*

---

## Third-party / Tools

| Tool           | Purpose                               |
|----------------|---------------------------------------|
| Godot MCP Pro  | AI-assisted editor control via Claude |
| Themey         | Free open-source Godot 4 UI theme pack — installed at `res://addons/Themey/` but no longer the active theme. Superseded by the custom Iron & Slate theme. |
| Ravenmore Fantasy Icon Pack | Item slot icons (`res://assets/icons/items/`) — CC-BY 3.0, credit: ravenmore.itch.io |
