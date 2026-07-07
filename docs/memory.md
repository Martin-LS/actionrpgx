# actionrpgx — Memory Transfer

> Consolidated dump of everything in the AI agent's persistent memory for this project, exported so work can continue from another machine / another agent. Snapshot: **2026-07-07** (updated after the buff/debuff-aura implementation wave landed).
>
> ⚠️ Point-in-time observations, not live state. `file:line` citations and "current state" claims can go stale — verify against code/docs before treating as fact. Where a decision is said to live in a doc (GDD, AGENTS.md Rule 6, `design-*.md`), **that doc is authoritative**; this file is history + rationale + current-status pointers.
>
> Orientation on session start: read `docs/index.md`; then `docs/technical-tips.md` (3D/anim/bone), `docs/visuals-style.md` (visual/UI/VFX), `docs/technical-coding.md` (registry/gameplay logic) as relevant.

---

## PART 1 — GAME DESIGN

### 1.1 Core project direction (design pillars)

**actionrpgx** is a fork of a horde-survival game, **pivoted to a full action RPG in the Diablo / Path of Exile lineage**. The repos have diverged — never carry horde-survival assumptions (auto-cast, 1 skill slot, timer-based runs) into ARPG work without checking the design docs first.

Resolved core pillars:

- **Fully craft-driven**: items, skills, and maps all come from crafting. Enemies drop crafting materials only — no direct item drops. Crafting is the progression engine.
- **Manual cast and manual targeting.** Auto-cast is kept in code for dev convenience only.
- **5 skill slots, freeform (PoE-style)**, all active — no passives on the skill bar.
- **PoE2 keybinding layout**: WASD move; Q E R F + mouse for skills; Space for dodge.
- **Base stats** (original scheme — §1.3's delivery-scaling revision supersedes the damage-coupling parts):
  - Strength → PhysicalDamage, PhysicalResistance, MaxHp, CritDamage
  - Dexterity → CritChance, Evasion
  - Intelligence → MagicDamage, MagicResistance, MaxFocus, FocusRegen
  - Movement speed is **NOT** a base stat.
- **CDR is a weapon property** — fixed per weapon type initially, global (all skills), roll ranges later. No character-level attack speed stat.
- **Augments**: separate socketable items; one type per skill at a time; all use an `on_enemy_hit_%` trigger with re-rollable %; freely removable. (Trigger roster expanded in the augment synthesis — see §2.5.)
- **Passives removed from the skill bar** — they live on armour augments instead.
- **Crit**: CritChance from Dex (global baseline) + Bow identity bonus + Critical Strike skill augment. CritDamage from Str. Any archetype can invest via augments; Rogue builds it naturally through Dex.
- **Evasion**: passive % chance to fully avoid a hit. Distinct from **Dodge** (active Space-bar roll). Terminology fixed: *Dodge* = roll, *Evasion* = passive stat.
- **Crafting (early)**: one resource ("crafting resource"), everything costs 1.
- **Equipment augments**: same model as skill augments; prototypes only early. Armour augment layer is **defensive-only** (see §2.5).
- **Stash**: account-shared. All crafting materials go to a shared pool.
- **"Player-facing skills" term retired** — all craftable skills are player-usable by definition. `EngineProof` kind reserved in code for future use.

### 1.2 Element / identity framework (promoted to GDD 2026-07-06, commit 17b15c3)

Sourced from `docs/ref-arpg-damage-types.md` (D4 / PoE2 / Last Epoch / Grim Dawn / No Rest for the Wicked survey). Promoted across design-skills / design-stats / design-mechanics / design-augments with justification lines.

- **Roster: elements-only.** Physical, Fire, Cold, Lightning (core). No "Magic"/"Arcane" (genre-oddity). **Magic → Fire migration shipped.** Later occult additions planned: Poison (stacking), Void/Shadow (Wither/Decay); player-side specials: Blood (leech), Holy (heal).
- **Model A: per-element enemy resistances** — each element its own soft resist channel. (Issue #53 / PR #57.)
- **Identity is a REAL, resist-balanced lever**, not a free skin. Parity held by resist *distribution* (fire-resistant packs exist), not by ailment power-equivalence.
- **Every damage type carries a signature ailment innately** (target-debuff class only):
  - Physical → **Bleed** (DoT); Fire → **Burn** (DoT); Cold → **Chill** (slow) + **Freeze** at a threshold; Lightning → **Shock** (damage-taken amp).
  - **Chain = delivery (not an ailment). Heal = player-side (not an ailment).**
- **Ownership-matrix pillar**: "every damage type carries its signature ailment; augments GRAFT off-type ailments (never amplify the innate)." EoT payload is co-owned (identity signature + augment grafts).
- **Ailment scaling = a generic Ailment stat family** (Ailment Damage / Effect / Duration / Chance), never per-element.
- **Deferred**: Freeze/hard-CC needs a CC-diminishing rule; Poison needs a stacking-EoT system + a 5th resist channel; player per-element resistances stay lumped as `ElementalResistance` placeholder until enemies deal elemental damage.

### 1.3 Delivery-scaling model (resolved — fixes "Dex/Rogue has no damage axis")

- **Damage scales by DELIVERY, not type:** Str → Melee, Dex → Ranged, Int → Spell/cast (sword / bow / wand; Warrior / Rogue / Mage).
- **Type fully DECOUPLED from scaling** — an element = type + ailment + resist only. Any archetype can play any element.
- **Crit is universal** (delivery-agnostic). Dex is no longer crit-only.
- **Hybrid-friendly, shallow curve** — splashing a weapon's stat is usable; committing is strong; ignoring is weak. A build in NEITHER of a weapon's stats is correctly weak (e.g. Str+Dex holding a wand).
- **Ripples (shipped as issue #49):** Wand `Ranged`→`Spell/cast`; 2 damage pools (Physical/Magic) → **3 delivery pools (Melee/Ranged/Spell)**; `PrimaryStatConversions` reworked.

### 1.4 Crit itemization (issue #50 / PR #56 — shipped)

`CritDamage` moved from derived-only (Exclusive) to a **composed, itemizable** stat: base 1.5× + Str-derived + `CritDamageBonus` gear affix + a crit-damage skill augment ("Critical Power"), composing additively. Crit is a genuine build archetype.

### 1.5 Stat ownership (guardrails)

`docs/design-stats.md` is authoritative: catalogue, **Stat Ownership Matrix (§5)**, **Deliberate Absences (§5C)**, and the ailment/buff magnitude rules (§5C/§5E — see §2.4 for the D1 resolution).

- **No per-skill damage multiplier.** Damage progression is anchored in the crafting economy via weapon tier.
- **No character attack/cast/CDR speed stat.** The weapon CDR property is the only gear lever. (This barred "Kill momentum" attack-speed augments — see §2.5.)
- **Damage type is skill-authoritative**, fixed at creation, overridable only by a socketed augment.

**Guard rule:** if a design idea puts a stat on the wrong entity or resurrects a deliberate absence, **push back before implementing** — amend the matrix consciously (discuss → edit design-stats.md first) or reject. No quiet exceptions.

### 1.6 Versioning / phase scheme (locked 2026-07-06)

- Old **v1/v2/v2+ vocabulary retired** across all docs.
- **Phase** = declared state; current = **pre-alpha** (exit to alpha = feature-complete, a decision not a number). Recorded in the Project Status block of `docs/index.md`.
- **Version** = SemVer `0.MINOR.PATCH` + phase pre-release suffix. Source of truth: `config/version` in `project.godot` (currently `0.1.0-prealpha`). MINOR bumps when a milestone ships to `main` (deliberate PR act); release workflow cuts tag `v<version>` + GitHub Release.
- **Docs discipline**: present tense for current truth — no version/scope tags on current features; deferrals say "deferred/future" with no target phase.

### 1.7 Character model (voxel human)

Built in `assets/models/characters/src/player.blend` (renamed from voxel_human.blend). Exported to `assets/models/characters/player.glb`.

- 16-part mesh joined into **VoxelHuman_v4**; 64×64 texture packed (peach skin, dark hair, red tunic, blue trousers, brown boots, yellow eyes). UV fix: TUNIC=36/64, PANTS=52/64, BOOTS=60/64.
- Full Mixamo-named armature, facing +Y; mesh parented with automatic weights.
- Bone hierarchy (matches player code):
  ```
  Root → Hips → Spine → Chest → Neck → Head
                              → UpperArm_L/R → LowerArm_L/R → Hand_L/R
               → UpperLeg_L/R → LowerLeg_L/R → Foot_L/R
  ```
- **Handedness (project rule):** player is right-handed — weapons attach to `Hand_R`. Sword/wand → `Hand_R` (right-arm swing `melee_right_atack`); Bow → `Hand_L` (left hand holds the bow; `melee_left_atack`). Old `player_character.glb` had the opposite convention and is retired.
- **Blender bone rotation direction (player.blend):** character faces −Y. Local-X rotation direction depends on rest orientation — downward bones (arms/legs) and upward bones (spine/head) are opposite. Empirical tail-position test before keying a new bone type (see AGENTS.md Rule 3).

---

## PART 2 — SKILL-SYSTEM SYNTHESIS

Skill design was synthesised from three tables in `docs/design-skill-system-brainstorming.md` (9 forms, 7 identities, 20+8 augments), in order forms → identities → augments.

### ✅ WHOLE synthesis COMPLETE (2026-07-07, design-only)

All three raw-extraction tables are resolved and promoted to docs. What remains is **downstream engine/system building and issue implementation**, not further synthesis. Design docs merged to `develop` via **PR #60** (identity commit 25968a8 + augment commit 1f40c66; merge ff677a1).

### 2.1 Forms pass — COMPLETE & shipped (tickets #35–#40 all merged)

9-form tally: **6 adopted** (~22 combos), 1 parked→later adopted, 2 rejected, 1 dissolved.

- **salvo** — entity_burst's 3rd form. Delivery granularity; N=3–5 (Balancer); tier = cooldown↓. Needed the **÷N proc-normalization engine rule**.
- **swarm-vs-singular** — stackable_zone + triggered_zone_burst (8 combos). Stack count = budget lever; instance "size" = radius/duration/tickrate, not damage. Pure data.
- **reserve-heavy/light** — self_aura (damage-aura scope). Reservation↔radius (4 combos). Buff/debuff-aura branch was parked on D1 (now resolved — §2.4).
- **blast/fuse/echo** — fixed_zone_burst (3 forms). echo shares salvo's multi-hit (÷N, N=2); fuse = lobbed bomb (Position mirror of quake).
- **ramp** — self_channeled_tick's 3rd form. Tick rate accelerates while held; tier = ramp-speed↑. Within-channel hold-time accumulator.
- **fleet-vs-enduring** — generic fallback axis, was PARKED (barred from damage-tick prototypes; sole home = entity_debuff), blocked on D1. **Now adopted + implemented (§2.4).**
- **pylon** — REJECTED as a form: *"a distinction that moves no budget lever is an identity skin, not a form."*
- **residue** — BLOCKED as a form; lingering tick = a secondary effect → augment-owned (lives as the trail augment).
- **STRUCTURAL: windup_burst dissolved** — differed from fixed_zone_burst only in WindUp (a budget lever). Retired as engine-proof scaffold (#40). Prototype roster 12→11.

**Recurring rulings:** (1) hit "size" = radius/rate/count, never per-target damage/magnitude; (2) a form must move a budget lever (pylon ruling).

### 2.2 Identity pass — COMPLETE (2026-07-07, design-only, promoted to docs, no code/issues yet)

Emergent principle: *a new identity is earned only by a genuine enemy-side, resist-relevant signature; else it's existing type + augment + skin.*

- **2 survive → occult wave (adopted-in-principle, GATED):** Poison (own channel + stacking-DoT; gated on a stacking-EoT engine + a 5th resist channel); Void/Shadow (signature Wither-vs-Decay + channel own-vs-shared-Chaos parked on a roster-growth decision; its raw "defence-bypass" signature was rejected).
- **5 rejected → decomposed:** Bone (Physical skin); Blood + Holy (player-side leech/heal = augment/heal-system, not signatures); Earth + Wind (Physical + CC/displacement augment).
- **Rulings promoted to `design-skills.md` Identity Layer:** identity adoption test (pylon's twin); *signature may not undermine resist-parity*; *player-side effects are never signatures*; *CC/displacement effects are never signatures*.

### 2.3 Augment pass — COMPLETE (2026-07-07, design-only, no issues yet)

Triage/classification (not adopt/reject). Governing rulings:
- **G2 — 4-bucket triage:** A named-derivation (ships when authored) / B mechanic-gated / C trigger-condition-gated / D rule-boundary.
- **G1 — trigger admission:** admitted ONE new base trigger `on_target_death_%` (unlocks spread/detonate-on-death, still mechanic-gated); deferred `on_crit` / `while_channeling` / hp-threshold as future *conditions* (base-type-vs-condition rule).
- **Batch-clear:** ~9 bucket-A named augments cleared to author-work (Bleed, Burn/Chill/Shock, Vulnerability, Root, Cold/Lightning conversions, Crit-damage). Bulk are bucket-B mechanic-gated (incl. all equipment defensive augments).
- **Bucket-D resolved:** Low-health surge (offensive damage-up rejected from armour → skill/offensive slot; defensive variant legal, gated on hp-threshold); Kill momentum (rejected as offensive tempo; attack-speed barred by §5C; needs an accumulation-state mechanic).
- **Sharpened rule:** the **armour augment layer is defensive-only** — offensive payloads (damage or stat buff) redirect to a skill augment / a future offensive equipment slot.

Promoted to `design-augments.md` + brainstorm doc.

### 2.4 D1 (EoT/buff-magnitude) — RESOLVED and IMPLEMENTED ✅

D1 was the long-standing wall parking fleet↔enduring and the reserve buff/debuff-aura branch. **Design resolution** (`design-stats.md` §5C/§5E): EoT/ailment/buff magnitude = **`EotData`/`BuffData` baseline (identity-owned) × character Ailment stats × an optional _power-neutral per-form modifier_** (either a duration↔magnitude re-slice, OR cost-proportional e.g. reservation→magnitude); **never a free scalar.** Design promoted to `design-skills.md` self_aura section + `design-stats.md`; the buff/debuff-aura mechanic was grilled and **split by cost** (debuff cheap / buff-framework expensive). Design merged via **PR #65** (merge ddd4bc4) and **PR #66** (merge b3251be, player-buff framework design).

**Implementation landed 2026-07-07** — three issues, all merged to `develop` (reviewed by Claude, rebased into a mergeable stack in dependency order):

| Issue | PR | What shipped |
|---|---|---|
| **#61** | #67 | fleet↔enduring forms on `entity_debuff` + the **D1 EoT-magnitude-modifier hook**. |
| **#63** | #68 | **debuff-emitter mode** on `self_aura` (Phase 1). |
| **#64** | #69 | **player-buff framework** + buff auras (Phase 2). |

Key implementation facts for whoever builds on this:
- **EoT re-slice (D1 shape 1):** magnitude is now **snapshotted onto `EotInstance`** (`SlowFraction`/`DamageTakenAmp`/`DamagePerTick`) at application, not re-read from `EotData`. A per-form `EotSlice` factor threads through the EoT tuple `(Id, Chance, Slice)` in the weapon spawners and `EnemyController.ApplyEot(eot, crit, eotSlice)`, which sets `TimeRemaining = Duration/slice` and magnitude `× slice`. **Product-invariant by construction** (fleet=1.5, enduring=0.5; tier tracks `EotMagnitudeUp`/`EotDurationUp` push the slice further, staying power-neutral). Only the form's own `DebuffEotId` is sliced; signature ailments and augment EoTs get slice 1.0.
- **Buff framework:** new `src/buffs/BuffData.cs` (`BuffModifier(StatId, ModifierType, Value)[]` + optional `Duration`) + `BuffRegistry`. New `ModifierSource.Buff`. It **reuses `StatBlock`'s existing transient modifier engine** (`AddModifier` / `RemoveModifiersFromSource`). `PlayerController` gained `ApplyBuff`/`RemoveBuff`/`TickBuffs` + a `RebuildStats()` that re-applies Buff-sourced modifiers **after every `BuildStatBlock()`** (so buffs survive equip/level rebuilds) and refreshes cached derived fields. Timed (duration→auto-expire) vs persistent (null→explicit removal); **refresh-on-reapply, no stacking**; **max-stat rule = ceiling only** (current clamps down, never granted); magnitude scales the **bonus portion** (Flat/Percent value; Multiply excess-over-1).
- **Aura modes:** `FormData` carries `DebuffEotId` / `BuffId`; `SkillData` carries `EotSlice` / `BuffId`. `WeaponController.FireAuraTick` branches: `BuffId` set → buff-aura (early-return, no damage; buff applied/removed on toggle in `TryFireSlot`); `DebuffEotId` set → debuff-aura (apply EoTs, no damage); else damage-aura. Damage+debuff hybrids come from grafting a debuff **augment** onto a damage aura, never from this mode.

### 2.5 Still open / next threads

- **#62 — reserve buff/debuff-aura FORM** (blocked, now unblocked by #61/#63/#64). **This is the key remaining piece:** #63 and #64 shipped the *plumbing* (`DebuffEotId`/`BuffId` fields + FireAuraTick branches + buff framework) but **no form/preset yet wires `DebuffEotId`/`BuffId` onto `self_aura`**, so neither the debuff-aura nor buff-aura mode is reachable through the craft wizard until #62. `BuffRegistry` currently holds seed buffs only (steel_wall / haste / focus_shield). The buff-aura's radius is vestigial → the reserve form's lever is reservation↔magnitude.
- **Occult wave** — stacking-EoT engine + Poison's 5th resist channel (unblocks Poison); roster-growth / shared-Chaos-channel decision (unblocks Void's channel+signature).
- **Deferred bucket-B/C gates** (become issues when scheduled): projectile/AoE augment systems, Sustain path, placement/mine, EoT-transfer, corpse-free detonate, equipment defensive mechanics, enemy-accuracy/armour models, hard-CC diminishing rule; the condition-layer triggers (`on_crit`/`while_channeling`/hp-threshold); an accumulation-state mechanic (Kill-momentum gate, also gates buff stacking).

---

## PART 3 — PROCESS & WORKFLOW

### 3.1 Multi-agent issue/PR workflow

Authoritative rule = **Rule 6** in `CLAUDE.md` / `.agents/AGENTS.md` (kept in sync). This is the rationale/history.

**The agents (all share the one human GitHub account `Martin-LS`):**
- **Claude CLI** — implementer; Godot MCP + Blender MCP.
- **Google Gemini via Antigravity CLI** — implementer, same MCP access. Original author of the AGENTS.md convention.
- **Hermes** (+ free coding LLMs) — coding-only "external"/weaker implementer, no MCP/editor access, separate clone driven by the user.

Because all agents commit under the same account, **ownership is by convention, not GitHub-enforced** — the claim comment + `in-progress` label is the only signal work has started.

**Routing is the user's call** — no scope/implementer labels. The MCP-only rules are the real guard against a no-MCP agent picking up editor work.

**Labels (issues only, not PRs):**
- **Stage** (mutually exclusive, always swap never stack): `submitted` → `ready` → `in-progress` → `needs-review`; `blocked` stackable (note blocking issue # in body); rework path swaps `needs-review` → `changes-requested`.
- **Complexity** (`complexity-low/medium/high`): a routing *floor* = minimum agent capability. Exactly one per issue; multiple or none → treat as `complexity-high` (fail-safe upward). Whoever creates an issue assigns exactly one.
- **Terminal**: `finished` (PR merged) / `rejected` (dumped). Applying either removes all other labels. If both land, treat as `rejected`.

**Handoffs are the agent's job** (each runs the `gh issue edit` swap):
- Claim: `ready`/`changes-requested` → `in-progress` + a one-line "starting work" comment. **Claiming comes FIRST — before branching or reading code.**
- Push PR: `in-progress` → `needs-review`.
- Review sends back: `needs-review` → `changes-requested`.
- Merge: remove stage labels, add `finished`. Reject: remove all, add `rejected`.

**Branching (git flow):** only after claiming, branch off `develop` as `feature/issue-<number>-<slug>`. **PRs target `develop`, not `main`.**

**Board sync:** `.github/workflows/sync-project-status.yml` maps labels → board Status (triggers on label/close/reopen). Board view filtered `-label:finished -label:rejected`, so a terminal label drops the card automatically. **Applying a terminal label also auto-closes the issue** (`finished`→completed, `rejected`→not planned) — this is the close trigger, because git-flow PRs target `develop` so GitHub's native `Closes #N` never fires. ⚠️ `issues:`-triggered workflows run from the **default-branch (`main`) copy** of the file, so the step only works once it's on `main`.

**Review mandate:** no PR merges without a review; an agent must **never review/merge its own PR** unless the user explicitly authorizes it for that task. Peer review (a different agent or the user) is required. *(Example done right today: Claude reviewed & merged Antigravity's #67/#68/#69.)*

### 3.2 Config-file facts (confirmed 2026-07-01, don't re-derive)

- **Claude Code**: `@path` imports are eager, loaded at launch. `CLAUDE.md` imports `@.agents/AGENTS.md`. Broken `@path` imports fail **silently** (literal `@filename` left unexpanded). Verify the target path exists relative to the importing file.
- **Hermes**: precedence is first-match-wins, exclusive: `.hermes.md` → `AGENTS.md` → `CLAUDE.md` → `.cursorrules`. No `.hermes.md` exists (deliberately — one would make Hermes ignore `AGENTS.md`).
- **Antigravity CLI**: reads `AGENTS.md`/`.agents/` natively.
- Net: **`AGENTS.md` is the single converged source of truth for all three agents.** `CLAUDE.md` = Claude-specific tooling on top; it `@`-imports AGENTS.md.

### 3.3 Issue-writing discipline

Keep issues **lean** — reference existing patterns, give acceptance criteria + touch points, not literal code or exhaustive tests. If a spec fully constrains the implementation, delegation is pointless — just implement it. Applies to pre-handoff verification too (catch an obvious scope-blocker, not an exhaustive audit).

**Exception — does NOT apply to reference docs** (`docs/technical-*.md`, `docs/design-*.md`): reused across every future issue, so full/careful speccing is correctly amortized.

**Spec-lever mapping (2026-07-04 lesson):** when writing a binding spec from design tables, walk **every row** and name the **exact spec field** carrying each lever, per targeting shape. A field existing on the runtime type ≠ the authoring type reaching it. Watch double-duty fields (`Range` = reach on Entity/Position but AoE radius on Self). Any lever without a named field = a spec gap; fix before creating the issue. (This once shipped a wrong PR sent back.)

### 3.4 Documentation habits

- **Justification lines**: recording a load-bearing decision (deliberate absences, brainstorm resolutions, matrix rows) → add an explicit italic `*Justification: …*` where a real rationale exists. A rule without its written why gets eroded. Don't force it on trivial calls.
- **`game-design-direction.md` is a scratchpad** for half-baked/open ideas only — never a resolved-decisions log. Resolved → formalise into the GDD and remove; rejected → remove, no trace. The GDD holds resolved decisions exclusively.

### 3.5 Tooling rules (Claude-specific — see CLAUDE.md)

- **Godot MCP Pro is the only way to do editor work** (nodes, scenes, particles, animations, materials, shaders). Never raw `.tscn`/`.tres` edits, GDScript workarounds, or C# `_Ready()` construction. After any `.tscn`/`.tres` change, call `reload_project`.
- **If Godot MCP times out / is unresponsive, STOP and tell the user** — no workarounds (they corrupt scenes).
- **Never run/query the live editor without explicit approval** each time (`play_scene`, screenshots, `get_output_log`, `get_editor_errors`, `get_scene_tree`, etc.). `reload_project` after an edit is the only exception. Vague "verify it works" is NOT approval.
- **"Done" = a clean build, nothing more.** Screenshots/playtesting/log inspection are optional extras needing separate approval. (Verified today: `dotnet build actionrpgx.csproj` is the build check; ran clean after each conflict resolution.)
- **Animation:** always set up AnimationTree via MCP in the editor — never construct it in C# (`Travel()` silently breaks). C# is limited to `AnimPlayer`/`Active`/`Travel()`; use `GetCurrentNode()` for state, no manual bool flags. Set loop modes at runtime in C# (GLB import ignores `.import` loop flags).
- **UI styling** goes through `assets/ui/game_theme.tres` via MCP theme tools. Procedural C# boxes / custom meshes are a last resort — discuss first.
- **Blender** always via Blender MCP (`execute_blender_code`), never headless `blender --background --python` for edits. Call `get_scene_info` first to verify connection; if it fails, STOP.
- **Placeholder VFX**: clone `src/vfx/effect1.tscn` as a plain file, rename, wire it in — never author bespoke effects or grind on appearance. Call `reload_project` after. The user styles real effects later.

### 3.6 Standing behavioural rules from the user

- **Conflicting/ambiguous instructions → STOP and ask.** Never silently pick the interpretation that lets you keep acting. Momentum from prior fast "yes, do it" turns is never a reason to guess. (Triggered by "create the issue, don't execute it" → assistant executed anyway; user generalized to *any* conflict.)
- **No changes without explicit approval.** Propose a plan, discuss, wait, implement — one item at a time when resolving a list. Format the approval prompt in **bold**.
- **Bug fixes are strictly localized** to the reported bug — change only the causing line(s); no refactors/renames/model changes. Spot a related flaw → report it and wait.

---

## PART 4 — CURRENT STATE (2026-07-07)

### Shipped to `develop`
- **Wave 1** fully shipped (#23–#27). Playtest gate dropped by user decision.
- **Pre-alpha declared**; versioning locked; docs de-versioned (~120 sites, 17 files).
- **Release v0.1.0-prealpha shipped** (#32 → #33 → develop→main #34, admin override). `release-on-main.yml` cut the tagged pre-release.
- **Element wave Phase 2**: Magic→Fire (#51), delivery-pool refactor (#49), Fire/Cold/Lightning identities + signature ailments (#52), per-element enemy resistances (#53/#57), crit itemization (#50/#56).
- **Auto-close-on-terminal-label workflow** now active on `main` (was PR #58/#59 at the prior snapshot).
- **Skill synthesis forms tickets** #35–#40.
- **Full skill-synthesis design** (identity + augment passes) via PR #60.
- **Buff/debuff-aura design** via PR #65 (mechanic split) + PR #66 (buff framework design).
- **Buff/debuff-aura implementation** via PR #67 (#61), #68 (#63), #69 (#64) — see §2.4. All merged today; issues `finished` + closed.

### Open right now
- **#62 — reserve buff/debuff-aura form** — the remaining piece to make the #63/#64 plumbing reachable via the craft wizard (see §2.5). Unblocked now that #61/#63/#64 are merged.
- Downstream engine gates + occult wave (§2.5) — become issues when scheduled.

### Housekeeping note
- Local `develop` may be behind `origin/develop` after the buff/debuff-aura merges (they were done server-side / in worktrees to avoid disturbing ~120 uncommitted `.import` files in the working tree). Fast-forward with `git pull` / `git merge --ff-only origin/develop` when convenient. The dirty `.import` files are Godot re-import churn, not source changes.
