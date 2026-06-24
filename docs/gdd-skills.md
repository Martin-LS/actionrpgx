# Game Design Document — Skills

> Part of the GDD. See `gdd-mechanics.md` for combat, characters, focus, damage types, and run structure. See `gdd-augments.md` for Skill Augments, Equipment Augments, and augment prototypes.
> Living document — details will evolve as the game is playtested.

---

### Skills

**Design rule for skills:**
- **EoTs and secondary effects (mines, traps) are added by augments, not baked into skills.** A skill's base behaviour is its damage delivery. Augments add what happens on top of that.

**Skill slot vs. Equipment Augment — the dividing line:**
- **Skill slot = things you actively trigger** (requires a button press — Active, Channeled, toggled auras, War Cries).
- **Equipment Augment = things that happen automatically** (no button press, gear-driven).

There is no Passive skill type on the skill bar. Everything in a skill slot requires player input to fire. Persistent stat buffs and background effects belong on Equipment Augments (e.g. Mending, Retaliation) — not skill slots. Skills like War Cry stay on the skill bar because they are intentional activations with cooldowns, not background passives.

**Named skills are clones of prototypes — no runtime template system.** When a named skill (e.g. Strike) is created from a prototype (e.g. entity_burst), it is a complete standalone definition. All values are copied at authoring time; the prototype has no runtime relationship to the named skill after that. Changes to a prototype never cascade to existing named skills or crafted instances. The `BasedOn` field on `SkillData` records which prototype a named skill was cloned from — documentation only, no runtime behaviour. This keeps item instances stable and predictable: a crafted Strike is never changed by a prototype balance update without the designer explicitly editing Strike's definition.

**Skill tags — limited to delivery-resolution and AoE in v1.** Skills carry two categories of tag in v1:

- **`AoE`** — marks skills that damage all enemies within a radius. Introduced now because the radius modifier math needs a hook. All skills that deal damage to a radius (self/zone/tracked) carry this tag.
- **`Melee` / `Range`** — delivery-override tags used internally by `WeaponController` to determine how entity hits are resolved. A skill with `Melee` always fires a melee swing regardless of equipped weapon; a skill with `Range` always fires a ranged projectile. No delivery tag means the skill inherits the weapon's preferred delivery type (weapon-adaptive). Only `self_channeled_tick` carries `Melee` in v1 — it spins in place and must always be a melee swing.

All other tags (e.g. `Attack`, `Burst`, `Debuff`) are post-v1. Tags are additive (enabling synergies) not restrictive — the no-gate philosophy holds; any augment can socket into any skill regardless of tags.

#### Area of Effect (AoE)

Any skill that damages all enemies within a radius carries the `AoE` tag. The radius is a **skill property** — a fixed constant per skill owned by the Balancer. Weapon range has no influence on AoE radius (see Range resolution table in `gdd-mechanics.md`).

**AoE modifier math (PoE / Last Epoch convention):**
Modifiers increase *area*, not radius directly. The effective radius is:

`effective_radius = base_radius × sqrt(1 + total_aoe_pct_increase)`

Example: base 250 units + 100% AoE → 250 × √2 ≈ 354 units. Each additional % yields diminishing radius gains — the standard ARPG tradeoff.

**Sources of AoE modifiers — post-v1, none in v1:** skill augments (e.g. "Increased Area"), gear affixes. Armour range modifiers and weapon range never feed AoE radius.

**v1 AoE skills:**

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

All skills in v1 are prototypes. Prototypes are the building blocks — they prove mechanics and cover the full design space. Named skills with unique identities are post-v1 and will be derived from these prototypes.

All 12 prototypes are craftable. The `EngineProof` kind is retained in code for future use but nothing in v1 is marked as such — all v1 skills are `Prototype`.

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

> **Tech note — renames, not new skills:** entity_burst, self_channeled_tick, self_duration_tick, and self_burst are renames of the existing Strike, Cyclone, Damage Aura, and Nova implementations. Rename in code and data — do not create new skill objects. v2 will create the real named versions (Strike, Cyclone, etc.) derived from these prototypes.

All archetypes start with plain entity_burst in slot 1, no augments pre-socketed.

**Universal skill properties** — every skill in the game has these fields:

| Property | Description |
|---|---|
| Description | What this skill is designed to prove or do (v1: mechanic proof; future: named skill flavour) |
| Kind | `Normal` = real named skill (post-v1). `Prototype` = all v1 skills are this kind — craftable. `EngineProof` = reserved for future use, nothing currently marked as such. |
| Targeting shape | Self / Position / Entity — how the skill resolves its target (see Targeting in `gdd-mechanics.md`) |
| Wind-up | Seconds of delay before effect lands; 0 = instant |
| Damage pattern | Burst (single hit) / Tick (over duration) / None (debuff or utility only) |
| Stack limit | Max simultaneous active instances; configurable per skill; — = not a zone skill |
| Zone tracks entity | Whether a zone follows a target entity after placement; — = not applicable |
| Duration | How long a placed zone or summon persists (seconds). `0` = permanent — lives until replaced by the stack cap or the run ends. Self skills and instant bursts always use 0. Zone and summon skills set this to prevent permanent effects (e.g. a Blizzard zone running forever would be broken). |
| Trigger radius | Detection radius that fires a trap when an enemy enters it (in tiles). `—` = not a trap skill. Default: 1 tile. |
| Arm time | Delay after placement before the trap becomes active (seconds). Prevents self-triggering. `—` = not a trap skill. |
| Trigger | How many times the trap fires before despawning. `Single` = fires once then despawns. `—` = not a trap skill. |

**Future field — Dispellable (not in v1):** whether a zone or effect can be removed before its duration expires — by an enemy cleanse ability, a player counter-skill, or a future mechanic. Not added until something in the game actually reads it. Note here so the axis is not forgotten when designing elite enemies or player utility skills.

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

Toggle on — the aura activates, reserves a flat amount of Focus (permanently reducing the available pool for other skills while active), and begins pulsing its effect on every tick. Toggle off — the aura deactivates and the reserved Focus is returned immediately. Proves the Aura toggle + Focus reservation mechanic. The only v1 prototype where a skill runs indefinitely with no player input after activation.

The effect the aura produces (damage AoE, player buff, enemy debuff AoE) is defined on each named skill cloned from this prototype in v2+. The prototype itself uses a placeholder damage tick.

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
| Effect | Placeholder damage tick in v1; buff, debuff, or damage AoE on named clones in v2+ |
| Acquire | Craft |

---

**Weapon is the root of the damage number.** Each weapon has a base damage value that increases with tier. The skill defines the damage type — entity_burst is physical (placeholder); future named skills define their own type. The weapon's identity bonus (flat % damage or crit) applies universally to all skills regardless of damage type — no skill-type gate. Archetype damage output scales through primary stat growth (see Archetype Stat Multipliers in `gdd-mechanics.md`), not an archetype-level multiplier table.

**Skills do not carry a per-skill damage multiplier.** Every skill draws from the same damage number: `weapon base × stat block`. A tick skill and a burst skill deal the same raw damage per hit — the design difference is delivery: tick rate, cooldown, AoE, and Focus cost. DPS balance between skills is the Balancer's domain, owned through tick rate and cooldown tuning. There is no `DamageMultiplier` field on a skill.
