# Tech To-Do

Code, systems, UI, and art tasks only. Design decisions live in the GDD files.

---

## Code — Design Sync

Items where the code diverges from the current design docs.

### 1. Add `DamageType` to `SkillData`

`SkillData` has no `DamageType` field. Each prototype declares its type in the GDD (Entity-Burst: Physical, Self-Duration-Tick: Magic, etc.) but this is not in code. Add `DamageType DamageType = DamageType.Physical` to the record; set the correct value per prototype in `SkillRegistry`.

### 2. Damage-type selection in `WeaponController`

Currently damage type is selected via `_baseDamageType` (set from weapon) and `slot.HasMagicDamage` (set from augments). Design: the skill's `DamageType` field is the source of truth; augments modify on top. Depends on task 1.

- Remove `_baseDamageType` field and `SetBaseDamageType()` method from `WeaponController`
- Remove `HasMagicDamage` from `SkillSlot`; remove `hasMagicDamage` param from `SetSlot()`
- Replace all `isMagic = (_baseDamageType == Magic) || slot.HasMagicDamage` guards with `isMagic = slot.Skill.DamageType == Magic`
- Remove `wc.SetBaseDamageType(weapon.BaseDamageType)` call in `PlayerController.ApplyWeaponDamage()`

### 3. Replace archetype multiplier table with primary stat growth

`PlayerController.ApplyWeaponDamage()` still uses `BalanceConfig.Archetypes.*.PhysicalDamageMultiplier / MagicDamageMultiplier` — the old per-archetype multiplier table. `CharacterData.BuildStatBlock()` uses `ArchetypeMultiplierRegistry.Get()` for HP, Speed, and PhysicalResistance scaling. Design: archetype scaling comes from primary stat growth (Str/Dex/Int gains per level × fixed conversion rates).

- Add `PrimaryStatGainRegistry` — Str/Dex/Int gains per level per archetype (TBD, Balancer-owned)
- Add `PrimaryStatConversions` — fixed conversion rates: Str → PhysicalDamage/MaxHp/PhysRes/CritDmg; Dex → CritChance/Evasion; Int → MagicDamage/MaxFocus/MagRes/FocusRegen
- Rewrite `BuildStatBlock()` to use: `primary_stat = archetype_base_primary + (level × gain_rate); derived = primary × conversion_rate + item_flat_bonus`
- Rewrite `ApplyWeaponDamage()` to read `physDmg` / `magicDmg` from `StatBlock` instead of computing with archetype multiplier
- Delete `ArchetypeMultiplierRegistry`
- Remove `BalanceConfig.Archetypes.*.PhysicalDamageMultiplier/MagicDamageMultiplier`

### 4. Clear RequiredTags on all v1 skill augments

`SkillAugmentRegistry` still gates every augment by tag: `splash=["Melee"]`, `pierce=["Range"]`, `slow/critical_strike/magic_damage=["Attack"]`. Design (no-gate philosophy): all v1 augments socket into any skill — `RequiredTags` should be `[]` for all five. The gate check was already removed from `CharacterManager.SocketSkillAugment`; this is just the registry data.

### 5. Flip 8 SkillRegistry entries from EngineProof to Prototype

`SkillRegistry` marks only 4 entries as `SkillKind.Prototype` (entity_burst, self_channeled_tick, self_duration_tick, self_burst) and 8 as `SkillKind.EngineProof`. GDD says all 11 player-facing prototypes are craftable — `SkillKind.Prototype`. The 8 EngineProof entries should be `Prototype`.

### 6. Add craft recipes for three player-facing prototypes

`self_channeled_tick`, `self_duration_tick`, and `self_burst` have no entries in `RecipeRegistry`. GDD says all prototypes except `entity_burst` (seeded free) are craftable. Add recipe entries for these three.

### 7. Add CritChance, CritDamage, Evasion to StatId enum

`StatId` only has: `MaxHp, Speed, PhysicalDamage, MagicDamage, PhysicalResistance, MagicResistance, MaxFocus, FocusRegen`. The primary stat conversion system (task 3) requires `CritChance` (Dex → crit %), `CritDamage` (Str → crit multiplier), and `Evasion` (Dex → dodge). Add these three values to the enum before implementing task 3.

### 8. Resolve per-skill-type damage multipliers in BalanceConfig

The design says "skills draw from the same damage number — no DamageMultiplier field on a skill." But `BalanceConfig` and `WeaponController` carry per-skill-type multipliers:

- `BalanceConfig.Focus.SelfChanneledTickDamageMultiplier` (0.4×) applied in `SetSlot` for Channeled skills
- `BalanceConfig.Focus.SelfBurstDamageMultiplier` (0.8×) applied in `FireSelfBurst`
- `BalanceConfig.Skills.*DamageMult` constants for zone/trap prototypes (TrackedTick, TriggeredZoneBurst, Windup, FixedZone, Stackable)
- `SkillSlot.DamageMultiplier` field wired to these constants

**Decision needed:** these are effectively per-skill multipliers living in BalanceConfig rather than SkillData. Options:
- (a) Accept as Balancer tuning constants — update design doc to say "no field on SkillData; Balancer applies scaling via BalanceConfig"; keep code as-is
- (b) Remove them — balance through tick rate and cooldown only; simplifies code but limits Balancer levers

---

## UI

- [ ] Ineffective augment combo warning — red/yellow exclamation on augment socket when the augment has no mechanical effect on the slotted skill (e.g. Pierce on a Self skill). Hover tooltip explains why.

---

## Systems / Features

- [ ] Map selection screen — currently one hardcoded arena; no selection or variety
- [ ] Craft New flow — not yet implemented (left-click empty skill/gear slot → craft new)
- [ ] Boss mechanic — run win condition triggers on timer expiry; boss is unimplemented

---

## Art / Assets

- [ ] Hollow Dark Forest assets — floor tile, tree trunk wall, wall corner (Blender); replace placeholder box geometry in DungeonGenerator
- [ ] Armour models — attachment offsets need tuning against current character proportions
- [ ] Weapon rotation fine-tuning — sword blade orientation may need a Blender tweak
- [ ] Cyclone animation — needs full-body spin; current partial blend looks wrong
