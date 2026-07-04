# Skill System — Brainstorming Scratchpad

> **Status: OPEN brainstorm, not committed design.** This is a working doc spanning multiple sessions.
> Nothing here is final until it is resolved and moved into `design-skills.md`. Rejected ideas get deleted, not archived.
> **The committed skill design lives elsewhere:** the 12 prototypes, AoE math, the Budget/Identity framework, the composition model (prototype + form + identity), and the wave-1 forms & preset recipe book are all in [design-skills.md](design-skills.md); stat ownership (incl. craft-time-components rule §5D) in [design-stats.md](design-stats.md); crafting flow in [design-progression.md](design-progression.md).

---

## What this doc is for

Working space for the **v2+ skill system questions that are still open**. The composition model is adopted and wave 1 is designed (locked 2026-07-04); what remains here is what comes next and what was deliberately deferred.

## Open threads

### Wave-1 follow-ups (needed before shipping)

- [x] ~~**Sanity check the locked wave-1 design**~~ DONE 2026-07-04 — design is rule-clean (no matrix violations, all form shapes map to existing `SkillData` fields incl. the new `TickRate`, no name collisions, guardrails pass). Two engine gaps found and added to the `design-skills.md` prerequisites: **WindUp only works on Position paths** (heavy + quake forms need it on Entity/Self), and **tier does not modify skill stats in code yet** (tier tracks = part of composition work).
- [ ] **Forms & identities: craftable items or internal data?** Are they separate craftable *items* (two material sinks for the D4 economy) or internal authoring data with materials spent directly in the preset craft? Decide before the crafting screen is built; interacts with D4 (reward/economy surface).
- [ ] **Formalise & ship proposal** — smallest slice that proves the point of wave 1 in-game; clear issues before anything wave-2-shaped.

### Custom-wizard era (gated on ~4+ identities)

- [ ] **Naming for custom combos** — presets carry curated names (the recipe book *is* the iconic-name table); the custom wizard needs derived names ("Frost Storm") vs. unlockable iconic names for famous combos.
- [ ] **Blandness watch** — combinatorial content guarantees coverage, not soul. Wave-1 presets mask it; re-check honestly at custom-wizard time.

### Element wave (arrives together with enemy-resist promotion, D1)

- [ ] **New identities** (Fire/Cold/Lightning…) — each multiplies against all existing forms.
- [ ] **Element effect flavours** — ⚠ levers dressed as flavours (ice-slow is CC, lightning-crit is a damage stat, fire-burn is a DoT). **Leading candidate: "the element whispers, augments make it shout" (b-lite)** — every element carries a token inherent effect of equivalent near-zero cost (ice: tiny brief slow; fire: tiny burn; lightning: hair of crit damage), augments scale it into real power. Requires: matrix EoT-row amendment ("token inherent" exception) + Balancer promise of equivalence. Fallback: pure affinity routing (element = damage channel; all effects opt-in via augments, D3-style).
- [ ] **Damage-model note**: mono-typed hits + typeless weapon are locked as deliberate absences (`design-stats.md` §5C, with no-immunities rule). Composite hits reopen only if this wave somehow demands them (not expected).

---

## Parked thread — Prototype coverage gaps (PoE2 re-assessment, 2026-07-04)

Full sweep of [ref-poe2-skills.md](ref-poe2-skills.md) against the 12 prototypes. Everything maps to an existing prototype, a candidate below, or a resolved/blocked family. Candidates are **not committed** — each needs its own design pass when picked up. Names follow the `targeting_pattern` convention. *(Under the composition model, each new prototype multiplies against the existing form/identity pools.)*

### Candidates needing only a new delivery shape

| Candidate | Covers | PoE2 exemplars |
|---|---|---|
| path_burst | Piercing projectile line, hits each enemy once | Frostbolt, Glacial Lance, Plasma Blast |
| moving_zone_tick | Tick zone travelling along cast direction | Ball Lightning, Solar Orb, Twister |
| chain_burst | Hit jumps to N nearby enemies | Arc, Lightning Arrow, Shockchain Arrow |
| cone_burst | Directional cone AoE (first non-circular burst shape) | Ice Shot, Freezing Shards, Bone Blast |
| wall_zone_tick | Line-shaped zone; optional collision-blocking variant | Flame Wall, Frost Wall, Bone Cage |
| detonated_zone_burst | Place charges, detonate on second press (player-triggered; cf. enemy-triggered triggered_zone_burst) | Detonating Arrow, Unleash |
| zone_debuff | Zone with `DamagePattern None` + `DebuffEotId` — area counterpart of entity_debuff | Cold Snap, Gas Grenade |
| zone_buff | Placed zone buffing the player inside it | Sigil of Power, Consecrate |
| self_buff_burst | Activation whose payload is a timed self-buff (war cry) | Seismic Cry, Infernal Cry |

### Candidates gated on a new mechanic or unparking a surface

| Candidate | Prerequisite | PoE2 exemplars |
|---|---|---|
| charge_release_burst | Accumulation-state mechanic (where stacks live = matrix conversation) | Boneshatter, Gathering Storm |
| movement_burst | Lift the "no movement skills" rule; leap/dash + burst at landing | Leap Slam, Flicker Strike |
| movement_path_burst | Same rule lift; damage along the travel route | Shield Charge, Stampede |
| summon_minion | Unpark minion stats (own HP/damage, targetable, own AI) | Skeletals, Raise Zombie |
| summon_totem | Minion-lite stats (stationary, destructible, auto-attacks). A "dumb" pulse totem needs nothing — it is fixed_zone_tick with a prop | Ballistas, Ancestral Warrior Totem |
| corpse_consume_burst | A corpse resource system (the system is the work) | Detonate Dead, Volatile Dead, Offerings |

### Resolved / blocked families (for the record)

- **Damage amplification** → Route A: enemy-owned "Exposed"-style EoT via existing debuff machinery (promotion path noted on the parked "-to-enemy" line in `design-stats-deferred.md`).
- **Conditional damage** → Route B: crit-condition augments ("always crit vs enemies below 30% HP / vs Slowed").
- **Raw per-skill conditional multipliers** → blocked by design, no route, no exception.
- **Alternative resource costs** → a Focus-cost-model variant on existing prototypes, never a new prototype. **Ammo/charge economies** → no analogue.

## Other parked threads

- **Full roster beyond wave 1** — gated on the wave-1 playtest verdict on the identity framework.
- **Weapon augment design** — what sockets into a weapon? Defensive equipment augments don't fit; the slot needs its own augment family. (The weapon-hosted identity-rune variant was assessed and rejected 2026-07-04 — conflicts with skill-owned DamageType and re-skins all 5 slots at once.)
- **Acquisition specifics / recipe costs** — deferred to the reward/economy surface (D4).

---

## Session log

- **2026-07-03** — Doc created. Opened Thread 1 (identity system); proposed the Budget/Identity lever split.
- **2026-07-04** — Traced stat locations in code; formalised the Stat Ownership Matrix; decided damage type is skill-authoritative. Matrix promoted to `design-stats.md`.
- **2026-07-04 (grill session)** — Thread 1 resolved: framework confirmed, tier rule, damage-type mutability, no mechanical identity lever. Thread 2 resolved: deep first wave on 4 prototypes. Rationale/doc pass merged (PRs #8–#10).
- **2026-07-04 (PoE2 coverage)** — Jump-off mapping + full coverage re-assessment (15 candidate prototypes). Two-route resolution for rule-blocked families adopted.
- **2026-07-04 (resumed session)** — Thread 3 structural questions answered (Phys/Magic pairs; varied contrast axes; classic names + VFX guardrail; prototypes = internal skills). Identity-rune idea generalised into **composition: prototype + form + identity — ADOPTED, presets-first**; Thread 3 absorbed. Damage model settled (mono-typed + typeless weapon; no-immunities rule → design-stats.md §5C). Element effects flagged levers-not-flavours; b-lite framing leading.
- **2026-07-04 (lock-in)** — All 8 forms + 16 presets approved and **promoted to `design-skills.md` v2 section**, with matrix addendum §5D (craft-time components are not stat surfaces) and `design-progression.md` recipe-book note. This doc stripped to open threads. Next: sanity check, then formalise/ship proposal.
