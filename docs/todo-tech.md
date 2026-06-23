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

## Augment Prototype Testing

Test each augment prototype in-game. Do skill augments first (individually), then mixes.

**Skill augments — individual:**
- [ ] Hit-EoT-Debuff (Slow) — socket on Entity-Burst, hit enemy, confirm Slow applies and expires, no double-stack, recasting refreshes duration
- [ ] Hit-EoT-Damage (Burn) — socket on Entity-Burst, hit enemy, confirm Burn ticks magic damage, crit stamp test: crit hit → Burn ticks deal crit×damage, non-crit reapply resets multiplier
- [ ] Hit-AoE (Splash) — socket on Entity-Burst, hit enemy in a group, confirm nearby enemies also take damage, splash hits roll EoT augments independently
- [ ] Hit-Pierce — socket on Entity-Burst (bow), shoot through enemy at another, confirm both take damage, pierce does not chain infinitely
- [ ] Hit-Damage-Mod (Crit) — socket on Entity-Burst, confirm per-skill crit bonus stacks on top of global Dex crit, crits fire at combined chance

**Augment mixes:**
- [ ] AoE+EoT — Hit-AoE + Hit-EoT-Damage on same skill; splash secondaries independently roll Burn; crit stamp propagates through splash
- [ ] Skill+Equip — Hit-EoT-Debuff (skill) + Equip-Reactive on armour; no cross-interference between skill and equipment augment triggers
- [ ] Ineffective combo — Hit-Pierce socketed into Self-Burst; UI shows warning on socket; no error when skill fires

**Pre-requisite check:**
- [ ] Verify Burn augment uses correct EoT name (GDD says `burn`, code may have `magic_damage` — resolve before testing)

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
