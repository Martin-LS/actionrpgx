# Tech To-Do

Code, systems, UI, and art tasks only. Design decisions live in the GDD files.

---

## Code — Design Sync

> All resolved. No open items.

---

## Prototype Testing

Test each prototype in-game: fires correctly, damage lands, VFX plays, no errors in output log.

- [x] Entity-Burst
- [x] Self-Channeled-Tick
- [x] Self-Duration-Tick
- [x] Self-Burst
- [x] Fixed-Zone-Tick
- [x] Fixed-Zone-Burst
- [x] Windup-Burst
- [x] Tracked-Tick
- [x] Entity-Debuff — lock target, fire, enemy visibly slows for 6s then returns to normal speed, no damage numbers, cooldown triggers (3s), focus spent (10), recasting refreshes duration (not double-stacking), no log errors
- [x] Stackable-Zone — cast 3 zones at different positions, all tick damage, cast a 4th and oldest despawns, zones stay fixed (don't follow), no log errors
- [x] Triggered-Zone-Burst — place trap, enemy walks into trigger radius, burst fires once and trap despawns, arm time prevents instant self-trigger (0.5s), 4th cast despawns oldest, 30s timeout despawn if never triggered, no log errors

---

## UI

> **Not in scope.**

- [ ] Ineffective augment combo warning — red/yellow exclamation on augment socket when the augment has no mechanical effect on the slotted skill (e.g. Pierce on a Self skill). Hover tooltip explains why.

---

## Systems / Features

> **Not in scope.**

- [ ] Map selection screen — currently one hardcoded arena; no selection or variety
- [ ] Craft New flow — not yet implemented (left-click empty skill/gear slot → craft new)
- [ ] Boss mechanic — run win condition triggers on timer expiry; boss is unimplemented

---

## Art / Assets

> **Not in scope.**

- [ ] Hollow Dark Forest assets — floor tile, tree trunk wall, wall corner (Blender); replace placeholder box geometry in DungeonGenerator
- [ ] Armour models — attachment offsets need tuning against current character proportions
- [ ] Weapon rotation fine-tuning — sword blade orientation may need a Blender tweak
- [ ] Cyclone animation — needs full-body spin; current partial blend looks wrong
