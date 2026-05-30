# Technical Design Document — godot1

> Living document — architecture will evolve as systems are built and playtested.

## Architecture Overview

Godot 4.6, C#, Forward Plus renderer. Game world is 3D (CharacterBody3D, XZ movement plane, Y-up); characters and enemies are rendered as KayKit `.glb` models loaded at runtime as `PackedScene` child nodes. Camera is perspective, fixed ~60° from horizontal (Diablo 4-style), no player rotation, parented to player. UI is 2D (`Control` / `CanvasLayer`) as standard in Godot — unaffected by the 3D world. Scene composition over inheritance — each system is a self-contained scene or node that communicates via signals. Two save layers: a persistent save file (meta) and an in-memory run session (discarded on run end).

---

## Rendering & Camera

| Decision         | Choice                        | Rationale                                                                 |
|------------------|-------------------------------|---------------------------------------------------------------------------|
| World dimensions | 3D, XZ movement plane, Y-up   | Standard for top-down 3D; gravity, navmesh, and lighting all assume Y-up  |
| Camera type      | `Camera3D`, perspective       | Subtle depth like Diablo 4; fixed angle, no player rotation               |
| Camera angle     | Fixed ~60° from horizontal    | Closer to overhead than classic 45° isometric; Diablo 4 reference         |
| Character render | KayKit `.glb` loaded as `PackedScene`, instanced as child `Node3D` | Player = Knight.glb (scale 12), enemies = Skeleton_Minion.glb (scale 10). Model child rotates independently via `_model.LookAt()` — CharacterBody3D stays unrotated so camera doesn't spin. |
| Lighting         | Single `DirectionalLight3D` parented to `Camera3D` | Global main light source, moves with camera; one light for now |
| Projectiles      | Physical traveling objects    | Visible projectile travel is core to ARPG feel (not raycasts)              |
| Target aspect ratio | 16:9, PC primary           | All UI scenes must use Godot anchor presets (no absolute offsets) — makes ratio changes free later. Mobile not in scope. |
| Base viewport resolution | 1280×720               | Set in project.godot; Godot stretch mode scales to player's screen. |
| Stretch mode        | `canvas_items`                | Scales UI and world together; crisp at integer multiples of 720p.  |
| UI theme            | Spacey (`res://addons/Themey/themes/spacey/spacey.tres`) | Free Themey pack; set as project-wide theme — all Control nodes inherit automatically. No per-scene theme overrides. |
| Floor               | Procedural checkerboard (`CheckerBackground.cs`) | Two-tone grey, generated at runtime — no external assets. |
| Pickup visuals      | Colored `BoxMesh` (10×10×10) | XP gem = green, coin = yellow, health = red. Opaque to all systems. |

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
    └── HSplit (HSplitContainer)
        ├── LeftPanel (PanelContainer, min width 280)
        │   └── LeftVBox (VBoxContainer)
        │       ├── InventoryTitle (Label)
        │       ├── InventoryInfo (Label)  ← "Gear: N/50  Skills: M/50  Coins: X  Common: Y"
        │       └── InventoryTabs (TabContainer, expand vertical)
        │           ├── Equipment tab
        │           │   └── InventoryScroll (ScrollContainer)
        │           │       └── InventoryGrid (GridContainer, 5 cols) ← 50 Button slots, runtime-populated from OwnedItemIds
        │           └── Skills tab
        │               └── SkillsScroll (ScrollContainer)
        │                   └── SkillsGrid (GridContainer, 5 cols) ← 50 Button slots, runtime-populated from OwnedSkillIds
        └── RightPanel (VBoxContainer)
            └── TabContainer
                ├── Equipment tab (VBoxContainer)
                │   └── CharacterView (VBoxContainer)
                │       ├── HSplit (HBoxContainer)
                │       │   ├── InfoVBox (VBoxContainer, expand) ← name/type/level/stats labels
                │       │   │   └── NameLabel, TypeLabel, LevelLabel, StatsLabel
                │       │   └── GearPanel (VBoxContainer) ← right column, shrink width
                │       │       ├── GearLabel ("— Equipment —")
                │       │       ├── WeaponSlot (VBoxContainer)
                │       │       │   ├── WeaponLabel ("Weapon")
                │       │       │   └── WeaponSlotButton (60×60, size_flags_h=shrink) ← popup (Unequip/Delete) if equipped; ItemPickerPanel if empty
                │       │       ├── ArmorSlot / ArmorLabel / ArmorSlotButton (same pattern)
                │       │       └── AccessorySlot / AccessoryLabel / AccessorySlotButton (same pattern)
                │       ├── SkillBar (HBoxContainer, size_flags_h=SHRINK_CENTER) ← centered row, below HSplit
                │       │   ├── SkillSlot1 (VBoxContainer) → SkillLabel1 ("Skill 1") + SkillSlotButton1 (60×60) ← popup (Unequip/Delete) if equipped; SkillPickerPanel if empty
                │       │   ├── SkillSlot2 / SkillLabel2 / SkillSlotButton2 (same pattern)
                │       │   └── SkillSlot3 / SkillLabel3 / SkillSlotButton3 (same pattern)
                │       └── Buttons (HBoxContainer)
                │           └── StartRunButton (Button, expand fill)
                ├── Crafting tab
                │   └── VBox ← runtime-populated gear recipe list (materials label + one Button per RecipeRegistry.ForType(Gear) entry)
                ├── Skill Crafting tab
                │   └── VBox ← runtime-populated skill recipe list (materials label + one Button per RecipeRegistry.ForType(Skill) entry)
                └── Sigils tab    ← empty; reserved for future sigil system
```
**Inventory grids:** Each tab has 50 slots (5 cols, scrollable), all always visible. Empty slots are dimmed. Clicking a filled slot opens a `PopupMenu` (Equip / Delete). Equip routes the item to the correct slot on the selected character, swapping any currently equipped item back to inventory. Capacity: `ProfileData.MaxInventory = 50` per tab — counts only unequipped items; equipped items live separately in `CharacterData.EquippedItems` / `SlottedSkillIds`. If `SelectedCharacter` is null on `_Ready`, redirects to `account_screen.tscn`.

**Skill slots:** Skill slot buttons open a `SkillPickerPanel` (parallel to `ItemPickerPanel`) when empty — filters `OwnedSkillIds` to skills compatible with that slot. Occupied skill slots use a `PopupMenu` (Unequip / Delete).

### `src/ui/item_picker_panel.tscn`
Modal overlay opened from gear slot buttons **when the slot is empty**. Occupied slots use a PopupMenu (Unequip / Delete) instead — ItemPickerPanel is never opened for an occupied slot.
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
Modal overlay opened from skill slot buttons **when the slot is empty**. Occupied skill slots use a PopupMenu (Unequip / Delete) instead.
```
SkillPickerPanel (Control, full-screen)
├── Dim (ColorRect, semi-transparent black)
└── Panel (PanelContainer, centered)
    └── VBox (VBoxContainer)
        ├── TitleLabel (Label) ← "Choose Skill"
        ├── Scroll (ScrollContainer)
        │   └── ItemList (VBoxContainer) ← buttons added at runtime, one per OwnedSkillIds entry
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
├── Background (Node3D)        ← procedural floor plane
├── WorldEnvironment
├── Hud (CanvasLayer)          ← health bar, XP bar, level, coin counter, run timer, skill bar
├── EnemySpawner (Node)
├── RunSession (Node)          ← tracks elapsed time; emits RunEnded(won, level, elapsed)
├── RunEndOverlay (CanvasLayer)← shown on RunEnded; returns to character_screen.tscn
├── PauseMenu (CanvasLayer)
└── DevOverlay (CanvasLayer)   ← debug only; hidden when OS.IsDebugBuild() is false
    ├── ToggleButton (Button)  ← anchored top-center; always visible in debug builds
    └── DevPanel (PanelContainer) ← hidden by default; toggled by ToggleButton
        └── VBox (VBoxContainer)
            ├── SpeedLabel (Label)   ← displays current speed value
            └── SpeedSlider (HSlider) ← live-edits PlayerController.Speed
```
**DevOverlay behaviour:** `_Ready()` checks `OS.IsDebugBuild()` — if false, hides (or frees) the entire overlay so no dev UI is visible in release exports. `ToggleButton` flips `DevPanel.Visible` on each press. The slider's `ValueChanged` signal writes directly to `PlayerController.Speed`.

---

## Core Systems

| System            | Responsibility                                               | Path                      | Status |
|-------------------|--------------------------------------------------------------|---------------------------|--------|
| CharacterManager  | Autoload — load/save characters, hold selected character     | `res://src/character/`    | ✅ done |
| Player            | Input, movement, stat sheet, taking damage                   | `res://src/player/`       | ✅ done |
| Weapon            | Auto-attack, targeting nearest enemy, firing on cooldown     | `res://src/weapon/`       | ✅ done |
| EnemySpawner      | Time-based wave scaling, spawning enemy scenes               | `res://src/enemies/`      | ✅ done |
| Enemy             | AI (chase), taking damage, death + XP gem spawning           | `res://src/enemies/`      | ✅ done |
| XpGem             | XP pickup — auto-collected on contact                        | `res://src/xp/`           | ✅ done |
| Hud               | Health bar, XP bar, level, coin counter, run timer           | `res://src/hud/`          | ✅ done |
| RunSession        | Run timer, win/lose detection, emits RunEnded signal         | `res://src/run/`          | ✅ done |
| UpgradePicker     | (removed from scene — code kept dormant)                     | `res://src/ui/`           | ❌ removed |
| AccountScreen     | Account hub: character roster, crafting tab; navigates to CharacterScreen on select | `res://src/ui/` | ✅ done |
| CharacterCreate   | Dedicated create screen: name input + archetype choice       | `res://src/ui/`           | ✅ done |
| CharacterScreen   | Per-character hub: inventory, gear slots, tabs, Start Run    | `res://src/ui/`           | ✅ done |
| ItemPickerPanel   | Modal picker for equipping/unequipping gear by slot          | `res://src/ui/`           | ✅ done |
| SkillPickerPanel  | Modal picker for equipping skills into skill slots           | `res://src/ui/`           | ✅ done |
| ItemRegistry      | Static catalogue of all `ItemData` records                   | `res://src/items/`        | ✅ done |
| SkillRegistry     | Static catalogue of all `SkillData` records                  | `res://src/skills/`       | ✅ done |
| RecipeRegistry    | Static catalogue of all `RecipeData` records                 | `res://src/crafting/`     | ✅ done |
| RunEndOverlay     | Show win/die results, flush run to character, return to character screen | `res://src/ui/` | ✅ done |
| CoinPickup        | Coin drop (25% on enemy death) — reports to RunSession       | `res://src/meta/`         | ✅ done |
| MetaProgression   | Level bonuses (automatic +HP/+DMG per level); coin bank accumulates — spend mechanic TBD | `res://src/meta/`, `src/ui/` | ✅ done |
| HealthPickup      | Health drop (10% on enemy death) — heals player on contact   | `res://src/health/`       | ✅ done |

---

## Data / Resource Types

| Class               | Kind        | Fields                                                         |
|---------------------|-------------|----------------------------------------------------------------|
| `ProfileData`       | Plain C#    | CoinBank, Materials (Dictionary\<string, int\> — material ID → quantity), OwnedItemIds (List\<string\>), OwnedSkillIds (List\<string\>), MaxInventory (const = 50) — applies separately to each list. Account-shared across all characters. Migration: old `craftingCurrency1` int field maps to `Materials["crafting_common"]` on load. |
| `CharacterData`     | Plain C#    | Id, Name, Type (enum), RunsCompleted, CurrentLevel, CurrentXp, EquippedItems (Dictionary\<string, string\>), SlottedSkillIds (List\<string\>). Archetype base stats (HP/Speed/Damage) computed inline in `BuildStatBlock()` — not stored as fields. |
| `CharacterType`     | C# enum     | Warrior, Rogue, Mage                                           |
| `ItemData`          | C# record   | Id, Name, Slot (enum), Tier (int), IconPath — plus slot-specific fields: `WeaponAffinity`, `SkillBonus (float)` for Weapon; `ArmorCategory`, `BonusHp`, `BonusSpeed`, `DamageReduction (float)` for Armor; `PhysicalResistance (float)` for Accessory. Unused fields default to zero. `BonusDamage` removed — weapons no longer contribute character damage. |
| `ItemSlot`          | C# enum     | Weapon, Armor, Accessory                                       |
| `SkillData`         | C# record   | Id, Name, Type (SkillType enum), Category (SkillCategory enum), Cooldown (float, seconds; 0 for Passive), Range (float), IconPath (string, default "") |
| `SkillType`         | C# enum     | Active, Passive                                                |
| `WeaponAffinity`    | C# enum     | None, Melee, RangedPhysical, RangedMagic                       |
| `ArmorCategory`     | C# enum     | None, Heavy, Medium, Light                                     |
| `SkillCategory`     | C# enum     | Melee, RangedPhysical, RangedMagic                             |
| `DamageType`        | C# enum     | Physical, Magic                                                |
| `ItemRegistry`      | Static class| `All` dict, `Get(id)`, `ForSlot(slot)` — 7 tier-1 starter items (sword, bow, wand, heavy/medium/light armor, accessory). `RandomDrop()` removed — items are not enemy drops. |
| `SkillRegistry`     | Static class| `All` dict, `Get(id)` — static catalog of all `SkillData` records. v1: 3 entries: `attack_melee` (Melee), `attack_ranged_physical` (RangedPhysical), `attack_ranged_magic` (RangedMagic). |
| `RecipeData`        | C# record   | Id, OutputItemId (string), RecipeType (enum), MaterialCosts (Dictionary\<string, int\> — material ID → quantity). `RecipeType.Gear` outputs to `OwnedItemIds`; `RecipeType.Skill` outputs to `OwnedSkillIds`. |
| `RecipeType`        | C# enum     | Gear, Skill                                                    |
| `CraftResult`       | C# enum     | Success, InsufficientMaterials, InventoryFull                  |
| `RecipeRegistry`    | Static class| `All` dict, `Get(id)`, `ForSlot(ItemSlot)`, `ForType(RecipeType)` — static catalog of all `RecipeData` records. v1: 7 tier-1 gear recipes + 3 skill recipes (Strike/Arrow/Bolt, 3× common each). |
| `EnemyData`         | C# record   | EnemyType (string), BaseSpeed, BaseHealth, ContactDamage, DamageInterval, PhysicalResistance (float), MagicResistance (float) |

---

## Save Layers

### Character Save (`user://save.json`)
Managed by `CharacterManager` autoload. Written on every create/delete/upgrade.
```json
{
  "profile": {
    "coinBank": 150,
    "materials": { "crafting_common": 30 },
    "ownedItemIds": ["bow_t1"],
    "ownedSkillIds": []
  },
  "characters": [
    {
      "id": "<guid>",
      "name": "Ironclad",
      "type": "Warrior",
      "runsCompleted": 3,
      "currentLevel": 7,
      "currentXp": 12,
      "equippedItems": { "Weapon": "sword_t1", "Armor": "heavy_armor_t1", "Accessory": "accessory_t1" },
      "slottedSkillIds": ["attack_melee", "attack_melee", "attack_melee"]
    }
  ]
}
```
`materials` holds all crafting material quantities keyed by material ID. Migration: on load, if the old `craftingCurrency1` key exists at the profile root, its value is moved into `materials["crafting_common"]`. `ownedItemIds` holds only **unequipped** gear; `ownedSkillIds` holds only **unequipped** skill items. Equipped items live exclusively in `equippedItems` / `slottedSkillIds`. `EquipItem` / `EquipSkill` move items out of the owned lists and swap old equipped items back in. `UnequipItem` / `UnequipSkill` return `false` (blocked) if the respective inventory is at capacity. Starter gear is seeded into `equippedItems` by `SeedStarterGear()`; starter skills are seeded into `slottedSkillIds` (all 3 slots) by `SeedStarterSkills()`. Both seed methods write directly to the character's slot fields, not to inventory. Fields default to empty if absent.

### Run Session (in-memory only)
Lives on the `RunSession` node. Discarded when the scene unloads. On run end, `CharacterManager.RecordRunCompletion(finalLevel, finalXp, coinsEarned)` writes the persistent state.
- Elapsed time
- Coins earned this run

Level and XP are NOT run-scoped — they live on `CharacterData` and are written back at run end.

### Future: Profile Envelope
If multi-user slots or cloud saves are ever needed, evaluate wrapping save data under a profile envelope. `CharacterManager` is the only entry point — the refactor scope is bounded (1 constant, a handful of callers).

---

## Weapon

Single weapon per character (v1). `WeaponController` manages:

- `BaseDamage (float)` — set at run start from `CharacterData` (archetype base + level bonuses + meta upgrades). The equipped weapon item does **not** contribute to base damage.
- `_slots[3]` — internal array of 3 slot states, each holding `{ SkillData Skill, float CooldownTimer, float SkillBonus }`. Each slot fires independently when its timer reaches 0. Empty slots (null Skill) are skipped.
- Per slot: `SkillBonus` is non-zero only when the weapon affinity matches the slot's skill category. `DamageType` per slot: `RangedMagic → Magic`, else `Physical`. `CooldownTimer` counts down independently from each slot's `Skill.Cooldown`.

Exposes: `SetDamage(float)`, `AddDamage(float)`, `SetSlot(int slotIndex, SkillData, float weaponSkillBonus)`.

`SetSlot` replaces `SetSkill` — called once per slot at run start. Stores `SkillData`, pre-computed `SkillBonus`, and seeds `CooldownTimer` to 0 so each slot fires immediately on the first frame.

Effective damage per shot: `BaseDamage + slot.SkillBonus`.

Emits: `SkillFired(int slotIndex, float cooldown)` when a slot fires — consumed by the HUD skill bar.

[TBD] Weapon upgrade path (stages, piercing, AoE) — deferred.

---

## Skill Bar (HUD)

An `HBoxContainer` anchored **bottom-center** of the HUD. **3 cells**, one per skill slot.

Each cell contains:
- Skill icon (placeholder if slot empty)
- A grey overlay — visible when the slot is on cooldown, hidden when ready
- A `ProgressBar` that **fills from bottom** (value 0 = just fired / empty, value 1.0 = ready to fire again)

`Hud._Ready()` wires `WeaponController.SkillFired` → `OnSkillFired(int slotIndex, float cooldown)`. On fire: set the matching cell's bar to 0, show grey overlay, begin filling. Filling is handled in `_Process` (bar value increments by `delta / cooldown` each frame). When bar reaches 1.0: hide grey overlay, stop incrementing.

The skill bar is the visual feedback loop for the auto-attack cadence — 3 independent timers give the player a read on all active slots without any input required.

---

## Crafting

Items are never dropped — they come exclusively from crafting. Each craftable item has one `RecipeData` entry in `RecipeRegistry`.

### Data shape

```
RecipeData(Id, OutputItemId, MaterialCosts: Dictionary<string, int>)
```

`MaterialCosts` keys are material IDs (`"crafting_common"`, `"crafting_rare"`, …). v1: every recipe costs only `"crafting_common"`. Quantities are TBD.

### `CharacterManager.CraftItem(string recipeId) → CraftResult`

```
recipe = RecipeRegistry.Get(recipeId)
if recipe == null → InsufficientMaterials

foreach (matId, qty) in recipe.MaterialCosts:
    if Profile.Materials.GetValueOrDefault(matId) < qty → InsufficientMaterials

if OwnedItemIds.Count >= MaxInventory → InventoryFull

foreach (matId, qty) in recipe.MaterialCosts:
    Profile.Materials[matId] -= qty

AddItemToInventory(recipe.OutputItemId)
Save()
return Success
```

### `CharacterManager.CraftSkill(string recipeId) → CraftResult`

Same logic as `CraftItem` but checks `OwnedSkillIds.Count >= MaxInventory` and adds to `OwnedSkillIds` on success.

### Crafting tab (CharacterScreen)

`ScrollContainer` / `VBoxContainer` recipe list populated at runtime from `RecipeRegistry.ForType(RecipeType.Gear)`. A `Label` at the top shows current material balances.

Each row is a `Button`:
- Text: `"[Item Name]  —  Common: N"`
- Disabled when: materials insufficient **or** inventory full
- On press: `CharacterManager.CraftItem(recipeId)`, then `Refresh()`

### Skill Crafting tab (CharacterScreen)

Parallel structure to the Crafting tab. Recipe list populated from `RecipeRegistry.ForType(RecipeType.Skill)`. On press: `CharacterManager.CraftSkill(recipeId)`, then `Refresh()`. Capacity check is against `OwnedSkillIds`.

### Material IDs

| ID | Display name | Source |
|----|--------------|--------|
| `crafting_common` | Common | 20% enemy drop (maps to old `craftingCurrency1`) |

Higher tiers (rare, exotic) will be added when their drop sources are designed.

---

## Damage Pipeline

### Player taking damage

`PlayerController.TakeDamage(float rawAmount, DamageType type)`

```
effectiveDamage = rawAmount × (1 − DamageReduction)
if type == Physical:
    effectiveDamage ×= (1 − PhysicalResistance)
CurrentHealth -= effectiveDamage
emit HealthChanged(CurrentHealth)
```

`DamageReduction` and `PhysicalResistance` are runtime stats on `PlayerController`, seeded at run start from the equipped armor and accessory respectively. All current chase enemies pass `DamageType.Physical`. Future ranged/magic enemies pass `DamageType.Magic`.

### Enemy taking damage

`EnemyController.TakeDamage(float rawAmount, DamageType type)`

```
effectiveDamage = rawAmount × (1 − resistance[type])
```

Resistance values per enemy type live in `EnemyData` (`PhysicalResistance`, `MagicResistance`).

### Stat seeding at run start

`PlayerController._Ready()` reads `CharacterManager.SelectedCharacter` and equipped items. After seeding, `GlobalPosition` is forced to `Vector3.Zero` — the map center — regardless of any saved node position in the scene file.

```
MaxHealth          = archetype base + level bonuses + meta upgrades + armor.BonusHp
Speed              = archetype base + meta upgrades + armor.BonusSpeed  // negative for Heavy
Damage             = archetype base + level bonuses + meta upgrades
DamageReduction    = armor.DamageReduction
PhysicalResistance = accessory.PhysicalResistance
for i in 0..2:
    skillId = character.SlottedSkillIds[i]   // null/empty = skip
    if skillId is set:
        skill = SkillRegistry.Get(skillId)
        bonus = (weapon.WeaponAffinity matches skill.Category) ? weapon.SkillBonus : 0
        WeaponController.SetSlot(i, skill, bonus)
```

---

## Enemy Spawner — Wave Scaling

Time-driven, no fixed waves. `EnemySpawner` recalculates each spawn:
- **Spawn rate** — starts immediately at t=0; interval = `InitialInterval / (1 + minutes * 0.5)`, clamped to `MinInterval = 0.3s`
- **Spawn position** — fixed-radius ring (350px) around the player; viewport-size-independent
- **Enemy types** — unlocked by elapsed minutes, chosen randomly from the available pool:

| Type     | Unlocks | Speed | HP | Damage |
|----------|---------|-------|----|--------|
| Standard | 0:00    | 75    | 1  | 10     |
| Runner   | 1:00    | 110   | 1  | 8      |
| Tank     | 2:00    | 45    | 1  | 18     |

All types receive a time-scaling bonus on top: `Speed += 10 * minutes`, `MaxHealth += 5 * (int)minutes`.

---

## Map Attributes

Each run is played on a map. Maps carry an attribute set that modifies run behaviour. The attribute set is small now and will grow.

| Attribute  | Type  | Effect                                                                     |
|------------|-------|----------------------------------------------------------------------------|
| `MapLevel` | `int` | On enemy death, `PlayerController.CollectXp(MapLevel)` is called directly — no pickup required. Stacks on top of any XP gem drop. |

`MapLevel` is passed into the run scene at startup (e.g. via `RunSession` or a `MapData` resource — exact wiring TBD when maps are selectable).

---

## Drop System

On enemy death, two XP sources fire independently:

1. **Kill XP** — `1 × MapLevel` XP granted instantly via `PlayerController.CollectXp()`
2. **XP gem drop** — physical `XpGem` scene spawned; player must walk over it (value = 5 XP)

Other drops hardcoded in `EnemyController.Die()`:

| Drop              | Chance | Notes                                                                  |
|-------------------|--------|------------------------------------------------------------------------|
| XP gem            | 100%   | Always dropped; value = 5 XP                                           |
| Coin              | 25%    | `CoinPickup` auto-collected; reports to `RunSession.AddCoin()`         |
| Health pack       | 10%    | `HealthPickup` heals player for 15 HP on contact                       |
| Crafting currency | 20%    | Instant; calls `RunSession.AddCraftingCurrency1(1)` — no pickup scene  |

> Planned: large XP gems, weighted drop tables via `EnemyData` resource.

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
| `LeveledUp(int)`            | Player         | Hud (level display)              |
| `SkillFired(int, float)`    | WeaponController | Hud skill bar (slotIndex, cooldown — resets cooldown overlay) |
| `Died(position)`            | Enemy          | (reserved — not yet wired)       |
| `CoinChanged(int)`          | RunSession     | Hud (coin counter)               |
| `RunTimerExpired`           | RunSession     | EnemySpawner (spawn boss)        |
| `RunEnded(result)`          | RunSession     | CharacterManager (flush coins/XP/level via `RecordRunCompletion`) |

---

## Third-party / Tools

| Tool           | Purpose                               |
|----------------|---------------------------------------|
| Godot MCP Pro  | AI-assisted editor control via Claude |
| Themey         | Free open-source Godot 4 UI theme pack — Spacey theme in use (`res://addons/Themey/`) |
| Ravenmore Fantasy Icon Pack | Item slot icons (`res://assets/icons/items/`) — CC-BY 3.0, credit: ravenmore.itch.io |
| KayKit (Kay Lousberg)       | Character models for testing — Knight (player), Skeleton_Minion (enemies) (`res://assets/models/`) — CC0, no attribution required |
