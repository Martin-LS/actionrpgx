# Skill System — Brainstorming Scratchpad

> **Status: OPEN brainstorm, not committed design.** This is a working doc spanning multiple sessions.
> Nothing here is final until it is resolved and moved into `design-skills.md`. Rejected ideas get deleted, not archived.
> **The committed skill design lives elsewhere:** the 12 prototypes, AoE math, the Budget/Identity framework, the composition model (prototype + form + identity), and the wave-1 forms & the 16 reachable combos are all in [design-skills.md](design-skills.md); stat ownership (incl. craft-time-components rule §5D) in [design-stats.md](design-stats.md); crafting flow in [design-progression.md](design-progression.md).

---

## What this doc is for

Working space for the **v2+ skill system questions that are still open**. The composition model is adopted and wave 1 is designed (locked 2026-07-04); what remains here is what comes next and what was deliberately deferred.

## Open threads

### Wave-1 follow-ups (needed before shipping)

- [x] ~~**Sanity check the locked wave-1 design**~~ DONE 2026-07-04 — design is rule-clean (no matrix violations, all form shapes map to existing `SkillData` fields incl. the new `TickRate`, no name collisions, guardrails pass). Two engine gaps found and added to the `design-skills.md` prerequisites: **WindUp only works on Position paths** (heavy + quake forms need it on Entity/Self), and **tier does not modify skill stats in code yet** (tier tracks = part of composition work).
- [x] ~~**Forms & identities: craftable items or internal data?**~~ RESOLVED 2026-07-05 — **catalogue entries (Model C)**, not inventory items: prototype/form/identity are registry authoring data, selected in a **craft wizard** with a resource cost debited per step. Presets-first superseded by wizard-first; presets dissolve (reachable skills = combos the registries allow). Naming = derived phrase + iconic overrides. Cost = resource bundle over a unified currency+material registry, with a reserved (empty in v1) requirements seam. All promoted to `design-skills.md` / `design-progression.md`.
- [ ] **Formalise & ship proposal** — smallest slice that proves the point of wave 1 in-game; clear issues before anything wave-2-shaped. *(Next thread — the wizard + registries + cost model above is now the thing to slice into issues.)*

### Custom-wizard era (gated on ~4+ identities)

- [x] ~~**Naming for custom combos**~~ RESOLVED 2026-07-05 — **derived phrase** (form/identity/prototype → one word each, ~25-entry map) as the default, **+ thin iconic-override table** for signature combos. Replaces hand-named presets. Wave 1 ships raw `[prototype][form][identity]` as placeholder. Promoted to `design-skills.md`. *(Remaining: author the actual word-map and pick which combos get iconic overrides — full-roster work.)*
- [ ] **Blandness watch** — combinatorial content guarantees coverage, not soul. The wave-1 **wizard now exposes composition directly** (presets no longer mask it), so blandness is visible from wave 1 — re-check honestly at playtest.

### Element wave (arrives together with enemy-resist promotion, D1)

- [ ] **New identities** (Fire/Cold/Lightning…) — each multiplies against all existing forms.
- [ ] **Element effect flavours** — ⚠ levers dressed as flavours (ice-slow is CC, lightning-crit is a damage stat, fire-burn is a DoT). **Leading candidate: "the element whispers, augments make it shout" (b-lite)** — every element carries a token inherent effect of equivalent near-zero cost (ice: tiny brief slow; fire: tiny burn; lightning: hair of crit damage), augments scale it into real power. Requires: matrix EoT-row amendment ("token inherent" exception) + Balancer promise of equivalence. Fallback: pure affinity routing (element = damage channel; all effects opt-in via augments, D3-style).
- [ ] **Damage-model note**: mono-typed hits + typeless weapon are locked as deliberate absences (`design-stats.md` §5C, with no-immunities rule). Composite hits reopen only if this wave somehow demands them (not expected).

---

## Active thread — Prototype expansion candidates (PoE2 re-assessment, 2026-07-04)

> **⚠️ NOT IMPLEMENTED YET.** Pulled out of the parking lot into active discussion (2026-07-05). Every candidate below is a **discussion item, not committed design and not built** — none exists in code, none has a GitHub issue. Each still needs its own design pass and must be fully nailed down *here* before it becomes an implementation issue. Listed ≠ approved.

Full sweep of [ref-arpg-skills.md](ref-arpg-skills.md) (PoE2 section) against the 12 prototypes. Everything maps to an existing prototype, a candidate below, or a resolved/blocked family. Names follow the `targeting_pattern` convention. *(Under the composition model, each new prototype multiplies against the existing form/identity pools — adding one is a whole new row of reachable combos, not a single skill.)*

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

- **Starter loadouts (preset revival, if ever)** — pre-composed skills handed to new characters. A loadout/onboarding feature over Model C data (drop pre-built skill instances into a starting inventory), *not* the old preset-recipe-book concept. Parked until onboarding is designed.
- **"Favourite recipe" bookmark** — player QoL to save a prototype+form+identity tuple for fast re-craft. A thin saved-tuple list over Model C data. Parked until the wizard exists and the need is felt.
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
- **2026-07-05** — Prototype coverage candidates **un-parked** into an active thread, explicitly flagged NOT IMPLEMENTED — surfaced for design discussion, each still needs its own pass before becoming an issue. No code or design committed by this move.
- **2026-07-05 (grill session)** — Thread "forms & identities as items vs data" resolved: **Model C** (catalogue entries, craft wizard, cost-per-step). Two locked decisions superseded — **presets-first → wizard-first** (presets dissolve into reachable catalogue combos) and **hand-named presets → derived phrase + iconic overrides** (raw `[proto][form][identity]` placeholder in wave 1). Crafting cost formalised as a **resource bundle** over a unified currency+material registry, with a reserved (v1-empty) `Requirements` predicate seam. Confirmed the architecture also accommodates future chance-based craft outcomes *only* via tier/augment/component gambles — never raw per-skill stat boosts (would break the no-per-skill-multiplier + budget-parity rules). Promoted to `design-skills.md` + `design-progression.md`. Next: formalise/ship proposal (slice the wizard + registries + cost model into issues).
