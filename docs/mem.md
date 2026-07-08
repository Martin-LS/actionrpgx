# Memory dump — cross-machine pickup (2026-07-08)

Scratch transfer file, not auto-loaded by any tool. Dumped from Claude's persistent
memory store so a session on another machine has full context. Once items here get
absorbed into permanent docs (GDD, AGENTS.md, etc.) or become stale, delete this file
or the relevant section — this is a snapshot, not a source of truth.

---

## 1. Project direction (core pillars — resolved)

This repo (actionrpgx) is a fork of the horde-survival game, pivoted to a full action
RPG in the vein of Diablo and Path of Exile. The two repos have diverged in design intent.

- Fully craft-driven: items, skills, maps all from crafting. Enemies drop crafting materials only.
- Manual cast and manual targeting. Auto-cast kept in code for dev convenience only.
- 5 skill slots, freeform (PoE-style), all active (no passives on skill bar).
- PoE2 keybinding layout: WASD move, Q E R F + mouse for skills, Space for dodge.
- Base stats: Strength (PhysicalDamage, PhysicalResistance, MaxHp, CritDamage), Dexterity
  (CritChance, Evasion), Intelligence (MagicDamage, MagicResistance, MaxFocus, FocusRegen).
  Movement speed is NOT a base stat.
- CDR is a weapon property — fixed per weapon type in v1, global (all skills), roll ranges
  post-v1. No character-level attack speed stat.
- Augments: separate socketable items, one type per skill at a time, all use `on_enemy_hit_%`
  trigger with re-rollable %, freely removeable.
- Passives removed from skill bar — live on armour augments instead.
- Crit: CritChance from Dex (global baseline) + Bow identity bonus + Critical Strike skill
  augment (per-skill bonus). CritDamage from Str. Any archetype can invest in crit via
  augments; Rogue builds it naturally through Dex.
- Evasion: passive % chance to completely avoid an incoming hit. Distinct from Dodge
  (active Space bar roll).
- v1 crafting: one resource ("crafting resource"), everything costs 1.
- Equipment augments: same model as skill augments, prototypes only in v1.
- Stash: account-shared. All crafting materials go to shared pool.
- "Player-facing skills" term retired — all craftable skills are player-usable by definition.

**Apply this lens always**: design and code from the action RPG direction, don't carry
over horde-survival assumptions (auto-cast, 1 skill slot, timer-based runs) without checking
the design docs first.

---

## 2. Versioning / phase scheme (locked 2026-07-06)

Old v1/v2/v2+ vocabulary is retired everywhere.

- **Phase** = declared state, current: **pre-alpha** (exit to alpha = feature-complete,
  a decision not a number). Recorded in the Project Status block at top of `docs/index.md`.
- **Version** = SemVer `0.MINOR.PATCH` + phase as pre-release suffix. Source of truth:
  `config/version` in `project.godot` (currently `0.1.0-prealpha`). MINOR bumps when a
  milestone ships to `main` (deliberate act in the PR, never auto-incremented); tag
  `v<version>` + GitHub Release cut by the workflow in issue #32.
- Docs describe current truth in plain present tense — no version/scope tags on current
  features; deferrals say "deferred/future" with no target phase (phases describe state,
  not scheduling; waves + D1–D4 do the scheduling).
- Waves stay ("wave 1", "element wave") — they all target pre-alpha.

---

## 3. Element / delivery-scaling design (PROMOTED to GDD + Phase-2 SHIPPED)

Promoted to GDD 2026-07-06 (commit 17b15c3, develop): design-skills.md, design-stats.md,
design-mechanics.md, design-augments.md. Reference: `docs/ref-arpg-damage-types.md`.

**Element/identity framework:**
- Roster: elements-only. Physical, Fire, Cold, Lightning (core), no generic "Magic"/"Arcane".
  Shipped Magic → Fire. Later occult: Poison (stacking, shipped #73), Void/Shadow
  (Wither/Decay, shipped #76); special player-side: Blood (leech), Holy (heal) — not built.
- Model A: per-element enemy resistances (each element its own soft resist channel).
- Identity is a REAL, resist-balanced lever — parity via resist *distribution*
  (fire-resistant packs exist), not ailment power-equivalence.
- Every damage type carries a signature ailment innately: Physical→Bleed(DoT),
  Fire→Burn(DoT), Cold→Chill slow+Freeze-at-threshold, Lightning→Shock(damage-taken amp).
  Chain = delivery (not an ailment). Heal = player-side (not an ailment).
- Ownership-matrix pillar rewrite: "no damage skill carries an inherent EoT" → "every damage
  type carries its signature ailment; augments GRAFT off-type ailments (never amplify the innate)."
- Ailment scaling: generic Ailment stat family (Ailment Damage/Effect/Duration/Chance),
  never per-element.
- Deferred: Freeze CC-diminishing rule; player-side per-element resistances.

**Delivery-scaling model** (fixes "Dex/Rogue has no damage axis"):
- Damage scales by DELIVERY, not type: Str→Melee, Dex→Ranged, Int→Spell/cast.
- Damage type fully decoupled from scaling — any archetype plays any element, scaled by
  how they deliver it.
- Crit is universal (delivery-agnostic).
- Hybrid-friendly shallow scaling curve — splash usable, commit strong, ignore weak.
- D4's "scale off class stat" rejected — our game is classless-skill (PoE2-like), not class-locked.

**Phase 2 SHIPPED (2026-07-07)**: Magic→Fire (#51), delivery-pool refactor #49 (2 damage
pools → 3: Melee/Ranged/Spell), Fire/Cold/Lightning identities + signature ailments (#52),
per-element enemy resistances (#53/#57), crit itemization #50/#56 (`CritDamage`
Exclusive→Composed: base 1.5× + Str + `CritDamageBonus` affix + Critical Power augment).
Nothing pending here — future element work = gated occult wave (now complete, see §7).

---

## 4. Stat ownership (authoritative doc: `docs/design-stats.md`)

`docs/design-stats.md` is the authoritative home for stats: catalogue, Stat Ownership
Matrix (§5), and Deliberate Absences (§5C) — stats that must never exist: no per-skill
damage multiplier (damage progression anchored in crafting economy via weapon tier), no
character attack/cast/CDR speed stat (weapon CDR property is the only gear lever). Damage
type is skill-authoritative, fixed at creation, overridable only by a socketed augment.

**Push back before implementing** if any design idea puts a stat on the wrong entity or
resurrects a deliberate absence — amend the matrix consciously or reject, no quiet exceptions.

Grill session 2026-07-04 resolved all open flags except: `InherentEotIds` → delete the
field (code change was pending as a GitHub issue — check if since resolved); `TickRate`
field missing on `SkillData` (implementation deferred, check current status). Skill tier
improves budget levers only, never hit size; sibling-clone parity defined at equal tier.

Design decisions should carry an explicit *"Justification: …"* line when a real rationale
exists (added 2026-07-04 after the no-immunities rule) — a load-bearing rule without its
written why is how rules erode later.

---

## 5. Multi-agent issue/PR workflow

**Canonical rule is `.agents/AGENTS.md` Rule 6** (kept in sync with `CLAUDE.md` via a
consistency-check hook) — read that for the actual mechanics (labels, branching,
claim-first discipline, terminal labels). This section is rationale/history only.

**The three agents:**
- **Claude CLI** (this agent) — internal implementer, has Godot MCP + Blender MCP.
- **Google Gemini via Antigravity CLI** — also internal, same MCP tool access. Original
  author of the AGENTS.md convention/file.
- **Hermes** (+ free coding LLM models) — external/weaker coding-only implementer, no
  MCP/editor access, works from a separate clone, routed manually by the user. Scope-logic
  issues only (routing is the user's call, not a label — see Rule 6 history).

**Config-file research (confirmed, don't re-derive):**
- Claude Code `@path` imports are eager, loaded at launch. `CLAUDE.md` → `.agents/AGENTS.md`
  import must use the correct relative path (`@.agents/AGENTS.md`, not `@AGENTS.md`) or it
  fails silently — see §8 below, this bit the project once already.
- Hermes precedence: first-match-wins, exclusive: `.hermes.md` → `AGENTS.md` → `CLAUDE.md`
  → `.cursorrules`. No `.hermes.md` exists (deliberately) — don't create one, it would make
  Hermes stop reading `AGENTS.md`.
- Antigravity CLI reads `AGENTS.md`/`.agents/` natively.

**Issue-spec discipline**: keep issues lean — if a spec/test suite is detailed enough to
fully constrain the implementation, you've effectively done the implementation work
yourself. Reference existing patterns, give acceptance criteria + touch points, not literal
code. Applies to issues and pre-handoff verification only — NOT to reference docs
(`docs/technical-*.md`), which should be fully specced since they're reused across every
future issue.

**Spec lever mapping** (2026-07-04 incident): before ticketing a binding implementation
spec (technical-systems.md style), walk every row of the design table and name the exact
spec field that expresses each lever. A field existing on the runtime type ≠ the authoring
type being able to reach it. Watch for double-duty fields (e.g. `Range` = reach on
Entity/Position targeting but AoE radius on Self-targeting). The wave-1 FormData spec once
omitted a radius knob for Self-targeted forms this way, and the implementer shipped a PR
that had to be sent back.

**No test infra**: as of last check, no test project/GdUnit4 wiring exists in the repo.

**Labels** (see Rule 6 for full current scheme): Stage (`submitted`→`ready`→`in-progress`
→`needs-review`, `blocked` stackable, `changes-requested` for rework), Complexity
(`complexity-low/medium/high`, resolves upward if malformed), Terminal (`finished`/`rejected`,
auto-closes issue via `sync-project-status.yml` since git-flow PRs target `develop` and
never trigger native `Closes #N`).

---

## 6. Cross-cutting working agreements (feedback memories)

- **Conflicting/ambiguous instructions → always stop and ask**, never silently resolve
  toward whichever reading lets you keep acting — even mid-session, even after a run of
  fast "yes, do it" exchanges. (Triggering incident: "create the issue, don't execute it,
  don't do anything else" got executed anyway because of session momentum.)
- **Godot MCP unresponsive** → stop immediately, tell the user, no workarounds (raw .tscn
  edits, headless scripts, C# `_Ready()` construction all violate the MCP-only rule and can
  corrupt scenes).
- **UI styling** always goes through the `.tres` theme file (`assets/ui/game_theme.tres`)
  via MCP theme tools. Procedural C# boxes / custom meshes are a last resort — discuss first.
- **Consistency-check hook** (`[CONSISTENCY CHECK FAILED]`): no pre-tool-call mention: put
  the specific diverging line in the post-read summary only.
- **`game-design-direction.md`** is a scratchpad for half-baked ideas only. Resolved →
  formalize into the GDD and remove from here. Rejected → remove, no trace.
- **`CLAUDE.md` `@path` imports fail silently** if the path is wrong — verify the target
  file actually exists at the resolved path before trusting an import works; a "session-start
  instructions aren't taking effect" symptom should make you suspect this first.

---

## 7. Session history / what's shipped (as of 2026-07-08)

**Backlog is EMPTY as of 2026-07-08** (`gh issue list --state open` → zero). Latest PR
review (this session) merged **PR #81 / issue #80 — stacking buffs** (see below); no
other issues were pending.

### Stacking buffs — SHIPPED 2026-07-08 (issue #80, PR #81, merged to `develop`)
Design resolved same day via grill session (`docs/design-skill-system-brainstorming.md`
accumulation-state log entry). Split the long-parked "accumulation state" question into
two independent mechanics:
- **Stacking buffs** (this one, built): `BuffData` gained `MaxStacks` (mirrors
  `EotData.MaxStacks`, default 1 = old refresh-in-place behaviour unchanged).
  `PlayerController._activeBuffs` became `Dictionary<string, List<ActiveBuff>>` — each
  instance ages out independently (Poison-style), cap-refresh reuses `EnemyController.ApplyEot`'s
  "refresh instance nearest to expiring" rule. `RebuildStats` sums modifiers across all
  instances via the existing `StatBlock` transient-modifier engine — no new magnitude math.
  Reviewed and merged same day: clean build (0 warnings/errors in an isolated worktree), no
  merge-tree conflicts, scope matched the issue exactly. One incidental improvement noted in
  review (non-blocking): `ApplyBuff` now falls back to `buff.Duration` when no explicit
  duration is passed (`duration ?? buff.Duration`) — previously `BuffData.Duration` was
  defined but never actually consulted by the only caller (`WeaponController`, which never
  passes a duration). No current `BuffData` entry sets `Duration`, so this was a no-op today,
  not a behavior change — closes a latent gap rather than introducing risk.
- **`charge_release_burst` accumulation** (design-only, NOT built): charge is
  skill-instance-owned (a runtime counter on the one skill slot, like its cooldown tracker),
  deliberately not a new character-level resource pool. Build mode (+1 charge/cast up to
  skill-owned `MaxCharges`) and release mode (spends all charge for a burst scaled by
  charges × per-charge baseline), no decay. Still needs an implementation ticket.

### Void identity — SHIPPED 2026-07-08 (issue #76, PR #78)
`DamageType.Void` + `VoidResistance` threaded through `EnemyData`/`EnemyController`/
`DungeonGenerator`/`BalanceConfig.Enemies.Skeleton` (mirrors the Poison-resistance precedent
from #73). New `decay` EoT: non-stacking, %-max-HP damage via `EotData.DamagePerTickFraction`
(resolved against target's own MaxHealth at apply time, not a static constant), crit-scaling
explicitly suppressed for `eot.Id == "decay"`. `void` identity registered in
`IdentityRegistry` (`SignatureEotId: "decay"`), craftable through the existing generic
proto/form/identity composition path. This closed out the occult-wave candidate list
(Poison #73 + Void/Decay #76 both done).

### D1 (EoT/buff-magnitude wall) — RESOLVED and SHIPPED 2026-07-07
Design: EoT/ailment/buff magnitude = `EotData`/`BuffData` baseline (identity-owned) ×
character Ailment stats × an optional power-neutral per-form modifier (duration↔magnitude
re-slice OR cost-proportional) — never a free scalar. Design via PR #65/#66; implementation
via #61/PR#67 (fleet↔enduring forms + D1 hook), #63/PR#68 (debuff-emitter `self_aura`
Phase 1), #64/PR#69 (player-buff framework + buff auras Phase 2), #62/PR#71 (reserve
buff/debuff-aura form, closed out the branch).

Code facts (all on develop, `src/buffs/BuffData.cs`, `src/buffs/BuffRegistry.cs`):
- EoT re-slice: magnitude snapshotted onto `EotInstance` at application (not re-read from
  `EotData`). `EnemyController.ApplyEot(eot, crit, eotSlice)` sets
  `TimeRemaining = Duration/slice`, magnitude × slice. Only the form's own `DebuffEotId` is
  sliced; signature/augment EoTs get slice 1.0.
- Buff framework: `BuffData` (`BuffModifier(StatId, ModifierType, Value)[]` + optional
  `Duration`) + `BuffRegistry`; new `ModifierSource.Buff`. `PlayerController` gained
  `ApplyBuff`/`RemoveBuff`/`TickBuffs` + `RebuildStats()`.
- Aura modes: `FormData` carries `DebuffEotId`/`BuffId`; `SkillData` carries
  `EotSlice`/`BuffId`. `WeaponController.FireAuraTick` branches on which is set. Damage+debuff
  hybrids only via grafting a debuff augment onto a damage aura.
- **Known open question (flagged in #62 review, not yet resolved)**: for debuff-mode auras,
  `EnemyController.ApplyEot` ties `EotSlice` to both magnitude and duration
  (`SlowFraction *= slice`, `Duration /= slice`) — so a lighter-reservation aura gives a
  weaker-but-longer effect (EoT-value roughly constant) rather than a pure magnitude cut.
  Buff-mode doesn't have this coupling. Worth an in-editor check before relying on
  debuff-aura balance numbers.

### Whole skill synthesis — COMPLETE (design-only, merged via PR #60)
All three raw-extraction tables in `design-skill-system-brainstorming.md` resolved and
promoted: Forms (6 adopted, ~22 combos, tickets #35–#40 merged), Identity pass (only
Poison + Void/Shadow survived to the occult wave, now both shipped — Bone/Blood/Holy/
Earth/Wind decomposed; rule: an identity is earned only by a genuine enemy-side,
resist-relevant signature), Augment pass (4-bucket triage, new base trigger
`on_target_death_%` admitted, armour augment layer is defensive-only).

### Deferred / candidate backlog (not yet ticketed)
- `charge_release_burst` implementation (design done, see above).
- Bucket-B/C gates: projectile/AoE augment systems, sustain, placement/mine, EoT-transfer,
  corpse-free detonate, equipment defensive mechanics, enemy-accuracy/armour, hard-CC
  diminishing, condition-layer triggers (`on_crit`/`while_channeling`/hp-threshold).
- Freeze CC-diminishing rule (deferred from element design).
- `InherentEotIds` field removal, `SkillData.TickRate` field (check current status —
  may have shipped since the 2026-07-04 grill).

---

## 8. Voxel character model (assets, mostly done as of ~8 days ago — verify current state)

Built in `assets/models/characters/src/player.blend` (renamed from voxel_human.blend).
Exported to `assets/models/characters/player.glb` as the player model.

- 16-part mesh joined into VoxelHuman_v4, complete with rig, ready for in-game testing
  (last checked).
- 64×64 texture (packed into blend) — peach skin, dark hair, vivid red tunic, blue
  trousers, brown boots, yellow eyes. UV fix applied: TUNIC=36/64, PANTS=52/64, BOOTS=60/64.
- Armature: full Mixamo-named hierarchy, facing +Y to match mesh; mesh parented with
  automatic weights.
- Bone hierarchy: `Root → Hips → Spine → Chest → Neck → Head`, `Chest → UpperArm_L/R →
  LowerArm_L/R → Hand_L/R`, `Hips → UpperLeg_L/R → LowerLeg_L/R → Foot_L/R`.
- **Next steps flagged last session**: test in Godot (animations on new mesh), check
  weapon attachment to `Hand_R`, tweak automatic weights if deformation looks bad, iterate
  proportions/colours if needed. Check whether this happened since — not confirmed shipped.

---

## Reading order if picking this up cold

1. `docs/index.md` (Project Status block) — current phase/version at a glance.
2. `.agents/AGENTS.md` Rule 6 — the actual workflow mechanics (labels, branching, handoffs).
3. This file, §7, for what shipped most recently and what's still open.
4. `gh issue list --state open` — authoritative current backlog (this file's snapshot may
   already be stale by the time you read it).
