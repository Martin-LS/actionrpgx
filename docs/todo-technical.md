# Tech To-Do

Code, systems, UI, and art tasks only. Design decisions live in the GDD files.

---


## Tech Spec Gaps — GDD designed, no technical spec written yet

Items where the GDD defines the design but no corresponding technical specification exists in the tech docs. Write the spec before implementing.

### UI

- [ ] Spec Game Options / Settings Menu — Centralized overlay accessible from Main Menu and Pause Menu containing Gameplay settings (auto-cast checkboxes) and audio/video placeholder controls.

> **(v1 — postponed)**

- [ ] Spec Skill Modify Panel — two-column layout, left augment slot column, context-sensitive right panel, empty→filled slot state transitions, Upgrade/Re-roll/Remove wiring
- [ ] Spec Equipment Modify Panel — same two-column pattern as Skill Modify Panel; augment slots use Equipment Augments component; Remove = unequip
- [ ] Spec Equipment Inventory Tab slot-type selector — four-button selector (Weapon/Hat/Body/Ring), filtered item list, two-step Craft flow (type → subtype), right-click quick-equip wiring
- [ ] Spec Craft New entry point — how CharacterScreen triggers craft+auto-slot flow for Skills and Augments; which controller/signal owns it; how crafted item transitions into Modify Panel
- [ ] Spec Re-roll mechanic — what is re-rolled (TriggerChance on augments? stat roll on gear?), cost, which CharacterManager method handles it
- [ ] Spec Run Results Overlay content — what data is displayed (XP gained, level reached, materials earned, coins, win/lose state) and how it is populated from RunSession

### Systems

> **(v1 — postponed)**

- [ ] Spec runtime targeting resolver — which class owns it, how Entity/Position/Self targeting shapes are resolved at runtime, cursor-to-world projection, range clamping for Position skills, auto-pick logic for Entity skills
- [ ] Spec zone skill lifecycle — how Fixed-Zone, Stackable-Zone, and Triggered-Zone-Burst zones are spawned/tracked/evicted; proximity detection for Triggered-Zone-Burst; Tracked-Tick entity follow; stack cap eviction order
- [ ] Spec AoE radius modifier pipeline — `AoEModifier` field on `SkillData` or `StatBlock`, accumulation across augments, application of formula `effective_radius = base_radius × sqrt(1 + total_aoe_pct_increase)`
- [ ] Spec Proximity Cluster System — which class owns the computation, connected-component algorithm (flood fill vs. union-find), when recomputation fires, Dormant→Idle transition on `MapReady` for pre-placed enemies



---

## Spec vs Code Gaps — implemented differently or not yet done

These were found by cross-checking tech spec against live code. Each entry is a confirmed delta; clarify intent before closing.

### Combat / Stats

- [ ] **Evasion not applied in TakeDamage** *(v2+)* — `BuildStatBlock()` computes `StatId.Evasion` (Dex × DexToEvasion) but `PlayerController` never reads it. Deferred by design for v1. When implemented: probability roll (`if Random() < _evasion: return`), not guaranteed full miss; v2 will tune the balance. Field `_evasion` also needs adding to PlayerController.
- [ ] **Physical/Magic Resistance scaling unimplemented** *(v2+)* — Strength is supposed to scale Physical Resistance and Intelligence is supposed to scale Magic Resistance, but StrToPhysResistance and IntToMagResistance are set to 0f in PrimaryStatConversions.cs and CharacterData.cs doesn't generate resistance modifiers.
### Skill Augments

- [ ] **AugmentInstance.TriggerChance (Skill and Equipment) has no gameplay effect** *(v2+, not in scope)* — Value is stored and serialized. Critical Strike uses hardcoded `BalanceConfig.SkillAugments.CritChance`; Slow uses `EotData.ApplyChance`; Equipment Augments trigger at 100% or static rates. Per-instance proc % wiring deferred until v2 balance pass.

### Inventory Systems

---

## Systems / Features

> **Not in scope.**

- [ ] Map selection screen — currently one hardcoded arena; no selection or variety
- [ ] Boss mechanic — run win condition triggers on timer expiry; boss is unimplemented

---

## Art / Assets

> **Not in scope.**

- [ ] Hollow Dark Forest assets — floor tile, tree trunk wall, wall corner (Blender); replace placeholder box geometry in DungeonGenerator
- [ ] **Roll animation** *(v2+)* — No roll clip exists in the player GLB; characters slide while dodging. Needs rig animation in Blender.
