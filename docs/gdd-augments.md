# Game Design Document — Augments

> Part of the GDD. See `gdd-progression.md` for gear slots, currencies, and UI. See `gdd-skills.md` for skill prototypes and targeting. See `gdd-mechanics.md` for EoT mechanics and damage pipeline.
> Living document — details will evolve as the game is playtested.

---

## Skill Augments

Skill Augments are craftable items that socket into a skill item to modify it. Any augment can go into any skill slot — no archetype or skill type restrictions. Each augment type can only be equipped once per skill (no duplicates).

**Ineffective combos — visual warning:** No augment is hard-locked from any skill, but some combinations produce no effect (e.g. Pierce on a Self skill — no projectile or impact point). The UI flags these with a warning indicator on the augment socket (red/yellow exclamation, hover tooltip explaining why). Exact indicator design TBD. Keeps the system open while giving players clear feedback when a slot is being wasted.

**Skill Augment slots per tier** — upgrading a skill unlocks deeper modification, not just bigger numbers:

| Skill tier | Skill Augment slots |
|------------|--------------|
| Common     | 1            |
| Uncommon   | 2            |
| Rare       | 3            |

- **Socketing:** choose a Skill Augment from inventory and place it into an open slot on the skill item
- **Removing:** free, Skill Augment returns to inventory

**Augment tag + trigger type system.** Each augment has a functional tag (e.g. `splash`, `pierce`, `slow`, `burn`). Each augment slot has a trigger type that declares which augment tags it accepts and how it fires. All skill augments use `on_enemy_hit_%` — the trigger % is a property of the augment item, rolled at craft time and re-rollable via crafting. Full tag/trigger taxonomy TBD at implementation.

**v1 Skill Augments:**

| Skill Augment | Tag | Trigger type | Effect |
|---|---|---|---|
| Splash | `splash` | `on_enemy_hit_%` | Hit damages a small area around the impact point. |
| Pierce | `pierce` | `on_enemy_hit_%` | Hit passes through the first enemy and continues. |
| Slow | `slow` | `on_enemy_hit_%` | Applies the Slow EoT on hit. |
| Burn | `burn` | `on_enemy_hit_%` | Applies the Burn EoT on hit. |
| Critical Strike | `crit` | `on_enemy_hit_%` | Adds a per-skill crit chance bonus on top of the global Dex-derived CritChance. |

Exact values (splash radius, trigger chances, slow %, burn damage, duration) are TBD.

All augments use `on_enemy_hit_%` trigger — including Splash and Pierce. The trigger % is a property of the augment item, rolled at craft time and re-rollable via crafting. Higher % is a meaningful crafting goal.

### Augment Resolution Order

Two augment categories determine resolution order:

- **Projectile augments** (Pierce, Chain) — resolve first. They determine what the projectile hits and how many times.
- **On-hit augments** (Splash, Burn, Slow, Critical Strike) — resolve on each resulting hit, independently and in parallel. Each rolls its `on_enemy_hit_%` independently per target.

**Splash** creates secondary hits on nearby enemies. Those secondary hits re-run all on-hit augments (each at their own %) — but never projectile augments. Splash breaks the projectile chain.

**Crit inheritance:** the primary hit's crit result is inherited by all splash-generated secondary hits. If the primary critted, all splash hits also crit. Splash hits do not roll crit independently.

**Augment interactions across damage types:** on-hit EoT augments are independent of the skill's damage type. A magic-type skill can carry a Burn augment — the hit deals magic damage and separately has a % chance to apply the Burn EoT.

**Socket order has no effect on resolution** — all on-hit augments resolve in parallel on each hit.

**Future augment pattern — mine/trap placement:** A mine augment triggers `on_enemy_hit_%` and places a proximity trap at the hit location. Successive hits place additional mines up to an active cap. The cap scales with augment tier (e.g. tier 1 = 2 active mines, tier 2 = 4, tier 3 = 6). This introduces augment-tier-scaling caps as a mechanic — design in full when crafting tiers are being expanded.

**Crafting cost (v1):** every Skill Augment costs **1 crafting resource** to craft.

Skill Augments are crafted (Craft New entry point TBD — not yet implemented; planned via left-click on an open augment socket) and live in the **Augments inventory tab**.

---

## Equipment Tags

Armour pieces (Hat and Body) carry a **category tag** that identifies their type. The tag drives the stat profile described in Hat & Body (see `gdd-progression.md`). No augment gating — any augment can socket into any equipment item regardless of category.

| Armour | Tag |
|---|---|
| Hat / Body (Heavy) | `Heavy` |
| Hat / Body (Medium) | `Medium` |
| Hat / Body (Light) | `Light` |

Category is fixed per item — a Heavy hat stays Heavy regardless of tier.

---

## Equipment Augments

Equipment Augments follow the same model as Skill Augments — separate socketable items, one type per equipment piece at a time, trigger % is rolled at craft time and re-rollable.

Equipment Augments are craftable items that socket into an equipment item to add a **behaviour** — not just a stat bonus, but something that changes how that piece of equipment *feels* to use. They are the gear-layer equivalent of Skill Augments.

**Design intent — Equipment Augments are the defensive build layer.** Skill Augments handle offensive variance (splash, crit, pierce, damage conversion). Equipment Augments handle defensive variance — how the character survives, recovers, and punishes attackers. There is no stagger or hit-recovery system; the hit feedback is D4-style (always in control, no interrupts). All defensive investment flows through Equipment Augments. Future equipment augments should continue in this direction: barriers, dodge, resistance boosts, shield-on-hit, recovery mechanics. Offensive Equipment Augments (weapon/ring slot) are TBD and secondary to this defensive purpose.

**Design rule:** `always` and `on_player_hit_%` augments may not deal proactive offensive damage — reactive damage (e.g. Retaliation/thorns) is acceptable. Auras via equipment augments are defensive or debuff only — a damage aura belongs on a skill augment, not armour. Offensive utility (e.g. cooldown reduction) is a grey area — flag for review when new augments are designed.

**Equipment Augment slots per tier** — mirrors the Skill Augment slot system:

| Equipment tier | Equipment Augment slots |
|----------------|------------------------|
| Common         | 1                      |
| Uncommon       | 2                      |
| Rare           | 3                      |

- **Socketing:** choose an Equipment Augment from inventory and place it into an open slot on the equipment item
- **Removing:** free, Equipment Augment returns to inventory
- Any augment can socket into any equipment item — no tag gate. Each augment type can only be equipped once per item (no duplicates).

**v1 Equipment Augments** — out of scope for v1. Listed here to establish the design space, not to be implemented. Same augment tag + trigger type system as skill augments, but player-based triggers:

| Augment     | Tag           | Trigger type        | Behaviour |
|-------------|---------------|---------------------|-----------|
| Retaliation | `retaliation` | `on_player_hit_%`   | On hit received: deal small Physical damage to attacker |
| Fortify     | `fortify`     | `on_player_hit_%`   | On hit received: reduce damage taken from the next hit |
| Dash Reflex | `dash_reflex` | `on_player_hit_%`   | On hit received: brief speed burst |
| Ghost Step  | `ghost_step`  | `on_kill_%`         | On kill: restore a small amount of HP |
| Mending     | `mending`     | `on_player_hit_%`   | Regenerate a small amount of HP every 3s |

Exact trigger chances and values are TBD — owned by the Balancer.

**Aura augments:** Auras are Equipment Augments with the `aura` tag — a persistent area effect emanating from the player. The trigger type is player's choice: an `always` slot keeps the aura permanently active; an `on_player_hit_%` slot fires it reactively on taking a hit. The augment tag defines what the aura does; the trigger type defines when it runs. Focus reservation may apply as a cost for `always` aura augments — TBD when augment design is expanded.

**Crafting cost (v1):** every Equipment Augment costs **1 crafting resource** to craft.

Equipment Augments are crafted (Craft New entry point TBD — not yet implemented; planned via left-click on an open augment socket) and live in the **Augments inventory tab**.

---

## Augment Prototypes

Augment prototypes follow the same philosophy as skill prototypes: each one proves a specific concept, covers a distinct design space, and is tested in isolation before combining. All augment prototypes are v1.

Named augments (e.g. "Slow", "Burn") are post-v1 and will be derived from these prototypes.

### Skill Augment Prototypes

| Prototype | Tag | Trigger | Concept proven |
|---|---|---|---|
| Hit-EoT-Debuff | `slow` | `on_enemy_hit_%` | Hit lands → on-hit % fires → non-damage EoT (Slow) applied |
| Hit-EoT-Damage | `burn` | `on_enemy_hit_%` | Hit lands → on-hit % fires → damage EoT (Burn) applied; crit stamping |
| Hit-AoE | `splash` | `on_enemy_hit_%` | Hit lands → on-hit % fires → secondary AoE hit around impact |
| Hit-Pierce | `pierce` | `on_enemy_hit_%` | Projectile augment resolves first; hit passes through, continues to next enemy |
| Hit-Damage-Mod | `crit` | `on_enemy_hit_%` | Per-skill crit chance bonus stacks with global Dex crit; crit result confirmed |

#### Hit-EoT-Debuff

Proves the non-damage EoT path. A % chance on each hit to apply Slow to the struck enemy — no damage, visible movement penalty only. Tests: Slow applies and expires correctly, doesn't stack, reapplication refreshes duration.

| Property | Value |
|---|---|
| Description | On-hit % chance to apply Slow EoT. Proves non-damage EoT augment path. |
| Tag | `slow` |
| Trigger type | `on_enemy_hit_%` |
| Trigger chance | 50% (test value) |
| Effect | Slow EoT for 3s (test value) |
| Acquire | Craft |

#### Hit-EoT-Damage

Proves the damage EoT path and crit stamping. A % chance on each hit to apply Burn — damage ticks until duration expires. Tests: Burn ticks deal magic damage per tick, crit stamp applies full duration if applying hit was a crit, reapplication refreshes (no double-stack), non-crit reapplication resets the crit multiplier.

| Property | Value |
|---|---|
| Description | On-hit % chance to apply Burn EoT. Proves damage EoT augment path and crit stamping. |
| Tag | `burn` |
| Trigger type | `on_enemy_hit_%` |
| Trigger chance | 50% (test value) |
| Effect | Burn EoT — 2 magic damage/tick, 1 tick/sec, 4s duration (test values) |
| Acquire | Craft |

#### Hit-AoE

Proves splash — on-hit secondary hits spread to nearby enemies. Tests: primary hit triggers % roll, on success nearby enemies in splash radius take damage, splash hits also run all on-hit augments (each independently), splash never triggers another splash.

| Property | Value |
|---|---|
| Description | On-hit % chance to deal splash damage to enemies near the impact point. Proves AoE augment path. |
| Tag | `splash` |
| Trigger type | `on_enemy_hit_%` |
| Trigger chance | 60% (test value) |
| Effect | Hits all enemies within 1.5-tile radius of impact (test value) |
| Acquire | Craft |

#### Hit-Pierce

Proves projectile augment resolution. When triggered, the projectile continues past the first enemy and can hit the next one in its path. Tests: resolves before on-hit augments, continuation hits run on-hit augments normally, only one continuation per pierce (not infinite chain).

| Property | Value |
|---|---|
| Description | On-hit % chance for the projectile to pierce and continue past the struck enemy. Proves projectile augment path. |
| Tag | `pierce` |
| Trigger type | `on_enemy_hit_%` |
| Trigger chance | 40% (test value) |
| Effect | Projectile continues past first enemy, hits next enemy in path |
| Acquire | Craft |

#### Hit-Damage-Mod

Proves per-skill crit chance bonus stacking on top of global Dex crit. Tests: stat shows correctly on character sheet, crits fire at the combined %, crit multiplier applies correctly, works for all damage types.

| Property | Value |
|---|---|
| Description | Adds a per-skill crit chance bonus on top of the global Dex-derived CritChance. Proves damage mod augment path. |
| Tag | `crit` |
| Trigger type | `on_enemy_hit_%` |
| Trigger chance | N/A — always active; modifies the skill's crit roll, not a separate triggered event |
| Effect | +20% crit chance for this skill (test value) |
| Acquire | Craft |

---

### Equipment Augment Prototypes

Equipment augment prototypes are **out of scope for v1**. Listed here to establish the design space for v2.

| Prototype | Tag | Trigger | Concept proven |
|---|---|---|---|
| Equip-Always | `mending` | `always` | Passive always-on background regen effect |
| Equip-Reactive | `retaliation` | `on_player_hit_%` | Reactive effect fires on taking damage |
| Equip-Reactive-Move | `dash_reflex` | `on_player_hit_%` | Reactive movement/speed effect on hit received |
| Equip-Regen | `ghost_step` | `on_kill_%` | Kill-triggered recovery; reward for aggressive play |

---

### Augment Mix Prototypes

Augment mixes are combinations tested together after individual augment prototypes are verified. Each mix proves an interaction or reveals a balance concern.

| Mix | Augments combined | Concept proven |
|---|---|---|
| AoE+EoT | Hit-AoE + Hit-EoT-Damage | Splash secondary hits independently roll Burn; crit stamp propagates through splash correctly |
| Skill+Equip | Hit-EoT-Debuff (skill) + Equip-Reactive (body) | Skill augment and equipment augment fire independently; no cross-interference |
| Ineffective combo | Hit-Pierce on Self-Burst | Pierce has no effect on a Self skill (no projectile); UI warns on socket; no error on fire |
