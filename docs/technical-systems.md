# Technical Design Document — Data, Systems & Crafting

> Part of the technical docs. See also `technical-scene.md` for scene layout, architecture overview, signals, and C# conventions.
> Living document — architecture will evolve as systems are built and playtested.

## Data / Resource Types

| Class               | Kind        | Fields                                                         |
|---------------------|-------------|----------------------------------------------------------------|
| `GearItemInstance`  | Plain C#    | Id (string, GUID), DefinitionId (string → `ItemRegistry`), Tier (int, 1–3), SocketedEquipmentAugmentIds (List\<string\> — instance GUIDs, one entry per augment slot; "" = empty; max length = augment slots for tier: 1/2/3). Authoritative tier for runtime stat scaling. |
| `EotInstance`       | Plain C#    | DefinitionId (string), TimeRemaining (float), TickTimer (float — damage EoTs only), **CritMultiplier (float, default 1.0f)** — set to the firing hit's crit multiplier when the applying hit was a critical hit; damage ticks use `DamagePerTick × CritMultiplier`. Non-damage EoTs ignore this field. |
| `SkillItemInstance` | Plain C#    | Id (string, GUID), DefinitionId (string → `SkillRegistry`), Tier (int, 1–3), SocketedSkillAugmentIds (List\<string\> — instance GUIDs, one entry per augment slot; "" = empty slot; max length = augment slots for tier: 1/2/3). |
| `ProfileData`       | Plain C#    | CoinBank, Materials (Dictionary\<string, int\>), OwnedGearInstances (List\<GearItemInstance\>), OwnedSkillInstances (List\<SkillItemInstance\>), OwnedSkillAugmentInstances (List\<SkillAugmentInstance\>), OwnedEquipmentAugmentInstances (List\<EquipmentAugmentInstance\>), MaxInventory (const = 50) — applies separately to each list. Account-shared. Migration: old `ownedItemIds`/`ownedSkillIds` string lists are wrapped into instances (new GUID, Tier = 1) on load. Old `augment`/`chainInstanceId` fields on skill instances are dropped on load. |
| `CharacterData`     | Plain C#    | Id, Name, Type (enum), RunsCompleted, CurrentLevel, CurrentXp, EquippedGear (Dictionary\<string, GearItemInstance\> — slot → full instance), SlottedSkillInstanceIds (List\<string\> — instance GUIDs; skill instances stay in `OwnedSkillInstances`), **SlotAutoActivate (List\<bool\> — one entry per skill slot; true = slot fires automatically every cooldown; false = player must manually trigger via keybind; serialized to save)**. Archetype base stats computed inline in `BuildStatBlock()` — applies primary stat growth formula (level × gain rate × conversion rate + item bonuses) before returning. |
| `CharacterType`     | C# enum     | Warrior, Rogue, Mage                                           |
| `StatId`            | C# enum     | MaxHp, Speed, PhysicalDamage, MagicDamage, PhysicalResistance, MagicResistance, MaxFocus, FocusRegen, CritChance, CritDamage, Evasion |
| `StatModifier`      | Plain C#    | StatId, ModifierType (FlatAdd), Value (float), ModifierSource (Level, Item) |
| `StatBlock`         | Plain C#    | Holds one effective value per `StatId`. `Get(StatId)` returns the final computed value — primary stat growth, conversion rates, and item bonuses all resolved by `BuildStatBlock()` before the block is returned, so callers always get ready-to-use effective values. |
| `ItemData`          | C# record   | Id, Name, Slot (enum), IconPath, Tags (string[] — equipment tags for augment compatibility; e.g. `["Melee"]` for Sword, `["Heavy"]` for heavy armour, `[]` for Accessory) — plus slot-specific fields: `WeaponRange (float, in tiles)`, `PreferredDelivery (string — "Melee" or "Ranged")` for Weapon; `ArmorCategory`, `BonusHp`, `BonusSpeed`, `DamageReduction (float)`, `RangeModifier (float, in tiles)` for Armor; `PhysicalResistance (float)` for Accessory. Unused fields default to zero. `Tier` removed — tier lives on `GearItemInstance`, not the definition. **Range fields are always in tiles** — multiply by `GameScale.TileSize` to get world units. |
| `ItemSlot`          | C# enum     | Weapon, Hat, Body, Ring                                        |
| `SkillData`         | C# record   | Id, Name, Type (SkillType enum), Tags (string[]) — e.g. `["Melee","Attack"]`, `["Ranged","Attack"]`, `["Ranged","Magic","Spell"]`. DamageType (DamageType enum, default Physical) — the damage type this skill deals; drives which damage pool fires and which enemy resistance applies. Cooldown (float, seconds — for Active: time between casts; for Channeled/Aura: damage tick interval; 0 for Passive), FocusCost (float — Active: flat spend per cast; Channeled: drain per second; Aura: flat Focus units to reserve while active; Passive: 0/ignored), Range (float — cast range for Position skills; damage/aggro radius for Self skills), **ZoneRadius (float, default 0f — damage radius at the landing/blast site; distinct from Range which is the cast range; 0 = fall back to weapon range)**, IconPath (string, default ""), Description (string, default ""), **Kind (SkillKind enum, default Normal — see `SkillKind` below)**, TargetingShape (SkillTargetingShape enum, default Self), WindUp (float seconds, default 0.0f — 0 = instant), DamagePattern (SkillDamagePattern enum, default Burst), StackLimit (int — −1 = not a zone skill; 1+ = max simultaneous active instances), ZoneTracksEntity (bool, default false), Duration (float seconds, default 0f — 0 = permanent; zone and summon skills must set this), TriggerRadius (float tiles, default 0f — 0 = not a trap; >0 = trap proximity detection radius), ArmTime (float seconds, default 0f — delay after placement before trap can trigger; ignored when TriggerRadius = 0), TriggerCount (int, default 0 — 0 = not a trap; 1 = single-trigger then despawn; >1 = multi-trigger), **InherentEotIds (string[]?, default null — EoT IDs always applied on hit at 100% chance regardless of augments; merged with augment-sourced EoT IDs at slot setup)**, **BasedOn (string?, default null — reserved parent prototype ID; unused at runtime)**. No Tier — tier lives on `SkillItemInstance`. No `BasePrototypeId` — prototype relationship is design-time documentation only; C# record required fields enforce new-field completeness at compile time. |
| `SkillKind`         | C# enum     | `Normal` (shipped skill), `Prototype` (v1 player-facing prototype — all v1 skills), `EngineProof` (internal test/legacy skill — retained for engine validation only, not surfaced to player). Replaces the earlier `IsPrototype: bool` design. |
| `SkillType`         | C# enum     | Active, Channeled, Aura, Passive                               |
| `SkillTargetingShape` | C# enum   | Self (effect fires from player position — no targeting input needed), Position (effect lands at locked target's world position on controller/keyboard; at cursor on mouse — no enemy required), Entity (must land on a specific enemy — blocked if no valid target; on mouse snaps to nearest enemy to cursor; on controller/keyboard uses locked target) |
| `SkillDamagePattern` | C# enum    | Burst (single hit fires on cast), Tick (damage repeats over duration at tick rate), None (debuff or utility only — no damage output) |
| `ArmorCategory`     | C# enum     | None, Heavy, Medium, Light                                     |
| `DamageType`        | C# enum     | Physical, Magic                                                |
| `ItemTier`          | C# static class (const ints) | Common = 1, Uncommon = 2, Rare = 3, Max = 3. `Label(int)` → display name. `BorderColor(int)` → Godot `Color`. Used for the rarity border colour on item slot buttons. |
| `BalanceConfig`     | Static class (nested) | Sections: `Weapons` (SwordRange/BowRange/WandRange), `Armour` (Heavy/Medium/Light — BonusHp, BonusSpeed, DamageReduction, RangeModifier per tier), `Accessories` (RingPhysicalResistance), `Skills` (cooldown + range per skill), `Eots` (ApplyChance, Duration, per-effect fields), `Enemies.Skeleton` + scaling consts + MeleeContactRange + `LostPlayerDistanceTiles` (float — current: 30 tiles for wave-spawned enemies, which must exceed their spawn radius of ~15.5 tiles or they return to idle immediately on spawn; will be split into `WaveSpawnLostPlayerTiles` and `PrePlacedLostPlayerTiles` when pre-placed enemies are implemented — intended 6–10 tile range applies to pre-placed only) + `EnemyAggroRadiusTiles` (float — idle enemy's individual aggro detection radius) + `ClusterProximityRadiusTiles` (float — max distance between two idle enemies to be considered in the same cluster), `Drops` (coin/health/crafting chances), `Pickups` (XpShardValue, HealthHealAmount), `Archetypes` (base derived stats per archetype — MaxHp/Speed/PhysDmg/MagDmg/MaxFocus/FocusRegen at level 1; primary stat gain rates and conversion rates live in `PrimaryStatGainRegistry` and `PrimaryStatConversions`), `LevelUp` (HpBonusPerLevel, DamageBonusPerLevel), `Focus` (per-archetype MaxFocus/RegenPerSec base values, ShieldFraction, ShieldRegenPerSec; per-skill FocusCost constants). All values are `const` — compile-time resolvable. |
| `ItemRegistry`      | Static class| `All` dict, `Get(id)`, `ForSlot(slot)` — 7 starter gear definitions. Definitions carry no tier — all instances start at Tier = 1 when crafted. |
| `SkillRegistry`     | Static class| `All` dict, `Get(id)` — v1: 4 renamed prototype entries: `entity_burst` (was `strike` — Active, Tags: `["Attack"]`, FocusCost: 5, IsPrototype: true), `self_channeled_tick` (was `cyclone` — Channeled, Tags: `["Melee","Attack"]`, FocusCost: 12/sec, Cooldown: 0.25s, IsPrototype: true), `self_burst` (was `nova` — Active, Tags: `["Attack"]`, FocusCost: 20, Cooldown: 1.5s, IsPrototype: true), `self_duration_tick` (was `damage_aura` — Active, Tags: `["Aura"]`, FocusCost: 15 flat, Cooldown: 2.0s after duration ends, IsPrototype: true). Plus 7 new targeting-system prototype entries (TBD — see todo). **Save migration:** `CharacterManager.Load()` must rewrite old definition IDs before any registry lookup: `strike` → `entity_burst`, `cyclone` → `self_channeled_tick`, `nova` → `self_burst`, `damage_aura` → `self_duration_tick`. Apply to all `DefinitionId` fields in `OwnedSkillInstances` and `SlottedSkillInstanceIds` resolution. |
| `RecipeData`        | C# record   | Id, OutputItemId (string — definition ID), RecipeType (enum), MaterialCosts (Dictionary\<string, int\>). Crafting always produces a new instance at Tier = 1. |
| `SkillAugmentData`  | C# record   | Id (string), Name (string), RequiredTags (string[]) — reserved for post-v1 use; all v1 skill augments use `[]` (universal — no gate). EotId (string?, nullable) — links augment to an EoT definition; null for augments with no timed effect. **ConflictGroup (string?, nullable) — augments sharing the same ConflictGroup cannot both be socketed into the same skill; enforcement is in `SocketSkillAugment`. Currently only `magic_damage` has ConflictGroup = `"damage_type"`.** No Effect field — behaviour dispatched by Id in code. v1: Slow (`[]`, EotId: `"slow"`), Critical Strike (`[]`, EotId: null — adds `BalanceConfig.SkillAugments.CritChance` as flat crit chance bonus for that skill slot), Magic Damage (`[]`, EotId: null, ConflictGroup: `"damage_type"` — overrides slot DamageType to Magic at fire time). Splash, pierce, burn are post-v1. |
| `SkillAugmentInstance` | Plain C# | Id (string, GUID), DefinitionId (string → `SkillAugmentRegistry`), **Tier (int, default 1 — augments can be upgraded via `UpgradeSkillAugment`; was "no tier" in earlier design)**, **TriggerChance (int, percentage, default 15 — rolled at craft time via `Random.Next(10, 31)`; re-rollable via `RerollSkillAugment`)**. Note: TriggerChance is stored and serialized but not yet wired into gameplay (crit strike uses `BalanceConfig.SkillAugments.CritChance`; slow proc uses `EotData.ApplyChance`). |
| `SkillAugmentRegistry` | Static class | `All` dict, `Get(id)`, `GetAll()` — static catalog of available Skill Augments. v1: 3 entries (magic_damage, slow, critical_strike). Splash, pierce, burn are post-v1. |
| `EquipmentAugmentData` | C# record | Id (string), Name (string). No tag gate — any Equipment Augment can socket into any equipment item regardless of category. No Effect field — behaviour dispatched by Id in `PlayerController`. v1: Retaliation, Fortify, Dash Reflex, Ghost Step, Mending (all universal). |
| `EquipmentAugmentInstance` | Plain C# | Id (string, GUID), DefinitionId (string → `EquipmentAugmentRegistry`), **Tier (int, default 1 — upgradeable via `UpgradeEquipmentAugment`)**, **TriggerChance (int, percentage, default 15 — rolled at craft, re-rollable via `RerollEquipmentAugment`; not currently used in effect dispatch)**. |
| `EquipmentAugmentRegistry` | Static class | `All` dict, `Get(id)`, `GetAll()` — static catalog of available Equipment Augments. v1: 5 entries (retaliation, fortify, dash_reflex, ghost_step, mending). |
| `RecipeType`        | C# enum     | Gear, Skill, SkillAugment, EquipmentAugment                   |
| `CraftResult`       | C# enum     | Success, InsufficientMaterials, InventoryFull                  |
| `RecipeRegistry`    | Static class| `All` dict, `Get(id)`, `ForSlot(ItemSlot)`, `ForType(RecipeType)` — v1: **10 gear recipes** (sword/bow/wand + 3 hat types + 3 body types + ring, 1× common each) + **11 skill recipes** (4 player-facing prototypes: entity_burst/self_channeled_tick/self_duration_tick/self_burst; plus 7 engine-proof: fixed_zone_burst, fixed_zone_tick, windup_burst, entity_debuff, tracked_tick, stackable_zone, triggered_zone_burst — all 1× common) + 3 SkillAugment recipes (magic_damage/slow/critical_strike, 1× common each) + 5 EquipmentAugment recipes (retaliation/fortify/dash_reflex/ghost_step/mending, 1× common each). |
| `EnemyData`         | C# record   | EnemyType (string), BaseSpeed, BaseHealth, ContactDamage, DamageInterval, PhysicalResistance (float), MagicResistance (float), ModelPath (string — GLB res:// path, defaults to enemy_generic.glb) |
| `EnemyPoolEntry`    | Plain C#    | EnemyType (string), Count (int — spawn weight), Modifiers: ArmorBonus (int), HpBonus (int), SpeedBonus (int), DamageBonus (int). Applied to enemy instance at spawn on top of base `EnemyData` values. v1: all modifier fields zero. |
| `EotData`           | C# record   | Id (string), Name (string), ApplyChance (float 0–1), Duration (float seconds), IsDamageEot (bool), TickRate (float seconds — ignored when IsDamageEot = false), DamagePerTick (float — ignored when IsDamageEot = false). All EoTs share these four properties; only damage EoTs use TickRate and DamagePerTick. |
| `EotInstance`       | Plain C#    | Runtime state per active EoT on an enemy: DefinitionId (string), TimeRemaining (float), TickTimer (float — only relevant for damage EoTs), CritMultiplier (float, default 1.0f — stamped with the applying hit's crit multiplier; damage ticks use DamagePerTick × CritMultiplier; non-damage EoTs ignore). Held in `EnemyController._activeEots (Dictionary<string, EotInstance>)` keyed by EotData.Id — enforces one instance per type. |
| `EotRegistry`       | Static class| `Get(id)`, `GetAll()` — static catalogue of all EoT definitions. v1: `slow` (IsDamageEot = false). `burn` is post-v1. |
| `PrimaryStatGainRegistry` | Static class | `GetGain(CharacterType, PrimaryStat) → float` — returns the archetype's per-level gain rate for Str/Dex/Int. Three entries per archetype (9 total). Owned by the Balancer. Lives in `src/character/`. |
| `PrimaryStatConversions` | Static class (const floats) | Fixed conversion rates from each primary stat to its derived stats — same for all archetypes. `StrToPhysDmg`, `StrToMaxHp`, `StrToPhysRes`, `StrToCritDmg`; `DexToCritChance`, `DexToEvasion`; `IntToMagDmg`, `IntToMaxFocus`, `IntToMagRes`, `IntToFocusRegen`. Owned by the Balancer. Lives in `src/character/`. |

---

## Save Layers

### Character Save (`user://save.json`)
Managed by `CharacterManager` autoload. Written on every create/delete/upgrade.
```json
{
  "profile": {
    "coinBank": 150,
    "materials": { "crafting_common": 30 },
    "ownedGearInstances": [
      { "id": "<guid>", "defId": "bow_t1", "tier": 1, "socketedEquipmentAugmentIds": ["", ""] }
    ],
    "ownedSkillInstances": [
      { "id": "<guid>", "defId": "strike", "tier": 1, "socketedSkillAugmentIds": ["<skill-augment-guid>", ""] }
    ],
    "ownedSkillAugmentInstances": [
      { "id": "<skill-augment-guid>", "defId": "slow" }
    ],
    "ownedEquipmentAugmentInstances": [
      { "id": "<equip-augment-guid>", "defId": "mending" }
    ]
  },
  "characters": [
    {
      "id": "<guid>",
      "name": "Ironclad",
      "type": "Warrior",
      "runsCompleted": 3,
      "currentLevel": 7,
      "currentXp": 12,
      "equippedGear": {
        "Weapon": { "id": "<guid>", "defId": "sword_t1", "tier": 1, "socketedEquipmentAugmentIds": [""] },
        "Hat":    { "id": "<guid>", "defId": "hat_heavy_t1", "tier": 1, "socketedEquipmentAugmentIds": [""] },
        "Body":   { "id": "<guid>", "defId": "body_heavy_t1", "tier": 1, "socketedEquipmentAugmentIds": [""] },
        "Ring":   { "id": "<guid>", "defId": "ring_t1", "tier": 1, "socketedEquipmentAugmentIds": [""] }
      },
      "slottedSkillInstanceIds": ["<skill-instance-guid>", "<skill-instance-guid>", "<skill-instance-guid>"]
    }
  ]
}
```
Note: empty augment slots serialize as `""` inside `socketedSkillAugmentIds` and `socketedEquipmentAugmentIds`. Load methods treat `""` entries as empty slots. Old saves carrying `socketedSupportInstanceIds` are migrated to `socketedSkillAugmentIds` on load. Old `augment`/`chainInstanceId` fields are dropped on migration.

`ownedGearInstances` and `ownedSkillInstances` hold only **unequipped** instances. Equipped gear lives as full `GearItemInstance` objects in `equippedGear` (nested dicts on disk). Slotted skills remain in `ownedSkillInstances`; `slottedSkillInstanceIds` holds GUIDs referencing them. `EquipItem` moves a gear instance from `OwnedGearInstances` to `EquippedGear`, swapping the old equipped instance back. `UnequipItem` / `UnequipSkillSlot` blocked if respective inventory is at capacity.

**Migration:** On load, if old `ownedItemIds` (string list) is present, each entry is wrapped into a `GearItemInstance` with a fresh GUID and Tier = 1. Same for `ownedSkillIds` → `SkillItemInstance`. Old `equippedItems` (slot → definition ID) wraps each into a `GearItemInstance` and sets the slot directly. Old `craftingCurrency1` int → `materials["crafting_common"]` migration also handled. Migration runs once on first load.

Starter gear and starter skills are both seeded in `SeedStarterGear()`. Starter gear is archetype-dependent — Warrior gets sword + heavy hat + heavy body + ring; Rogue gets bow + medium hat + medium body + ring; Mage gets wand + medium hat + medium body + ring. Starter gear writes directly to `EquippedGear`. The single starter `SkillItemInstance` (always `entity_burst`) goes into `OwnedSkillInstances`, referenced 3× in `SlottedSkillInstanceIds`. Starter skills have no pre-socketed augments (matching the GDD starter loadouts).

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

**Damage model — weapon is the root of all damage.** `PlayerController` computes damage at run start (and on level-up) via `ApplyWeaponDamage()` and pushes the results into `WeaponController`. Both pools are always pre-computed; each slot selects its pool from `skill.DamageType` at fire time:

```
// Both pools pre-computed from StatBlock — skill.DamageType selects the pool at fire time
// Damage increase per level is implicit: primary stat growth raises PhysicalDamage/MagicDamage multipliers
physDmg  = weapon.BaseDamage × statBlock.Get(PhysicalDamage) × (1 + weapon.DamageBonus)
magicDmg = weapon.BaseDamage × statBlock.Get(MagicDamage)    × (1 + weapon.DamageBonus)
```

`WeaponController` receives two calls from `PlayerController.ApplyWeaponDamage()`:
- `SetDamage(physDmg, magicDmg)` — sets `_physicalDamage` and `_magicDamage`
- `SetGlobalCritChance(float)` — aggregated flat crit chance from all non-skill sources (Dex stat baseline + Bow weapon identity bonus + future equipment augments, rings). Replaces the old `SetWeaponCritBonus()`.

**Crit stat architecture — two pools, globally aggregated:**

Crit Chance and Crit Multiplier are universal stats. `PlayerController` is responsible for aggregating all contributions before passing them into `WeaponController`:

| Pool | Sources | How passed |
|---|---|---|
| Global Crit Chance | **Dexterity stat** (primary baseline) + Bow weapon identity bonus + equipment augments + ring stats | `SetGlobalCritChance(float)` — once per run start / level-up |
| Per-slot Crit Chance | Critical Strike skill augment on that skill | `SetSlot()` `critChanceBonus` float parameter |
| Global Crit Multiplier | **Strength stat** (`CritDamage`) — fixed 1.5× in v1; grows with Str investment post-v1 | `SetCritMultiplier(float)` — once per run start / level-up |

At fire time: `critChance = _globalCritChance + slot.CritChanceBonus`. If `critChance > 0` and roll succeeds: `baseDmg *= _critMultiplier`.

`SkillSlot.HasCriticalStrike (bool)` is replaced by `SkillSlot.CritChanceBonus (float)` — the aggregated float from that skill's augments. This removes the bool flag and makes adding future crit-granting skill augments a parameter change, not a new flag.

**Slot state:** `_slots[5]` — internal array of 5 slot states:
```
{ SkillData Skill, float CooldownTimer, List<string> EotIds,
  bool HasMagicDamage, Items.DamageType EffectiveDamageType,
  float CritChanceBonus, bool AutoActivate,
  bool IsChanneling, float DurationTimer,
  List<Node3D> ActiveZones }
```
`HasMagicDamage` — set when the Magic Damage augment is socketed; overrides `EffectiveDamageType` to Magic at fire time. `AutoActivate` — mirrors `CharacterData.SlotAutoActivate[i]`; when true the slot fires every cooldown automatically; when false the slot only fires on explicit `TryFireSlot(i)` call. `HasSplash` and `HasPierce` are post-v1 (removed until Splash/Pierce augments are implemented). `AuraActive` — whether the Aura toggle is currently on; `AuraReserved` — absolute Focus units locked while active (set at toggle-on time from `slot.Skill.FocusCost`). Aura slots are excluded from the `AutoActivate` path — they only fire via `TryFireSlot` toggle. Each slot fires independently. Empty slots (null Skill) are skipped.

Exposes: `SetDamage(float, float)`, `SetGlobalCritChance(float)`, `SetCritMultiplier(float)`, `SetSlot(int, SkillData, ...)`. `SetBaseDamageType` removed — pool selection is per-slot at fire time via `slot.Skill.DamageType`.

`SetSlot` is called once per slot at run start. `SetGlobalCritChance` and `SetCritMultiplier` are called once at run start and again on level-up (same cadence as `SetDamage`).

Emits: `SkillFired(int slotIndex, float cooldown, string delivery)` — consumed by HUD skill bar (cooldown overlay) and `PlayerController` (`OnSkillFired` selects animation: `"Ranged"` → `shot_left` OneShot, anything else → `shot_right` OneShot + TimeScale).

**Delivery strings — three values, not two.** Spec previously listed "Melee" and "Ranged". A third value `"RangeMagic"` is used by the Wand weapon (`PreferredDelivery = "RangeMagic"`). All three trigger the `shot_right` animation — `shot_left` fires only for `"Ranged"`. The distinction exists so VFX/audio can differentiate a magic bolt from a physical arrow in the future; no gameplay difference yet.

**Animation/VFX is delivery-driven, not skill-identity-driven.** All skills that share the same `SkillType` emit the same `delivery` string and play the same animation. Non-prototype skills cloned from a prototype inherit this automatically — no per-skill animation mapping exists. The cyclone VFX is the only exception: it is hardcoded to `SkillType.Channeled` via `IsAnySlotChanneling()`, so any Channeled skill (not just Cyclone) will show the spinning ring.

[TBD] Weapon upgrade path (stages, piercing, AoE) — deferred.

---

## Focus (Skill Resource)

All archetypes spend Focus to fire skills. `PlayerController` owns the pool; `WeaponController` deducts or reserves on fire.

### Runtime state (PlayerController)

```
float CurrentFocus      // initialized to MaxFocus at run start
float _maxFocus         // seeded from StatId.MaxFocus via statBlock
float _focusRegen       // seeded from StatId.FocusRegen via statBlock
float _totalReserved    // sum of all active Aura reservation amounts; starts at 0
```

Regen in `_PhysicsProcess`: `CurrentFocus = Min(CurrentFocus + _focusRegen × delta, _maxFocus)`. Emits `FocusChanged(CurrentFocus, _maxFocus)` each tick.

Available Focus (amount skills may spend or draw against): `Max(0, CurrentFocus - _totalReserved)`.

### Methods (called by WeaponController)

```
float GetAvailableFocus()  →  Max(0, CurrentFocus - _totalReserved)

bool TrySpendFocus(float amount):
    if GetAvailableFocus() >= amount → CurrentFocus -= amount; emit FocusChanged; return true
    return false

void ReserveFocus(float absoluteAmount)   → _totalReserved += amount; emit FocusChanged
void UnreserveFocus(float absoluteAmount) → _totalReserved = Max(0, _totalReserved - amount); emit FocusChanged
```

### Firing guards by SkillType (WeaponController)

| SkillType | Guard | On fire |
|---|---|---|
| Active | `GetAvailableFocus() >= FocusCost` | `TrySpendFocus(FocusCost)` |
| Channeled | `IsChanneling == true` + `GetAvailableFocus() >= FocusCost × Cooldown` | `TrySpendFocus(FocusCost × Cooldown)` per tick |
| Aura (toggle on) | `GetAvailableFocus() >= FocusCost` | `ReserveFocus(FocusCost)` — no per-tick spend |
| Aura (toggle off) | — | `UnreserveFocus(slot.AuraReserved)` |
| Passive | — | — |

Channeled `FocusCost` is drain/sec; `Cooldown` is tick interval — so `FocusCost × Cooldown` is drain per tick. Aura `FocusCost` is a fraction (0.0–1.0); multiply by `_maxFocus` to get the absolute reservation amount.

Auras do **not** auto-deactivate at 0 Focus — the reservation is committed at toggle time. Active and Channeled skip the tick when insufficient Focus is available.

**Channeled `IsChanneling` state:**
- Manual: `TryFireSlot` sets `IsChanneling = true`; `ReleaseSlot` (key-up) sets it false.
- AutoActivate: `_PhysicsProcess` auto-sets `IsChanneling = FindNearestEnemy(_range) != null` each frame — starts channeling when an enemy enters range, stops when none remain.

**Channeled exclusivity:** while any slot has `IsChanneling = true`, `ProcessActiveSlot` returns immediately. Active skills deal no damage and fire no animations. Aura slots are unaffected — they bypass the `active` check and process independently via `ProcessAuraSlot`.

**Aura slot state additions (WeaponController `_slots` array):**

```
bool  AuraActive    // is the toggle currently on?
float AuraReserved  // absolute Focus units reserved while active
```

### Focus Shield

All archetypes. A separate damage-absorbing pool seeded at run start.

```
_maxFocusShield     = _maxFocus × BalanceConfig.Focus.ShieldFraction   // 30%
_currentFocusShield = _maxFocusShield
```

**Damage intercept** in `PlayerController.TakeDamage()` — after resistance calculation, before HP deduction:

```
absorbed = Min(_currentFocusShield, effectiveDamage)
_currentFocusShield -= absorbed
effectiveDamage -= absorbed
emit ShieldChanged(_currentFocusShield, _maxFocusShield)
```

**Shield regen** in `_PhysicsProcess`:

```
if _currentFocusShield < _maxFocusShield:
    _currentFocusShield = Min(_currentFocusShield + ShieldRegenPerSec × delta, _maxFocusShield)
emit ShieldChanged(_currentFocusShield, _maxFocusShield)
```

`ShieldRegenPerSec` is a `BalanceConfig.Focus` constant. No `StatId` entry yet — added when augments invest into shield regen.

**MaxFocus changes mid-run** (future — not v1): ceiling = `_maxFocus × ShieldFraction`, recalculated instantly. Current shield clamped to new ceiling on decrease; increases do not auto-fill.

### Archetype starting values (BalanceConfig.Focus)

| Archetype | MaxFocus base | RegenPerSec base | Shield base |
|---|---|---|---|
| Warrior | 80 | 12 | 24 (30% of 80) |
| Rogue | 100 | 15 | 30 (30% of 100) |
| Mage | 150 | 10 | 45 (30% of 150) |

### HUD

`FocusChanged(float, float)` → Focus bar, blue, below the health bar.  
`ShieldChanged(float, float)` → Focus Shield bar, light blue, below the Focus bar. Visible for all archetypes.

---

## Dodge Roll

Active tactical roll available to the player character at all times.

### Design Details
* **Cost:** Free (no Focus cost).
* **Speed:** Overrides movement velocity with a 2.0x multiplier based on base movement speed.
* **Duration:** 0.35 seconds.
* **Cooldown:** 1.0 second, starts immediately upon roll initiation.
* **Direction:** Current WASD vector. If standing still, rolls in the direction the character is currently facing.
* **Invincibility (I-frames):** Grants full damage immunity for the duration.
* **Animation:** Bypassed in v1 (characters slide). Model instantly snaps to face the roll direction and locks rotation until the roll ends.
* **Skill Cancellation:** Cancels and aborts any active or channeled skills on activation.

### Technical Flow
1. **Input Detection:** `PlayerController._UnhandledInput` checks for the `Key.Space` press event.
2. **Initiation (`TryStartDodge`):**
   * Guarded by `_isDodging || _dodgeCooldownTimer > 0`.
   * Aborts `shot_right` and `shot_left` OneShot nodes on the `AnimationTree`.
   * Invokes `WeaponController.CancelActiveSkills()` to disable skill channeling.
   * Resolves movement vector (or facing vector derived from `_yaw` if WASD input is zero) as `_dodgeDirection`.
   * Instantly snaps `_model.Rotation` (and `_yaw`) to `_dodgeDirection`.
   * Sets `_isDodging = true`, `_dodgeTimer = 0.35f`, and `_dodgeCooldownTimer = 1.0f`.
3. **Movement (`_PhysicsProcess`):**
   * Ticks down `_dodgeTimer` and `_dodgeCooldownTimer`.
   * If `_isDodging` is active, overrides `Velocity` to `_dodgeDirection * Speed * BalanceConfig.Dodge.SpeedMultiplier`.
   * Bypasses character rotation logic and movement state machine transitions during the roll.
4. **Damage Mitigation (`TakeDamage`):**
   * Early-returns (ignores all damage and hitstop feedback) if `_isDodging` is true.
5. **UI Integration:**
   * `PlayerController` declares and emits a `DodgeFired(float cooldown)` signal when a dodge roll is successfully initiated.
   * `Hud.cs` connects to `DodgeFired` at initialization.
   * `Hud.cs` creates a separate `SkillCell _dodgeCell` for the Dodge cooldown slot.
   * In `BuildSkillBar()`, after placing the 5 skill cells, a spacer of 16px is added to the `HBoxContainer`, followed by the Dodge slot cell (a `PanelContainer` with a `ProgressBar` and a centered `Label` overlay showing `"Space"`).
   * The `_Process` loop ticks `_dodgeCell` timers and updates its `ProgressBar` fill from bottom to top as the cooldown recovers.

---

## Skill Bar (HUD)

An `HBoxContainer` anchored **bottom-center** of the HUD. **5 cells visible in v1** (mapped to Q E R F + Right Click).

Each cell contains:
- Skill icon (placeholder if slot empty)
- A grey overlay — visible when the slot is on cooldown, hidden when ready
- A `ProgressBar` that **fills from bottom** (value 0 = just fired / empty, value 1.0 = ready to fire again)

`Hud._Ready()` wires `WeaponController.SkillFired` → `OnSkillFired(int slotIndex, float cooldown)`. On fire: set the matching cell's bar to 0, show grey overlay, begin filling. Filling is handled in `_Process` (bar value increments by `delta / cooldown` each frame). When bar reaches 1.0: hide grey overlay, stop incrementing.

The skill bar is the visual feedback loop for the skill cadence — 5 independent timers give the player a read on all active slots whether firing automatically or triggered manually.

---

## Crafting

Items are never dropped — they come exclusively from crafting. Each craftable item has one `RecipeData` entry in `RecipeRegistry`.

### Data shape

```
RecipeData(Id, OutputItemId, MaterialCosts: Dictionary<string, int>)
```

`MaterialCosts` keys are material IDs (`"crafting_common"`, `"crafting_rare"`, …). v1: every recipe costs `{ "crafting_common": 1 }`.

### `CharacterManager.CraftGearItem(string recipeId) → CraftResult`

```
recipe = RecipeRegistry.Get(recipeId)
if recipe == null → InsufficientMaterials
foreach (matId, qty): if insufficient → InsufficientMaterials
if OwnedGearInstances.Count >= MaxInventory → InventoryFull
deduct materials
OwnedGearInstances.Add(new GearItemInstance { Id = NewGuid(), DefinitionId = recipe.OutputItemId, Tier = 1 })
Save(); return Success
```

### `CharacterManager.CraftSkillItem(string recipeId) → CraftResult`

Same pattern, adds `new SkillItemInstance { Id = NewGuid(), DefinitionId = recipe.OutputItemId, Tier = 1, SocketedSkillAugmentIds = [] }` to `OwnedSkillInstances`.

### `CharacterManager.UpgradeGearItem(string instanceId) → CraftResult`

```
instance = OwnedGearInstances.Find(id) ?? equippedGear lookup
if instance == null → InsufficientMaterials
if instance.Tier >= MaxTier (3) → InsufficientMaterials  (already max)
if Profile.Materials["crafting_common"] < 1 → InsufficientMaterials
Profile.Materials["crafting_common"] -= 1
instance.Tier++
Save(); return Success
```

### `CharacterManager.UpgradeSkillItem(string instanceId) → CraftResult`

Same pattern as `UpgradeGearItem`, searches both `OwnedSkillInstances` and slotted skill instances.

### `CharacterManager.CraftSkillAugmentItem(string recipeId) → CraftResult`

Same pattern as `CraftSkillItem`. Adds `new SkillAugmentInstance { Id = NewGuid(), DefinitionId = recipe.OutputItemId }` to `OwnedSkillAugmentInstances`.

### `CharacterManager.SocketSkillAugment(string skillInstanceId, int slotIndex, string augmentInstanceId) → CraftResult`

```
skill = find SkillItemInstance by id (owned or slotted)
augment = OwnedSkillAugmentInstances.Find(augmentInstanceId)
if skill == null || augment == null → InsufficientMaterials
if slotIndex >= MaxAugmentSlots(skill.Tier) → InsufficientMaterials
// No tag gate — any skill augment sockets into any skill (see GDD § Skill Augments)
skill.SocketedSkillAugmentIds[slotIndex] = augmentInstanceId
Save(); return Success
```

### `CharacterManager.RemoveSkillAugment(string skillInstanceId, int slotIndex)`

Sets `skill.SocketedSkillAugmentIds[slotIndex] = ""`. Free. Save().

### `CharacterManager.CraftEquipmentAugmentItem(string recipeId) → CraftResult`

Same pattern as `CraftSkillAugmentItem`. Adds `new EquipmentAugmentInstance { Id = NewGuid(), DefinitionId = recipe.OutputItemId }` to `OwnedEquipmentAugmentInstances`.

### `CharacterManager.SocketSkillAugment` — duplicate augment prevention

Before writing the slot, `SocketSkillAugment` checks all *other* slots on the same skill: if any existing socketed augment shares the same `DefinitionId` as the one being inserted, `InsufficientMaterials` is returned. Same check applies in `SocketEquipmentAugment`. This prevents two copies of the same augment on a single item.

In addition, `SocketSkillAugment` enforces `ConflictGroup`: if the new augment has a `ConflictGroup` and any other socketed augment on that skill shares the same group, the socket is blocked.

### `CharacterManager.SocketEquipmentAugment(string gearInstanceId, int slotIndex, string augmentInstanceId) → CraftResult`

```
gear = find GearItemInstance by id (owned or equipped)
augment = OwnedEquipmentAugmentInstances.Find(augmentInstanceId)
if gear == null || augment == null → InsufficientMaterials
if slotIndex >= MaxAugmentSlots(gear.Tier) → InsufficientMaterials
augmentDef = EquipmentAugmentRegistry.Get(augment.DefinitionId)
// No tag gate in v1 — any equipment augment sockets into any item
// Duplicate check: if any other slot already has same DefinitionId → InsufficientMaterials
gear.SocketedEquipmentAugmentIds[slotIndex] = augmentInstanceId
Save(); return Success
```

### `CharacterManager.RemoveEquipmentAugment(string gearInstanceId, int slotIndex)`

Sets `gear.SocketedEquipmentAugmentIds[slotIndex] = ""`. Free. Save().

### Modify panel (CharacterScreen)

Opened via left-click → **Modify** on any item (inventory or equipped slot). Implemented as a dynamically-built modal overlay — no pre-authored scene.

Structure (built in `ShowGearModifyPanel` / `ShowSkillModifyPanel`):
- `ColorRect` (full-screen, semi-transparent, `MouseFilter=Stop`) as the overlay
- `PanelContainer` (Iron & Slate styled, centered via `Anchor 0.5/GrowBoth`, min width 380px)
  - Title: item name + tier label; **✕** button closes the overlay
  - `HSeparator`
  - **Upgrade** button — disabled when `Tier >= ItemTier.Max` or `crafting_common < 1`. On press: `UpgradeGearItem(instanceId)` / `UpgradeSkillItem(instanceId)`, close overlay, `Refresh()`
  - (if `MaxEquipmentAugSlots > 0` or `MaxSkillAugmentSlots > 0`): separator + sub-label + `HBoxContainer` of slot buttons
    - **Filled slot** → click removes augment (`RemoveEquipmentAugment` / `RemoveSkillAugment`), rebuilds row in-place
    - **Empty slot** → click opens `NewStyledPopup()` listing compatible owned augments; on pick: `SocketEquipmentAugment` / `SocketSkillAugment`, rebuilds row in-place

Augment compatibility filter (gear): `EquipmentAugmentData.RequiredTags` empty OR intersects `ItemData.Tags`. No tag gate for skill augments (any augment can socket into any skill in v1).

### Equipment Inventory Tab — slot-type selector

**State**

`CharacterScreen` gains one field:

```
ItemSlot? _activeEquipmentFilter = null  // null = no type selected
```

**Tab layout**

The Equipment tab replaces the flat 50-item `_inventoryGrid` with two layers:

1. **Type selector row** — `HBoxContainer` of four `Button`s: Weapon / Hat / Body / Ring.
   - Active button gets a distinct tint (Bright Flame `#E88A28`) to show selection.
   - Each button's `Pressed` handler sets `_activeEquipmentFilter` and calls `RebuildEquipmentList()`.

2. **List area** — `VBoxContainer _equipmentListArea` beneath the selector.
   - Rebuilt every time `_activeEquipmentFilter` changes, or on `Refresh()`.
   - When `_activeEquipmentFilter` is null: list area shows nothing (type selector alone is visible).

**`RebuildEquipmentList()`**

Called from `Refresh()` and from selector button handlers. Reads `_activeEquipmentFilter`.

```
if _activeEquipmentFilter == null → clear _equipmentListArea, return

slot = _activeEquipmentFilter.Value
items = Profile.OwnedGearInstances
        .Where(inst => inst.Definition?.Slot == slot)
        .ToList()

_equipmentListArea:
  [Craft button]  → ShowEquipmentCraftPopup(slot)
  foreach item in items:
    row button (name + tier label, tooltip: BuildGearTooltip)
    GuiInput:
      Left-click  → ShowGearModifyPanel(inst, itemDef)
      Right-click → EquipItem(charId, slot, inst.Id); RebuildEquipmentList()
  if items.Count == 0: show "None owned — craft one" label
```

**`ShowEquipmentCraftPopup(ItemSlot slot)`**

Shows a `PopupMenu` with the subtypes for that slot:

| Slot | Subtypes → recipe IDs |
|---|---|
| Weapon | Sword → `recipe_sword_t1`, Bow → `recipe_bow_t1`, Wand → `recipe_wand_t1` |
| Hat | Heavy → `recipe_heavy_hat_t1`, Medium → `recipe_medium_hat_t1`, Light → `recipe_light_hat_t1` |
| Body | Heavy → `recipe_heavy_body_t1`, Medium → `recipe_medium_body_t1`, Light → `recipe_light_body_t1` |
| Ring | Ring → `recipe_ring_t1` |

On selection: `CraftGearItem(recipeId)`. On `InventoryFull`: flash a red error label in `_equipmentListArea` (auto-hides after 3s). On `Success`: `RebuildEquipmentList()` (list refreshes in place; tab stays open).

**Loadout slot interaction (extending `OnGearSlotInput`)**

- **Empty slot — left-click**: switch `TabContainer` to Equipment tab; set `_activeEquipmentFilter = slot`; call `RebuildEquipmentList()` (skips the selector step, opens pre-filtered to that slot's type).
- **Filled slot — left-click**: `ShowGearModifyPanel(instance, itemDef)` — no change from current.
- **Filled slot — right-click**: `UnequipItem(charId, slot)`. If returns `false` (inventory full): `Refresh()` shows unchanged state.

**What the current implementation replaces**

- The flat `_inventoryGrid` in `RefreshInventory()` is replaced by the selector + `_equipmentListArea` layout.
- `OpenPicker(slot)` / `ItemPickerPanel` flow is removed; the Equipment tab pre-filtered to the slot type takes over.
- `ShowCraftGearForSlotPanel(slot)` is replaced by `ShowEquipmentCraftPopup(slot)` inside the tab (no modal overlay for gear crafting).

### Equipment Augment Runtime Effects

Equipment augments are seeded into `PlayerController._activeAugments (HashSet<string>)` at run start from all equipped gear's `SocketedEquipmentAugmentIds`. Effects are dispatched by `DefinitionId` string — no subclassing.

| DefinitionId | Effect | Mechanic |
|---|---|---|
| `retaliation` | Deal 5 physical damage back to melee attacker | Checked in `TakeDamage()` — if attacker is an `EnemyController`, calls `ec.TakeDamage(5f, Physical)` |
| `fortify` | Reduce every hit after the first by 50% | `_fortifyActive` bool: false on run start, set true after first hit (giving 50% reduction on all subsequent hits while active) |
| `dash_reflex` | +100 Speed for 1 second on being hit | `_dashReflexTimer` countdown; Speed += 100 while timer > 0 (in `_PhysicsProcess` movement calc) |
| `ghost_step` | Heal 10 HP if an enemy is killed within 2s of being hit | `_ghostStepTimer` armed in `TakeDamage()`; `EnemyController.Die()` calls `pc.OnEnemyKilled()` which checks the timer |
| `mending` | Heal 5 HP every 3 seconds (passive, always active) | `_mendingTimer` ticked in `_PhysicsProcess`; calls `Heal(5)` on expiry, resets to 3.0s |

### Material IDs

| ID | Display name | Source |
|----|--------------|--------|
| `crafting_common` | Common | 20% enemy drop (maps to old `craftingCurrency1`) |

Higher tiers (rare, exotic) will be added when their drop sources are designed.

---

## Effects over Time (EoT)

All timed effects on enemies — whether damage or control — flow through a single EoT system. See GDD § Effects over Time for design rules.

### Data

```
EotData(Id, Name, ApplyChance, Duration, IsDamageEot, TickRate, DamagePerTick)
EotInstance { DefinitionId, TimeRemaining, TickTimer, CritMultiplier }
```

`EotRegistry` is a static catalogue. `SkillAugmentData` references EoT by id (e.g. `slow` Skill Augment → `"slow"` EoT id). The mapping is 1-to-1 in v1 but augments may reference no EoT (e.g. Magic Damage is purely mechanical — it overrides damage type with no timed effect). Splash and Pierce are post-v1.

### Application flow

```
Projectile hits EnemyController
→ foreach socketed Skill Augment on the firing skill slot:
    eot = EotRegistry.Get(augment.EotId)   // null if augment has no EoT
    if eot == null: skip
    if Random() < eot.ApplyChance:
        enemy.ApplyEot(eot, critMultiplier)   // critMultiplier = hit's crit mult (1.0 if not a crit)
```

`Projectile` carries a `List<string> SkillAugmentEotIds` and a `float CritMultiplier` (1.0f if the firing hit was not a crit; crit multiplier value if it was). Both are resolved by `WeaponController` at fire time. No registry lookups on the hot path.

### `EnemyController.ApplyEot(EotData eot, float critMultiplier = 1.0f)`

```
if _activeEots.ContainsKey(eot.Id):
    _activeEots[eot.Id].TimeRemaining = eot.Duration   // refresh duration
    if eot.IsDamageEot:
        _activeEots[eot.Id].CritMultiplier = critMultiplier  // re-stamp with new hit's crit status
    return
// first application:
_activeEots[eot.Id] = new EotInstance {
    DefinitionId   = eot.Id,
    TimeRemaining  = eot.Duration,
    TickTimer      = eot.TickRate,
    CritMultiplier = eot.IsDamageEot ? critMultiplier : 1.0f
}
ApplyEotEffect(eot)   // e.g. reduce speed for Slow
```

**Crit stamping rule:** when the applying hit was a critical hit, the EoT instance is stamped with the crit multiplier for its full duration. Re-applying with a non-crit resets `CritMultiplier` to 1.0; re-applying with a crit refreshes it at the new crit multiplier. Non-damage EoTs always store 1.0 and the field has no effect.

### `EnemyController._Process(delta)` — EoT tick

```
foreach (id, inst) in _activeEots:
    inst.TimeRemaining -= delta
    if inst.TimeRemaining <= 0:
        RemoveEotEffect(eot)
        _activeEots.Remove(id)
        continue
    if eot.IsDamageEot:
        inst.TickTimer -= delta
        if inst.TickTimer <= 0:
            TakeDamage(eot.DamagePerTick * inst.CritMultiplier, DamageType.Magic)
            inst.TickTimer = eot.TickRate
```

### Effect dispatch

Effects are applied/removed by id — no subclassing:

| EoT id | On apply | On remove |
|---|---|---|
| `slow` | `Speed *= (1 − SlowFraction)` | `Speed /= (1 − SlowFraction)` |
| `burn` | _(nothing — damage fires on tick)_ | _(nothing)_ |

`SlowFraction` is a field on `EotData` [TBD value]. Damage EoTs use `DamagePerTick` from `EotData`. New EoTs are added by extending the dispatch switch — no new classes needed.

### Signal

`EnemyController` does not emit a signal for EoT state. HUD / VFX feedback is deferred — exact signal contract TBD when visual effects are designed.

---

## VFX

Hit effects use pre-built scenes from the **EffectBlocks** pack (see `technical-assets.md`). Custom effects live under `res://src/vfx/`. All effects are spawned by C# code: instantiate, add to scene root, set `GlobalPosition`, then schedule `QueueFree` via a timer.

| Scene | Trigger | Description |
|---|---|---|
| `res://PolyBlocks/EffectBlocks/assets/impacts/impact_5.tscn` | `Projectile.HitEnemy()` — every hit, melee and ranged | Orange + blue billboard sparkle burst at hit position. 4 particles, 0.5s lifetime. `activate_effects()` called via `Call()` after spawn. Auto-freed after 2s via C# `CreateTimer`. ScaleMin=40, ScaleMax=80. |
| `res://PolyBlocks/EffectBlocks/assets/impacts/impact_5.tscn` | `PlayerController.OnSkillFired()` — melee attacks only (`isMelee = true`) | Swing VFX: same scene, larger scale (ScaleMin=35, ScaleMax=55). 12 particles, 0.6s lifetime. Spawned at `GlobalPosition + (0, 20, 0)` (character height). Auto-freed after 2s. |

**Spawning from C#**:
```csharp
var fx = ImpactHitScene.Instantiate<GpuParticles3D>();
var mat = (ParticleProcessMaterial)fx.ProcessMaterial.Duplicate();  // duplicate — never modify the shared resource
mat.ScaleMin = 40f;
mat.ScaleMax = 80f;
fx.ProcessMaterial = mat;
GetTree().Root.AddChild(fx);
fx.GlobalPosition = hitPos;
fx.Call("activate_effects");
GetTree().CreateTimer(2.0).Timeout += fx.QueueFree;
```
Always wrap in `try/catch` — if VFX instantiation throws, the exception must not prevent `QueueFree()` in the calling `OnBodyEntered` from running (projectile would otherwise stay alive and appear to pass through enemies).

**World scale note** — player model is scale 9. Node-level `Scale` on `GPUParticles3D` does not reliably affect rendered particle size. Set `ProcessMaterial.ScaleMin/Max` directly at runtime (duplicate first). Current hit effect: `ScaleMin = 40, ScaleMax = 80`.

---

## Damage Pipeline

### Player taking damage

`PlayerController.TakeDamage(float rawAmount, DamageType type)`

```
// Dodge I-frames — invulnerable during active dodge roll
if _isDodging:
    return

// Evasion — passive % chance to completely avoid the hit (no damage, no effects)
if Random() < _evasion:
    return

effectiveDamage = rawAmount × (1 − DamageReduction)
if type == Physical:
    effectiveDamage ×= (1 − PhysicalResistance)
else if type == Magic:
    effectiveDamage ×= (1 − MagicResistance)

// Focus Shield intercept — all archetypes
if _currentFocusShield > 0:
    absorbed = Min(_currentFocusShield, effectiveDamage)
    _currentFocusShield -= absorbed
    effectiveDamage -= absorbed
    emit ShieldChanged(_currentFocusShield, _maxFocusShield)

CurrentHealth -= effectiveDamage
emit HealthChanged(CurrentHealth)

// Hit feedback (D4 style — no interrupt, screen-only)
Engine.TimeScale = 0.0                                  // hit stop: ~2 frames
CreateTimer(0.05s, ignoreTimeScale: true).Timeout       // restore TimeScale to 1.0
emit PlayerHit()                                        // HUD shows screen flash
```

**Hit stop** (`Engine.TimeScale = 0`) pauses the entire game world for ~0.05s — enemies, projectiles, animations all freeze. The HUD timer uses a real-time timer (`ignoreTimeScale: true`) to restore it. No guard needed against stacking since the restore always fires.

**Screen flash** is a full-screen `ColorRect` in the HUD (red, alpha ~0.3), tweened to alpha 0 over ~0.15s. Triggered by the `PlayerHit` signal. No gameplay effect — purely visual feedback.

**Melee windup delay** — `WeaponController` delays `HitMelee` by `cooldown × MeleeWindupFraction (0.35)` via `CreateTimer`. Damage lands mid-animation at the strike frame rather than on button press. Timer captures target by reference and skips the hit if target is already dead (`IsQueuedForDeletion()`).

`DamageReduction` and `PhysicalResistance` are runtime stats on `PlayerController`, seeded at run start from the equipped armor and accessory respectively. All current chase enemies pass `DamageType.Physical`. Future ranged/magic enemies pass `DamageType.Magic`.

### Enemy taking damage

`EnemyController.TakeDamage(float rawAmount, DamageType type)`

```
effectiveDamage = rawAmount × (1 − resistance[type])
```

Resistance values per enemy type live in `EnemyData` (`PhysicalResistance`, `MagicResistance`).

### Stat seeding at run start

`PlayerController._Ready()` reads `CharacterManager.SelectedCharacter` and equipped items. After seeding, `GlobalPosition` is set by `DungeonGenerator` to `SpawnPosition` (the centre floor cell, `CellToWorld(0,0)`) — not world origin. `DungeonGenerator._Ready()` runs after `PlayerController._Ready()` (scene order), so it always finds the player in the group and moves it.

All stats are derived via the archetype multiplier formula (see Archetype Multiplier System below) — `BuildStatBlock()` returns pre-computed effective values.

```
statBlock          = character.BuildStatBlock()   // applies multiplier formula internally

MaxHealth          = statBlock.Get(MaxHp)
Speed              = statBlock.Get(Speed)
PhysicalResistance = statBlock.Get(PhysicalResistance)
MagicResistance    = statBlock.Get(MagicResistance)
DamageReduction    = hat.DamageReduction + body.DamageReduction  // flat sum, not multiplied
EffectiveRange     = weapon.WeaponRange * hat.RangeMultiplier * body.RangeMultiplier * GameScale.TileSize
                     // Multiplicative: each armour piece multiplies the running total independently.
                     // Higher base ranges take a larger absolute hit from heavy armour — by design.
                     // Applies to all weapons. RangeMultiplier = 1.0 for Medium (neutral).
                     // tile values × TileSize → world units; standalone float, not part of StatId

// Weapon-rooted damage — computed in PlayerController.ApplyWeaponDamage():
// Both pools pre-computed; slot.Skill.DamageType selects which fires at runtime
// Damage increase per level is implicit through primary stat growth (no separate level multiplier)
physDmg  = weapon.BaseDamage × statBlock.Get(PhysicalDamage) × (1 + weapon.DamageBonus)
magicDmg = weapon.BaseDamage × statBlock.Get(MagicDamage)    × (1 + weapon.DamageBonus)

WeaponController.SetDamage(physDmg, magicDmg)
WeaponController.SetCritMultiplier(statBlock.Get(CritDamage))  // Str-driven; fixed 1.5× in v1

// Global crit chance — Dex stat baseline + weapon identity bonus (Bow) + future equipment augment / ring contributions
globalCritChance   = statBlock.Get(CritChance) + weapon.CritChanceBonus
WeaponController.SetGlobalCritChance(globalCritChance)

// Evasion — seeded into PlayerController for use in TakeDamage()
_evasion           = statBlock.Get(Evasion)  // passive % chance to completely avoid a hit

// Per-slot setup — skill augments resolved via AugmentResolver
for i in 0..2:
    instanceId = character.SlottedSkillInstanceIds[i]   // "" = skip
    if instanceId is non-empty:
        skill            = CharacterManager.FindSkillInstance(instanceId).Definition
        activeAugments   = AugmentResolver.Resolve(instance.SocketedSkillAugmentIds, lookup)
        slotCritChance   = sum of TriggerChance from any Critical Strike augments in activeAugments
        hasMagicDamage / eotIds  = resolved from activeAugments
        WeaponController.SetSlot(i, skill, eotIds, hasMagicDamage, slotCritChance)
```

**`ApplyWeaponDamage` is called at run start and again on every level-up** — same cadence as `BuildStatBlock()`. `SetGlobalCritChance` and `SetCritMultiplier` follow the same cadence.

### Archetype Stat Scaling System

Stats scale via **primary stat growth per level** (D4 / Last Epoch pattern). Two tables replace the old per-derived-stat multiplier registry:

**Table 1 — Primary stat → derived stat conversions (fixed, all archetypes):**

| Primary stat | Derived stats | Constants |
|---|---|---|
| Strength | PhysicalDamage, MaxHp, PhysicalResistance, CritDamage | `StrToPhysDmg`, `StrToMaxHp`, `StrToPhysRes`, `StrToCritDmg` |
| Dexterity | CritChance, Evasion | `DexToCritChance`, `DexToEvasion` |
| Intelligence | MagicDamage, MaxFocus, MagicResistance, FocusRegen | `IntToMagDmg`, `IntToMaxFocus`, `IntToMagRes`, `IntToFocusRegen` |

All constants live in `PrimaryStatConversions` — TBD, owned by the Balancer.

**Table 2 — Primary stat gains per level per archetype:**

| Archetype | Str/level | Dex/level | Int/level |
|---|---|---|---|
| Warrior | TBD (high) | TBD (low) | TBD (low) |
| Rogue | TBD (low) | TBD (high) | TBD (low) |
| Mage | TBD (low) | TBD (low) | TBD (high) |

Values live in `PrimaryStatGainRegistry` — TBD, owned by the Balancer.

**Formula:**
```
strGained  = level × PrimaryStatGainRegistry.GetGain(archetype, Str)
dexGained  = level × PrimaryStatGainRegistry.GetGain(archetype, Dex)
intGained  = level × PrimaryStatGainRegistry.GetGain(archetype, Int)

physDmg    = base.PhysDmg    + strGained × StrToPhysDmg    + item.PhysDmgBonus
maxHp      = base.MaxHp      + strGained × StrToMaxHp       + item.MaxHpBonus
physRes    = base.PhysRes    + strGained × StrToPhysRes     + item.PhysResBonus
critDmg    = base.CritDmg   + strGained × StrToCritDmg     + item.CritDmgBonus
critChance = base.CritChance + dexGained × DexToCritChance  + item.CritChanceBonus
evasion    = base.Evasion    + dexGained × DexToEvasion     + item.EvasionBonus
magDmg     = base.MagDmg    + intGained × IntToMagDmg      + item.MagDmgBonus
maxFocus   = base.MaxFocus   + intGained × IntToMaxFocus    + item.MaxFocusBonus
magRes     = base.MagRes     + intGained × IntToMagRes      + item.MagResBonus
focusRegen = base.FocusRegen + intGained × IntToFocusRegen  + item.FocusRegenBonus
```

Base archetype stats (the `base.*` values) are applied directly at level 1 — not subject to the primary stat formula. Item bonuses are direct derived stat additions — flat, class-agnostic.

**Items:** items may grant +primary stat bonuses (converted via the fixed rates above) **or** +derived stat bonuses directly (flat, same value for all archetypes). Both are valid affix types.

**`BuildStatBlock()`** computes the full formula above and returns a `StatBlock` with effective values. Called once at run start and once per level-up — callers always receive ready-to-use effective values.

**Mid-run level-up:** `PlayerController` calls `BuildStatBlock()` with the new level and reseeds all stats from the result. ~10 calls over a 5-minute run — negligible cost.

`DamageReduction` is **not** part of this system — it is a flat percentage from armour and applied as-is in `TakeDamage()`.

---

## Enemy Spawner — Wave Scaling

Time-driven, no fixed waves. `EnemySpawner` recalculates each spawn:
- **Spawn rate** — starts immediately at t=0; interval = `InitialInterval / (1 + minutes * 0.5)`, clamped to `MinInterval = 0.3s`
- **Spawn position** — random floor tile from `DungeonGenerator.FloorPositions` at least `SpawnRadius * 0.5` world units from the player; falls back to a ring spawn if no dungeon is present
- **Enemy types** — v1: single type only (`Skeleton`). Pool will expand in future milestones.

| Type     | Speed | HP | Damage | Physical Resist | Model                        |
|----------|-------|----|--------|-----------------|------------------------------|
| Skeleton | 42    | 2  | 5      | 10%             | `kaykit_enemy_skeleton.glb`  |

All types receive a time-scaling bonus on top: `Speed += 5 * minutes`, `MaxHealth += 3 * (int)minutes`.

---

## Map Attributes

Each run is played on a map. Maps carry an attribute set that modifies run behaviour. The attribute set is small now and will grow.

| Attribute  | Type  | Effect                                                                     |
|------------|-------|----------------------------------------------------------------------------|
| `MapLevel` | `int` | On enemy death, `PlayerController.CollectXp(MapLevel)` is called directly — no pickup required. Stacks on top of any XP Shard drop. |

`MapLevel` is passed into the run scene at startup (e.g. via `RunSession` or a `MapData` resource — exact wiring TBD when maps are selectable).

---

## Drop System

On enemy death, two XP sources fire independently:

1. **Kill XP** — `1 × MapLevel` XP granted instantly via `PlayerController.CollectXp()`
2. **XP Shard drop** — physical `XpShard` scene spawned; player must walk over it (value = 5 XP)

Other drops hardcoded in `EnemyController.Die()`:

| Drop              | Chance | Notes                                                                  |
|-------------------|--------|------------------------------------------------------------------------|
| XP Shard            | 100%   | Always dropped; value = 5 XP                                           |
| Coin              | 25%    | `CoinPickup` auto-collected; reports to `RunSession.AddCoin()`         |
| Health pack       | 10%    | `HealthPickup` heals player for 15 HP on contact                       |
| Crafting currency | 20%    | Instant; calls `RunSession.AddCraftingCurrency1(1)` — no pickup scene  |

> Planned: large XP Shards, weighted drop tables via `EnemyData` resource.
