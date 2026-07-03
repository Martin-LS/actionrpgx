# Game Design Document — Stats: Deferred & Parked Surfaces

> Companion to **[design-stats.md](design-stats.md)** — read section 0 there first for the surface/tier model.
> This doc holds the 🟡 Deferred and ⚪ Parked stat surfaces. It is the **cold** doc: load it only when a push-back needs substance or a surface is being promoted to Active.
>
> **The golden rule applies throughout: "out of scope" ≠ "never mention."**

## Scope banner (applies to every section below)

> **Scope: Deferred/Parked.** This surface is real and matters in ARPGs. It is intentionally out of scope for now — do not design it into features unprompted. It is **not** forbidden: raise it as an interesting push-back whenever a design decision touches it. Promoting a surface = discuss → change its tier in `design-stats.md` → then design.

---

## 🟡 Deferred surfaces

### D1. Enemy / monster stats

What modern ARPGs put here: enemy HP/damage/resist curves, rarity tiers (normal/magic/rare/elite/boss) with modifier pools, pack-size and pack-modifier systems, aggro/leash stats, crowd-control resistance.

Our current state: basic (`EnemyPoolEntry` with flat HP/damage bonuses, `BalanceConfig.Enemies` scaling consts, per-type table in `design-mechanics.md`). No formal enemy stat model.

Keep in mind: enemy resistances only matter once skill damage types matter — the skill-authoritative DamageType decision makes enemy resist design a natural follow-on.

### D2. Map / run / encounter stats

What modern ARPGs put here: area level, enemy density modifiers, environmental hazards, drop quantity/rarity bonuses, run modifiers the player opts into for reward (PoE map mods, D4 sigils).

Our current state: `design-map.md` has exactly one attribute (Map Level → XP scaling), with density/hazards/drop bonuses named as future.

Keep in mind: **maps are crafted in our game** — map attributes are the natural sink for map-crafting outcomes. When map crafting gets designed, this surface promotes almost automatically.

### D3. Hit / interaction stat model ⭐ *(recommend revisiting early)*

The stats of the **damage event itself** — not owned by character, skill, or item, but assembled from contributors at the moment of a hit: penetration, added flat damage, damage conversion, ailment chance & magnitude, leech, on-hit triggers.

Our current state: this is the "composed stats" layer of the ownership matrix (damage number, crit chance). The composed-stat concept already is a hit model in miniature — it just has very few entries.

Why ⭐: the **closed contributor list** for a damage event is structural — retrofitting penetration or conversion after content exists is one of the most painful refactors an ARPG can do. Deciding what a hit *can never have* is as valuable as deciding what it has. The "no per-skill damage multiplier" rule lives here and needs deliberate company or deliberate absence.

### D4. Reward / economy stats ⭐ *(recommend revisiting early)*

Drop quantity, drop rarity, XP gain — and in our craft-driven game specifically: **crafting-material quantity and rarity**. Since enemies drop only materials and all progression is crafting, these stats *are* the economy.

Why ⭐: whether "increased material quantity/rarity" exists as a stat, and which surface owns it (character gear? map attribute? both?), is a first-class economy decision that shapes itemisation — not a late tuning knob.

### D5. Speed family stance — ✅ DECIDED & PROMOTED (2026-07-04)

Resolved: no character-level attack/cast/CDR stat, ever. The decision now lives in `design-stats.md` section 5C (Deliberate absences). This entry remains only as a tombstone.

---

## ⚪ Parked (niche / special-case)

One line each; all real ARPG stats, none needed to prove our pillars:

- **Block / parry** — shield-gated mitigation stat (PoE block, Grim Dawn shields).
- **Dodge-as-stat** — probabilistic full-avoid distinct from our Evasion (we have Evasion; a second avoidance layer is parked).
- **Minion stats** — minion damage/HP/speed scaling; whole sub-system (relevant only if summon-type named skills get designed).
- **Flask / charge systems** — consumable-with-stats sub-system (PoE flasks, D4 potion upgrades).
- **Curse / debuff effect %** — "effect of your debuffs" scaling stat.
- **Aura effect %** — "effect of your auras" scaling stat (self_aura exists, but aura *effect scaling* is parked).
- **Max-resist / mitigation caps** — cap-raising stats; matters only once resists have caps.
- **Life/shield on-hit, leech gating** — recovery-on-action stats beyond FocusRegen.
- **"-to-enemy" debuff stats** — exposure/shred style resist reduction and damage-taken amplification applied to enemies. *Promotion path already designed (2026-07-04, brainstorm doc): implemented as an enemy-owned EoT type ("Exposed"), delivered via existing debuff machinery — never as a skill-owned multiplier.*

---

*Created 2026-07-04, split out of `design-stats.md`. Same day: D5 (speed family) decided and promoted to the main doc.*
