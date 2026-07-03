# Skill System — Brainstorming Scratchpad

> **Status: OPEN brainstorm, not committed design.** This is a working doc spanning multiple sessions.
> Nothing here is final until it is resolved and moved into `design-skills.md`. Rejected ideas get deleted, not archived.
> For the committed skill design (the 12 v1 prototypes, AoE math, targeting rules), see [design-skills.md](design-skills.md). For stat ownership, see [design-stats.md](design-stats.md) (Section 5 — the governing matrix).

---

## What this doc is for

Designing the **v2 named skills** — the flavoured skills (Strike, Cyclone, Blizzard, …) cloned from the 12 v1 prototypes. The framework and wave shape are decided (see Committed foundations below); the active work is **designing the 8 first-wave skills themselves**.

## Hard constraints (committed elsewhere — do not relitigate here without flagging)

- **No per-skill damage multiplier.** Every hit deals `weapon base × stat block`. Rationale in `design-skills.md`.
- **Named skills are standalone clones of prototypes.** `BasedOn` is documentation only — no runtime template link.
- **EoTs, mines, traps come from augments** on any damage-dealing skill. Narrow exception: a `DamagePattern == None` skill carries a single `DebuffEotId` as its base behaviour (issue #11 implements).
- **Any augment sockets into any skill** — no tag gates.
- **Damage type is fixed at skill creation** — not re-rollable by crafting; only a socketed augment overrides at fire time.
- **Skill tier advances budget levers only** — never hit size; sibling parity is defined at equal tier.
- **No character attack/cast/CDR stat** — throughput is skill-owned + the one global weapon CDR property.

---

## Committed foundations (reference — resolved 2026-07-04)

### Budget/Identity lever framework

Every lever on a named skill is one of two kinds:

- **Budget levers** — cooldown, tick rate, AoE/zone radius, Focus cost, wind-up. They move throughput, so across siblings of the same prototype they must net to the same power budget **at equal tier** ("no free lunch" — enforced as design discipline, exchange rates Balancer-owned). Trading among them changes the skill's *shape*, not its power.
- **Identity levers** — damage type, VFX/animation/sound, wind-up-as-telegraph, and *which* budget lever the tier track advances. Free: they place the skill in the build ecosystem without moving throughput.

**Identity = prototype (delivery fantasy) × damage type × VFX × budget-spend shape × tier track.**

No stronger mechanical identity lever for now — budget-spend *rhythm* is the mechanical identity. Escape hatch if the first wave feels same-y in playtesting: revisit a signature-EoT carve-out consciously (matrix amendment in `design-stats.md` first).

### First-wave shape

**Deep, not broad: two sibling clones each on 4 prototypes = 8 named skills.** Siblings side by side are the test the framework must survive. Acquisition: crafted via Craft New; recipe costs deferred to the reward/economy surface (D4 in `design-stats-deferred.md`).

| Host prototype | Why chosen |
|---|---|
| entity_burst | Starter/most-played delivery; maximum playtest exposure for siblings |
| self_burst | Purest test of the cooldown↔AoE budget trade |
| fixed_zone_tick | Brings the zone lever set (radius, duration, tick rate) into the test |
| self_channeled_tick | The distinct rhythm (channel vs. pulses); most dependent on per-skill VFX work |

### Engine prerequisites (issues to write when design is done)

1. **Per-skill VFX mapping** — the engine has none today (`technical-systems.md`: animation/VFX is delivery-driven; all skills of a `SkillType` look identical; the cyclone ring is hardcoded to `SkillType.Channeled`). Write the issue *after* the 8 skills are designed, when the requirements are known.
2. **`DebuffEotId` + `TickRate` on `SkillData`** — already written: **issue #11** (`ready`).

---

## ACTIVE — Thread 3: Design the 8 first-wave named skills

For each sibling pair: both clones share the prototype's delivery fantasy; they must differ in **shape** (budget-spend), **element**, **tier track**, and **feel** (VFX/telegraph) — never in power at equal tier.

### Per-skill worksheet (fill in per skill)

| Field | Meaning |
|---|---|
| Name | Flavour name (Strike, Nova, Blizzard, …) |
| Prototype | Which of the 4 hosts it clones |
| Damage type | Fixed at creation (see open question 1) |
| Budget-spend shape | Where the budget goes: e.g. heavy-slow (long CD, big radius, wind-up) vs. rapid-small |
| Tier track | The one budget lever tier upgrades advance (identity axis) |
| VFX/telegraph sketch | One line of felt identity |

### Roster grid (to fill during discussion)

| # | Prototype | Skill A | Skill B |
|---|---|---|---|
| 1 | entity_burst | — | — |
| 2 | self_burst | — | — |
| 3 | fixed_zone_tick | — | — |
| 4 | self_channeled_tick | — | — |

### PoE2 reference jump-off (from [ref-poe2-skills.md](ref-poe2-skills.md) — adapt concepts, not names)

PoE2 organises by weapon; we organise by prototype (delivery is weapon-driven for us). Filtered to concepts compatible with our constraints — no damage multipliers, no stack/charge builders, no travel projectiles, no corpses/minions:

| Host prototype | PoE2 inspiration pool | The shape idea worth stealing |
|---|---|---|
| entity_burst | Snipe, Perfect Strike, Crossbow Shot, Rake, Lightning Bolt | The **fast-reliable vs. charged-heavy** contrast: Snipe/Perfect Strike are wind-up + long-cooldown single hits; Crossbow Shot/Rake are rapid low-commitment pokes. Maps 1:1 onto our budget levers (wind-up + CD vs. neither) |
| self_burst | Ice Nova, Shock Nova, Wave of Frost, Earthquake | Nova is the archetype. **Earthquake's delayed shockwave** is the interesting one: a wind-up self-burst reads as a "brace, then boom" telegraph — instant-small vs. delayed-huge siblings |
| fixed_zone_tick | Firestorm, Cold Snap, Galvanic Field, Flame Wall, Orb of Storms | Two zone temperaments: **the storm** (big radius, long duration, slow ticks — Firestorm) vs. **the trap-floor** (small, short, fast ticks — Galvanic Field). Same budget, opposite rhythm |
| self_channeled_tick | Whirling Slash/Whirlwind Lance, Incinerate, Mana Tempest | **Melee spin vs. magic storm**: the spin is tight-radius fast-tick (our prototype already has the Melee tag); Incinerate/Mana Tempest suggest a wider, slower-tick, higher-drain "channelled tempest" sibling |

Concepts the reference makes tempting but our rules exclude (raise consciously if wanted): conditional damage bonuses (Cull the Weak, Shattering Palm — that's a damage multiplier), stack-and-release mechanics (Boneshatter, Gathering Storm — new mechanic, not in v2 scope), resist-shredding curses (Flammability — "-to-enemy" stats are Parked). Temporal Chains is worth remembering later: it's exactly our entity_debuff, so PoE2's curse list is the natural inspiration pool when debuff-pattern named skills get their wave.

### Open questions for the discussion

1. **Damage-type palette.** The game currently has exactly two damage types — Physical and Magic (`design-mechanics.md`: elemental types are future expansion). Damage type is supposed to be the strongest identity lever, but with 8 skills and 2 types it can't differentiate siblings alone. Options: (a) design the wave within Phys/Magic and lean harder on shape/VFX; (b) introduce the first elemental types (Fire/Cold/…) with this wave — which pulls in enemy resist entries, a DamageType enum extension, and per-type damage-number colours; (c) hybrid — one elemental pair as the pilot.
2. **Sibling contrast pattern.** Do all 4 pairs use the same contrast (e.g. always heavy-slow vs. rapid-fast), or does each pair explore a different budget trade? Same pattern is cleaner to read as a system; varied patterns test more of the framework.
3. **Naming convention.** Classic ARPG vocabulary (Strike, Cleave, Nova, Blizzard) vs. an original naming scheme. Classic names carry free player expectations — which cuts both ways.
4. **Do prototypes stay craftable in v2**, or do named skills replace them in the crafting pool once they exist? (`design-skills.md` says all 12 prototypes are craftable in v1; v2 stance unstated.)

---

## Parked thread — Prototype coverage gaps (PoE2 re-assessment, 2026-07-04)

Full sweep of [ref-poe2-skills.md](ref-poe2-skills.md) against the 12 prototypes. Everything maps to an existing prototype, a candidate below, or a rule-blocked family. Candidates are **not committed** — each needs its own design pass when picked up. Names follow the `targeting_pattern` convention.

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

### Formerly rule-blocked families — RESOLVED via two routes (2026-07-04, no rule amendment needed)

- **Damage amplification** (Malice, Wither, Vulnerability, Oil Grenade) → **Route A: enemy-owned debuff.** "This enemy takes +X% damage" is a state on the *enemy* (a new EoT type, e.g. "Exposed"), delivered via the existing debuff machinery (EoT augments, entity_debuff clones, zone_debuff). No skill carries a multiplier; every skill benefits equally from the debuffed enemy; amplification must be actively delivered, so the crafting-economy anchor holds. Prerequisite when picked up: consciously promote the ⚪ Parked "-to-enemy debuff stats" line in `design-stats-deferred.md`.
- **Conditional damage** (Cull the Weak, Shattering Palm, Killing Palm, Blood Hunt) → **Route B: crit conditions.** Execute/punish fantasies become conditions on the one sanctioned hit-event multiplier: augments like "hits vs enemies below 30% HP always crit" or "always crit vs Slowed enemies." Bounded by CritDamage, rides entirely on existing math, lives on augments (build choice, not skill stat).
- **Still blocked, by design:** a raw per-skill conditional multiplier ("this skill deals 1.4× when X") — the exact thing the no-multiplier rule exists to prevent. No route, no exception.
- **Alternative resource costs** (Dark Pact, Exsanguinate, Mana Tempest) — a Focus-cost-model variant on existing prototypes, never a new prototype.
- **Ammo/charge economies** (Emergency Reload, Power Siphon) — presuppose resource systems with no analogue here.

## Other parked threads

- **Full roster beyond the first wave** — gated on the first-wave playtest verdict on the identity framework.
- **Acquisition specifics / recipe costs** — deferred to the reward/economy surface (D4).

---

## Session log

- **2026-07-03** — Doc created. Opened Thread 1 (identity system); proposed the Budget/Identity lever split.
- **2026-07-04** — Traced stat locations in code; formalised the Stat Ownership Matrix; decided damage type is skill-authoritative. Matrix promoted to `design-stats.md`.
- **2026-07-04 (grill session)** — Thread 1 fully resolved: framework confirmed, tier rule (budget levers only, parity at equal tier), damage-type mutability, no mechanical identity lever. Thread 2 resolved: deep first wave, 2 siblings × 4 prototypes. Settled outside this doc: no-multiplier rationale documented; speed-family stance decided & promoted; Boots drift → docs follow code; multiplier drift fixed.
- **2026-07-04 (later)** — `InherentEotIds` resolution amended: **narrow, not delete** (entity_debuff's Slow lives there) → single `DebuffEotId`, valid only when `DamagePattern == None`. Code change = issue #11. Doc cleaned and restructured for the Thread 3 discussion: resolved threads compressed into "Committed foundations", worksheet + roster grid added, 4 open questions posed (damage-type palette is the big one).
- **2026-07-04 (PoE2 coverage)** — Added PoE2 jump-off mapping for the 4 wave hosts. Full coverage re-assessment recorded as parked thread: 15 candidate prototypes (9 delivery-shape, 6 mechanic/surface-gated). **Two-route resolution adopted** for the rule-blocked families: amplification → enemy-owned "Exposed"-style EoT (Route A); conditional damage → crit-condition augments (Route B); raw per-skill conditional multipliers stay blocked with no exception. No amendment to the no-multiplier rule.
