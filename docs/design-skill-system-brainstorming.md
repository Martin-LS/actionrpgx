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

## Thread 3: Design the first wave — ABSORBED INTO THREAD 4 (2026-07-04)

> **Composition adopted (see Thread 4): named skills are now preset recipes over `prototype + form + identity`.** This thread's artifacts survive re-labelled: the four structural answers below still govern; the contrast axes from question 2 *are* the form pairs; the roster grid becomes the **preset recipe book**; the tabled pair-1 proposal (Strike/Smite) becomes the entity_burst form pair (swift/heavy) + identity assignment. Active design work continues in Thread 4 terms: author 8 forms + 2 identities.

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

**Pair pattern (decided 2026-07-04): each pair = one Physical + one Magic sibling.** Phys couples to Str builds, Magic to Int builds — real ecosystem placement with today's two types. Elements deferred to their own wave, arriving together with enemy-resist promotion (D1).

| # | Prototype | Skill A (Physical) | Skill B (Magic) |
|---|---|---|---|
| 1 | entity_burst | Strike *(PROPOSED)* | Smite *(PROPOSED)* |
| 2 | self_burst | — | — |
| 3 | fixed_zone_tick | — | — |
| 4 | self_channeled_tick | — | — |

**Pair 1 proposal (TABLED mid-discussion 2026-07-04, awaiting approval):** Strike (Physical) = instant, short CD, moderate Focus, tier track: cooldown. Smite (Magic) = wind-up telegraph, longer CD, very low Focus, tier track: focus cost. Two flags raised with it: (a) **"heavy hitter" is impossible on entity_burst** (no multiplier + no radius ⇒ the commitment axis is honestly *tempo vs. Focus-efficiency*, not big-hit-vs-small-hit); (b) **Range stays off the budget-lever list** for now — a long-range entity sibling (Snipe fantasy) would need Range made tradeable, and weapon-driven delivery makes melee-at-range visually incoherent; revisit if a ranged-identity skill wants it.

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

1. ~~**Damage-type palette.**~~ **ANSWERED 2026-07-04: option (a)** — first wave stays within Phys/Magic; every sibling pair = one Physical + one Magic skill (see roster grid). Elements arrive as their own wave together with enemy-resist promotion. The deeper phys/magic *model* question (mono-typed vs composite hits) is a separate revisit thread below.
2. ~~**Sibling contrast pattern.**~~ **ANSWERED 2026-07-04: varied — each pair explores a different budget trade** (tests four exchange rates instead of one; avoids the roster reading as a fast/slow mode toggle). Axes per pair: entity_burst = **commitment** (rapid poke vs. wind-up heavy); self_burst = **telegraph timing** (instant small vs. delayed huge); fixed_zone_tick = **duration↔tick-rate** (long slow storm vs. short fast trap-floor); self_channeled_tick = **radius↔drain** (tight cheap spin vs. wide expensive tempest). Accepted cost: four exchange rates for the Balancer.
3. ~~**Naming convention.**~~ **ANSWERED 2026-07-04: classic ARPG vocabulary, with a guardrail** — names must not promise mechanics we don't deliver (no "Chain Lightning" without chaining, no "Leap Slam" without movement). Names carry the flavour; mechanics stay on-budget.
4. ~~**Do prototypes stay craftable in v2?**~~ **ANSWERED 2026-07-04: all 12 stay craftable through the first wave — but conceptually they are *internal* skills, not player-facing content.** They exist as (a) the authoring basis named skills are cloned from, and (b) testable base skills proving the skill system works — including serving as the playtest control group for whether identity earns its keep (if players keep crafting plain fixed_zone_tick next to Blizzard/Firestorm, the identity framework isn't working). Eventual retirement from the player craft pool (likely via the `SkillKind` gate) is a full-roster-time decision, not now.

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

## ACTIVE — Thread 4: The three-component model (prototype + form + identity)

*(Opened 2026-07-04, promoted from the identity-rune parked idea. If adopted, this reshapes Thread 3: named skills stop being hand-authored clones and become craft-time compositions.)*

**Agreed frame (2026-07-04):** `crafted skill = prototype + form + identity`, **fused and flattened at craft time** into a standalone snapshot item (preserving: standalone instances, damage type fixed at creation, Balancer-owned budget levers, the ownership matrix).

| Component | Owns | Composability |
|---|---|---|
| **Prototype** | Delivery chassis: targeting shape, damage pattern, skill type, base budget stats | The 12 internal base skills |
| **Form** | Budget-spend shape (where the budget goes) + **tier track** (which lever tier advances) | Per-prototype: each prototype offers 2–3 forms (swift/heavy for bursts, storm/trap-floor for zones) |
| **Identity** | Element (damage type) + VFX/audio skin + name fragment | Universal: composes with any prototype × form |

**Element effect flavours — flagged, NOT settled (element-wave decision):** damage types will bring effect flavours (ice slows/freezes, lightning boosts crit damage, fire burns). ⚠ These are **levers dressed as flavours** — CC, crit stats, and DoTs are real power. Two rule-clean routes when elements arrive: (a) **affinity routing** — identity amplifies matching socketed augments (fire identity → Burn augments trigger more; affinity precedent exists on the Wand) — preserves "EoTs are augment-only" and identity-budget-neutrality; (b) **conscious amendment** — inherent ailments PoE-style, requiring the matrix EoT row amended *and* each ailment costed into the budget (identities stop being free). Do not bake effects into identities without picking a route.

**Decisions on this thread (2026-07-04):**
- ✅ **ADOPTED — composition is the skill architecture, shipped presets-first.** The data model is `prototype + form + identity` (fused & flattened at craft) from day one; the wave-1 crafting UI is a **recipe book of presets only** ("Craft: Blizzard" fills all three components) — the custom three-choice wizard is a later unlock, shipped when the pools are big enough that combining feels like creating (~4+ identities). This **replaces the hand-authored named-skill wave**: wave 1 = author **8 forms (2 per host prototype) + 2 identities (Physical/Magic)** → 16 craftable preset skills.
  *Justification: at wave-1 scale (2×2) the combo picker yields nothing a player can feel — all combos are authored designs either way, and "did I create this?" is weak when your Blizzard is everyone's Blizzard. The model's payoff is multiplicative growth (the element wave's ~3 identities become ~24 new skills for 3 authored components — support-gem-style network effects) and the creation feeling arriving with pool size. Presets-first buys the architecture without a premature crafting-UI project, with zero player-visible loss at wave 1 and no retrofit later.*
- ✅ **VFX guardrail committed** (extends the naming guardrail): visuals must not promise mechanics or elements we don't deliver. First-wave identity skins are Physical (kinetic/brutal) and Magic (arcane) themed — **no elemental cosplay**. Blizzard does not exist until ice damage does.
- ✅ **Element effect framing — leading candidate = "the element whispers, augments make it shout" (b-lite):** every element carries a token inherent effect (flavour-grade, below build-power threshold: ice = tiny brief slow, fire = tiny burn, lightning = hair of crit damage), and augments scale it into real power. Identity-neutrality preserved by construction (every element carries one token effect of equivalent near-zero cost). Requires at element-wave time: matrix EoT row amendment ("token inherent" exception) + Balancer promise that token effects stay equivalent. Fallback: pure affinity routing (element = damage channel, all effects opt-in via augments, D3-style). **Decide at element wave, alongside enemy resists.**

**Open questions on this thread:**
- [x] ~~Does this model replace the Thread 3 hand-authored wave?~~ **Yes — adopted above.** Wave 1 = 8 forms + 2 identities → 16 preset skills.
- [x] ~~Craft UI shape?~~ **Presets-first** (recipe book at wave 1; custom wizard later — see adoption decision).
- [ ] Naming: presets carry curated names (the recipe book *is* the iconic-name table); still open for the later custom wizard — derived names ("Frost Storm") vs. unlockable iconic names for famous combos.
- [ ] Are forms and identities separate craftable *items* (two D4 material sinks) or internal data only, with materials spent directly in the craft? Interacts with D4; decide before the crafting screen is built.
- [ ] Known risk to watch: combinatorial content guarantees coverage, not soul — the blandness trap. Wave-1 presets mask it; re-check at custom-wizard time.

## Parked idea — Identity runes (raised 2026-07-04, unassessed beyond first pass)

**The idea:** a "rune" item carries a named skill's identity package (element + VFX skin + possibly tier track) — e.g. a Blizzard rune vs. a Firestorm rune making the same zone prototype into two different skills.

First-pass assessment against committed rules:

- **Weapon-hosted runes: likely dead.** Conflicts with skill-owned DamageType (matrix), and one weapon serves all 5 slots — a weapon rune would re-skin every skill at once, destroying per-skill identity. *Salvageable observation:* weapons currently have **no augment story** (defensive equipment augments make no sense on a weapon) — that gap is real and needs its own answer someday, rune-shaped or not.
- **Skill-hosted, freely swappable: dead as stated.** Violates "damage type fixed at creation, only augments override at fire time."
- **Skill-hosted, fused at craft time: promising.** "Craft New named skill = pick prototype + fuse identity rune" would make the rune the *recipe ingredient* that produces the standalone clone. Everything committed survives (standalone clone, type fixed at creation, budget levers Balancer-owned), and it gives the D4 economy a concrete material sink + makes the roster combinatorial (rune pool × prototypes) instead of hand-authored. Open questions if picked up: are first-wave named skills fixed recipes or free rune×prototype combos? Does a rune carry the budget-spend shape too, or only element/VFX? How does this interact with per-skill VFX mapping (rune-driven VFX would *be* that system)?

Status: parked until the first-wave roster design forces the acquisition question (open question 4 territory / D4).

## ~~Revisit later~~ RESOLVED same day — the Physical/Magic damage-model split (2026-07-04)

Discussed and settled: **mono-typed hits + typeless weapon root confirmed as deliberate design** (D4 lineage, consciously chosen over the D2/PoE composite lineage — weapons there carry typed components and one hit checks multiple resists with conversion math). Now recorded as a Deliberate Absence in `design-stats.md` §5C, together with the companion rule it forced:

- **No enemy immunities — resistances cap below 100%, ever.**
  *Justification: the "does every enemy take at least some physical?" worry is the PoE immune-mob problem in disguise; with mono-typed hits, immunity would hard-wall mono-typed builds. Capping resists solves it without a phys-floor on every hit.*
- **Mono-typed / typeless-weapon model.**
  *Justification: (1) crafting anchor — one weapon number serves all builds, no Str/Int weapon fork; (2) attribute identity — phys→Str, magic→Int stays crisp only if a hit scales one channel; (3) keeps the D3 hit-event model closed (composite portions/conversion are the classic painful retrofit).*

Reopens only if the element wave someday demands composite hits (not expected — elements can stay mono-typed per hit).

## Other parked threads

- **Full roster beyond the first wave** — gated on the first-wave playtest verdict on the identity framework.
- **Acquisition specifics / recipe costs** — deferred to the reward/economy surface (D4).
- **Weapon augment design** — what sockets into a weapon? Defensive equipment augments don't fit; the slot needs its own augment family (or runes, per above). Raised 2026-07-04.

---

## Session log

- **2026-07-03** — Doc created. Opened Thread 1 (identity system); proposed the Budget/Identity lever split.
- **2026-07-04** — Traced stat locations in code; formalised the Stat Ownership Matrix; decided damage type is skill-authoritative. Matrix promoted to `design-stats.md`.
- **2026-07-04 (grill session)** — Thread 1 fully resolved: framework confirmed, tier rule (budget levers only, parity at equal tier), damage-type mutability, no mechanical identity lever. Thread 2 resolved: deep first wave, 2 siblings × 4 prototypes. Settled outside this doc: no-multiplier rationale documented; speed-family stance decided & promoted; Boots drift → docs follow code; multiplier drift fixed.
- **2026-07-04 (later)** — `InherentEotIds` resolution amended: **narrow, not delete** (entity_debuff's Slow lives there) → single `DebuffEotId`, valid only when `DamagePattern == None`. Code change = issue #11. Doc cleaned and restructured for the Thread 3 discussion: resolved threads compressed into "Committed foundations", worksheet + roster grid added, 4 open questions posed (damage-type palette is the big one).
- **2026-07-04 (PoE2 coverage)** — Added PoE2 jump-off mapping for the 4 wave hosts. Full coverage re-assessment recorded as parked thread: 15 candidate prototypes (9 delivery-shape, 6 mechanic/surface-gated). **Two-route resolution adopted** for the rule-blocked families: amplification → enemy-owned "Exposed"-style EoT (Route A); conditional damage → crit-condition augments (Route B); raw per-skill conditional multipliers stay blocked with no exception. No amendment to the no-multiplier rule.
- **2026-07-04 (resumed session)** — Thread 3 structural questions all answered: Phys/Magic sibling pairs (elements deferred to their own wave with enemy resists); varied contrast per pair (4 axes); classic names with the no-false-promises guardrail; all 12 prototypes stay craftable but are *internal* skills (authoring basis + system proof + playtest control group). Pair 1 (Strike/Smite) proposed, then tabled. Identity-rune idea assessed (weapon-hosted dead, fused-at-craft promising) and generalised into **Thread 4: prototype + form + identity**. Same day: three-component split agreed; VFX guardrail committed (no elemental cosplay before elements exist); element effect flavours flagged as levers-not-flavours with "whisper/shout" (b-lite) as leading framing; Phys/Magic damage model settled (mono-typed hits + typeless weapon = deliberate, D4-lineage; **no-immunities rule** committed to design-stats.md §5C). **Composition ADOPTED, presets-first** — Thread 3 absorbed; wave 1 = author 8 forms + 2 identities → 16 preset skills. Next: design the 8 forms (start from the tabled Strike/Smite as entity_burst's swift/heavy forms).
