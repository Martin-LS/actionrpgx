# Session Memory Transfer — 2026-07-23

> Portable handoff dump for moving work from laptop → main PC. Covers today's session (design discussions + decisions) plus relevant standing memory. Separate from `docs/mem.md` (the curated project-memory snapshot dated 2026-07-07).

---

## Git / process state at end of session

- Started on `develop`, pulled latest (`f7fcd4c` → `768758f`). Two new remote branches appeared in the pull: `feature/issue-73-poison-identity`, `docs/issue-72-void-decay-signature-resolution`.
- Did design-doc work → committed to `docs/design-mechanics.md` → **PR #83** (`docs: skill concurrency model + range-resolution clarification`) → **merged to `develop`** (`c25eabf`), feature branch deleted.
- User explicitly authorized the self-merge of #83 (Review Mandate normally forbids self-merge).
- **Currently on branch `updates/2026-07-23`** (branched off `develop` after the merge). Working tree still carries the pre-existing unrelated `.import` modifications (untouched all session).
- This `mem.md` is uncommitted at time of writing.

---

## MAIN DELIVERABLE (shipped): Skill Concurrency & Cast Timing

New `### Skill Concurrency & Cast Timing` section added to `docs/design-mechanics.md` (Combat block, before Targeting). Resolved model:

- **One animation, overlapping damage.** No global cooldown; each of 5 slots has its own independent cooldown/drain. Manual Active (OneShot) skills are **not** mutually exclusive — their damage all resolves; only **one attack animation renders at a time** (cosmetic, the "OneShot guard"). This overlap is what lets a duplicate stack rapid-fire.
- **Channels are the one exclusive case (rule 5A).** A channel (`self_channeled_tick`, `Melee`-tagged spin) is a held looping body animation. Firing any **body-attack** while channeling **cancels the channel** (same mechanism as Dodge). *Only body-attacks cancel it* — toggling an aura or dropping an animation-free Self/Position effect coexists with the channel. This forbids manual "spin-and-slash."
- **Duplicates encouraged.** Slotting 5 `entity_burst`-variants = a legit "signature-attack toolkit" build. A stack **rapid-fires** (front-loads burst/tempo), does NOT raise sustained DPS, and costs all 5 slots (no zones/auras/utility).
- **Shared Focus pool = the single sustained-output throttle.** Every skill costs Focus — **cheap ≠ free** (chose "Option B"). Sustained output = `regen ÷ cost`; a 5× stack drains the shared pool ~5× faster and settles to the same regen-bound rate. Cooldown is a **rhythm/pacing lever only**.
  - **Design principle committed:** *a skill's Focus cost, not its cooldown, is its true sustained-output throttle; cooldown only sets rhythm.* Supersedes any "no-duplicate" inventory rule — duplication is a build, not an exploit.
- **Alpha strike** = the same phenomenon as the duplicate burst (spend the pool faster; bounded by current pool). A legit playstyle (panic/boss burst), guarded per-skill only where a single dump could trivialize a fight (high Focus cost or short wind-up).
- **Genre precedent:** PoE 1/2, Diablo 3/4, Last Epoch all resolve manual casts serially. "Spin and blast" comes from **triggers** (PoE Cyclone + Cast-on-Crit) or **instant utility** (Diablo Whirlwind + Shouts), never two manual animations. Manual animation layering = explicit **non-goal**; triggers (not in game) are the only future path to real concurrency.
- **Ripple fixed:** `entity_burst` Focus row reworded from "effectively free; regens faster than you spend" → "low but non-trivial… a rapid stack still drains the shared pool." (Numbers are Balancer-owned placeholders; Balancer doesn't exist yet.)

### Consistency audit that drove the rewrite
First draft invented an "animation cap" throttle that **contradicted**:
- `design-mechanics.md` line ~158 — *"Attack animation speed syncs to cooldown"* (`scale = animLength/cooldown`, damage at 35%). There is NO fixed animation floor.
- `design-ideas.md` line 74 — OneShot guard protects one animation but **damage fires for all hits** (actives overlap).
- `design-ideas.md` line 84 — duplicate skills are **intended** to feel like a rapid-fire build.

Rewrote to describe actual behavior + the Focus-throttle principle. Now cross-checks clean against `design-skills.md`, `design-progression.md`, `design-ideas.md`.

---

## SECONDARY DELIVERABLE (shipped): Range resolution clarified

Also in `docs/design-mechanics.md` (Range resolution per targeting shape block):

- **Range source follows targeting *shape*, not skill *type*.** Entity → weapon Effective Range; Self/Position → skill's own `Range` field.
- **Dropped "Channeled" from the own-range bucket** — a channel inherits its shape's rule, so a future **Entity-shaped** channel (brainstormed `entity_channeled_tick` beam) correctly uses weapon range. Fixed the matching "buffs that modify range" bullet too.
- **Recorded rejected idea:** weapon-limited Position placement (sword → short trap range). Cut because it's weird/unfun AND violates one-source-per-skill. **Position placement stays weapon-independent** (the existing firm rule was KEPT).

---

## Melee/Range transferability (validated, no change needed)

Already committed in `design-skills.md` line 24; we only confirmed it:
- **Entity untagged = weapon-adaptive delivery** (sword swing / arrow / wand bolt). `Melee`/`Range` tags pin delivery. Only `self_channeled_tick` is `Melee`-tagged.
- **Position/Self are NOT delivery-transferable.**
- Fire `entity_burst` across sword/bow/wand = the **same single fire hit**, only the delivery motion differs. "Explosion" (splash) is an **AoE augment**, never innate to the element or to single-target `entity_burst`.

---

## Registry / composition facts established today (current code state)

**Composition model:** a skill = **prototype × form × identity**.
- **Prototype** = delivery chassis: targeting shape + skill type + damage pattern + structural mechanics (zone/trap/tracking/stacking/tick). NOT just targeting — e.g. 4 Position prototypes share targeting but differ in pattern/mechanics.
- **Form** = budget-spend shape + tier track. **BOUND to exactly one prototype** (enforced by `SkillRegistry.ValidateCombo`: `form.PrototypeId` must match).
- **Identity** = element + ailment + skin. **FREE** — any identity rides any prototype. (Asymmetry: form locked, identity free.)

**Prototypes — 11 in `SkillRegistry.cs`:**
- Player-facing: `entity_burst`, `self_channeled_tick`, `self_duration_tick`, `self_burst`
- Engine-proof: `tracked_tick`, `triggered_zone_burst`, `stackable_zone`, `entity_debuff`, `fixed_zone_burst`, `fixed_zone_tick`, `self_aura`
- (`windup_burst` retired 2026-07-06 → wind-up now lives as forms quake/fuse.)

**Entity + Tick IS possible:** `tracked_tick` = Entity + Tick + `ZoneTracksEntity` (ticking zone attached to locked enemy, expires on death). Tick delivered by the attached zone, not the weapon hit. `entity_channeled_tick` (Entity beam) is brainstormed, not built.

**Identities — 6 in `IdentityRegistry.cs`:** physical (Bleed), fire (Burn), cold (Chill), lightning (Shock), poison (Poison), void (Decay).
- Core roster = Physical/Fire/Cold/Lightning (universal free-riders on any damage skill, by design).
- Poison/Void = **later occult additions**, just merged in (issue-73 / issue-72), still being built (poison needs a stacking-EoT extension the current one-instance model lacks).
- **Only Physical + Fire are wired into presets.** Cold/Lightning/etc. composable via craft wizard but no authored presets yet.
- Non-damage skills (`entity_debuff`, `DamagePattern.None`) don't take the element axis meaningfully.

**Forms — 23 across 9 prototypes (`FormRegistry.cs`):**
- `entity_burst`: swift, heavy, salvo
- `self_burst`: nova, quake
- `self_channeled_tick`: spin, vortex, ramp
- `fixed_zone_burst`: blast, echo, fuse
- `fixed_zone_tick`: storm, floor
- `stackable_zone`: swarm, singular
- `triggered_zone_burst`: trap_swarm, trap_singular
- `self_aura`: reserve_heavy, reserve_light, reserve_heavy_magnitude, reserve_light_magnitude (recent D1 buff/debuff-aura work)
- `entity_debuff`: fleet, enduring
- **No forms:** `self_duration_tick`, `tracked_tick`.

**`entity_burst` presets — 6:** Strike/Arcane Strike (swift, phys/fire); Crushing Blow/Smite (heavy, phys/fire); Flurry/Arcane Flurry (salvo, phys/fire).

**Naming inconsistency flagged (potential cleanup issue):** "Arcane Strike" / "Arcane Flurry" are **stale** — they're Fire identity but named "Arcane" (pre-elements-only migration). heavy-fire already migrated cleanly to "Smite." Doc uses "Fire Strike." → rename Arcane* presets to Fire-flavored.

---

## TABLED DISCUSSION — "mix-and-match atomic pieces" (resume here)

User is exploring a *different way of mixing* skill building blocks (acknowledges full-atomic would never ship — too unwieldy). **Tabled**, to resume.

**The atoms:** targeting shape / skill type / damage pattern / persistence mechanics / form (budget-spend) axis / identity. The **prototype is just an opinionated bundle** of the first four; real question = "which axes are free vs. bundled."

**Why full-atomic fails (not just cognitive load):** (1) nonsense combos (Entity+trap, Self+tracking), (2) degenerate balance, (3) production — each prototype has **bespoke pre-authored VFX/animation**; free mixing needs procedural/composable VFX.

**Three alternative cuts I proposed (each keeps coherence + legibility + pre-authorable visuals):**
- **Cut A (my rec — lowest risk/highest value):** free the *form* axis — make forms a small set of universal spend-profiles ("fast/cheap", "slow/heavy", "wide/expensive", "multi-hit") applying to any prototype. Prototypes stay as coherent chassis (visuals safe); tuning becomes mix-and-match like identity; kills per-prototype form-authoring burden.
- **Cut B:** base + behavior-modules (PoE support-gem model). Thin base skills; move persistence atoms (Tracking/Trap/Nova/Echo/Stack) into stackable modifiers. **Already partly the augment system** — decide how far to push it.
- **Cut C:** two-axis grammar — *delivery* (strike/bolt/nova/lob/beam) × *behavior* (hit/DoT/debuff/buff), both free + a `ValidateCombo`-style pruner. Closest to "atomic" while staying in-head.

**Open steering question for the user:** is the goal (a) more expressive power, (b) a different cognitive frame (fewer, more meaningful knobs), or (c) a different crafting *feel* (assembling vs. picking)? Each points to a different cut.

---

## Standing project memory (relevant context, from prior sessions)

- **Design pillars (resolved, don't contradict):** craft-driven; elements-only damage roster; delivery-scaling (Str→Melee / Dex→Ranged / Int→Spell). Element (damage type) is decoupled from scaling.
- **Skill synthesis status:** whole forms→identities→augments synthesis COMPLETE (design merged, PR #60). D1 buff/debuff-aura wave implemented (#61/#63/#64 merged). #62 reserve-form remaining.
- **`docs/mem.md`** = consolidated design/skill/process memory snapshot (2026-07-07) — read for full rationale.
- **Statusline** is now global (`~/.claude/statusline.js`), not project-local.
- **Workflow:** multi-agent issue flow with GitHub labels; git-flow (branch off `develop`, PRs target `develop`). Review Mandate: no self-review/self-merge unless user authorizes per task.

---

## Loose ends / next actions

1. Resume the **tabled mix-and-match discussion** (answer the steering question → pick a cut).
2. Optional: file an **issue for the Arcane→Fire preset rename** (naming consistency with elements-only).
3. Decide whether this `mem.md` should be committed/pushed (transfer vehicle) or folded into `docs/mem.md`.
