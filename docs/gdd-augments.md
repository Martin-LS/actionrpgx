# Game Design Document — Augments

> Part of the GDD. See `gdd-progression.md` for gear slots, currencies, and UI. See `gdd-skills.md` for skill prototypes and targeting. See `gdd-mechanics.md` for EoT mechanics and damage pipeline.
> Living document — details will evolve as the game is playtested.

---

## Skill Augments

Skill Augments are craftable items that socket into a skill item to modify it. Any augment can go into any skill slot — no archetype or skill type restrictions. Each augment type can only be equipped once per skill (no duplicates).

**Skill Augment slots per tier** — upgrading a skill unlocks deeper modification, not just bigger numbers:

| Skill tier | Skill Augment slots |
|------------|--------------|
| Common     | 1            |
| Uncommon   | 2            |
| Rare       | 3            |

- **Socketing:** choose a Skill Augment from inventory and place it into an open slot on the skill item
- **Removing:** free, Skill Augment returns to inventory
- **Conflict Groups:** certain augments share a Conflict Group. A skill item cannot have more than one socketed augment sharing the same Conflict Group (for example, you cannot socket multiple damage type conversion augments on the same skill).

**Augment tag + trigger type system.** Each augment has a functional tag. Each augment slot has a trigger type that declares which augment tags it accepts and how it fires. All skill augments use `on_enemy_hit_%` — the trigger % is a property of the augment item, rolled at craft time and re-rollable via crafting. Full tag/trigger taxonomy TBD at implementation.

**v1 Skill Augments — generics only.** v1 uses three generic augments, each proving a concept. Named versions (Burn, Splash, Pierce, etc.) are post-v1 and will be derived from these generics in the same way named skills are derived from skill prototypes.

| Skill Augment | Tag | Trigger type | Proves |
|---|---|---|---|
| Magic Damage | `magic_damage` | `on_enemy_hit_%` | Damage-type augment: modifies the skill's effective damage type to Magic. Future named versions: Burn (magic DoT), Arcane Bolt, etc. |
| Slow | `slow` | `on_enemy_hit_%` | Debuff augment: applies the Slow EoT on hit. Future named versions: Chill, Weaken, etc. |
| Critical Strike | `crit` | `on_enemy_hit_%` | Stat-mod augment: adds a per-skill crit chance bonus on top of the global Dex-derived CritChance. |

Exact trigger chances and values are TBD — owned by the Balancer.

### Augment Resolution Order

All v1 skill augments are on-hit augments — they resolve on each hit, independently and in parallel. Each rolls its `on_enemy_hit_%` independently per target. Socket order has no effect.

**Post-v1 resolution categories** (for when named augments are added):

- **Projectile augments** (e.g. Pierce, Chain) — will resolve first, determining what the projectile hits and how many times.
- **AoE augments** (e.g. Splash) — will create secondary hits on nearby enemies. Secondary hits re-run all on-hit augments independently, but never projectile augments.
- **Crit inheritance** — if the primary hit crits, all splash-generated secondary hits will also crit. Splash hits do not roll crit independently.

**Augment interactions across damage types:** on-hit EoT augments are independent of the skill's damage type. A magic-type skill can carry a Slow augment — the hit deals magic damage and separately has a % chance to apply the Slow EoT.

**Future augment pattern — mine/trap placement:** A mine augment triggers `on_enemy_hit_%` and places a proximity trap at the hit location. Successive hits place additional mines up to an active cap. The cap scales with augment tier (e.g. tier 1 = 2 active mines, tier 2 = 4, tier 3 = 6).

**Crafting cost (v1):** every Skill Augment costs **1 crafting resource** to craft.

Skill Augments are crafted (Craft New entry point TBD — not yet implemented; planned via left-click on an open augment socket) and live in the **Skill Augments inventory tab**.

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

**v1 Equipment Augments** — same augment tag + trigger type system as skill augments, but player-based triggers:

| Augment     | Tag           | Trigger type        | Behaviour |
|-------------|---------------|---------------------|-----------|
| Retaliation | `retaliation` | `on_player_hit_%`   | On hit received: deal small Physical damage to attacker |
| Fortify     | `fortify`     | `on_player_hit_%`   | On hit received: reduce damage taken from the next hit |
| Dash Reflex | `dash_reflex` | `on_player_hit_%`   | On hit received: brief speed burst |
| Ghost Step  | `ghost_step`  | `on_kill_%`         | On kill: restore a small amount of HP |
| Mending     | `mending`     | `always`            | Regenerate a small amount of HP every 3s |

Exact trigger chances and values are TBD — owned by the Balancer.

**Aura augments:** Auras are Equipment Augments with the `aura` tag — a persistent area effect emanating from the player. The trigger type is player's choice: an `always` slot keeps the aura permanently active; an `on_player_hit_%` slot fires it reactively on taking a hit. The augment tag defines what the aura does; the trigger type defines when it runs. Focus reservation may apply as a cost for `always` aura augments — TBD when augment design is expanded.

**Crafting cost (v1):** every Equipment Augment costs **1 crafting resource** to craft.

Equipment Augments are crafted (Craft New entry point TBD — not yet implemented; planned via left-click on an open augment socket) and live in the **Equipment Augments inventory tab**.

---

## Augment Prototypes

Augment prototypes follow the same philosophy as skill prototypes: each one proves a specific concept, covers a distinct design space, and is tested in isolation before combining. All augment prototypes are v1.

Named augments (e.g. "Burn", "Splash", "Pierce") are post-v1 and will be derived from these generics.

### Skill Augment Prototypes

| Prototype | Tag | Trigger | Concept proven |
|---|---|---|---|
| Hit-Debuff | `slow` | `on_enemy_hit_%` | Debuff augment path: on-hit % → non-damage EoT applied |
| Hit-Damage | `magic_damage` | `on_enemy_hit_%` | Damage augment path: switches skill's effective damage type to Magic |
| Hit-Damage-Mod | `crit` | `on_enemy_hit_%` | Stat-mod augment path: per-skill crit bonus stacks with global Dex crit |

#### Hit-Debuff

Proves the debuff augment path. A % chance on each hit to apply Slow — no damage, visible movement penalty. Tests: Slow applies and expires correctly, doesn't stack, reapplication refreshes duration.

| Property | Value |
|---|---|
| Description | On-hit % chance to apply Slow EoT. Generic debuff augment — post-v1 named versions: Chill, Weaken, etc. |
| Tag | `slow` |
| Trigger type | `on_enemy_hit_%` |
| Trigger chance | 50% (test value) |
| Effect | Slow EoT for 3s (test value) |
| Acquire | Craft |

#### Hit-Damage

Proves the damage-type augment path. Switches the skill's effective damage type to Magic. The Mage archetype uses this augment on their starter skill. Tests: damage numbers show as magic type, magic resistance applies, works regardless of equipped weapon.

| Property | Value |
|---|---|
| Description | Converts the skill's effective damage type to Magic. Generic damage augment — post-v1 named versions: Burn (magic DoT), Arcane Bolt, etc. |
| Tag | `magic_damage` |
| Trigger type | `on_enemy_hit_%` |
| Trigger chance | N/A — always active when socketed |
| Effect | Skill deals Magic damage instead of its base type |
| Acquire | Craft |

#### Hit-Damage-Mod

Proves the stat-mod augment path. Per-skill crit chance bonus stacks on top of global Dex crit. Tests: crits fire at the combined %, crit multiplier applies correctly, works for all damage types.

| Property | Value |
|---|---|
| Description | Adds a per-skill crit chance bonus on top of the global Dex-derived CritChance. Generic stat-mod augment. |
| Tag | `crit` |
| Trigger type | `on_enemy_hit_%` |
| Trigger chance | N/A — always active; modifies the skill's crit roll |
| Effect | +20% crit chance for this skill (test value) |
| Acquire | Craft |

---

### Equipment Augment Prototypes

Equipment augment prototypes are **v1 scope** — all five are implemented, craftable, and active in the runtime.

| Prototype | Tag | Trigger | Concept proven |
|---|---|---|---|
| Equip-Always | `mending` | `always` | Passive always-on background regen effect |
| Equip-Reactive | `retaliation` | `on_player_hit_%` | Reactive effect fires on taking damage |
| Equip-Reactive-Def | `fortify` | `on_player_hit_%` | Damage reduction on the next hit after being struck |
| Equip-Reactive-Move | `dash_reflex` | `on_player_hit_%` | Reactive movement/speed effect on hit received |
| Equip-Regen | `ghost_step` | `on_kill_%` | Kill-triggered recovery; reward for aggressive play |

---

### Augment Mix Prototypes

Augment mixes are combinations tested together after individual augment prototypes are verified. Each mix proves an interaction or reveals a balance concern.

| Mix | Augments combined | Concept proven |
|---|---|---|
| Damage+Debuff | Hit-Damage + Hit-Debuff on same skill | Both augments fire independently on each hit; magic damage type applies, Slow EoT rolls separately |
| Damage+Crit | Hit-Damage + Hit-Damage-Mod on same skill | Magic damage type applies; crit rolls at the boosted chance; crit multiplier applies correctly |
| Debuff+Crit | Hit-Debuff + Hit-Damage-Mod on same skill | Slow EoT rolls at its own %; crit fires at boosted chance; both operate independently per hit |
