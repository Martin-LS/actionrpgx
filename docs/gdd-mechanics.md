# Game Design Document — Mechanics & Characters

> Part of the GDD. See also `gdd-skills.md` for skill design, skill prototypes, and AoE rules. See `gdd-augments.md` for Skill Augments, Equipment Augments, and augment prototypes. See `gdd-progression.md` for meta-progression, gear, crafting, and UI.
> Living document — details will evolve as the game is playtested.

## Overview

A top-down action RPG in the vein of Diablo and Path of Exile. The player builds a persistent character, equips crafted gear and skills, and takes them into combat runs against escalating enemies.

**Core design goal — fully craft-driven progression:**
- Items, skills, and maps are all obtained through crafting. Nothing meaningful is dropped by enemies directly.
- Enemies drop only **crafting materials**. Materials are the currency of all progression.
- The crafting system is the progression engine: players farm materials in runs, then craft and upgrade their gear, skills, and maps between runs.

Every run makes the character permanently stronger: level and XP carry over, stat bonuses stack, and all crafting materials earned go into a shared account pool. The game has two intertwined goals: **grow your character through runs**, and **build your power through crafting**.

---

## Core Mechanics

### Movement
- Top-down, 8-directional
- **v1:** WASD movement + Space dodge roll. No movement skills.

**Move speed model — all archetypes share one base speed.** Speed is not an archetype identity stat — every archetype starts at the same base. Speed variance comes entirely from gear and effects:

| Source | Effect |
|---|---|
| Archetype base | Shared flat value (all archetypes) |
| Heavy armour | −% per piece (hat + body each contribute) |
| Light armour | +% per piece |
| Medium armour | No modifier |
| Slow EoT | −% for duration |
| Dash Reflex augment | Temporary +% on hit received |

- Rogue feels faster naturally because Light armour is their default — not because of a higher base
- Primary stats (Str/Dex/Int) do not affect speed
- Level progression does not affect speed
- Movement is instant top-speed — no acceleration or deceleration
- Speed values are placeholder, owned by the Balancer

### Combat

Skills drive all combat. Each skill has a **type**:

| Skill type | Behaviour | Focus cost |
|---|---|---|
| Active | Fires on manual activation or auto-activate on cooldown. | Flat cost per activation |
| Channeled | Hold button to run, release to stop. Drains Focus continuously while held. Stops automatically at 0 Focus. Auto-cast holds the button indefinitely — will empty the Focus bar if left unchecked. Player responsibility. | Per-second drain while held |
| Aura | Toggle on — reserves a flat amount of Focus and begins pulsing its effect on every tick. Toggle off — unreserves Focus immediately. Does not auto-deactivate at 0 Focus; the reservation is committed at toggle time. | Flat amount reserved from Max Focus while active (not spent — locked until toggled off) |

The **skill bar** on the run HUD shows the slotted skill, its cooldown state, and whether auto-activate is enabled.

**Auto-activate.** The player can toggle their skill to fire automatically on cooldown. When enabled, movement is where the player's active attention lives — positioning, dodging, kiting. Auto-activate must be DPS-equivalent to manual: a player pressing the skill key manually on cooldown gets the same output as auto-activate. It is pure convenience, not a power reduction. Auto-activate is retained in the codebase for development convenience only — all skill design assumes manual casting.

**v1:** 5 skill slots. Slots can be empty — an empty slot does nothing. All slots are available from the start, no unlock progression. Each skill has its own cooldown or drain rate.

**Attack / cast speed — no character stat; CDR lives on the weapon.** There is no global attack speed stat on the character. A skill's cooldown belongs to the skill item and is reduced by tier upgrades. The weapon's **CDR property** is the one gear-level lever — it applies globally to all skills regardless of type. Different weapon types have different base CDR values; v1 CDR is fixed per weapon type (no roll variance).

**Damage model.** The weapon provides the base damage number. The skill defines the damage type. Delivery (how the attack animates) is always driven by the equipped weapon — a Sword always swings, a Bow always shoots, a Wand always fires a bolt — regardless of which skill is equipped.

**Weapon bonus is type-agnostic.** Each weapon type carries an identity bonus (e.g. Sword +10% damage, Wand +10% damage, Bow +8% crit chance). This bonus applies to all damage the character deals regardless of skill damage type — a Sword warrior casting a magic skill still benefits from the Sword's damage bonus. There are no skill-type gates on weapon bonuses.

Damage output scales through the archetype's primary stat growth — a Warrior gains Strength faster per level, which converts to higher PhysicalDamage; a Mage gains Intelligence faster, which converts to higher MagicDamage. A mismatched build (e.g. Warrior equipping a magic-type skill) is viable but produces lower output because the stat multiplier for that damage type grows slowly. The weapon bonus never blocks or reduces mismatched output — only the stat multiplier is weaker. See Archetype Stat Multipliers for the full formula.

### Targeting

**Entity skills** fire at the **locked target** — a single enemy that has a persistent target marker on them. **Self skills** ignore the lock entirely and always fire from the player. The targeting system is always active; players on keyboard experience it as "skills just work." Controller players can redirect the lock with the right stick.

**How the lock works:**

1. **Auto-pick** — at run start, when no lock exists, or when the current target dies, the game silently picks the nearest enemy in the player's facing or movement direction.
2. **Soft lock** — the marker persists on that enemy until it dies. Skills fire at the locked target regardless of player facing or movement direction.
3. **Right stick override (controller)** — pushing the right stick sweeps the target marker through nearby enemies in that direction. Aim assist magnetises to the nearest enemy in the pushed direction. Releasing the stick holds the last selected target.
4. **Skill targeting shape** — each skill declares one of three targeting shapes (see below); the targeting system resolves the correct input per shape and per input device automatically.
5. **Character facing** — the character faces the locked target during skill casts, decoupled from movement direction (see Combat Facing).

**Priority rules:** nearest enemy wins. If multiple enemies are equidistant, the one closest to the player's current facing direction is preferred.

**Keyboard behaviour:** steps 1–2 handle everything automatically. No manual targeting input exists or is needed — the system is invisible.

**Controller behaviour:** same auto-pick foundation; right stick adds voluntary override without changing anything else.

#### Skill Targeting Shapes

Every skill declares one of three targeting shapes. The targeting system resolves the correct position or entity per shape and per input device:

| Shape | Description | Mouse (PC) | Controller / Keyboard |
|---|---|---|---|
| **Self** | Effect originates from or is centered on the player | Player position | Player position |
| **Entity** | Effect is applied to a specific enemy; blocked if no valid target | Nearest enemy to cursor | Locked target |
| **Position** | Effect lands at a ground location; no enemy required | Cursor world position | Locked target's world position |

- **Self:** no targeting input needed — always fires from the player.
- **Entity:** must land on an enemy. On PC snaps to the nearest enemy to the cursor. On controller/keyboard fires at the locked target. Skill is blocked if no valid target exists.
- **Position:** requires manual ground placement. Best suited to manual-cast builds.

#### Range resolution per targeting shape

**This is a firm design rule — push back if a proposed skill violates it.**

| Shape | Cast range source | Rationale |
|---|---|---|
| **Entity** | Effective Range (weapon + armour + buffs) | You are reaching out to hit an enemy — your weapon's reach determines how far you can do that |
| **Position** | Skill's own `Range` field | You are placing an effect on the ground — this is a skill property, not a weapon property. A sword warrior can drop a zone as far as a wand mage if the skill allows it. |
| **Self** | Skill's own `Range` field | The skill defines its own radius — a wide Self-Duration-Tick radius on a sword warrior should not be shrunk by the sword's 1.5-tile reach |

- **Entity skills always use Effective Range.** A new Entity skill must not define a separate cast range — it inherits the character's gear-driven range automatically.
- **Position, Self, and Channeled skills always use their own `Range` field.** This is a skill property, not a gear property. Weapon and armour have no influence on zone placement distance or self/channeled radius.
- **Buffs that modify range** (e.g. a future Shout skill) must call `AddRangeBuffBonus` / `RemoveRangeBuffBonus` on the player — they affect Effective Range, which propagates to Entity skills only. Position/Self/Channeled ranges are unaffected.
- **Out-of-range clamping (Position skills):** if the cursor is beyond the skill's cast range, the zone lands at the range boundary in the direction of the cursor — never blocked, never silent. This matches standard ARPG behaviour (Diablo 4, PoE).

---

### Combat Facing

While **not attacking**, the character faces their movement direction. While **attacking** (OneShot animation active), the character always rotates to face the **locked target** — even if the player is moving in the opposite direction. This ensures attacks always connect visually and lets players kite while staying engaged with targets behind them.

### Weapon Animations & Handedness

Weapons are held in different hands depending on type, which drives which animation plays for melee attacks.

| Weapon type | Hold hand (visual) | Skeleton bone | Melee animation |
|---|---|---|---|
| Sword, Axe, Club, Dagger | Right hand | `Hand_R` | `melee_right_atack` |
| Bow | Left hand | `Hand_L` | `melee_left_atack` |
| Wand | Right hand | `Hand_R` | `melee_right_atack` |

- **`melee_right_atack`** — right-arm swing/slash; used by all right-hand weapons
- **`melee_left_atack`** — left-arm horizontal sweep or butt-strike; used when a bow is equipped and a melee skill fires

The weapon's `AttachBone` property (future field on `WeaponData`) drives which bone the mesh attaches to at runtime. The `OnSkillFired` handler selects the animation based on the equipped weapon's hold hand, not the skill's delivery tag alone.

Idle and run animations are shared across all weapon types in v1.

**Attack animation speed syncs to cooldown.** The animation playback speed is set dynamically at fire time so the clip completes in exactly one cooldown window (`scale = animLength / cooldown`). Damage lands at 35% through the cooldown (the wind-up frame) rather than instantly. As attack speed increases (shorter cooldown), the animation visibly speeds up — the same feel as Diablo's attack speed scaling.

### Hit Feedback

**Design reference: Diablo 4.** No character animation or action interrupt on hit — the player stays in full control through all damage. There is no stagger system, no hit-recovery stat, no flinch animation. Defensive build variance comes entirely from Equipment Augments (see `gdd-progression.md`).

Danger is communicated through health/shield depletion, not through action interruption. This keeps horde combat fluid regardless of difficulty.

#### HP Bars

Both the player and enemies display a floating HP bar above their head.

| Entity | Visibility | Fill colour | Hex |
|---|---|---|---|
| Player | Always visible | Danger Red | `#A32D2D` |
| Enemy | Appears on hit; fades after ~2s of no damage | Muted Red | `#8C2E2E` |

Both bars use Iron Black (`#181C1F`) as the track background. Bar width scales proportionally to entity size. The player's floating bar coexists with the HUD health bar — both are always present during a run.

Enemy bars are on-hit only to preserve readability during large hordes: bars only appear where hits are landing, keeping the screen uncluttered at peak density.

#### Damage Numbers

Every hit displays a floating damage number above the struck entity's head. Numbers float upward and fade out over ~0.8s.

| Case | Colour | Hex | Size |
|---|---|---|---|
| Physical hit | Bone White | `#E8DCC8` | Normal |
| Magic hit | Ice Shimmer | `#B8D8E8` | Normal |
| Critical hit | Gold | `#D4A017` | ~50% larger |

Critical hit colour overrides the damage-type colour — a magic crit shows gold, not blue. This makes crits immediately legible regardless of damage type.

Numbers are individual per hit — no stacking. self_channeled_tick ticks each pop their own number; this preserves tick-rate readability and lets the player feel the difference between a fast and slow attack speed.

Both player-received and enemy-received hits produce damage numbers. There is no threshold — all damage shows.

### Skills

> Skill design rules, targeting shapes, AoE rules, skill prototypes, and damage model are in `docs/gdd-skills.md`.

### Focus

Focus is the universal skill resource. All archetypes spend Focus to fire skills; skills cannot activate when Focus is empty.

**Regeneration:** Passive regen over time at a steady rate. Always recovering — no kill-based acceleration.

**Aura reservation:** Active Auras lock a flat amount of Focus permanently while toggled on. The available pool — the amount other skills can spend — is `CurrentFocus − reserved`. Auras do not auto-deactivate at 0 Focus; their reservation is committed at toggle time and held until toggled off.

**At 0 Focus:** Skills cannot fire. Cooldowns still count down; auto-activate waits until enough Focus regens to cover the cost. Channeled skills stop automatically when Focus hits 0. Auras are unaffected — their cost is already reserved, not re-spent each tick.

**Channeled skill tag:** Skills tagged `Channeled` drain Focus continuously while the button is held. Releasing the button stops the skill immediately. If Focus hits 0 the skill stops automatically regardless of input. Auto-cast holds the button indefinitely — player responsibility to not auto-cast a skill that drains their entire Focus bar.

**Skill costs (placeholder — owned by Balancer):**

| Skill | Cost |
|---|---|
| entity_burst | 5 Focus (flat) — effectively free; regens faster than you spend |
| self_burst | 20 Focus (flat) — meaningful burst cost |
| self_channeled_tick | 12 Focus/sec (drain while held) — expensive over time, requires management |
| self_duration_tick | 15 Focus (flat, on activation) — burst cost like self_burst; ticks for duration then cooldown |
| self_aura | 15 Focus (reserved) — locks 15 Focus from the available pool while active; no per-tick cost |

**Starting values (placeholder — owned by Balancer):**

| Archetype | Max Focus | Regen/sec |
|---|---|---|
| Warrior | 80 | 12 |
| Rogue | 100 | 15 |
| Mage | 150 | 10 |

**Focus Shield**
Every archetype has a Focus Shield — a damage buffer that absorbs hits before HP. Once depleted, damage hits HP directly.

- Shield size = 30% of current Max Focus (all archetypes)
- Casting does not drain the shield — Focus pool and shield are managed independently
- Shield regens passively (slow baseline, investable through augments)
- Investable augment paths: shield regen rate (time-based recovery; rewards brief retreats) and shield on attack (hit-based recovery; rewards aggressive combat)
- If Max Focus increases (buff): shield ceiling rises, current shield stays — regen up to the new cap
- If Max Focus decreases (debuff): shield ceiling drops, current shield is immediately clamped to the new maximum

Investing in Max Focus through gear grows both the casting pool and the shield simultaneously. Natural shield sizes at base: Warrior 24, Rogue 30, Mage 45. The Mage has the largest Focus pool — and therefore the largest shield — by default.

---

### Dodge

Every character has a dodge roll available at all times.

- **Input:** Space bar
- **Cost:** Free — no Focus cost
- **Direction:** Current movement input direction (WASD). If standing still (no input), rolls in the current character facing direction.
- **I-frames:** Grants full invincibility frames (immunity to all damage) for the duration of the roll (0.35 seconds).
- **Cooldown:** 1.0 second, starting immediately when the dodge begins.
- **Speed Boost:** Overrides movement velocity with a 2.0x speed multiplier during the roll.
- **Skill Cancellation:** Instantly interrupts and cancels any active or channeled skill animation/cast on activation.

---

### Damage Types

Every damage source has a **damage type**. Every entity that can take damage has a **resistance** value per type (percentage reduction).

`effective damage = raw damage × (1 − resistance)`

**v1 damage types:** Physical, Magic

**Future expansion:** Elemental types (Fire, Lightning, Frost, etc.) will be added as the system grows — the formula and resistance model extend naturally. Getting Magic right in v1 is the template: a new damage type means adding a resistance value per enemy, a DamageType enum entry, and a weapon or augment that produces it. Nothing else changes.

Resistances are always soft (never total immunity). Exact values are TBD.

### Critical Hits

Crit applies to the hit that applies an EoT — damage EoT ticks inherit the crit multiplier of that hit for their full duration.

`Final damage (on crit) = Skill base damage × Crit Multiplier`

- **CritChance** — global baseline comes from Dexterity. The Bow identity bonus adds a flat % on top. The Critical Strike skill augment adds a further per-skill bonus on top of the global chance — any archetype can invest in crit this way; Rogue builds it more naturally through Dex.
- **CritDamage (Crit Multiplier)** — comes from Strength. Fixed at 1.5× in v1 at base; grows with Str investment.

### Effects over Time (EoT)

Skill Augments can apply **Effects over Time (EoT)** to enemies. EoTs are not applied by skills directly — they always come from augments. The augment's trigger chance determines whether the EoT is applied on a given hit; once triggered, the EoT applies at 100%.

Every EoT has the same three properties:

| Property | All EoTs | Notes |
|---|---|---|
| Duration | Yes | How long the effect lasts; refreshes on reapply |
| Tick rate | Damage EoTs only | Ignored for non-damage EoTs |
| Damage per tick | Damage EoTs only | Ignored for non-damage EoTs |

**Application rules (all EoTs):**
- No stacking — only one instance of each EoT type per enemy at a time
- Reapplying refreshes the duration rather than stacking or being ignored
- **Crit stamping** — if the applying hit was a crit, damage EoT ticks deal `DamagePerTick × CritMultiplier` for the full duration. Re-applying with a non-crit resets the multiplier to 1×; re-applying with a crit refreshes it. Non-damage EoTs are unaffected.

The EoT type defines *what it does* when active:

| EoT | Damage per tick? | What it does |
|---|---|---|
| Slow | No | Reduces enemy movement speed |
| Burn | Yes (Magic) | Deals Magic damage per tick |
| Vulnerability | No | Increases damage taken by the enemy — **post-v1** (no augment or EotRegistry entry in v1) |

When designing new EoTs: if it deals damage per tick, set tick rate and damage per tick. If not, leave those blank. That is the only distinction.

### Interaction
- Collectibles auto-collected on contact (XP Shards, coins, health)
- [TBD] Any interactive objects (chests, shrines, etc.)

---

## Characters

Every run requires a character. Characters are created by the player, persist between runs, and grow over time. A player may own multiple characters simultaneously and delete any they no longer want.

### Character Archetypes

| Archetype | Max HP (base) | Speed (base) | Max Focus (base) | Focus Regen/sec (base) | Primary stat emphasis | Default build |
|-----------|--------|-------|-----------|-----------------|--------------|---------------|
| Warrior   | 150    | Shared base | 80        | 12              | Strength (→ PhysicalDamage, MaxHp, PhysRes, CritDmg) | Sword + Heavy armour — close-range brawler |
| Rogue     | 80     | Shared base | 100       | 15              | Dexterity (→ CritChance, Evasion) | Bow + Medium armour — fast, agile kiter |
| Mage      | 100    | Shared base | 150       | 10              | Intelligence (→ MagicDamage, MaxFocus, MagRes, FocusRegen) | Wand + Medium armour — glass cannon; largest Focus Shield by default |

All values are base stats at level 1 — placeholder, owned by the Balancer. Damage and defensive stats scale through primary stat growth per level (see Archetype Stat Multipliers below).

#### Archetype Stat Multipliers

Stats scale through **primary stat growth per level** (D4 / Last Epoch pattern):

- Each level the character gains primary stats (Str/Dex/Int) at archetype-specific rates
- Primary stats convert to derived stats via fixed conversion rates — the same for all archetypes
- Items may grant either +primary stats (converted via fixed rates) or +derived stats directly (flat bonus, class-agnostic)

**Primary stat → derived stat groupings (fixed for all archetypes):**

| Primary stat | Derived stats it feeds |
|---|---|
| Strength | PhysicalDamage, MaxHp, PhysicalResistance, CritDamage |
| Dexterity | CritChance, Evasion |
| Intelligence | MagicDamage, MaxFocus, MagicResistance, FocusRegen |

Conversion rates are fixed constants — TBD, owned by the Balancer.

**Primary stat gains per level per archetype:**

| Archetype | Str/level | Dex/level | Int/level |
|---|---|---|---|
| Warrior | TBD (high) | TBD (low) | TBD (low) |
| Rogue | TBD (low) | TBD (high) | TBD (low) |
| Mage | TBD (low) | TBD (low) | TBD (high) |

Exact values are TBD — owned by the Balancer.

**Formula:** `effective_derived = archetype_base + (level × primary_gain_rate × conversion_rate) + item_derived_bonus`

Base archetype stats (at level 1) are applied directly — not subject to the primary stat formula. At level 1 with no items, effective stats equal the archetype base stats unchanged.

### Character Lifecycle
1. **Create** — player picks a name (required) and archetype
2. **Select** — choose a character from the roster before a run
3. **Run** — character starts at their saved level; every new level gained during the run is permanent
4. **Grow** — level and XP carry over to the character; coins and crafting materials go to the shared account pool; permanent stat bonuses are applied automatically on level up
5. **Delete** — player can permanently remove a character (irreversible)

A run cannot start without a selected character.

### Controls
| Input | Action |
|---------|---------------------------------------------|
| WASD | Move |
| Q / E / R / F / Mouse button | Activate skill slots 1–5 |
| Space | Dodge roll |
| ESC | Pause |

---

## In-Run Progression

### Level Up
- Killing an enemy grants **`1 XP × map level`** instantly (kill reward, no pickup required)
- Killing enemies also drops **XP Shards** — collecting them adds further XP
- Both sources fill the same XP bar
- On level up: permanent HP bonus (flat) is applied automatically; damage increase is implicit through primary stat growth (more Str/Int per level → higher stat multiplier → more damage)
- Level and XP within the current level persist when the run ends; the character picks up exactly where they left off
- No popup or pause — levelling up is seamless

### Enemy Drops
| Drop               | Effect                                         | Drop chance |
|--------------------|------------------------------------------------|-------------|
| XP Shard             | Feeds level-up bar                             | 100%        |
| Coin               | Added to coin bank on run end                  | 25%         |
| Health pickup      | Restores HP instantly                          | 10%         |
| Crafting material (common) | Added to crafting-currency-1 bank on run end | 20% |
| Crafting material (higher tiers) | Added to respective material bank on run end | [TBD — rarer, tied to item tier] |

Drop rarity for crafting materials scales with tier — the more exotic the items a material can produce, the rarer it drops.

---

## Maps

> Map design, procedural generation, biomes, chunks, and obstacle props are documented in `docs/gdd-map.md`.

Maps are the arenas where runs take place. Each map has a **Map Level** attribute that scales kill XP — killing an enemy grants `1 XP × map level` directly, on top of any XP Shard the enemy drops.

---

## Run Structure

- **Spawn:** Player always starts at the center of the map
- **Duration:** Fixed time limit — 5 minutes
- **Map:** Each run takes place on a map; the map's attributes apply for the full run
- **Difficulty scaling:** Enemy count, speed, and variety increase over time
- **Run end conditions:**
  - Player dies → run over
  - Timer expires → run won [boss mechanic TBD]
- **Run rewards:** Level, XP, coins, and crafting materials earned all persist; player returns to the character screen

---

## Enemies

| Type     | Behavior     | Physical Resist | Magic Resist | Notes                                      |
|----------|--------------|-----------------|--------------|--------------------------------------------|
| Skeleton | Chase player | 10%             | 0%           | v1 only enemy — bone-white voxel model     |
| [TBD]    | Chase fast   | —               | —            | Future runner-type                         |
| [TBD]    | Ranged       | —               | —            | Future ranged attacker                     |
| [TBD]    | Boss         | —               | —            | Spawns when timer expires                  |

All types scale with elapsed time — speed and HP increase per minute. Spawn rate also accelerates.

### Enemy Navigation

**Always navigate.** Enemies use navmesh pathfinding at all times — they never walk into walls or get stuck on corners. Competent movement is non-negotiable for horde feel; a skeleton bumping into a pillar reads as broken, not charming.

**No separation (v1).** Enemies do not avoid each other. The blob is intentional — 30 skeletons converging on the same point is the visual threat mass that self_burst and self_channeled_tick are designed to answer. Spreading enemies out would make horde skills feel weaker and the threat more diffuse. Light natural spreading from collision is sufficient. Revisit post-v1 if playtesting reveals a problem.

**Chokepoints are a feature.** Map corridors and doorways are intentional tactical geometry. Enemies funneling through a doorway is a core fun moment — position at the mouth of a corridor, pop a self_burst or self_duration_tick, clear the flood. This falls out of correct pathfinding for free; no extra design work needed. Map design should treat chokepoints as a first-class tool, not an obstacle routing problem.

### Enemy Spawning

There are two categories of enemy presence in a run:

**Pre-placed enemies** — authored into room templates when the map is built. Start idle. Aggro via the proximity cluster system below.

**Wave-spawned enemies** — spawned dynamically during the run as part of escalating difficulty. Always aggro immediately on spawn — no idle state, no cluster membership. These are the horde.

#### Enemy Pool (wave-spawned)

What wave-spawned enemies can appear is defined per map by an **enemy pool** — a list of typed variants with counts and stat modifiers:

```
EnemyPoolEntry:
  EnemyType: string       // e.g. "skeleton", "archer_skeleton"
  Count: int              // drives spawn ratio (weight in the pool)
  Modifiers:
	ArmorBonus: int       // e.g. +5, +10
	HpBonus: int          // future
	SpeedBonus: int       // future
	DamageBonus: int      // future
```

The spawner draws randomly from the pool weighted by `Count`. Modifiers are applied to the enemy instance at spawn time on top of base stats. v1: one entry, `skeleton`, count 1, all modifiers zero.

**Map crafting hook:** when maps become craftable, the player configures the enemy pool — e.g. "warrior skeletons only" or "5× light skeleton + 10× heavy skeleton". The spawner consumes whatever the pool defines; no spawner changes needed.

#### Proximity Cluster System (pre-placed enemies)

Pre-placed enemies are always **idle** at map load. Clusters are not authored or pre-computed — they are an emergent property of which idle enemies are near each other at any given moment.

**How clusters form:** think of it like cells organising. Each idle enemy looks at its neighbours. If another idle enemy is within proximity range, they are connected. Any two idle enemies connected directly or through a chain of connections belong to the same cluster. This is recalculated dynamically — not stored permanently.

**Enemy states:**

| State | Behaviour |
|---|---|
| Idle | Standing still. Participates in proximity clustering. Has an individual aggro radius. |
| Chasing | Independently pursuing the player via navmesh. No cluster membership while chasing. |

**Aggro trigger:** each idle enemy checks player distance every frame. If the player enters that enemy's aggro radius → the enemy wakes → its entire connected idle cluster wakes simultaneously. Clusters produce the "whole group snaps to attention" feel without manual authoring.

**Losing the player:** if a chasing enemy loses the player (player moves beyond `BalanceConfig.Enemies.LostPlayerDistanceTiles` × tile size), the enemy returns to idle and immediately re-runs proximity scanning. It joins the nearest idle cluster or forms a new one with other nearby idle enemies. Clusters naturally reform around survivors and returning chasers.

> **Current value: 30 tiles (1080 world units).** Wave-spawned enemies need a threshold that exceeds their spawn distance (~560 units / ~15.5 tiles from the nearest room centre), or they switch to Idle immediately on spawn. Once pre-placed enemies are implemented, they should use a separate, smaller config value (6–10 tiles is the intended design range for pre-placed). `LostPlayerDistanceTiles` will be split into `WaveSpawnLostPlayerTiles` and `PrePlacedLostPlayerTiles` at that point.

**Wave-spawned enemies** are always created in the chasing state — they never participate in clustering.

**No manual clump authoring required.** Room designers place enemies; the proximity system handles grouping automatically. The only tuning lever is the proximity radius (how far apart enemies can be and still cluster together) — this can live on `MapData` to allow different maps to feel tighter or looser.

---

## Win / Lose Conditions

| Condition     | Outcome                                                        |
|---------------|----------------------------------------------------------------|
| Player HP = 0 | Run lost — level, XP, coins, and crafting materials earned still saved |
| Timer expires | Run won — all rewards saved; [boss mechanic TBD]              |

In both cases the player is returned to the character screen. There is no death penalty — every run makes the character stronger regardless of outcome.

---

## Future Design Notes

### Archetype Defense System

All archetypes share Focus Shield as a universal defensive layer — damage hits the shield before HP. Natural shield size varies by archetype because it scales with Max Focus. Beyond the universal shield, each archetype has a primary defensive identity:

| Archetype | Primary defense | Focus Shield at base | Philosophy |
|-----------|----------------|----------------------|------------|
| Warrior | Physical Resistance | 24 (80 × 30%) | Mitigation — physical resistance reduces raw damage; shield buys extra time. Rewards staying in melee range. |
| Rogue | Evasion + Dodge | 30 (100 × 30%) | Avoidance — Dex investment increases Evasion (passive % to avoid hits); Dodge roll is the active fallback when Evasion fails. |
| Mage | Focus Shield | 45 (150 × 30%) | Resource management — Focus Shield is the primary defense. Investing in Max Focus grows both the casting pool and the shield simultaneously. |

Focus Shield is investable by any archetype through Equipment Augments (shield regen rate, shield on attack). This enables cross-archetype builds — a Warrior who invests in Max Focus and shield augments plays as a melee fighter with a magical damage buffer (Paladin-style), constrained naturally by their weapon (short range, physical damage primary).

**Physical Resistance (Warrior) and Dodge (Rogue) as investable stats are post-v1.** Design when the archetype multiplier system is being expanded.

**Focus Shield is v1 for all archetypes** — see Focus section under Core Mechanics.
