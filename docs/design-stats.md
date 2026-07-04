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
| **Strength (Str)** | Physical Damage mult, Max HP, Physical Resistance, Crit Damage |
| **Dexterity (Dex)** | Crit Chance, Evasion |
| **Intelligence (Int)** | Magic Damage mult, Max Focus, Magic Resistance, Focus Regen |

Conversion rates live in `PrimaryStatConversions.cs` (Balancer-owned). Primary stats do **not** affect move speed, and level progression does not affect move speed.

### Derived stat block (`StatId`)

The in-game-effective stats. This is the "stat block" in the damage formula `weapon base × stat block`.

| Stat | What it does | Notes |
|---|---|---|
| **MaxHp** | Character health pool | Str-derived + Armour BonusHp + level |
| **Speed** | Move speed | Composed — see the move-speed model in `design-mechanics.md`. **Not** an archetype identity stat; all archetypes share one base |
| **PhysicalDamage** | Multiplier on weapon base for physical hits | Str-derived. This is a *multiplier*, not flat damage |
| **MagicDamage** | Multiplier on weapon base for magic hits | Int-derived. Multiplier, not flat |
| **PhysicalResistance** | Reduces incoming physical damage | Str-derived + accessory contributions |
| **MagicResistance** | Reduces incoming magic damage | Int-derived; no item source |
| **MaxFocus** | Focus resource pool (skill fuel) | Int-derived + level |
| **FocusRegen** | Focus regenerated per second | Int-derived + level |
| **CritChance** | Chance for a hit to crit | Dex-derived + Weapon CritChanceBonus + `critical_strike` augment |
| **CritDamage** | Crit damage multiplier | Base 1.5× + Str-derived |
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
| **DamageType** | **Skill** | **Skill-authoritative** (decided 2026-07-04). The skill defines the element. `weapon.BaseDamageType` is fallback/display only and never overrides the skill. **Mutability (decided 2026-07-04):** set at skill creation, not re-rollable by crafting; only a socketed augment may override it at fire time (e.g. the Magic Damage augment). |
| BaseDamage | **Weapon** | root of the damage number |
| ArmorCategory, DamageReduction | **Armour** | |
| MaxFocus, FocusRegen, Evasion, CritDamage, MagicResistance | **Character stat block** | derived from primary stats; MagicResistance has no item source |
| EoT payload (`EotId`) | **Augment** | On damage-dealing skills, EoTs come exclusively from augments — never baked in. **Narrow exception (amended 2026-07-04):** a skill with `DamagePattern == None` carries a single `DebuffEotId` — the debuff *is* its base behaviour, not an add-on (e.g. entity_debuff's Slow). No damage skill may ever carry an inherent EoT. |

### B. Composed stats (fixed formula, closed contributor list)

| Stat | Contributors | Formula / rule |
|---|---|---|
| **Damage number** | Weapon `BaseDamage` × Weapon `DamageBonus` × Character `Physical/MagicDamage` mult | weapon is root, character scales. **No per-skill damage multiplier — ever.** |
| **Crit chance** | Character `CritChance` + Weapon `CritChanceBonus` + `critical_strike` augment | additive |
| **Physical Resistance** | Character (Str-derived) + Ring accessory `PhysicalResistance` | additive |
| **Range** | Weapon `WeaponRange` × Armour `RangeMultiplier`, overridden by Skill `Range` | resolution chain |
| **Max HP** | Character stat block (Str-derived + level) + Armour `BonusHp` | additive |
| **Move speed** | Archetype base + Heavy/Light armour (−/+% per piece: hat, body, boots) + Slow EoT (−%) + Dash Reflex augment (+%) | additive %; full model in `design-mechanics.md` |

### C. Deliberate absences (decided stats that do NOT exist)

These are written decisions, not omissions. Adding any of them requires amending this section first.

| Absent stat | Rule | Decided |
|---|---|---|
| **Per-skill damage multiplier** | Never. Damage = `weapon BaseDamage × DamageBonus × character mult`. Rationale in `design-skills.md`. | 2026-07-04 (rationale documented) |
| **Character attack speed / cast speed / cooldown-recovery stat** | Never. Skill throughput is owned by the skill's own cooldown/tick levers (tuned by Balancer, advanced by skill tier) plus the single global **weapon CDR property** (`design-mechanics.md`). CDR may never appear on armour, rings, or augments — `design-directions.md`'s "CDR on skills" idea requires amending this row first. | 2026-07-04 (D5 closed) |
| **Enemy immunity (resistance ≥ 100%)** | Never. Enemy resistances always cap below 100% — a resistant enemy takes *reduced* damage, never zero. *Justification: hits are mono-typed by design (see below), so immunities would hard-wall mono-typed builds (the PoE immune-mob problem); capping resists is the clean alternative to forcing a "minimum physical damage" floor onto every hit.* | 2026-07-04 |
| **Composite/multi-typed hits & typed weapon damage** | Never (as designed). Every hit is 100% one damage type; weapon `BaseDamage` stays a typeless scalar stamped by the skill's `DamageType`. *Justification: (1) the crafting anchor — one weapon number must serve every build or the weapon pool forks into Str/Int weapons, breaking "any character equips any weapon"; (2) attribute build identity — phys→Str, magic→Int only stays meaningful if hits scale one channel; (3) keeps the D3 hit-event model closed — composite portions/conversion are the retrofit pain the D3 ⭐ warning exists for. D4-lineage model, chosen deliberately over the D2/PoE composite lineage.* | 2026-07-04 |

Related decision: **skill tier improves budget levers only** (never hit size); power parity between named clones is defined at equal tier — see `design-skills.md`.

### Open flags

- ✅ **Damage type owner = Skill** (skill-authoritative). Weapon `BaseDamageType` is fallback/display. Mutability decided 2026-07-04 — see the DamageType matrix row.
- ✅ **`InherentEotIds` on `SkillData`** — resolved 2026-07-04, amended same day: **narrow, don't delete.** Initial resolution was deletion, but entity_debuff (`DamagePattern: None`) delivers its Slow through the field — a debuff-pattern skill's payload is its base behaviour, not an augment-style add-on. Final rule: replace the general `InherentEotIds` list with a single `DebuffEotId`, valid only when `DamagePattern == None`. No signature-EoT carve-out for damage skills — that escape hatch (revisit if named skills feel same-y) still requires a conscious matrix amendment. Code change tracked as a GitHub issue.
- ⬜ **`TickRate` field** does not exist on `SkillData` yet; matrix says it must. Implementation deferred.
- ✅ **Boots drift** — resolved 2026-07-04: **docs follow code.** Boots is a first-class armour slot; all armour-composed stats (BonusHp, BonusSpeed, DamageReduction, RangeMultiplier) draw from Hat, Body, and Boots. `design-mechanics.md` and `design-progression.md` updated.

---

*Created 2026-07-04. Restructured same day around the six-surface model; Deferred/Parked surfaces split out to `design-stats-deferred.md`. Same day: grill session closed all open flags except `TickRate`, decided damage-type mutability, added the Deliberate Absences section (speed-family stance promoted out of the deferred doc).*
