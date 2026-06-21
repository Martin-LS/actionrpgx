# Tech To-Do

Code, systems, UI, and art tasks only. Design decisions live in `docs/game-design-direction.md` and the GDD files.

---

## Code — Design Sync

- [ ] Rename in code and data: Strike → Entity-Burst, Cyclone → Self-Channeled-Tick, DamageAura → Self-Duration-Tick, Nova → Self-Burst
- [ ] Remove Adaptation Equipment Augment from implementation (cut from v1 design)
- [ ] Add duplicate augment-type check to `SocketSkillAugment` and `SocketEquipmentAugment` in CharacterManager

## UI

- [ ] Ineffective augment combo warning — red/yellow exclamation on augment socket when the augment has no effect on the slotted skill (e.g. Pierce on a Self skill). Hover tooltip explains why.

## Systems / Features

- [ ] Map selection screen — currently one hardcoded arena; no selection or variety
- [ ] Craft New flow — not yet implemented (left-click empty skill/gear slot → craft new)
- [ ] Boss mechanic — run win condition triggers on timer expiry; boss is unimplemented

## Art / Assets

- [ ] Hollow Dark Forest assets — floor tile, tree trunk wall, wall corner (Blender); replace placeholder box geometry in DungeonGenerator
- [ ] Armour models — attachment offsets need tuning against current character proportions
- [ ] Weapon rotation fine-tuning — sword blade orientation may need a Blender tweak
- [ ] Cyclone animation — needs full-body spin; current partial blend looks wrong
