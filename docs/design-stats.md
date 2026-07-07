# Game Design Document — Stats

> Authoritative home for the game's stats: what they are, which entity ("surface") owns them, and what is in or out of scope.
> Other design docs defer to this doc on stat ownership. Where a stat has a fuller treatment elsewhere (e.g. the move-speed model in `design-mechanics.md`), this doc links rather than duplicates.
> All numeric rates are placeholder, owned by the Balancer — the *rules* here are design, the *numbers* are not.

---

## 0. How to read this doc

Stats do not all belong to the character. They live on **surfaces** — the entity a stat describes. Each surface has a **scope tier**:

| Tier | Meaning | Behaviour rule |
|---|---|---|
| **🟢 Active** | In scope, being designed & built now | Fully fleshed out below |
| **🟡 Deferred** | Real & important; we are aware; keep in mind — but not designed now | Do **not** design/build unprompted. **Fair to raise as a push-back** whenever a decision touches it. ⭐ = recommend revisiting early |
| **⚪ Parked** | Niche / special-case; unlikely for our pillars | Out of scope, low expectation — but still not "never" |

**The golden rule: "out of scope" ≠ "never mention."** Deferred and Parked surfaces are allowed in discussion at any time. What's disallowed is silently designing content onto them without promoting them first (discuss → change tier here → then design).

Deferred and Parked surfaces are detailed in **[design-stats-deferred.md](design-stats-deferred.md)** — load that doc only when a push-back needs substance or a surface is being promoted.

## 1. Stat Surfaces overview

| # | Surface | Owns | Tier |
|---|---|---|---|
| 1 | **Character** | Primary stats + derived stat block (HP, damage mults, resists, crit, focus…) | 🟢 Active |
| 2 | **Skill** | Per-ability delivery: cooldown, focus cost, area, duration, tick rate, damage type… | 🟢 Active |
| 3 | **Item / gear** | Weapon base damage & identity bonuses, armour category bonuses, accessory mods | 🟢 Active |
| 4 | **Enemy / monster** | Enemy HP/damage/resists, aggro, pack modifiers, rarity tiers | 🟡 Deferred |
| 5 | **Map / run / encounter** | Area level, enemy density, drop/material bonuses, hazards | 🟡 Deferred |
| 6 | **Hit / interaction** (the damage *event*, not an entity) | Penetration, added/converted damage, ailment chance & magnitude, leech | 🟡 Deferred ⭐ |

Cross-cutting items tracked in the deferred doc: **Reward / economy stats** (material quantity/rarity — our whole economy) 🟡 ⭐, and the ⚪ Parked list (block, dodge-as-stat, minion stats, flasks, curse/aura effect…). The **speed-family stance** was decided 2026-07-04 and now lives in the matrix (section 5C).

---

## 2. 🟢 Character stats

### Primary stats

The three character primary stats. They grow with level (archetype-specific rates) and drive the derived stats via fixed conversions. Primary stats never touch the game directly — they only feed derived stats.

| Primary | Feeds (derived) |
|---|---|
| **Strength (Str)** | **Melee** Damage mult, Max HP, Physical Resistance, Crit Damage |
| **Dexterity (Dex)** | **Ranged** Damage mult, Crit Chance, Evasion |
| **Intelligence (Int)** | **Spell** Damage mult, Max Focus, Elemental Resistance, Focus Regen |

*Justification (delivery-scaling, 2026-07-06): damage scales by **delivery** (melee/ranged/spell), not by damage type. This gives all three primary stats a damage role — Dex was previously crit-only, leaving the Rogue archetype with no damage pool of its own — and decouples the element (damage type) from scaling, so any archetype can wield any element, scaled by how they deliver it. See the "Damage number" composed stat and `design-mechanics.md`.*

Conversion rates live in `PrimaryStatConversions.cs` (Balancer-owned). Primary stats do **not** affect move speed, and level progression does not affect move speed.

### Derived stat block (`StatId`)

The in-game-effective stats. This is the "stat block" in the damage formula `weapon base × stat block`.

| Stat | What it does | Notes |
|---|---|---|
| **MaxHp** | Character health pool | Str-derived + Armour BonusHp + level |
| **Speed** | Move speed | Composed — see the move-speed model in `design-mechanics.md`. **Not** an archetype identity stat; all archetypes share one base |
| **MeleeDamage** | Multiplier on weapon base for **melee-delivered** hits | Str-derived. A *multiplier*, not flat. Selected by delivery (sword/melee), not damage type |
| **RangedDamage** | Multiplier on weapon base for **ranged-delivered** hits | Dex-derived. Multiplier, not flat. Selected by delivery (bow/ranged) |
| **SpellDamage** | Multiplier on weapon base for **spell/cast-delivered** hits | Int-derived. Multiplier, not flat. Selected by delivery (wand/spell) |
| **PhysicalResistance** | Reduces incoming physical damage | Str-derived + accessory contributions |
| **ElementalResistance** | Reduces incoming elemental damage | Int-derived. **Placeholder — lumped for now**; per-element player resistances are deferred until enemies deal elemental damage (enemy-design work). Renamed from MagicResistance (no "Magic" damage type exists) |
| **MaxFocus** | Focus resource pool (skill fuel) | Int-derived + level |
| **FocusRegen** | Focus regenerated per second | Int-derived + level |
| **CritChance** | Chance for a hit to crit | Dex-derived + Weapon CritChanceBonus + gear affixes + `critical_strike`/equipment augments — itemizable |
| **CritDamage** | Crit damage multiplier | Base 1.5× + Str-derived + gear affixes + crit-damage augment — itemizable (crit is a build archetype) |
| **Evasion** | Chance to avoid an incoming hit | Dex-derived |

Related but **not** a `StatId`: **Focus Shield** — a Focus sub-mechanic (fraction of MaxFocus; `ShieldFraction` / `ShieldRegenPerSec` in `BalanceConfig.Focus`), not a stat on the block.

## 3. 🟢 Skill stats

Skill-owned stats are the delivery levers: Cooldown, FocusCost/drain/reservation, WindUp, Duration, ZoneRadius, StackLimit, TriggerRadius/ArmTime/TriggerCount, TickRate, DamageType, Range (override). Full definitions and per-skill values live in **[design-skills.md](design-skills.md)**; ownership rules are in the matrix below.

## 4. 🟢 Item / gear stats

| Stat | Owner | What it does |
|---|---|---|
| BaseDamage | Weapon | Root of the damage number; scales with tier |
| DamageBonus | Weapon | % bonus to this weapon's damage (weapon identity) |
| CritChanceBonus | Weapon | Flat crit chance added by weapon identity |
| CritDamageBonus | Weapon / gear | Flat crit damage added by affix — new source (crit is now a build archetype; see the matrix) |
| WeaponRange | Weapon | Base attack range before armour/skill modifiers |
| DamageReduction | Armour | Flat % incoming damage reduction by category |
| RangeMultiplier | Armour | Multiplies attack range by category |
| BonusHp / BonusSpeed | Armour | Flat HP / % speed by category and tier |
| PhysicalResistance | Ring (accessory) | Flat physical resistance |

Slot/category/tier allocation rules live in `design-progression.md`.

---

## 5. Stat Ownership Matrix (governing)

> **This section is DECIDED and governs every stat in the game: which entity is *allowed* to carry which stat.**
>
> **⚠️ Guard note (Claude, act on this):** If any design idea — in any doc — puts a stat on the wrong entity (e.g. gives a skill a damage multiplier, or a weapon a cooldown), **push back before implementing.** Do not silently design over this matrix. Either we consciously amend the matrix (discuss → change here first) or we reject the idea. No quiet exceptions.

Two kinds of stat:

- **Exclusively-owned** — exactly one entity type may carry it. No other entity gets that stat.
- **Composed** — several defined contributors combine by a fixed formula. The contributor list *is* the rule; nothing outside it may feed the stat.

### A. Exclusively-owned stats

| Stat | Sole owner | Note |
|---|---|---|
| Cooldown | **Skill** | |
| FocusCost / drain / reservation | **Skill** | |
| WindUp | **Skill** | also a telegraph identity cue |
| DamagePattern, TargetingShape, Type | **Skill** (prototype-locked) | inherited by clones, not varied |
| StackLimit, Duration, ZoneRadius, TriggerRadius, ArmTime, TriggerCount | **Skill** | zone/trap shape |
| **TickRate** | **Skill** | *Not yet a field — currently lives only in `BalanceConfig`, read directly by `WeaponController`. Formalised as skill-owned; needs a `TickRate` field on `SkillData`.* |
| **DamageType** | **Skill** | **Skill-authoritative** (decided 2026-07-04). The skill defines the element. `weapon.BaseDamageType` is fallback/display only and never overrides the skill. **Mutability (decided 2026-07-04):** set at skill creation, not re-rollable by crafting; only a socketed augment may override it at fire time (e.g. the Fire Damage conversion augment, migrated from Magic). |
| BaseDamage | **Weapon** | root of the damage number |
| ArmorCategory, DamageReduction | **Armour** | |
| MaxFocus, FocusRegen, Evasion, ElementalResistance | **Character stat block** | derived from primary stats; ElementalResistance is a lumped placeholder (per-element player resistances deferred). **CritDamage moved out of this row 2026-07-06** — now a composed, itemizable stat (see below) |
| EoT / ailment payload (`EotId`) | **Identity + Augment (co-owned — amended 2026-07-06)** | **Every damage type carries one *signature ailment* innately, owned by its identity:** Physical→Bleed, Fire→Burn, Cold→Chill, Lightning→Shock. **Augments *graft* an off-type ailment** onto a skill that lacks it (e.g. a Burn augment on a Lightning skill) — they never amplify the innate signature; scaling of any ailment comes from the generic **Ailment stat family** (see §6 note), never per-skill. The `DamagePattern == None` → single `DebuffEotId` case still holds for pure-debuff skills. *Justification: elements are a genre-standard build lever, and an element that doesn't do its own thing (fire that doesn't burn) reads as broken; parity between types is held by **per-element enemy-resistance distribution** — resistant packs make each element situationally strong/weak — not by suppressing the ailment. **Supersedes the pre-2026-07-06 "no damage skill may ever carry an inherent EoT" rule** (that rule assumed a Physical/Magic-only roster with no elemental identities).* |

### B. Composed stats (fixed formula, closed contributor list)

| Stat | Contributors | Formula / rule |
|---|---|---|
| **Damage number** | Weapon `BaseDamage` × Weapon `DamageBonus` × Character `Melee/Ranged/Spell` damage mult (channel selected by the skill's **delivery**, not its damage type) | weapon is root, character scales. Delivery picks the scaling channel; damage *type* (element) is independent. **No per-skill damage multiplier — ever.** *Justification: scaling by delivery (Str→melee, Dex→ranged, Int→spell) gives every primary stat a damage role and decouples element from scaling — see the Primary-stats justification above.* |
| **Crit chance** | Character `CritChance` (Dex) + Weapon `CritChanceBonus` + gear affixes + `critical_strike` / equipment augments | additive; **itemizable** |
| **Crit damage** | Base 1.5× + Character `CritDamage` (Str) + Weapon/gear `CritDamageBonus` + crit-damage augment | additive multiplier; **itemizable**. **Amended 2026-07-06: promoted from Exclusively-owned (Str-only) to a composed, itemizable stat.** *Justification: with hits mono-typed and no per-skill multiplier, crit is the marquee damage-variance lever; making it a full itemization build (chance + multiplier chased on gear + augments + attributes) adds a cross-archetype "crit build" archetype. Str stays the native head-start (martial-favored); other archetypes itemize into crit at opportunity cost. Crit-stamping means a crit build also amplifies ailments — an intended crit+ailment synergy.* |
| **Physical Resistance** | Character (Str-derived) + Ring accessory `PhysicalResistance` | additive |
| **Range** | Weapon `WeaponRange` × Armour `RangeMultiplier`, overridden by Skill `Range` | resolution chain |
| **Max HP** | Character stat block (Str-derived + level) + Armour `BonusHp` | additive |
| **Move speed** | Archetype base + Heavy/Light armour (−/+% per piece: hat, body, boots) + Slow EoT (−%) + Dash Reflex augment (+%) | additive %; full model in `design-mechanics.md` |

### C. Deliberate absences (decided stats that do NOT exist)

These are written decisions, not omissions. Adding any of them requires amending this section first.

| Absent stat | Rule | Decided |
|---|---|---|
| **Per-skill damage multiplier** | Never. Damage = `weapon BaseDamage × DamageBonus × character mult`. Rationale in `design-skills.md`. | 2026-07-04 (rationale documented) |
| **Free per-skill / per-form ailment magnitude** | Never a free scalar. Ailment magnitude/duration = `EotData` **baseline** (identity-owned) × character **Ailment stats**, plus an optional **per-form modifier that must be power-neutral** — either product-invariant with duration (a magnitude↔duration re-slice) or proportional to a real budget cost paid (e.g. Focus reservation). A form may never simply scale an ailment up. See §E for the composition. *Justification: the EoT analogue of hit-size constancy — bars *buying* a bigger ailment, permits *re-slicing* a fixed one or *paying a real cost* for it. This is the D1 resolution that unblocks fleet↔enduring and the reserve buff/debuff-aura branch without resurrecting a per-skill multiplier.* | 2026-07-07 (D1 resolved) |
| **Character attack speed / cast speed / cooldown-recovery stat** | Never. Skill throughput is owned by the skill's own cooldown/tick levers (tuned by Balancer, advanced by skill tier) plus the single global **weapon CDR property** (`design-mechanics.md`). CDR may never appear on armour, rings, or augments — `design-directions.md`'s "CDR on skills" idea requires amending this row first. | 2026-07-04 (D5 closed) |
| **Enemy immunity (resistance ≥ 100%)** | Never. Enemy resistances always cap below 100% — a resistant enemy takes *reduced* damage, never zero. *Justification: hits are mono-typed by design (see below), so immunities would hard-wall mono-typed builds (the PoE immune-mob problem); capping resists is the clean alternative to forcing a "minimum physical damage" floor onto every hit.* | 2026-07-04 |
| **Composite/multi-typed hits & typed weapon damage** | Never (as designed). Every hit is 100% one damage type; weapon `BaseDamage` stays a typeless scalar stamped by the skill's `DamageType`. *Justification: (1) the crafting anchor — one weapon number must serve every build or the weapon pool forks into Str/Int weapons, breaking "any character equips any weapon"; (2) attribute build identity — the scaling channel is now **delivery** (melee→Str, ranged→Dex, spell→Int, per the delivery-scaling model), and build identity only stays meaningful if a hit scales exactly one channel; mono-typing the hit keeps that clean (the *element* rides independently on top); (3) keeps the D3 hit-event model closed — composite portions/conversion are the retrofit pain the D3 ⭐ warning exists for. D4-lineage model, chosen deliberately over the D2/PoE composite lineage.* | 2026-07-04 |

Related decision: **skill tier improves budget levers only** (never hit size); power parity between named clones is defined at equal tier — see `design-skills.md`.

### D. Craft-time components are not stat surfaces

Player-facing skills are craft-time compositions of `prototype + form + identity` (`design-skills.md`, The Composition Model section). **Form and identity are authoring/crafting components only: their contents flatten into skill-owned stats at creation.** At runtime there is no "form" or "identity" entity that owns a stat — the composed skill item owns everything, exactly per the matrix above. Any future design that wants a *live* stat on a form or identity (e.g. a swappable identity changing damage type post-craft) must amend this section first.
*Justification: keeps the ownership matrix closed under the composition model — composition changes how skills are authored, not who owns stats at runtime.*

### E. Element wave — resist model & the Ailment stat family (added 2026-07-06)

The element wave promotes two surfaces alongside the identity rework (elements-only roster: Physical, Fire, Cold, Lightning — see `design-skills.md`).

- **Per-element enemy resistances (Model A).** Each damage type carries its **own soft resist channel** on the enemy (Physical / Fire / Cold / Lightning resistance), replacing the single lumped MagicResistance on the enemy side. Resistances stay soft (no immunity, §5C). *Justification: with no per-element **scaling** on the player side (all elements scale through their delivery channel, not a per-element stat), the enemy resist channel is the one place element choice becomes build-relevant — "bring the element this pack doesn't resist." Per-element resist **distribution** is also the lever that holds identity power-parity (see the EoT/ailment matrix row), so it must be a real per-type channel, not a lump.*
- **Ailment stat family (Hit/interaction surface — surface 6, 🟡 Deferred for *sources*).** A **generic** family — Ailment Damage, Ailment Effect (non-DoT magnitude), Ailment Duration, Ailment Chance — scales *all* ailments, never per-element. The signature ailments themselves are innate per identity (matrix row above); this family is how they *grow*. *Justification: generic (not per-element) scaling is the only model that fits three primary stats and two-plus delivery pools without inventing a stat per element; it is also how every reference ARPG scales ailments. The stats exist as a designed surface; their **sources** (gear affixes / augments) are deferred, so ailments currently sit at their innate baseline.*
- **EoT/ailment magnitude ownership (D1 — resolved 2026-07-07).** Magnitude and duration compose as `EotData` **baseline** (identity-owned — the single source of "how strong is Burn"; no skill authors its own magnitude, per §5C) × character **Ailment stats** (above) × an optional **per-form modifier**. The modifier is the newly-permitted surface, and it is constrained to **exactly two power-neutral shapes**: (1) a **duration↔magnitude re-slice** whose total effect integral is invariant — `fleet` = ×k magnitude / ×(1/k) duration, `enduring` the reverse (the EoT twin of salvo's ÷N payload split); or (2) a **cost-proportional scale** where magnitude rises only with a real budget cost paid — e.g. Focus reservation on a buff/debuff aura, so `reserve-heavy` = more reservation + stronger effect. Any other modifier (a free multiplier) is barred by §5C. Currently `EotData` freezes magnitude+duration on the shared record (`src/eot/EotData.cs`); the baseline-vs-modifier split is an **implementation-issue concern**, not built by this ruling. *Justification: a `None`-pattern debuff or a buff/debuff aura has no radius/tick lever to trade — its only lever is the effect's own strength, so a form needs a magnitude handle to exist at all. Keeping the baseline identity-owned and the modifier power-neutral gives forms that handle without opening a per-skill throughput leak. This is the D1 gate that parked fleet↔enduring and the reserve buff/debuff-aura branch (`design-skills.md`); both are now unblocked (adoption is downstream synthesis).*
- **Buff magnitude ownership (D1 extended to player buffs — 2026-07-07).** Player-buff magnitude follows the same model: the baseline `Value` lives on a **`BuffData` def** (buff-owned single source, a `BuffRegistry` mirroring `EotRegistry`), scaled by the same power-neutral **per-form modifier** — for a reserve buff-aura, the **cost-proportional** shape (magnitude rises with reservation paid). Never a free scalar (§5C). The scaling operates on the **bonus portion** (a `FlatAdd` value, a `PercentAdd` value, or a `Multiply` modifier's excess-over-1) so it is well-defined for every `ModifierType`. **No character buff-scaling stat family** — deliberately *not* added (unlike ailments' Ailment stat family above); buffs scale only by `BuffData` baseline × form modifier. **Framework reuses the existing `StatBlock` modifier engine** — a buff is a set of transient `StatModifier`s tagged with a new `Buff` `ModifierSource`, applied/torn down via the existing `AddModifier`/`RemoveModifiersFromSource`; no new stat math. Tracked in #64. *Justification: the buff surface gets the same baseline-vs-modifier ownership as ailments with zero new stat surfaces — the modifier engine and the D1 model both already exist, so buffs are a data + lifecycle layer, not a new system.*
- **Buff stat coverage (2026-07-07).** A buff may modify **any of the 12 `StatId`s** — the framework places no restriction; which buffs exist is a Balancer/authoring call. **`Range` is the one exception:** it is not a `StatId` (`EffectiveRange` composes multiplicatively over weapon+equipment), so range buffs stay on the ad-hoc `_rangeBuffBonus` path outside `StatBlock`. Unifying range into the buff framework (Range-as-`StatId` + `EffectiveRange` rework) is deferred future cleanup — not #64 scope, since no planned buff-aura needs range.
- **Buff lifecycle (2026-07-07).** Two apply modes: **timed** (a `Duration` → auto-remove on expiry) or **persistent** (no timer; removed explicitly — the aura/toggle mode). **Refresh-on-reapply, no stacking** — keyed by buff id, reapply refreshes and replaces its mods (mirrors `EnemyController.ApplyEot`); **stacking buffs are deferred** (they need the accumulation-state mechanic, the same gate blocking Kill-momentum). **Max-stat rule:** a MaxHp/MaxFocus buff adjusts the **ceiling only** — current value clamps to the new max, never granted (exploit-free; a per-buff "heal on apply" flag is a future option). The recompute wiring (re-applying `Buff`-sourced modifiers across `StatBlock` rebuilds + refreshing `PlayerController`'s cached derived fields on apply/expire) is #64 implementation, not a design surface.

### Open flags

- ✅ **Damage type owner = Skill** (skill-authoritative). Weapon `BaseDamageType` is fallback/display. Mutability decided 2026-07-04 — see the DamageType matrix row.
- ✅ **`InherentEotIds` on `SkillData`** — resolved 2026-07-04 (narrow to `DebuffEotId` for `DamagePattern == None`), then **the signature-EoT carve-out was consciously taken 2026-07-06**: the element wave gives every damage type an innate signature ailment owned by its **identity** (Bleed/Burn/Chill/Shock), the matrix amendment the earlier flag reserved. Implementation will need identity→signature-ailment wiring at flatten-time in addition to the `DebuffEotId` path. Tracked with the element-wave tickets.
- ⬜ **`TickRate` field** does not exist on `SkillData` yet; matrix says it must. Implementation deferred.
- ✅ **Boots drift** — resolved 2026-07-04: **docs follow code.** Boots is a first-class armour slot; all armour-composed stats (BonusHp, BonusSpeed, DamageReduction, RangeMultiplier) draw from Hat, Body, and Boots. `design-mechanics.md` and `design-progression.md` updated.

---

*Created 2026-07-04. Restructured same day around the six-surface model; Deferred/Parked surfaces split out to `design-stats-deferred.md`. Same day: grill session closed all open flags except `TickRate`, decided damage-type mutability, added the Deliberate Absences section (speed-family stance promoted out of the deferred doc).*
