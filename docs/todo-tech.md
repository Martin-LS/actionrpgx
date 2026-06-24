# Tech To-Do

Code, systems, UI, and art tasks only. Design decisions live in the GDD files.

---

## 🤖 Gemini Tasks

Self-contained tasks with enough context to action without broader codebase knowledge.

- [ ] **Swing VFX** — `src/player/PlayerController.cs`, `OnSkillFired(int slotIndex, float cooldown, string delivery)`: when `delivery == "Melee"`, load and instantiate `res://src/vfx/impact_5.tscn`, cast to `GpuParticles3D`, set position to `GlobalPosition + new Vector3(0, 20, 0)`, set `Amount = 12` and `OneShot = true`, then add as child of the scene root (`GetTree().Root.AddChild`). The particle scale (min/max 35–55) lives on the process material — check `ProcessMaterial` cast to `ParticleProcessMaterial` and set `ScaleMin`/`ScaleMax` there. Context: hit VFX (`entity_burst_vfx.tscn`) already fires at the target from `WeaponController.HitMelee()` — do not touch it.
- [ ] **Armour attachment offsets** — Blender MCP (`mcp__blender__execute_blender_code`): `.blend` sources are `assets/models/equipment/armour_light.blend`, `armour_medium.blend`, `armour_heavy.blend`; exported GLBs are `hat_light/medium/heavy.glb` and `body_light/medium/heavy.glb`. Player skeleton is `assets/models/characters/player.blend`. Problem: armour pieces float or clip against the character — adjust bone attachment offsets on Head and Chest bones to sit flush. Re-export each GLB after adjusting.
- [ ] **Weapon rotation** — Blender MCP: source is `assets/models/equipment/weapon_sword.blend`, export target is `weapon_sword.glb`. Problem: sword blade faces the wrong direction in-game. Rotate the blade mesh ~90° on the Y or Z axis so the cutting edge faces forward (away from the player along -Z in world space). Re-export as GLB.
- [ ] **Cyclone animation** — Blender MCP: source is `assets/models/characters/player.blend`, export target is `assets/models/characters/player.glb`. Problem: the cyclone/spin animation clip only rotates part of the body — it should be a full-body spin. Find the animation clip (likely named "cyclone" or "spin"), extend the Z-axis rotation keyframes to include the Hips bone so the whole body rotates. Bake and re-export the GLB.

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

## Spec vs Code Gaps — implemented differently or not yet done

These were found by cross-checking tech spec against live code. Each entry is a confirmed delta; clarify intent before closing.

### Combat / Stats

- [ ] **Evasion not applied in TakeDamage** *(v2+)* — `BuildStatBlock()` computes `StatId.Evasion` (Dex × DexToEvasion) but `PlayerController` never reads it. Deferred by design for v1. When implemented: probability roll (`if Random() < _evasion: return`), not guaranteed full miss; v2 will tune the balance. Field `_evasion` also needs adding to PlayerController.

### Skill Augments

- [ ] **SkillAugmentInstance.TriggerChance has no gameplay effect** *(v2+, not in scope)* — Value is stored and serialized. Critical Strike uses hardcoded `BalanceConfig.SkillAugments.CritChance`; Slow uses `EotData.ApplyChance`. Per-instance proc % wiring deferred until v2 balance pass.

### Aura SkillType

- [ ] **Aura Focus reservation system unimplemented** *(v1 — spec before implementing)* — Needs spec written first: how AuraActive/AuraReserved live on SkillSlot, how `_totalReserved` accumulates, how `ReserveFocus()`/`UnreserveFocus()` interact with `GetAvailableFocus()`, and the toggle-firing guard in WeaponController. `self_aura_tick` is `EngineProof` so no Aura skill can ship until this is in place.

---

## Systems / Features

> **Not in scope.**

- [ ] Map selection screen — currently one hardcoded arena; no selection or variety
- [ ] Boss mechanic — run win condition triggers on timer expiry; boss is unimplemented

---

## Art / Assets

> **Not in scope.**

- [ ] Hollow Dark Forest assets — floor tile, tree trunk wall, wall corner (Blender); replace placeholder box geometry in DungeonGenerator
- [ ] Armour models — attachment offsets need tuning against current character proportions *(🤖 Gemini — use Blender MCP `mcp__blender__execute_blender_code`; open `assets/models/equipment/`, adjust bone attachment offsets on chest/head pieces against the player skeleton in `assets/models/characters/player.glb`)*
- [ ] Weapon rotation fine-tuning — sword blade orientation may need a Blender tweak *(🤖 Gemini — use Blender MCP; open `assets/models/equipment/weapon_sword.glb`, rotate blade mesh so edge faces forward along -Z; export in place)*
- [ ] Cyclone animation — needs full-body spin; current partial blend looks wrong *(🤖 Gemini — use Blender MCP; open the player `.blend`, find the cyclone/spin animation clip, extend rotation to full-body on the Hips bone, bake and re-export `assets/models/characters/player.glb`)*
