# Game Design Document — Skills

> Part of the design docs. See `design-mechanics.md` for combat, characters, focus, damage types, and run structure. See `design-augments.md` for Skill Augments, Equipment Augments, and augment prototypes.
> Living document — details will evolve as the game is playtested.

---

### Skills

**Design rule for skills:**
- **EoTs and secondary effects (mines, traps) are added by augments, not baked into skills.** A skill's base behaviour is its damage delivery. Augments add what happens on top of that. **Narrow exception:** a debuff-pattern skill (`DamagePattern == None`, e.g. entity_debuff) carries a single `DebuffEotId` — its debuff *is* the base behaviour, not an add-on. No damage-dealing skill may ever carry an inherent EoT (see the ownership matrix in `design-stats.md`).

**Skill slot vs. Equipment Augment — the dividing line:**
- **Skill slot = things you actively trigger** (requires a button press — Active, Channeled, toggled auras, War Cries).
- **Equipment Augment = things that happen automatically** (no button press, gear-driven).

There is no Passive skill type on the skill bar. Everything in a skill slot requires player input to fire. Persistent stat buffs and background effects belong on Equipment Augments (e.g. Mending, Retaliation) — not skill slots. Skills like War Cry stay on the skill bar because they are intentional activations with cooldowns, not background passives.

**Named skills are clones of prototypes — no runtime template system.** When a named skill (e.g. Strike) is created from a prototype (e.g. entity_burst), it is a complete standalone definition. All values are copied at authoring time; the prototype has no runtime relationship to the named skill after that. Changes to a prototype never cascade to existing named skills or crafted instances. The `BasedOn` field on `SkillData` records which prototype a named skill was cloned from — documentation only, no runtime behaviour. This keeps item instances stable and predictable: a crafted Strike is never changed by a prototype balance update without the designer explicitly editing Strike's definition.

**Skill tags — currently limited to delivery-resolution and AoE.** Skills carry two categories of tag:

- **`AoE`** — marks skills that damage all enemies within a radius. Introduced now because the radius modifier math needs a hook. All skills that deal damage to a radius (self/zone/tracked) carry this tag.
- **`Melee` / `Range`** — delivery-override tags used internally by `WeaponController` to determine how entity hits are resolved. A skill with `Melee` always fires a melee swing regardless of equipped weapon; a skill with `Range` always fires a ranged projectile. No delivery tag means the skill inherits the weapon's preferred delivery type (weapon-adaptive). Only `self_channeled_tick` currently carries `Melee` — it spins in place and must always be a melee swing.

All other tags (e.g. `Attack`, `Burst`, `Debuff`) are deferred. Tags are additive (enabling synergies) not restrictive — the no-gate philosophy holds; any augment can socket into any skill regardless of tags.

#### Area of Effect (AoE)

Any skill that damages all enemies within a radius carries the `AoE` tag. The radius is a **skill property** — a fixed constant per skill owned by the Balancer. Weapon range has no influence on AoE radius (see Range resolution table in `design-mechanics.md`).

**AoE modifier math (PoE / Last Epoch convention):**
Modifiers increase *area*, not radius directly. The effective radius is:

`effective_radius = base_radius × sqrt(1 + total_aoe_pct_increase)`

Example: base 250 units + 100% AoE → 250 × √2 ≈ 354 units. Each additional % yields diminishing radius gains — the standard ARPG tradeoff.

**Sources of AoE modifiers — deferred, none yet:** skill augments (e.g. "Increased Area"), gear affixes. Armour range modifiers and weapon range never feed AoE radius.

**Current AoE skills:**

| Skill | AoE coverage |
|---|---|
| self_channeled_tick | All enemies within radius of player |
| self_duration_tick | All enemies within radius of player |
| self_burst | All enemies within radius of player |
| fixed_zone_tick | All enemies within zone radius |
| fixed_zone_burst | All enemies within zone radius at detonation |
| windup_burst | All enemies within zone radius at detonation |
| tracked_tick | Tracked enemy + all enemies within radius around them |
| stackable_zone | All enemies within zone radius |
| triggered_zone_burst | All enemies within zone radius at detonation |

entity_burst and entity_debuff are single-target — no AoE tag.

#### Skill Prototypes

All authored skills are prototypes. Prototypes are the building blocks — they prove mechanics and cover the full design space. Player-facing skills are derived from them via the composition model (see The Composition Model section below).

All 12 prototypes are craftable. The `EngineProof` kind is retained in code for future use but nothing is currently marked as such — all authored skills are `Prototype`.

| Prototype | Targeting | Damage pattern | Skill type |
|---|---|---|---|
| entity_burst | Entity | Burst | Active |
| self_channeled_tick | Self | Tick | Channeled |
| self_duration_tick | Self | Tick | Active |
| self_burst | Self | Burst | Active |
| fixed_zone_tick | Position | Tick | Active |
| fixed_zone_burst | Position | Burst | Active |
| windup_burst | Position | Burst | Active |
| tracked_tick | Entity | Tick | Active |
| entity_debuff | Entity | None | Active |
| stackable_zone | Position | Tick | Active |
| triggered_zone_burst | Position | Burst | Active |
| self_aura | Self | Tick | Aura |

> **Tech note — renames, not new skills:** entity_burst, self_channeled_tick, self_duration_tick, and self_burst are renames of the existing Strike, Cyclone, Damage Aura, and Nova implementations. Rename in code and data — do not create new skill objects. Player-facing versions (Strike, Cyclone, etc.) are composed from these prototypes via the craft wizard (see The Composition Model section below).

All archetypes start with plain entity_burst in slot 1, no augments pre-socketed.

**Universal skill properties** — every skill in the game has these fields:

| Property | Description |
|---|---|
| Description | What this skill is designed to prove or do (prototypes: mechanic proof; future: named skill flavour) |
| Kind | `Normal` = real named skill (future). `Prototype` = all authored skills are this kind — craftable. `EngineProof` = reserved for future use, nothing currently marked as such. |
| Targeting shape | Self / Position / Entity — how the skill resolves its target (see Targeting in `design-mechanics.md`) |
| Wind-up | Seconds of delay before effect lands; 0 = instant |
| Damage pattern | Burst (single hit) / Tick (over duration) / None (debuff or utility only) |
| Stack limit | Max simultaneous active instances; configurable per skill; — = not a zone skill |
| Zone tracks entity | Whether a zone follows a target entity after placement; — = not applicable |
| Duration | How long a placed zone or summon persists (seconds). `0` = permanent — lives until replaced by the stack cap or the run ends. Self skills and instant bursts always use 0. Zone and summon skills set this to prevent permanent effects (e.g. a Blizzard zone running forever would be broken). |
| Trigger radius | Detection radius that fires a trap when an enemy enters it (in tiles). `—` = not a trap skill. Default: 1 tile. |
| Arm time | Delay after placement before the trap becomes active (seconds). Prevents self-triggering. `—` = not a trap skill. |
| Trigger | How many times the trap fires before despawning. `Single` = fires once then despawns. `—` = not a trap skill. |

**Future field — Dispellable (not yet added):** whether a zone or effect can be removed before its duration expires — by an enemy cleanse ability, a player counter-skill, or a future mechanic. Not added until something in the game actually reads it. Note here so the axis is not forgotten when designing elite enemies or player utility skills.

#### entity_burst

*(Renamed from Strike. Do not create a new skill — rename the existing implementation.)*

The universal starter prototype. Hits the locked target using whatever the character has equipped — a sword swing, an arrow, a wand bolt. All archetypes start with plain entity_burst, no augments pre-socketed. As players acquire new skills, entity_burst slots get replaced. entity_burst can still be kept in any slot intentionally.

| Property | Value |
|---|---|
| Description | Proves Entity targeting and weapon-adaptive delivery. Universal starter — fires at locked target using equipped weapon. |
| Kind | Prototype |
| Targeting shape | Entity |
| Wind-up | 0 (instant) |
| Damage pattern | Burst |
| Stack limit | — |
| Zone tracks entity | — |
| Damage type | Physical |
| Cooldown | 0.8s (tier 1) — lower at higher tiers |
| EoTs | None |
| Acquire | Free — slot 1 pre-filled at character creation |

#### self_channeled_tick

*(Renamed from Cyclone. Do not create a new skill — rename the existing implementation.)*

Spin continuously in place, hitting all enemies within melee range on each tick. A Channeled skill — hold to spin, release to stop. Drains Focus while held, stops automatically at 0 Focus. Lower damage per hit than entity_burst; the value is continuous multi-target coverage.

| Property | Value |
|---|---|
| Description | Proves Channeled skill type with Self targeting. Continuous ticking damage while held; drains Focus over time. |
| Kind | Prototype |
| Targeting shape | Self |
| Wind-up | 0 (instant) |
| Damage pattern | Tick |
| Stack limit | — |
| Zone tracks entity | — |
| Type | Channeled |
| Damage type | Physical |
| Focus cost | 12 Focus/sec drain |
| Tick rate | 4 hits/sec |
| Acquire | Craft |

> **Balancer note:** At 4 ticks/sec with no per-skill damage multiplier, single-target DPS is ~3.2× entity_burst (which fires at 1.25 hits/sec). The compensating levers are tick rate and Focus drain — lower tick rate until single-target DPS sits at the intended ratio vs entity_burst, accepting that AoE coverage is the skill's actual advantage.

#### self_duration_tick

*(Renamed from Damage Aura. Do not create a new skill — rename the existing implementation.)*

Activate once — pulses magic damage to all nearby enemies repeatedly for a few seconds, then enters cooldown. Proves Active Self ticking damage over a fixed duration. Natural pairing with Heavy armour and Wand: tanky magic build that stands in the horde and lets the damage tick. Wand EoT affinity means augments (e.g. Burn) trigger frequently per tick.

| Property | Value |
|---|---|
| Description | Proves Active Self skill with ticking damage over a fixed duration. Activate → ticks damage in radius for duration → cooldown. |
| Kind | Prototype |
| Targeting shape | Self |
| Wind-up | 0 (instant) |
| Damage pattern | Tick |
| Stack limit | — |
| Zone tracks entity | — |
| Type | Active |
| Damage type | Magic (placeholder) |
| Focus cost | 15 Focus (flat, on activation — placeholder) |
| Tick rate | 2/sec (placeholder) |
| Duration | 3s (placeholder) |
| Cooldown | 2s (after duration ends — placeholder) |
| Range | Short radius around player |
| Acquire | Craft |

#### self_burst

*(Renamed from Nova. Do not create a new skill — rename the existing implementation.)*

An instant explosion centered on the player — hits all enemies within a medium radius simultaneously, then enters cooldown. Proves Active Self burst. Panic button feel — surrounded, pop it, create space.

| Property | Value |
|---|---|
| Description | Proves Active Self burst. Instant explosion centered on player; flat Focus cost. |
| Kind | Prototype |
| Targeting shape | Self |
| Wind-up | 0 (instant) |
| Damage pattern | Burst |
| Stack limit | — |
| Zone tracks entity | — |
| Type | Active |
| Damage type | Physical (placeholder) |
| Focus cost | 20 Focus (flat) |
| Cooldown | 1.5s |
| Radius | Medium (larger than melee range) |
| Acquire | Craft |

---

All values (damage, cooldown, radius, tick rate, duration) are TBD — owned by the Balancer.

**fixed_zone_tick**

| Property | Value |
|---|---|
| Description | Proves Position targeting with a fixed ticking zone. Zone stays where cast; enemies walk through it. |
| Good for | Skills that place a persistent damage field at a location — enemies walk into it and take repeated hits. Traps, pools, hazard zones. |
| Kind | Prototype |
| Targeting shape | Position |
| Wind-up | 0 (instant) |
| Damage pattern | Tick |
| Tick rate | 1/sec (test value) |
| Stack limit | 1 |
| Zone tracks entity | No |
| Duration | 5s (test value) |
| Type | Active |
| Damage type | Magic |

**fixed_zone_burst**

| Property | Value |
|---|---|
| Description | Proves Position targeting with a single burst hit. A remote instant explosion — self_burst placed at a chosen location. |
| Good for | Skills that detonate a single explosion at a chosen spot — remote instant damage with no lingering effect. |
| Kind | Prototype |
| Targeting shape | Position |
| Wind-up | 0 (instant) |
| Damage pattern | Burst |
| Stack limit | 1 |
| Zone tracks entity | No |
| Duration | 0 — instant burst, no persistent zone |
| Type | Active |
| Damage type | Magic |

**windup_burst**

| Property | Value |
|---|---|
| Description | Proves wind-up mechanic. Telegraphed 1.5s delay before a high-damage burst lands at target position. Wind-up is the balancing cost. |
| Good for | Skills with a visible telegraph before a powerful hit lands — high damage that enemies can theoretically walk out of. |
| Kind | Prototype |
| Targeting shape | Position |
| Wind-up | 1.5s |
| Damage pattern | Burst |
| Stack limit | 1 |
| Zone tracks entity | No |
| Duration | 0 — instant burst on detonation, no persistent zone |
| Type | Active |
| Damage type | Magic |

**tracked_tick**

| Property | Value |
|---|---|
| Description | Proves Entity targeting with a zone that follows the target. Ticks damage to the tracked enemy and all enemies within radius around them. Zone moves with the entity. |
| Good for | Skills that attach a persistent effect to an enemy — follows the target and damages it (and nearby enemies) continuously. Curses, brands, haunts. |
| Kind | Prototype |
| Targeting shape | Entity |
| Wind-up | 0 (instant) |
| Damage pattern | Tick |
| Tick rate | 1/sec (test value) |
| Stack limit | 1 |
| Zone tracks entity | Yes |
| Duration | 5s (test value) — zone persists after target dies (stops following, keeps ticking in place until duration expires) |
| Type | Active |
| Damage type | Magic |
| AoE | Hits tracked enemy + all enemies within radius around them |

**entity_debuff**

| Property | Value |
|---|---|
| Description | Proves Entity targeting with no damage output. Applies a debuff directly to the locked target; effect follows the target for its duration. |
| Good for | Utility/debuff skills — intentional no-damage builds, CC support, or mixed builds that pair debuffing with other damage slots. |
| Kind | Prototype |
| Targeting shape | Entity |
| Wind-up | 0 (instant) |
| Damage pattern | None |
| Stack limit | 1 |
| Zone tracks entity | Yes |
| Duration | 6s (test value) |
| Type | Active |
| Damage type | Magic (N/A) |
| Effect | Slow (placeholder) |

**stackable_zone**

| Property | Value |
|---|---|
| Description | Proves configurable stack limit. Each cast places an independent ticking zone; up to the stack cap active simultaneously. |
| Good for | Skills where you want multiple independent instances active simultaneously — turrets, totems, summons, overlapping zones. |
| Kind | Prototype |
| Targeting shape | Position |
| Wind-up | 0 (instant) |
| Damage pattern | Tick |
| Tick rate | 1/sec (test value) |
| Stack limit | 3 (test value) |
| Zone tracks entity | No |
| Duration | 10s (test value) — oldest instance despawns when a 4th is cast before duration elapses |
| Trigger radius | — |
| Arm time | — |
| Trigger | — |
| Type | Active |
| Damage type | Magic |

**triggered_zone_burst**

| Property | Value |
|---|---|
| Description | Proves trigger-on-proximity mechanic. Placed at a position, dormant until an enemy enters the trigger radius, then fires once and despawns. |
| Good for | Traps, proximity mines, tripwires — placed hazards that punish enemies for moving through an area. |
| Kind | Prototype |
| Targeting shape | Position |
| Wind-up | 0 (instant) |
| Damage pattern | Burst |
| Stack limit | 3 (test value) |
| Zone tracks entity | No |
| Duration | 30s (test value) — despawns if not triggered before expiry |
| Trigger radius | 1 tile (test value) |
| Arm time | 0.5s (test value) — prevents self-triggering immediately after placement |
| Trigger | Single (fires once, despawns) |
| Type | Active |
| Damage type | Magic |

**self_aura**

Toggle on — the aura activates, reserves a flat amount of Focus (permanently reducing the available pool for other skills while active), and begins pulsing its effect on every tick. Toggle off — the aura deactivates and the reserved Focus is returned immediately. Proves the Aura toggle + Focus reservation mechanic. The only prototype where a skill runs indefinitely with no player input after activation.

The effect the aura produces (damage AoE, player buff, enemy debuff AoE) will be defined on future composed skills derived from this prototype. The prototype itself uses a placeholder damage tick.

| Property | Value |
|---|---|
| Description | Proves Aura toggle + Focus reservation mechanic. Toggle on → reserves Focus and pulses effect each tick. Toggle off → unreserves Focus. |
| Kind | Prototype |
| Targeting shape | Self |
| Wind-up | 0 (instant) |
| Damage pattern | Tick |
| Stack limit | — |
| Zone tracks entity | — |
| Type | Aura |
| Focus reservation | TBD (Balancer) — flat amount reserved from Max Focus while active |
| Tick rate | TBD (Balancer) |
| Effect | Placeholder damage tick for now; buff, debuff, or damage AoE on future composed derivatives |
| Acquire | Craft |

---

**Weapon is the root of the damage number.** Each weapon has a base damage value that increases with tier. The skill defines the damage type — entity_burst is physical (placeholder); future named skills define their own type. The weapon's identity bonus (flat % damage or crit) applies universally to all skills regardless of damage type — no skill-type gate. Archetype damage output scales through primary stat growth (see Archetype Stat Multipliers in `design-mechanics.md`), not an archetype-level multiplier table.

**Skills do not carry a per-skill damage multiplier.** Every skill draws from the same damage number: `weapon base × stat block`. A tick skill and a burst skill deal the same raw damage per hit — the design difference is delivery: tick rate, cooldown, AoE, and Focus cost. DPS balance between skills is the Balancer's domain, owned through tick rate and cooldown tuning. There is no `DamageMultiplier` field on a skill.

**Why (rationale, confirmed 2026-07-04):**

1. **Damage progression is anchored in the crafting economy.** Upgrading weapon tier is *the* way to increase damage output. In a fully craft-driven game the weapon is the damage sink for crafting investment — a per-skill multiplier would create a second, competing damage-progression axis: players would shop for the highest-multiplier skill instead of crafting a better weapon.
2. **No skill can be ranked by a number.** With no multiplier, no skill is "the 1.3× one." Skills compete on delivery shape only — this is the design space the Budget/Identity lever framework (see The Composition Model section below) formalises.
3. **It collapses the balance surface.** The Balancer tunes tick rate and cooldown only — never a per-skill damage table.

**Skill tier improves budget levers only (decided 2026-07-04).** A skill's tier upgrade advances a fixed per-skill upgrade track over its budget levers (e.g. cooldown down, or radius up — whatever that named skill's track is) and never touches hit size. Which lever a skill's track improves is itself an identity axis. Power parity between named clones of the same prototype is defined **at equal tier**.

---

## The Composition Model & Wave 1

> Locked in 2026-07-04 (promoted from `design-skill-system-brainstorming.md`). Governs how player-facing skills come to exist. **Revised 2026-07-05:** the *presets-first* shipping rule and the *hand-named presets* naming model were superseded — see the Wizard-first and Naming rules below. Remaining open follow-ups (element-wave decisions, full-roster naming word-map authoring) stay in the brainstorm doc.

### Architecture: skill = prototype + form + identity

A player-facing skill is a **craft-time composition** of three components, **fused and flattened at creation** into a standalone snapshot item:

| Component | Owns | Composability |
|---|---|---|
| **Prototype** | Delivery chassis: targeting shape, damage pattern, skill type, base budget stats | The 12 internal base skills above |
| **Form** | Budget-spend shape (where the power budget goes) + **tier track** (which budget lever tier upgrades advance) | Per-prototype (2 in wave 1; up to ~3 later) |
| **Identity** | Damage type + VFX/audio skin + name fragment | Universal — composes with any prototype × form |

Flattening at craft preserves every prior rule: instances are standalone (no runtime template link), damage type is fixed at creation, budget levers stay Balancer-owned, and the ownership matrix is untouched — the composed item is "the skill" and owns its stats (`design-stats.md`).

*Justification for composition over hand-authored named skills: content scales multiplicatively (a future wave adding 3 identities yields 8 forms × 3 = 24 new skills for 3 authored components — support-gem-style network effects), while authoring scales additively; and the crafting pillar gets a native expression (assembling a skill is crafting).*

**Wizard-first shipping rule (2026-07-05, supersedes presets-first).** Wave 1 exposes composition directly as a **craft wizard**: the player picks a prototype, then a form, then an identity, paying a resource cost at each step (see the crafting cost model in `design-progression.md`), and receives the flattened skill. Forms and identities are **catalogue entries** — authoring data in per-prototype (form) and universal (identity) registries — **not inventory items**; the only item produced is the finished skill. Registries start sparse and grow as forms/identities are authored ("build as we go"); a prototype with no forms yet shows an empty list and a disabled advance button. There is **no separate preset recipe book** — the reachable skills are simply the combinations the registries currently allow (wave 1: 4 prototypes × 2 forms × 2 identities = 16 reachable combos, *emergent from the catalogue*, not hand-authored recipes).

*Justification: presets were a content-authoring vehicle whose only job — shipping composition to the player — is done directly and more cheaply by the wizard over sparse registries. A curated preset recipe book serves no purpose at the current dev stage; possible future uses (starter loadouts for new characters, a "favourite recipe" bookmark) are different features that sit on top of the catalogue data and are parked until needed. The blandness and playtest-control-group concerns that motivated presets-first are accepted as playtest questions, not blockers.*

**Naming (2026-07-05, supersedes hand-named presets).** A skill's name is a **derived phrase** built from a per-component word map — form→adjective, identity→adjective, prototype→noun (e.g. swift · Magic · entity_burst → "Swift Arcane Strike"). The map is ~25 entries total (one word per form/identity/prototype), so names scale for free and are self-documenting — no wiki lookup, no hundreds of hand-authored names. A thin **iconic-override table** may assign curated names to a small set of signature combos ("Cyclone", "Nova"); every other combo uses the derived phrase. Wave 1 ships the raw composite `[prototype][form][identity]` as a placeholder display string; the derived phrase and any iconic overrides are a later display-layer swap over unchanged composition data.

*Justification: hand-authored per-combo names scale as prototypes × forms × identities (hundreds of names to invent) and force players to look up which combo a name refers to. Derived phrases are self-documenting and free; iconic overrides preserve soul for the combos that earn it.*

### The Budget/Identity lever framework (governing)

Every lever on a composed skill is one of two kinds:

- **Budget levers** — cooldown, tick rate, AoE/zone radius, Focus cost/drain, wind-up, **stack count** (the `StackLimit` field — more simultaneous instances is more total throughput; added 2026-07-06 with the swarm↔singular axis below). They move throughput. Across any two forms of the same prototype they must net to the same power budget **at equal tier** — "no free lunch," enforced as design discipline (exchange rates Balancer-owned, not exact math). Trading among them changes a skill's *shape*, never its power.
- **Identity levers** — damage type, VFX/animation/sound, wind-up-as-telegraph, and *which* budget lever the tier track advances. Free: they place the skill in the build ecosystem without moving throughput.

**Skill identity = prototype (delivery fantasy) × form (budget-spend shape + tier track) × identity (type + skin).**

**A distinction that moves no budget lever is an identity skin, not a form** (ruling 2026-07-06). If two candidate "forms" tick the same enemies at the same radius/rate/duration/cost and differ only in look or fantasy — e.g. a zone drawn as a planted emitter pulsing outward vs. a uniform carpet (the rejected *pylon* candidate) — that is an identity/VFX skin, selectable without changing mechanics. A form must spend budget differently. (A pulsing emitter only becomes mechanically real if it gains its own HP → that is the summon_totem prototype, not a zone form; or if its pulse travels as a distance-band → the moving_zone_tick prototype.)

`Range`-as-**reach** (cast range on Entity/Position skills) is deliberately **not** a budget lever — a long-range form (Snipe fantasy) requires consciously revisiting this line first (weapon-driven delivery makes melee-at-range visually incoherent). Clarified 2026-07-04: on **Self** skills the `Range` field *is* the AoE damage radius, which is a listed budget lever — Self-prototype forms (nova/quake, spin/vortex) tune their radius through it. Same field, two meanings; the constraint is per targeting shape (enforced in registry validation — see `technical-systems.md`).

**Hit size is not a lever (clarified 2026-07-06).** A single hit's damage is `weapon base × stat block` — constant, never bought or sold. It is neither a budget lever (you cannot trade cooldown or Focus for a bigger hit) nor an identity lever. What a form *may* change is **how a press's payload is delivered** — e.g. splitting one press's constant total into N equal sub-hits (the **salvo** shape below) — because that leaves per-press total damage untouched; the size of the payload itself never moves. Multi-hit delivery carries its own engine rule so it cannot leak power through per-hit augment procs — see per-hit proc normalization in the salvo spec. This is the precise reading of "hit size is constant, so a heavy-hitter is impossible by design": the constraint bars *buying* a bigger hit, not *dividing* a fixed one.

### Structural decisions (wave 1)

- **Identities in wave 1: Physical and Magic** — a real mechanical split (Phys couples to Str builds, Magic to Int). Elements arrive as their own wave together with enemy-resist promotion (D1), where the "element whispers, augments shout" token-effect framing is the leading candidate (see brainstorm doc).
- **Prototypes are internal skills**, not player-facing content: the authoring basis for composition, the testable proof of the base system, and the wave-1 playtest control group (if players keep crafting plain prototypes next to the presets, identity isn't earning its keep). They stay craftable through wave 1; retirement from the player pool is a full-roster-time decision.
- **Naming: classic ARPG vocabulary with the no-false-promises guardrail**, applied to genre expectations as well as literal words (no "Whirlwind" for a stationary spin). **The guardrail extends to VFX:** visuals must not promise mechanics or elements we don't deliver — wave-1 skins are kinetic (Physical) and arcane (Magic); no elemental cosplay before elements exist.
- **Contrast axes are varied per prototype** (one budget trade each) — tests four exchange rates instead of one, and prevents the roster reading as a fast/slow mode toggle.

### Wave 1: the 8 forms

| Prototype | Contrast axis | Form | Budget shape | Tier track |
|---|---|---|---|---|
| entity_burst | Commitment (tempo vs. Focus-efficiency — hit size is constant, so "heavy hitter" is impossible by design) | **swift** | Instant, short CD, moderate Focus — constant pressure | Cooldown ↓ |
| | | **heavy** | Wind-up telegraph, long CD, very low Focus — deliberate, nearly free | Focus cost ↓ |
| self_burst | Telegraph timing | **nova** | Instant burst, modest radius, short-ish CD — reactive panic button | Cooldown ↓ (quickens) |
| | | **quake** | Delayed detonation (wind-up), long CD, big radius — "brace, then boom" | Radius ↑ (grows) |
| fixed_zone_tick | Duration↔tick-rate | **storm** | Big radius, long duration, slow ticks, long CD — premeditated area denial | Radius ↑ |
| | | **floor** | Small patch, short duration, fast ticks, short CD, cheap — kiting breadcrumbs | Cooldown ↓ |
| self_channeled_tick | Radius↔drain | **spin** | Tight radius, fast ticks, low drain — aggressive grinder | Tick rate ↑ |
| | | **vortex** | Wide radius, slower ticks, heavy drain — anchored storm; the Focus bar is the real cooldown | Drain ↓ |

### Wave 1: the 16 reachable combos

These are the combinations the wave-1 registries allow (4 prototypes × 2 forms × 2 identities) — they **emerge from the catalogue**, they are not authored preset recipes (see the Wizard-first rule above). Names default to the derived phrase; the curated names below are **candidate iconic overrides** for the combos that deserve a signature name.

| Prototype | Physical | Magic |
|---|---|---|
| entity_burst · swift | **Strike** | **Arcane Strike** |
| entity_burst · heavy | **Crushing Blow** | **Smite** |
| self_burst · nova | **Shockwave** | **Nova** |
| self_burst · quake | **Quake** | **Cataclysm** |
| fixed_zone_tick · storm | **Rockfall** | **Tempest** |
| fixed_zone_tick · floor | **Caltrops** | **Glyph of Agony** |
| self_channeled_tick · spin | **Cyclone** | **Arcane Cyclone** |
| self_channeled_tick · vortex | **Bladestorm** | **Maelstrom** |

All numeric values per form are placeholder, owned by the Balancer.

### Beyond wave 1 — adopted forms & axis rulings

Forms and contrast-axis rulings resolved after the wave-1 lock, promoted here from the brainstorm doc as each clears synthesis (budget-parity + rule-compliance). Adopted forms are not in the shipped registries yet — each becomes its own implementation issue, and each new form multiplies against the existing identity pool (so it adds a whole row of reachable combos, not one skill). Axis rulings are accepted design tools that may not yet have a built form.

#### salvo (entity_burst, third form) — adopted 2026-07-06

| Field | Value |
|---|---|
| Prototype | entity_burst |
| Contrast axis | **Delivery granularity** — one chunk (swift/heavy) vs. a volley. A third axis on entity_burst, distinct from the swift↔heavy commitment axis. |
| Budget shape | One press = **N rapid sub-hits** over a short volley window (~0.5s), the press's constant payload split evenly across them. |
| N (sub-hits) | Fixed per form, **3–5 (Balancer-owned)**. Deliberately **not** the tier track — N is power-neutral under proc normalization, so advancing it would be a dead tier-up. |
| Tier track | **Cooldown ↓** — the volley fires more often, sharpening the thing the form is about. (Shares the cooldown track with swift; the varied-contrast-axes rule governs *contrast axes*, not tier tracks, so the repeat is legal.) |
| Budget trade | Pays for its smoother texture with **spread payload**: full damage arrives across the volley window, not instantly, so burst and time-to-first-kill are worse than swift's instant hit, and any sub-hits after the target dies are wasted. The Balancer nets it to swift/heavy parity at equal tier via cooldown/Focus. |
| Word-map fragment | "volley" / "flurry" |

Two new reachable combos (× Physical, Magic) — candidate iconic names TBD (e.g. "Flurry", "Arcane Barrage").

**Why it is a form and not power:** under the Budget/Identity framework a form must change *shape*, never throughput. Salvo's whole substance is texture — smoother damage, lower crit variance, a real mid-volley-waste downside — all power-neutral. The one thing that would leak power, N× per-hit augment procs per press, is closed by the engine rule below.

**Engine need — per-hit proc normalization.** A multi-hit skill's per-hit augment trigger chance is divided by N so expected procs per press stay form-invariant (the genre-standard solution; PoE normalizes per projectile). The change is local to multi-hit skills, and it is the precedent the future **Splash** and **projectile-augment** families will reuse. Until it exists, salvo cannot ship.

#### fleet ↔ enduring (duration ↔ potency) — accepted as generic fallback axis, parked 2026-07-06

The **designated fallback contrast axis** for a prototype that has no richer budget trade: the same budget spread over time two ways — short-intense (fleet) vs. long-weak (enduring). Accepted in principle as a tool; **no form is built and its concrete use is parked.**

- **Barred from damage-tick prototypes.** After the hit-size clarification above, "potency" on a tick skill cannot mean damage-per-tick — the only throughput lever left is tick rate, so "duration ↔ potency" collapses into the existing **storm/floor duration↔tick-rate axis** (`fixed_zone_tick`). Using it on `self_duration_tick` or `tracked_tick` adds no new exchange rate and pushes the roster toward the fast/slow-toggle reading the varied-axes rule prevents. Not to be spent there.
- **Its one unique home is `entity_debuff`** — a `None`-pattern skill with no damage, radius, or tick rate, whose only possible trade is duration ↔ debuff-strength. **Blocked:** both a debuff's duration and its strength (`SlowFraction`, etc.) live on the shared `EotData` *definition* (`design-stats.md` EoT-payload ownership), not on the skill or form. A short-strong/long-weak pair would require promoting EoT magnitude/duration to a per-form override — a stat-ownership decision that belongs with the **element/EoT wave (D1)**, where debuff magnitudes and the "element whispers, augments shout" token effects get designed. **Unblock condition:** that wave rules on whether a form may override an EoT's magnitude/duration.

#### swarm ↔ singular (stack count ↔ instance size) — adopted 2026-07-06

The contrast axis for the two multi-instance prototypes: **stackable_zone** (Position/Tick — independent ticking zones) and **triggered_zone_burst** (Position/Burst — proximity traps). Trade board coverage against concentrated power at constant budget: **many small instances** (swarm) vs. **few big ones** (singular). Ships on pure data — `StackLimit` already exists and is enforced; no engine work.

| | stackable_zone | triggered_zone_burst |
|---|---|---|
| **swarm** | Many small, short-radius ticking zones — area saturation | Many small-radius traps blanketing an area |
| **singular** | Few (**stack floor ≥ 2**) large, long-duration zones | One or two big-radius traps |
| swarm tier track | Stack count ↑ | Stack count ↑ |
| singular tier track | Radius ↑ | Radius ↑ |

Adds **8 reachable combos** (2 prototypes × 2 forms × 2 identities) and gives both prototypes their first player-facing forms.

**Instance "size" is never per-instance damage** (hit-size constancy) — it is the instance's radius/duration/tick-rate for the tick zones, and radius alone for the burst traps (a burst's per-hit damage is fixed, so a "big" trap just catches more). **Overlap guard:** stackable_zone's *singular* end keeps a **stack floor ≥ 2** — dropping to one ticking zone would make it a `fixed_zone_tick` clone (which owns the storm/floor forms). triggered_zone_burst has no such twin, so its singular end may go to a single trap.

*Note: this is the one axis intentionally shared across two prototypes. Legal because the chassis are mechanically distinct (persistent zones vs. one-shot traps) and stacking is the *defining* trade for both — it reads as two fantasies exploring one exchange rate, not a fast/slow mode toggle.*

#### reserve-heavy ↔ reserve-light (Focus reservation ↔ radius) — adopted 2026-07-06, damage-aura scope

The contrast axis for **self_aura** (Self/Tick/Aura) — the only prototype that pays with Focus *reservation* (a flat chunk locked out of Max Focus while toggled on), so this is the only axis it can express. Trades reservation against area at constant budget: **reserve-heavy** = big reservation + big radius (a strong aura that taxes a large slice of your Focus ceiling), **reserve-light** = small reservation + small radius (a cheap aura that leaves Focus for your active skills). Adds 4 reachable combos (self_aura × 2 forms × 2 identities) — self_aura's first player-facing forms.

| | reserve-heavy | reserve-light |
|---|---|---|
| Budget shape | Large Focus reservation, large damage radius | Small reservation, small radius |
| Tier track | Radius ↑ (grows the aura) | Reservation ↓ (frees more Focus) |

- **Effect "size" is radius, never per-target magnitude** (hit-size constancy). For a damage aura, damage-per-tick is constant, so the tradeable lever is the aura's radius (`Range`-as-radius on Self skills, the same machinery self_duration_tick/self_burst use). Both levers are real and built — reservation on self_aura, radius on its Self siblings — so this ships without new engine work. *(One verify at implementation: confirm self_aura applies damage across its `Range` radius rather than a degenerate placeholder tick.)*
- **Scoped to damage/area auras.** The buff/debuff-aura branch — where "effect strength" would be per-target buff/debuff magnitude — is **blocked on the same D1 EoT/buff-magnitude question as fleet↔enduring** (magnitude lives on the shared effect definition, not the form). Those auras don't get this form until that question is ruled.

*Note: superficially adjacent to self_channeled_tick·vortex (both "Focus ↔ radius"), but the Focus sub-lever differs — vortex is continuous **drain** (trades against sustain, the bar empties as you hold), reserve-heavy is flat **reservation** (trades against your Focus budget for other skills, set-and-forget). Different build math on distinct prototypes.*

#### blast ↔ echo (single burst ↔ split delayed aftershock) — adopted 2026-07-06

The contrast axis for **fixed_zone_burst** (Position/Burst — a zone that detonates at the cast position), giving it its first forms: one instant blast vs. a blast split into an initial hit **+ a delayed second hit at the same spot** (aftershock). Adds 4 reachable combos.

| | blast | echo |
|---|---|---|
| Budget shape | Single instant detonation at the target position | Payload split into an initial hit + a delayed aftershock at the same position |
| Tier track | Cooldown ↓ | Radius ↑ (both hits catch more) |

- **Same family as salvo, same rules.** Each of echo's two hits is payload/2 — dividing a fixed payload, not buying a bigger one (hit-size clarification) — and its two per-hit augment procs are neutralized by the **÷N proc normalization** (N=2). Echo therefore **shares salvo's multi-hit engine machinery**: build "two-plus hits from one activation" + ÷N normalization once, both forms consume it.
- **The delayed hit reuses existing scheduling.** "Fire a hit at a stored position after delay D" is exactly the wind-up flow windup_burst and the quake/heavy forms already use — so the only genuinely new engine piece is the shared multi-hit hook above.
- **Budget trade:** echo pays for its second-chance area coverage (the aftershock catches enemies who lingered or returned) with spread payload — full damage lands over the delay window, not instantly — plus the risk the second hit whiffs if the area clears. Balancer nets it to blast parity.

*Homed on fixed_zone_burst deliberately: its position is fixed, so "same spot" for the aftershock is unambiguous. **Not** placed on self_burst (its nova/quake pair already owns the telegraph-timing axis, and "same spot" is ambiguous when the player moves) or windup_burst (left clear for the pending wind-up-as-form-axis question — see the fuse candidate in the brainstorm doc).*

### Engine prerequisites for wave 1

1. **Per-skill VFX mapping** — animation/VFX is currently delivery-driven (all skills of a `SkillType` look identical; the channeled ring is hardcoded per `SkillType`). Identity skins require a per-skill (per-composition) VFX key. The self_channeled_tick forms lean hardest on this.
2. **Composition data model + craft wizard** — prototype/form/identity as authoring data in registries (form registry per-prototype, identity registry universal), flattened into `SkillData` snapshots at craft. Craft wizard UI: prototype → form → identity, one resource cost debited per step, empty-list + disabled-advance handling for unauthored registries. No preset recipe book — reachable skills are the combinations the registries allow.
   - **Crafting cost model** — cost is a **resource bundle** (a list of `(Resource, quantity)` entries), never a scalar. Currencies (coins) and materials share **one unified `Resource` registry** — gold is just another resource. Affordability and debit operate on the whole bundle **atomically** (check all, deduct all, or fail). One shared cost type across *all* crafting (skills, gear, augments), consumed per wizard step. Wave 1 = every entry's bundle is `[(CraftingMaterial, 1)]`. See `design-progression.md`. Recipes also reserve a **`Requirements` seam** — a list of non-consumable predicates (e.g. level/reputation gates) evaluated at an eligibility step *distinct from* cost payment; **none in wave 1** (the step always passes).
   - **Naming** — derived-phrase word map (form/identity/prototype → one word each) + iconic-override table; wave 1 ships the raw `[prototype][form][identity]` composite as a placeholder, swapped to the derived phrase later without touching composition data.
3. **WindUp on Entity and Self fire paths** *(sanity check 2026-07-04)* — `SkillData.WindUp` is currently honored only in `FireAtPosition` (Position-targeted skills). The **heavy** form (entity_burst) and **quake** form (self_burst) need the wind-up telegraph + delayed-hit flow on the Entity and Self paths too; the existing `WindupTelegraph` node is reusable.
4. **Tier tracks** *(sanity check 2026-07-04)* — `SkillItemInstance.Tier` currently gates only augment-slot count; no code applies tier to any skill stat. Implementing per-form tier tracks (cooldown ↓ / radius ↑ / etc. at tier-up) is part of the composition work.
