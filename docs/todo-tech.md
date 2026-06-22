# Tech To-Do

Code, systems, UI, and art tasks only. Design decisions live in the GDD files.

---

## Code — Design Sync

### 1. Resolve per-skill-type damage multipliers in BalanceConfig

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
