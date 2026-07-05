# Skill System — Brainstorming Scratchpad

> **Status: OPEN brainstorm, not committed design.** This is a working doc spanning multiple sessions.
> Nothing here is final until it is resolved and moved into `design-skills.md`. Rejected ideas get deleted, not archived.
> **The committed skill design lives elsewhere:** the 12 prototypes, AoE math, the Budget/Identity framework, the composition model (prototype + form + identity), and the wave-1 forms & the 16 reachable combos are all in [design-skills.md](design-skills.md); stat ownership (incl. craft-time-components rule §5D) in [design-stats.md](design-stats.md); crafting flow in [design-progression.md](design-progression.md).

---

## What this doc is for

Working space for the **skill-system questions that are still open**. The composition model is adopted and wave 1 is designed (locked 2026-07-04); what remains here is what comes next and what was deliberately deferred.

## Open threads

### Wave-1 follow-ups (needed before shipping)

- [x] ~~**Sanity check the locked wave-1 design**~~ DONE 2026-07-04 — design is rule-clean (no matrix violations, all form shapes map to existing `SkillData` fields incl. the new `TickRate`, no name collisions, guardrails pass). Two engine gaps found and added to the `design-skills.md` prerequisites: **WindUp only works on Position paths** (heavy + quake forms need it on Entity/Self), and **tier does not modify skill stats in code yet** (tier tracks = part of composition work).
- [x] ~~**Forms & identities: craftable items or internal data?**~~ RESOLVED 2026-07-05 — **catalogue entries (Model C)**, not inventory items: prototype/form/identity are registry authoring data, selected in a **craft wizard** with a resource cost debited per step. Presets-first superseded by wizard-first; presets dissolve (reachable skills = combos the registries allow). Naming = derived phrase + iconic overrides. Cost = resource bundle over a unified currency+material registry, with a reserved (currently empty) requirements seam. All promoted to `design-skills.md` / `design-progression.md`.
- [x] ~~**Formalise & ship proposal**~~ DONE 2026-07-06 — the wizard + registries + cost model shipped as issues #23–#27 (skill snapshot/serialization, `CraftComposedSkill`, name generator, craft wizard UI, preset dissolution), all merged. Wave 1 is live in-game. The planned playtest gate was dropped by decision (2026-07-06) — we move forward with what we have.

### Custom-wizard era (gated on ~4+ identities)

- [x] ~~**Naming for custom combos**~~ RESOLVED 2026-07-05 — **derived phrase** (form/identity/prototype → one word each, ~25-entry map) as the default, **+ thin iconic-override table** for signature combos. Replaces hand-named presets. Wave 1 ships raw `[prototype][form][identity]` as placeholder. Promoted to `design-skills.md`. *(Remaining: author the actual word-map and pick which combos get iconic overrides — full-roster work.)*
- [ ] **Blandness watch** — combinatorial content guarantees coverage, not soul. The wave-1 **wizard now exposes composition directly** (presets no longer mask it), so blandness is visible from wave 1. Standing watch item (no playtest gate — dropped 2026-07-06): re-check honestly whenever new forms/identities land or anyone actually plays a session.

### Element wave (arrives together with enemy-resist promotion, D1)

- [ ] **New identities** (Fire/Cold/Lightning…) — each multiplies against all existing forms.
- [ ] **Element effect flavours** — ⚠ levers dressed as flavours (ice-slow is CC, lightning-crit is a damage stat, fire-burn is a DoT). **Leading candidate: "the element whispers, augments make it shout" (b-lite)** — every element carries a token inherent effect of equivalent near-zero cost (ice: tiny brief slow; fire: tiny burn; lightning: hair of crit damage), augments scale it into real power. Requires: matrix EoT-row amendment ("token inherent" exception) + Balancer promise of equivalence. Fallback: pure affinity routing (element = damage channel; all effects opt-in via augments, D3-style).
- [ ] **Damage-model note**: mono-typed hits + typeless weapon are locked as deliberate absences (`design-stats.md` §5C, with no-immunities rule). Composite hits reopen only if this wave somehow demands them (not expected).

---

## Active thread — Prototype expansion candidates (cross-game re-assessment, 2026-07-04/05)

> **⚠️ NOT IMPLEMENTED YET.** Pulled out of the parking lot into active discussion (2026-07-05). Every candidate below is a **discussion item, not committed design and not built** — none exists in code, none has a GitHub issue. Each still needs its own design pass and must be fully nailed down *here* before it becomes an implementation issue. Listed ≠ approved.

Full sweep of [ref-arpg-skills.md](ref-arpg-skills.md) against the 12 prototypes — originally PoE2 only (2026-07-04), re-swept 2026-07-05 after the catalogue gained Diablo 4, Last Epoch, Torchlight III, and Lost Ark. Everything maps to an existing prototype, a candidate below, or a resolved/blocked family. Names follow the `targeting_pattern` convention. *(Under the composition model, each new prototype multiplies against the existing form/identity pools — adding one is a whole new row of reachable combos, not a single skill.)*

**Coverage validation (2026-07-05):** every one of the 12 prototypes is heavily represented across all five games — the list is well-grounded. The re-sweep confirmed all PoE2-derived candidates with independent cross-game exemplars and surfaced **two new candidates** (entity_channeled_tick; transformation_buff). Strongest cross-game signals, by sheer catalogue frequency: the **directional shapes** (path_burst / cone_burst — the genre's single most common delivery family, and we have no Direction targeting shape at all), **self_buff_burst** (shouts/imbuements appear in every game and `design-skills.md` already promises War Cry-style skills on the bar), and **summon_minion/totem** (the largest catalogue family we don't cover).

### Candidates needing only a new delivery shape

| Candidate | Covers | Exemplars (cross-game) |
|---|---|---|
| path_burst | Piercing projectile line, hits each enemy once — also covers ground-waves (Sunder-style) and returning projectiles as variants | Frostbolt, Glacial Lance, Plasma Blast (PoE2); Bone Spear, Penetrating Shot (D4); Javelin, Shadow Cascade (LE); Lethal Shot (TL3) |
| moving_zone_tick | Tick zone travelling along cast direction | Ball Lightning, Solar Orb, Twister (PoE2); Frozen Orb, Tornado, Blood Wave (D4); Tornado (LE) |
| chain_burst | Hit jumps to N nearby enemies | Arc, Lightning Arrow, Shockchain Arrow (PoE2); Chain Lightning (D4/TL3/LA) |
| cone_burst | Directional cone AoE (first non-circular burst shape) | Ice Shot, Freezing Shards, Bone Blast (PoE2); Rend (D4); Harvest (LE); Flame Wave (TL3/LA) |
| wall_zone_tick | Line-shaped zone; optional collision-blocking variant | Flame Wall, Frost Wall, Bone Cage (PoE2); Firewall (D4); Ice Wall (LA) |
| detonated_zone_burst | Place charges, detonate on second press (player-triggered; cf. enemy-triggered triggered_zone_burst) | Detonating Arrow, Unleash (PoE2); Detonating Arrow (LE) |
| zone_debuff | Zone with `DamagePattern None` + `DebuffEotId` — area counterpart of entity_debuff; displacement CC (pull/knockback zones) would also live here | Cold Snap, Gas Grenade (PoE2); Smoke Grenade (D4); Magnetic Field (TL3 — pull); Shadow Cage (LA) |
| zone_buff | Placed zone buffing the player inside it | Sigil of Power, Consecrate (PoE2); Sanctuary (LA) |
| self_buff_burst | Activation whose payload is a timed self-buff (war cry) — Self/None mirror of entity_debuff; `design-skills.md` already names War Cry as skill-bar content | Seismic Cry, Infernal Cry (PoE2); Rallying Cry, War Cry, imbuements (D4); Battle Cry (TL3); Berserk (LA) |
| entity_channeled_tick | **NEW 2026-07-05.** Channeled tick on the locked target (beam) — the only Channeled prototype today is Self-radial; this proves Channeled × Entity and fits weapon-adaptive delivery | Incinerate (PoE2/D4); Rapid Fire (D4); Voltaic Ray, Arcane Blast (TL3); Arcane Ray (LA) |

### Candidates gated on a new mechanic or unparking a surface

| Candidate | Prerequisite | Exemplars (cross-game) |
|---|---|---|
| charge_release_burst | Accumulation-state mechanic (where stacks live = matrix conversation) | Boneshatter, Gathering Storm (PoE2); combo/rage gauges (TL3/LA) |
| movement_burst | Lift the "no movement skills" rule; leap/dash + burst at landing | Leap Slam, Flicker Strike (PoE2); Leap, Shadow Step (D4); Soaring (LA) |
| movement_path_burst | Same rule lift; damage along the travel route | Shield Charge, Stampede (PoE2); Charge (D4); Lunge (LE) |
| summon_minion | Unpark minion stats (own HP/damage, targetable, own AI) | Skeletals, Raise Zombie (PoE2); Hydra, Wolves, Golem (D4); Falconry, Wraith (LE); imps→dragons (LA) |
| summon_totem | Minion-lite stats (stationary, destructible, auto-attacks). A "dumb" pulse totem needs nothing — it is fixed_zone_tick with a prop | Ballistas, Ancestral Warrior Totem (PoE2); Storm Totem (LE); Turret, Sentry (TL3/LA) |
| corpse_consume_burst | A corpse resource system (the system is the work) | Detonate Dead, Volatile Dead, Offerings (PoE2); Corpse Explosion (D4) |
| transformation_buff | **NEW 2026-07-05.** A skill-set-swap mechanic (activation replaces the skill bar with form skills for a duration) — a whole new surface, by far the largest prerequisite here | Lich Form, Werebear/Werewolf Form (LE); Grizzly Rage, Wrath of the Berserker (D4) |

### Resolved / blocked families (for the record)

- **Damage amplification** → Route A: enemy-owned "Exposed"-style EoT via existing debuff machinery (promotion path noted on the parked "-to-enemy" line in `design-stats-deferred.md`).
- **Conditional damage** → Route B: crit-condition augments ("always crit vs enemies below 30% HP / vs Slowed").
- **Raw per-skill conditional multipliers** → blocked by design, no route, no exception.
- **Alternative resource costs** → a Focus-cost-model variant on existing prototypes, never a new prototype. **Ammo/charge economies** → no analogue.
- **Inherent-DoT skills** (bleed/poison/burn attacks — Rake, Puncture, Rabies, Poison Dart…) → already resolved by the ownership matrix: EoTs come from augments, never baked into a damage skill.
- **Party/ally support** (Bard/Artist heals, party buffs, taunt-for-allies) → no party in this game; no analogue.
- **Chain / pierce / fork as modifiers** → where a catalogue skill is an ordinary hit *plus* chaining (rather than chaining being the whole shape), that's augment territory — only the dedicated chain delivery (chain_burst above) is a prototype question.

### Raw extraction — form candidates (2026-07-05, unsynthesised)

> **Raw material, not proposals.** Crunched from the full [ref-arpg-skills.md](ref-arpg-skills.md) catalogue: recurring *budget-spend shapes* observed across games, expressed against our form framework (budget levers: cooldown, tick rate, radius, Focus, wind-up, duration, stack count). Wave 1's 8 forms (swift/heavy, nova/quake, storm/floor, spin/vortex) already cover the genre's most common shapes — these are the shapes *beyond* them. Each needs a synthesis pass (budget-parity check, rule-compliance) before becoming a real form.

| Form concept | Budget shape (where the budget goes) | Fits prototypes | Exemplars | ⚠ Flags |
|---|---|---|---|---|
| ~~**salvo**~~ ✅ | ~~One press = N small rapid sub-hits; payload split across the volley~~ | entity_burst | — | **PROMOTED 2026-07-06 → `design-skills.md`** (entity_burst 3rd form, granularity axis; proc-parity closed by ÷N normalization) |
| ~~**echo**~~ ✅ | ~~Payload split into initial hit + delayed second hit at the same spot~~ | fixed_zone_burst (only) | — | **ADOPTED 2026-07-06 → `design-skills.md`** as fixed_zone_burst's blast↔echo pair (4 combos, its first forms). Same family as salvo — shares its ÷N proc-normalization + multi-hit machinery (N=2); delayed hit reuses existing wind-up scheduling. Not on self_burst (nova/quake own timing) or windup_burst (left for the fuse question). |
| **ramp** | Channeled damage/tick rate builds the longer the channel is held; drain constant | self_channeled_tick (and any future channeled) | Incinerate (PoE2/D4), Tempest Flurry (PoE2), Flameblast (PoE2) | Within-channel state only — no stored stacks between activations, so *not* gated on the charge_release mechanic |
| **fuse** | Payload lands on a placement timer (lobbed grenade feel) instead of instantly | fixed_zone_burst | Explosive Grenade (PoE2), Grenade (TL3/LA), Cryo Bomb (TL3), Shadow Bomb (TL3) | **Overlaps windup_burst the prototype** — catalogue suggests wind-up may really be a *form axis* on zone bursts, not its own prototype; synthesis question |
| ~~**swarm vs singular**~~ ✅ | ~~Stack count ↔ per-instance payload~~ | stackable_zone + triggered_zone_burst | — | **ADOPTED 2026-07-06 → `design-skills.md`** (both prototypes; 8 new combos). Stack count promoted to a budget lever; instance "size" = radius/duration/tickrate not damage; ≥2 stack floor on stackable_zone singular guards vs fixed_zone_tick clone. No engine work. |
| ~~**pylon**~~ ❌ | ~~Zone as a small planted emitter pulsing outward, vs uniform carpet~~ | — | — | **REJECTED as a form 2026-07-06** — moves no budget lever (same radius/rate/duration/cost), so it's an **identity/VFX skin**, not a form (ruling recorded in `design-skills.md` framework). Mechanical cousins already covered: destructible emitter → summon_totem; traveling pulse → moving_zone_tick. |
| ~~**reserve-heavy vs reserve-light**~~ ✅ | ~~Focus reservation ↔ effect strength/radius~~ | self_aura | — | **ADOPTED 2026-07-06 → `design-skills.md`** (damage-aura scope; 4 combos, self_aura's first forms). Reservation↔radius (both levers real, no engine work); "size" = radius not per-target magnitude. Buff/debuff-aura branch parked on the D1 magnitude question (as fleet↔enduring). |
| ~~**fleet vs enduring**~~ ✅ | ~~Duration ↔ potency~~ | entity_debuff (only) | — | **RESOLVED 2026-07-06 → `design-skills.md`**: accepted as generic fallback axis but **parked**. Barred from damage-tick prototypes (collapses into storm/floor duration↔tickrate). Sole unique home entity_debuff is blocked — EoT magnitude/duration live on shared `EotData`; unblocks when the element/EoT wave (D1) rules on per-form EoT-magnitude override. |
| **residue** | Burst that leaves a brief weak tick zone behind (budget split burst→lingering) | burst prototypes | Fangs of Frost trail, Incendiary Shot, Poisonburst Arrow (PoE2), Volcanic Fissure (PoE2) | **Likely rule-breaking** — crosses the damage-pattern boundary (Burst prototype gaining a Tick component); synthesis must rule form vs blocked |

**Form-axes observed in the catalogue but blocked by existing rules (for the record):** reach/range as a form (Snipe fantasy — `Range`-as-reach is explicitly not a budget lever); hit size (no heavy-hitter by design — hit size is constant); movement forms (no-movement-skills rule); conditional-damage forms (execute thresholds → Route B augments); cross-activation charge accumulation (→ gated charge_release_burst candidate).

### Raw extraction — identity candidates (2026-07-05, unsynthesised)

> **Raw material, not proposals.** Damage-type families recurring across the five games, expressed against our identity framework (damage type + VFX/audio skin + name fragment + b-lite token effect). Physical and Magic/Arcane are shipped (wave 1); Fire/Cold/Lightning are the planned element wave. Everything beyond the trio raises the same synthesis question: **does it get its own enemy-resist channel, share one, or fold into an existing type as a skin?** (Identities couple to enemy-resist promotion, D1.)

| Identity candidate | Status | Token inherent effect (b-lite candidate) | Skin fantasy / name fragments | Exemplars |
|---|---|---|---|---|
| **Physical** | ✅ shipped | — (baseline) | kinetic, steel, crushing | everywhere |
| **Magic / Arcane** | ✅ shipped | — (baseline) | arcane, glyph, mana | Arcane Blast (TL3), Mana Burst (LA), Spellsword kit (LA) |
| **Fire** | planned (element wave) | tiny burn DoT | flame, ember, inferno, magma | massive in all five games |
| **Cold** | planned (element wave) | tiny brief slow | frost, glacial, ice, cryo | massive in all five games |
| **Lightning** | planned (element wave) | hair of crit damage | storm, voltaic, arc, thunder | massive in all five games |
| **Poison** | new candidate | tiny stacking micro-DoT (distinct from burn: stacks, slower) | venom, toxic, gas, blight | PoE2 chaos-poison family, D4 Poison Imbuement/Rabies, TL3 Rogue kit, LA Shadowhunter |
| **Void / Shadow** | new candidate | sliver of defence-bypass (armour/shield-ignoring whisper) | void, shadow, umbral, abyssal | LE Void Knight kit, D4 Shadow/darkness, PoE2 Soulrend/Hand of Chayula, LA Soul Master |
| **Blood** | new candidate | tiny self-life interplay (whisper of leech, or whisper of life-cost) | blood, crimson, gore | D4 Necro blood family (Blood Lance/Surge/Wave), PoE2 Exsanguinate/Reap, LE Rip Blood |
| **Holy / Radiant** | new candidate | token unclear — heal sliver? (⚠ "bonus vs undead" would be a gate — violates no-gate philosophy) | radiant, divine, sacred, solar | LA Inquisitor/Artist/Paladin kits, PoE2 Spear of Solaris, LE Paladin |
| **Earth / Nature** | new candidate | whisper of stagger/knockdown | stone, thorn, boulder, seismic | D4/LE Druid+Primalist kits, LA Destroyer/Druid, PoE2 slam family |
| **Wind** | new candidate | whisper of knockback/push | gale, tempest, zephyr | LA Wardancer kit, D4 Wind Shear, PoE2 Wind Blast |
| **Bone** | skin variant, not a type | — | bone, marrow, splinter | D4/PoE2 Necro bone families — reads as a *Physical skin*, not a new damage channel |

**Mechanics residue (extracted to neither table — for the record).** A few catalogue mechanics crunch to neither a form nor an identity; noted here so they aren't silently dropped: **alternating-element strikes** (Primal Strikes, Mantra of Destruction — PoE2; Spell Weaving — TL3): cycles the damage type per use, conflicts with damage-type-fixed-at-craft; would need its own design conversation. **Fire-mode toggles** (Rapid Shot — PoE2): a stance that re-shapes an existing skill's budget — form-switching at runtime, no current analogue. **Synergy constructs** (Tempest Bell, Lightning Conduit — PoE2; Hexblast-style debuff detonation): skill A places a state that skill B pays off — cross-skill interaction surface, deferred. **Taunt / decoy** (Challenging Shout — D4; Decoy — LE): threat redirection needs an enemy-aggro model; parked with summons. Most of these already fall under resolved/blocked families or gated candidates; none blocks the form/identity synthesis.

### Raw extraction — augment candidates (2026-07-05, unsynthesised)

> **Raw material, not proposals.** Mined from the full [ref-arpg-skills.md](ref-arpg-skills.md) catalogue: every secondary effect, modifier, or on-hit behaviour that other games bake into skills but that our rules route to **augments** (EoTs and secondary effects are never inherent to skills). Grouped by the augment paths in `design-augments.md`. Each needs a synthesis pass before becoming a named augment; trigger chances and values are Balancer territory throughout.

**Skill Augment candidates:**

| Path | Candidate | Effect sketch | Exemplars | ⚠ Flags |
|---|---|---|---|---|
| Hit-Debuff (EoT) | **Bleed** | phys DoT on hit | Rake, Puncture (PoE2), Rend (D4), Rending Slash (TL3), Bleeding Edge (LA) | |
| Hit-Debuff (EoT) | **Poison** | stacking DoT on hit | Gas Arrow (PoE2), Poison Imbuement (D4), Poison Dart (TL3) | Couples to the Poison identity question |
| Hit-Debuff (EoT) | **Burn / Chill / Shock** | element-branded EoTs (burn DoT / slow / damage-taken amp) | the entire elemental catalogue | Already anticipated as named Hit-Debuff versions; arrive with the element wave |
| Hit-Debuff (EoT) | **Freeze / Stun** | brief hard CC on hit | Permafrost Bolts, Storm Spear (PoE2), Ground Stomp (D4), Stunning Blow (LA) | Hard CC on a %-roll needs an elite/boss diminishing rule before it exists |
| Hit-Debuff (EoT) | **Root** | immobilise, can still act | Vine Arrow (PoE2), Glacial Spike (TL3), Entangling Roots (LE) | Distinct from Slow/Freeze — a third CC intensity |
| Hit-Debuff (EoT) | **Blind** | enemy accuracy down | Flash Grenade (PoE2), Light Orb (LA) | Needs an enemy-accuracy stat to exist |
| Hit-Debuff (EoT) | **Knockback / Pull** | displacement on hit | Wind Blast (PoE2), Gale (LA); Steel Grasp (D4), Magnetic Field (TL3) | Displacement-as-EoT — physics interaction, new EoT class |
| Hit-Debuff (EoT) | **Armour break / damage amp** | enemy takes more phys/all damage | Armour Breaker (PoE2), Vulnerability, Exposed family | Already resolved as **Route A** (enemy-owned EoT via debuff machinery) |
| Hit-Damage (type) | **per-element conversions** | named versions of the Magic conversion, one per identity | imbuements (D4), elemental strikes everywhere | One per shipped identity; conversion conflict group already exists |
| Hit-Damage-Mod (stat) | **Crit damage** | per-skill crit multiplier bonus | Snipe (PoE2), Headshot, Deadly Shot (LA) | Sibling of the existing crit-chance generic |
| Hit-Damage-Mod (stat) | **Armour penetration** | ignore % of enemy armour | Armour Piercing / High Velocity Rounds (PoE2) | Needs the enemy-armour model finalised |
| Hit-Damage-Mod (stat) | **Conditional crit** | always/bonus crit vs low-HP, Slowed, Frozen, Stunned targets | Cull the Weak, Killing Palm, Shattering Palm (PoE2), Death Blow (D4), Assassinate (TL3) | Already resolved as **Route B** — this is its concrete augment form |
| Projectile (future) | **Pierce / Chain / Fork / Homing / Bounce / Return** | hit-resolution modifiers, resolve first per the declared order | Glacial Lance, Arc, Fragmentation Rounds, Magnetic Salvo (PoE2), Thunder Ball (TL3), Twisting Blades (D4) | Pierce/Chain already named; Fork/Homing/Bounce/Return are new members of the same family |
| AoE (future) | **Splash** | secondary hits around the primary target | every "explodes on impact" skill in all five games | Already named; crit-inheritance rule defined |
| Sustain | **Life leech on hit** | small % of damage returned as HP | Siphoning Strike (PoE2), Blood Frenzy (TL3), Parasite, Life Drain (LA) | |
| Sustain | **Focus on hit / on kill** | small Focus refund | Siphoning Strike, Mana Drain (PoE2/TL3), resource-generator basics (D4) | Skill-side `on_kill_%` trigger doesn't exist yet (equipment-only today) |
| Placement | **Mine on hit** | place proximity trap at hit location, capped | — | Already specced as the future mine/trap pattern in `design-augments.md` |
| Placement | **Trail** | movement/hits leave a brief ticking trail | Fangs of Frost (PoE2), Magma Flow (TL3) | Burst→Tick boundary question, same as the **residue** form candidate |
| On-death | **Spread on death** | on target death, the skill's EoTs jump to nearby enemies | Contagion, Essence Drain, Decompose (PoE2), Rabies (D4) | Needs a new trigger: `on_target_death_%` |
| On-death | **Detonate on death** | killed target explodes for AoE | Volatile Zombie (LE), Poisonburst-style pops | Same new trigger; corpse-system-free version of Detonate Dead |

**Equipment Augment candidates (defensive layer):**

| Candidate | Trigger | Effect sketch | Exemplars | ⚠ Flags |
|---|---|---|---|---|
| **Barrier on hit** | `on_player_hit_%` | absorb shield for a few seconds | Flame Shield, Iron Skin (D4), Frost Shield (LE), Mana Shield (TL3) | Already anticipated in the design-intent list |
| **Dodge boost** | `on_player_hit_%` / `always` | brief or passive dodge chance | Evasion (TL3), Concealment (D4) | Needs the dodge stat surfaced |
| **Phase escape** | `on_player_hit_%` | briefly untargetable on being struck | Phase Shift (TL3), Blood Mist (D4), Mist Step (LA) | Strong panic-button; low trigger % territory |
| **Cleanse** | `on_player_hit_%` / `on_kill_%` | remove a debuff from the player | Purify (LA) | Needs player-side debuffs to exist first |
| **Reflect curse** | `on_player_hit_%` | attacker takes % of damage it deals for a duration | Iron Maiden (D4), Defensive Stance (LA) | Retaliation's big sibling — reactive damage is allowed |
| **Slow aura** | `always` | enemies near the player are slowed | Temporal Chains (PoE2), Debilitating Roar (D4) | Legal per the aura rule (debuff auras on equipment OK); Focus reservation TBD |
| **Low-health surge** | `always` (threshold) | damage/speed up while below X% HP | Enrage (TL3/LA), Berserk (LA), Blood Howl (D4) | **Grey area** — proactive offense on equipment; also needs a threshold condition |
| **Kill momentum** | `on_kill_%` | brief stacking attack/move speed on kill | Frenzy stacks (TL3/D4), Momentum, Adrenaline (LA) | Stacking buff = accumulation state (same conversation as charge_release); offense-on-equipment grey area |

**New trigger types the catalogue implies** (today's vocabulary: `on_enemy_hit_%`, `on_player_hit_%`, `on_kill_%`, `always`): `on_target_death_%` (spread/detonate), `on_crit_%` (crit-payoff augments), `while_channeling` (channel-only bonuses), and below-X%-HP thresholds — which may be a trigger *modifier* rather than a type. Flag: every new trigger type multiplies the augment matrix; synthesis should admit them sparingly.

## Other parked threads

- **Starter loadouts (preset revival, if ever)** — pre-composed skills handed to new characters. A loadout/onboarding feature over Model C data (drop pre-built skill instances into a starting inventory), *not* the old preset-recipe-book concept. Parked until onboarding is designed.
- **"Favourite recipe" bookmark** — player QoL to save a prototype+form+identity tuple for fast re-craft. A thin saved-tuple list over Model C data. Parked until the wizard exists and the need is felt.
- **Full roster beyond wave 1** — un-gated 2026-07-06 (playtest gate dropped; the identity framework proceeds as designed). The path there is the active synthesis of the raw-extraction tables above.
- **Weapon augment design** — what sockets into a weapon? Defensive equipment augments don't fit; the slot needs its own augment family. (The weapon-hosted identity-rune variant was assessed and rejected 2026-07-04 — conflicts with skill-owned DamageType and re-skins all 5 slots at once.)
- **Acquisition specifics / recipe costs** — deferred to the reward/economy surface (D4).

---

## Session log

- **2026-07-03** — Doc created. Opened Thread 1 (identity system); proposed the Budget/Identity lever split.
- **2026-07-04** — Traced stat locations in code; formalised the Stat Ownership Matrix; decided damage type is skill-authoritative. Matrix promoted to `design-stats.md`.
- **2026-07-04 (grill session)** — Thread 1 resolved: framework confirmed, tier rule, damage-type mutability, no mechanical identity lever. Thread 2 resolved: deep first wave on 4 prototypes. Rationale/doc pass merged (PRs #8–#10).
- **2026-07-04 (PoE2 coverage)** — Jump-off mapping + full coverage re-assessment (15 candidate prototypes). Two-route resolution for rule-blocked families adopted.
- **2026-07-04 (resumed session)** — Thread 3 structural questions answered (Phys/Magic pairs; varied contrast axes; classic names + VFX guardrail; prototypes = internal skills). Identity-rune idea generalised into **composition: prototype + form + identity — ADOPTED, presets-first**; Thread 3 absorbed. Damage model settled (mono-typed + typeless weapon; no-immunities rule → design-stats.md §5C). Element effects flagged levers-not-flavours; b-lite framing leading.
- **2026-07-04 (lock-in)** — All 8 forms + 16 presets approved and **promoted to `design-skills.md` v2 section**, with matrix addendum §5D (craft-time components are not stat surfaces) and `design-progression.md` recipe-book note. This doc stripped to open threads. Next: sanity check, then formalise/ship proposal.
- **2026-07-05** — Prototype coverage candidates **un-parked** into an active thread, explicitly flagged NOT IMPLEMENTED — surfaced for design discussion, each still needs its own pass before becoming an issue. No code or design committed by this move.
- **2026-07-05 (grill session)** — Thread "forms & identities as items vs data" resolved: **Model C** (catalogue entries, craft wizard, cost-per-step). Two locked decisions superseded — **presets-first → wizard-first** (presets dissolve into reachable catalogue combos) and **hand-named presets → derived phrase + iconic overrides** (raw `[proto][form][identity]` placeholder in wave 1). Crafting cost formalised as a **resource bundle** over a unified currency+material registry, with a reserved (v1-empty) `Requirements` predicate seam. Confirmed the architecture also accommodates future chance-based craft outcomes *only* via tier/augment/component gambles — never raw per-skill stat boosts (would break the no-per-skill-multiplier + budget-parity rules). Promoted to `design-skills.md` + `design-progression.md`. Next: formalise/ship proposal (slice the wizard + registries + cost model into issues).
- **2026-07-05 (full-catalogue sweep)** — [ref-arpg-skills.md](ref-arpg-skills.md) extended with Diablo 4, Last Epoch, Torchlight III, and Lost Ark (plus markdown repair of the pasted sections); full re-sweep against the 12 prototypes. All PoE2-derived candidates confirmed with cross-game exemplars; **two new candidates added** (entity_channeled_tick — delivery-shape only; transformation_buff — gated on a skill-set-swap mechanic); three resolved/blocked families recorded (inherent DoTs, party support, chain-as-modifier). Coverage of the existing 12 validated — every prototype is heavily represented in all five games. Follow-up same day: **raw form & identity extraction** added as two unsynthesised tables (9 form shapes beyond wave 1's 8, with rule flags; 7 identity candidates beyond the shipped pair + planned trio, each pending the resist-channel question). Third pass same day: **augment extraction** — 20 skill-augment candidates across the Hit-Debuff/Hit-Damage/Hit-Damage-Mod/projectile/sustain/placement/on-death paths, 8 defensive equipment-augment candidates, and 4 implied new trigger types. Also restored the "Other parked threads" heading accidentally consumed by the first extraction edit.
- **2026-07-06 (echo synthesis)** — Sixth form-candidate pass. **ADOPTED as fixed_zone_burst's blast↔echo pair** (single instant burst vs. blast + delayed aftershock at the same spot; 4 combos, its first forms; tier tracks blast→cooldown↓, echo→radius↑). Same family as salvo — inherits both salvo rulings (each hit = payload/2, ÷N proc normalization with N=2) and **shares salvo's multi-hit engine machinery** (build once, both consume). Delayed hit reuses existing wind-up scheduling, so the only new engine piece is the shared multi-hit hook. Homed on fixed_zone_burst (fixed position → unambiguous "same spot"); kept off self_burst (timing axis taken by nova/quake) and windup_burst (reserved for the fuse question). Next: boundary fights — ramp (channel-ramp), then fuse + residue (pattern-boundary probes).
- **2026-07-06 (pylon synthesis)** — Fifth form-candidate pass. **REJECTED as a form.** Emitter-vs-carpet moves no budget lever (identical radius/rate/duration/cost) → it's an identity/VFX skin, not a form. Crystallized the reusable ruling "a distinction that moves no budget lever is an identity skin, not a form" into the `design-skills.md` framework. Residue re-filed: the planted-emitter *look* is an available zone VFX skin; the mechanical cousins are summon_totem (destructible emitter, gated) and moving_zone_tick (traveling pulse, separate candidate). No combos, fixed_zone_tick unchanged (keeps storm/floor). Next: echo/ramp (engine-hook), then boundary fights (fuse, residue).
- **2026-07-06 (reserve-heavy/light synthesis)** — Fourth form-candidate pass. **ADOPTED for self_aura, damage-aura scope.** Reservation↔radius (self_aura's only natural axis — reservation is unique to it); 4 combos, self_aura's first forms; tier tracks reserve-heavy→radius↑, reserve-light→reservation↓. Both levers already built (reservation on self_aura, radius via Range on its Self siblings) — no engine work, one impl-time verify on self_aura's radius. Hit-size clarification's 4th application: "size" = radius, never per-target magnitude. Buff/debuff-aura branch parked on the D1 magnitude question (same as fleet↔enduring). Distinct from vortex (reservation = flat ceiling tax vs drain = continuous burn). Next form: pylon, then echo/ramp (engine-hook), then boundary fights (fuse, residue).
- **2026-07-06 (swarm-vs-singular synthesis)** — Third form-candidate pass. **ADOPTED for both stackable_zone and triggered_zone_burst** (their defining stack↔size trade; 8 new reachable combos, first forms for these engine-proof prototypes). **Stack count promoted to a budget lever** (already the `StackLimit` field — a new exchange rate no wave-1 form uses). Hit-size clarification applied a third time: instance "size" = radius/duration/tickrate, never per-instance damage. Overlap guard: stackable_zone·singular keeps stack floor ≥2 to avoid becoming a fixed_zone_tick clone. Ships on pure data — no engine work. One intentional exception noted: same axis on two prototypes, justified by distinct chassis. Next form: reserve-heavy/light, pylon, echo, ramp, then the boundary fights (fuse, residue).
- **2026-07-06 (fleet-vs-enduring synthesis)** — Second form-candidate pass. **Duration↔potency accepted as a generic fallback contrast axis but PARKED** (no form built). Two findings: (1) on damage-tick prototypes "potency" can only mean tick rate (hit-size constancy), so it duplicates the wave-1 storm/floor axis — barred there; (2) its one unique home, entity_debuff (None-pattern), needs duration↔debuff-strength, but both live on the shared `EotData` definition, not the skill/form — promoting EoT magnitude to a per-form override is a stat-ownership call that belongs with the element/EoT wave (D1). Recorded as an axis ruling in `design-skills.md` ("Beyond wave 1" section, heading broadened to cover axis rulings) with a defined unblock condition. Next form: swarm-vs-singular or reserve-heavy/light.
- **2026-07-06 (salvo synthesis)** — First form-candidate synthesis pass. **salvo ADOPTED** as entity_burst's third form (contrast axis: delivery granularity; N fixed 3–5 Balancer-owned; tier track cooldown↓; budget trade = spread-payload for smoother texture). Two rule collisions resolved: (1) **hit-size constancy clarified** to "bars *buying* a bigger hit, not *dividing* a fixed one" — promoted to the governing framework in `design-skills.md`; (2) **per-hit augment procs** neutralised by a **÷N proc-normalization** engine rule (genre-standard; precedent for future Splash + projectile augments). Promoted to `design-skills.md` under a new "Beyond wave 1 — adopted forms" heading. Next form in the queue: clean budget trades — swarm-vs-singular or fleet-vs-enduring.
- **2026-07-06** — Bookkeeping: "Formalise & ship proposal" ticked off (shipped as issues #23–#27, merged; no GitHub issue needed — nothing left to implement). **Playtest gate dropped by decision** — blandness watch reworded to a standing item, full-roster thread un-gated. Next: synthesis passes over the three raw-extraction tables, in order **forms → identities → augments** (forms are self-contained and settle the residue/trail and fuse-vs-windup boundary questions the other tables inherit; the identity pass naturally becomes the element-wave design; augments last since many couple to identity outcomes and new-trigger admission). Forms order: clean budget trades first (salvo, swarm-vs-singular, fleet-vs-enduring, reserve-heavy/light), then engine-hook questions (echo, ramp, pylon), then boundary fights (fuse, residue).
