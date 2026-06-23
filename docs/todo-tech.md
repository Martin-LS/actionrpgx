# Tech To-Do

Code, systems, UI, and art tasks only. Design decisions live in the GDD files.

---

## Tech Spec Gaps — GDD designed, no technical spec written yet

Items where the GDD defines the design but no corresponding technical specification exists in the tech docs. Write the spec before implementing.

### UI

- [ ] Spec Skill Modify Panel — two-column layout, left augment slot column, context-sensitive right panel, empty→filled slot state transitions, Upgrade/Re-roll/Remove wiring
- [ ] Spec Equipment Modify Panel — same two-column pattern as Skill Modify Panel; augment slots use Equipment Augments component; Remove = unequip
- [ ] Spec Equipment Inventory Tab slot-type selector — four-button selector (Weapon/Hat/Body/Ring), filtered item list, two-step Craft flow (type → subtype), right-click quick-equip wiring
- [ ] Spec Craft New entry point — how CharacterScreen triggers craft+auto-slot flow for Skills and Augments; which controller/signal owns it; how crafted item transitions into Modify Panel
- [ ] Spec Re-roll mechanic — what is re-rolled (TriggerChance on augments? stat roll on gear?), cost, which CharacterManager method handles it
- [ ] Spec Run Results Overlay content — what data is displayed (XP gained, level reached, materials earned, coins, win/lose state) and how it is populated from RunSession

### Systems

- [ ] Spec runtime targeting resolver — which class owns it, how Entity/Position/Self targeting shapes are resolved at runtime, cursor-to-world projection, range clamping for Position skills, auto-pick logic for Entity skills
- [ ] Spec zone skill lifecycle — how Fixed-Zone, Stackable-Zone, and Triggered-Zone-Burst zones are spawned/tracked/evicted; proximity detection for Triggered-Zone-Burst; Tracked-Tick entity follow; stack cap eviction order
- [ ] Spec AoE radius modifier pipeline — `AoEModifier` field on `SkillData` or `StatBlock`, accumulation across augments, application of formula `effective_radius = base_radius × sqrt(1 + total_aoe_pct_increase)`
- [ ] Spec Proximity Cluster System — which class owns the computation, connected-component algorithm (flood fill vs. union-find), when recomputation fires, Dormant→Idle transition on `MapReady` for pre-placed enemies



---

## Systems / Features

> **Not in scope.**

- [ ] Map selection screen — currently one hardcoded arena; no selection or variety
- [ ] Boss mechanic — run win condition triggers on timer expiry; boss is unimplemented

---

## Art / Assets

> **Not in scope.**

- [ ] Hollow Dark Forest assets — floor tile, tree trunk wall, wall corner (Blender); replace placeholder box geometry in DungeonGenerator
- [ ] Armour models — attachment offsets need tuning against current character proportions
- [ ] Weapon rotation fine-tuning — sword blade orientation may need a Blender tweak
- [ ] Cyclone animation — needs full-body spin; current partial blend looks wrong
