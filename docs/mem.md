# actionrpgx — Memory Transfer

> Consolidated dump of everything in Claude's persistent memory for this project, exported so it can be pulled and worked on from another machine. Snapshot date: **2026-07-07**.
>
> ⚠️ These are point-in-time observations, not live state. `file:line` citations and "current state" claims may be stale — verify against the code/docs before treating as fact. Where the note says a decision now lives in a doc (GDD, AGENTS.md Rule 6, design-stats.md, etc.), that doc is the authoritative source; this file is history + rationale.

---

## PART 1 — GAME DESIGN

### 1.1 Core project direction (design pillars)

This repo (**actionrpgx**) is a fork of the horde-survival game, **pivoted to a full action RPG in the vein of Diablo and Path of Exile**. The two repos have diverged in design intent — design and code from the ARPG direction, never carry over horde-survival assumptions (auto-cast, 1 skill slot, timer-based runs) without checking the design docs first.

Resolved core pillars:

- **Fully craft-driven**: items, skills, and maps all come from crafting. Enemies drop crafting materials only — no direct item drops. Crafting is the progression engine.
- **Manual cast and manual targeting.** Auto-cast is kept in code for dev convenience only.
- **5 skill slots, freeform (PoE-style)**, all active — no passives on the skill bar.
- **PoE2 keybinding layout**: WASD move; Q E R F + mouse for skills; Space for dodge.
- **Base stats** (original scheme — see §1.3 for the delivery-scaling revision that supersedes the damage-coupling parts):
  - Strength → PhysicalDamage, PhysicalResistance, MaxHp, CritDamage
  - Dexterity → CritChance, Evasion
  - Intelligence → MagicDamage, MagicResistance, MaxFocus, FocusRegen
  - Movement speed is **NOT** a base stat.
- **CDR is a weapon property** — fixed per weapon type initially, global (all skills), roll ranges later. No character-level attack speed stat.
- **Augments**: separate socketable items; one type per skill at a time; all use an `on_enemy_hit_%` trigger with re-rollable %; freely removable.
- **Passives removed from the skill bar** — they live on armour augments instead.
- **Crit**: CritChance from Dex (global baseline) + Bow identity bonus + Critical Strike skill augment (per-skill bonus). CritDamage from Str. Any archetype can invest in crit via augments; Rogue builds it naturally through Dex.
- **Evasion**: passive % chance to fully avoid a hit. Distinct from **Dodge** (active Space-bar roll). Terminology is fixed: *Dodge* = roll mechanic, *Evasion* = passive stat.
- **Crafting (early)**: one resource ("crafting resource"), everything costs 1.
- **Equipment augments**: same model as skill augments; prototypes only early (trigger models TBD).
- **Stash**: account-shared. All crafting materials go to a shared pool.
- **"Player-facing skills" term retired** — all craftable skills are player-usable by definition. `EngineProof` kind reserved in code for future use.

### 1.2 Element / identity framework (PROMOTED to GDD 2026-07-06, commit 17b15c3)

Sourced from `docs/ref-arpg-damage-types.md` (survey of D4 / PoE2 / Last Epoch / Grim Dawn / No Rest for the Wicked). Promoted across design-skills.md, design-stats.md, design-mechanics.md, design-augments.md, with justification lines throughout.

- **Roster: elements-only.** Physical, Fire, Cold, Lightning (core). No "Magic"/"Arcane" — generic magic is a genre-oddity (no reference game has it). Shipped **Magic → Fire migration**. Later occult additions planned: Poison (stacking), Void/Shadow (Wither/Decay); special player-side: Blood (leech), Holy (heal).
- **Model A: per-element enemy resistances** — each element its own soft resist channel. Chosen for genre fidelity. (Implemented in issue #53 / PR #57.)
- **Identity promoted from a FREE lever to a REAL, resist-balanced lever.** Parity is held by resist *distribution* (fire-resistant packs exist), NOT by ailment power-equivalence.
- **Every damage type carries a signature ailment innately** (target-debuff class only):
  - Physical → **Bleed** (DoT)
  - Fire → **Burn** (DoT)
  - Cold → **Chill** (slow) + **Freeze** at a threshold
  - Lightning → **Shock** (damage-taken amplification)
  - **Chain = delivery (NOT an ailment). Heal = player-side (NOT an ailment).**
- **Ownership-matrix pillar rewrite**: the old rule "no damage skill carries an inherent EoT" inverts to "**every damage type carries its signature ailment; augments GRAFT off-type ailments (never amplify the innate).**" EoT payload becomes co-owned (identity signature + augment grafts). Documented as a deliberate amendment with justification.
- **Ailment scaling = a generic Ailment stat family** (Ailment Damage / Effect / Duration / Chance), never per-element.
- **Deferred**: Freeze needs a CC-diminishing rule; Poison needs a stacking-EoT system; player-side per-element resistances (only matter once enemies deal elemental damage — player resists stay lumped as `ElementalResistance` placeholder for now).

### 1.3 Delivery-scaling model (resolved — fixes "Dex/Rogue has no damage axis")

- **Damage scales by DELIVERY, not by type:** Str → Melee, Dex → Ranged, Int → Spell/cast (aligned with sword / bow / wand and Warrior / Rogue / Mage).
- **Damage type is fully DECOUPLED from scaling** — an element = type + ailment + resist only, no stat coupling. Any archetype can play any element, scaled by how they deliver it.
- **Crit is universal** (delivery-agnostic) — Dex splash benefits any build; Dex is no longer crit-only.
- **Hybrid-friendly, shallow scaling curve** — splashing a weapon's stat = usable; committing = strong; ignoring = weak. Off-alignment is viable-but-suboptimal. A build investing in NEITHER of a weapon's stats is correctly weak (e.g. Str+Dex holding a wand).
- **Why D4's "scale off class stat" was rejected**: D4 is class-locked; our game is classless-skill (PoE2 is the closest cousin).
- **Ripples at promotion**: Wand `Ranged` → `Spell/cast`; 2 damage pools (Physical/Magic) → **3 delivery pools (Melee/Ranged/Spell)**; §5C mono-typed justification re-bases to "melee→Str, ranged→Dex, spell→Int"; `PrimaryStatConversions` reworked. (Delivery-pool refactor shipped as issue #49.)

### 1.4 Crit itemization (issue #50 / PR #56 — shipped)

`CritDamage` moved from a derived-only (Exclusive) stat to a **composed, itemizable** stat: base 1.5× + Str-derived + `CritDamageBonus` gear-affix source + a crit-damage skill augment ("Critical Power"), all composing additively. Crit is now a genuine build archetype.

### 1.5 Stat ownership (guardrails)

`docs/design-stats.md` is the authoritative home for stats: catalogue, **Stat Ownership Matrix (§5)**, and **Deliberate Absences (§5C)** — stats that must never exist:

- **No per-skill damage multiplier.** Damage progression is anchored in the crafting economy via weapon tier (rationale in design-skills.md).
- **No character attack/cast/CDR speed stat.** The weapon CDR property is the only gear lever.
- **Damage type is skill-authoritative**, fixed at creation, overridable only by a socketed augment.

Guard rule: if any design idea puts a stat on the wrong entity or resurrects a deliberate absence, **push back before implementing** — amend the matrix consciously (discuss → edit design-stats.md first) or reject. No quiet exceptions.

Open threads from the 2026-07-04 grill: `InherentEotIds` → delete the field (code change was pending as an issue); `TickRate` field missing on `SkillData` (implementation deferred). Skill tier improves **budget levers only**, never hit size; sibling-clone parity is defined at equal tier.

### 1.6 Versioning / phase scheme (locked 2026-07-06)

- Old **v1/v2/v2+ vocabulary is retired** across all docs.
- **Phase** = declared state; current = **pre-alpha** (exit to alpha = feature-complete, a decision not a number). Recorded in the Project Status block at the top of `docs/index.md`.
- **Version** = SemVer `0.MINOR.PATCH` + phase pre-release suffix. Source of truth: `config/version` in `project.godot` (currently `0.1.0-prealpha`). MINOR bumps when a milestone ships to `main` (deliberate act in the PR, never auto-incremented); tag `v<version>` + GitHub Release cut by the release workflow.
- **Docs discipline**: describe current truth in present tense — no version/scope tags on current features; deferrals say "deferred/future" with no target phase. "Waves" (wave 1, element wave) stay and all target pre-alpha.

### 1.7 Character model (voxel human)

Built in `assets/models/characters/src/player.blend` (renamed from voxel_human.blend). Exported to `assets/models/characters/player.glb` as the player model.

- 16-part mesh joined into **VoxelHuman_v4**; 64×64 texture packed into the blend (peach skin, dark hair, red tunic, blue trousers, brown boots, yellow eyes). UV fix applied: TUNIC=36/64, PANTS=52/64, BOOTS=60/64.
- Armature with full Mixamo-named hierarchy, facing +Y to match the mesh; mesh parented with automatic weights.
- Bone hierarchy (matches player code):
  ```
  Root → Hips → Spine → Chest → Neck → Head
                              → UpperArm_L/R → LowerArm_L/R → Hand_L/R
               → UpperLeg_L/R → LowerLeg_L/R → Foot_L/R
  ```
- **Handedness (project rule):** player is right-handed — weapons attach to `Hand_R`. Sword/wand → `Hand_R`; Bow → `Hand_L` (left hand holds the bow).
- Next steps: verify animations on the new mesh in Godot; check weapon attachment to `Hand_R`; tweak automatic weights if deformation looks bad; iterate proportions/colours.

---

## PART 2 — SKILL-SYSTEM SYNTHESIS (as of 2026-07-06)

Raw design is being synthesised from three tables in `design-skill-system-brainstorming.md` (9 forms, 7 identities, 20+8 augments). Order: **forms → identities → augments**. The 9-form pass is **COMPLETE**; identity + augment tables are still raw.

### 2.1 The 9-form synthesis tally

**6 adopted (~22 new combos):**

- **salvo** — ADOPTED as entity_burst's 3rd form. Axis = delivery granularity; N fixed 3–5, Balancer-owned; tier track = **cooldown↓**; budget trade = spread-payload for smoother texture. Requires a **÷N proc-normalization engine rule** before it ships (precedent for future Splash + projectile augments).
- **swarm-vs-singular** — ADOPTED for both stackable_zone + triggered_zone_burst (8 combos). Stack count promoted to a budget lever. Instance "size" = radius/duration/tickrate, not damage. Guard: stackable_zone·singular keeps stack floor ≥2. Pure data, no engine work.
- **reserve-heavy/light** — ADOPTED for self_aura (damage-aura scope). Reservation↔radius (4 combos); tier tracks reserve-heavy→radius↑, reserve-light→reservation↓. "Size" = radius. Buff/debuff-aura branch parked on the D1 magnitude question.
- **blast/fuse/echo** — ADOPTED on fixed_zone_burst (now 3 forms):
  - **echo** = single burst vs. blast + delayed aftershock (4 combos); shares salvo's multi-hit machinery (÷N with N=2); delayed hit reuses wind-up scheduling.
  - **fuse** = lobbed bomb, wind-up + long CD ↔ big radius (Position mirror of quake). **THE STRUCTURAL ONE** → see structural ruling below.
- **ramp** — ADOPTED as self_channeled_tick's 3rd form. Tick rate accelerates the longer you hold, constant drain, resets on release (2 combos); tier track = ramp-speed↑. Power-neutral via a Balancer ramp curve. Bounded engine hook: within-channel hold-time accumulator → tick rate.

**1 parked axis:**

- **fleet-vs-enduring** (duration↔potency) — ACCEPTED as a generic fallback axis but PARKED. Barred from damage-tick prototypes. Sole unique home = entity_debuff, blocked because EoT magnitude+duration live on a shared `EotData` def, not on the skill/form — unblocks when the element/EoT wave (D1) rules on per-form EoT-magnitude override.

**2 blocked/rejected:**

- **pylon** — REJECTED as a form. Emitter-vs-carpet moves no budget lever → it's an identity/VFX skin, not a form. Crystallized ruling: *"a distinction that moves no budget lever is an identity skin, not a form."*
- **residue** — BLOCKED as a form. Lingering tick = a secondary effect → augment-owned (ownership matrix). Lives as the trail augment.

**1 structural dissolution:**

- **STRUCTURAL RULING: wind-up is a form axis, not a chassis → windup_burst dissolves.** It differed from fixed_zone_burst only in WindUp (a budget lever; quake is the wave-1 precedent). windup_burst gets no player forms and is retired as engine-proof scaffold in a follow-up impl issue. Prototype roster 12→11.

### 2.2 Recurring rulings doing repeat work

1. **Hit-size clarification**: "size" = radius / rate / count, **never** per-target damage/magnitude. Used in all 4 adopts. (Governing framework wording: "bars buying a bigger hit, not dividing a fixed one.")
2. **The D1 EoT/buff-magnitude wall**: fleet-vs-enduring and reserve-heavy/light's buff/debuff branch both park here; they unblock together when the element/EoT wave rules on per-form EoT magnitude.
3. **Form must move a budget lever** (the pylon ruling).

### 2.3 Implementation tickets created 2026-07-06 (#35–#40)

All were `ready` at creation. (Some may since have moved — verify current issue state.)

| # | Complexity | What | Dependency |
|---|---|---|---|
| #35 | medium | multi-hit-from-one-activation + ÷N proc normalization engine + salvo form (entity_burst). Foundational. | none |
| #36 | medium | fixed_zone_burst forms blast/fuse/echo. blast+fuse ready; echo needs #35. | blocked on #35 |
| #37 | medium | ramp form + within-channel hold-time accumulator (self_channeled_tick). | none |
| #38 | low | swarm+singular forms (stackable_zone + triggered_zone_burst). Pure data. | none |
| #39 | low | reserve-heavy/light forms (self_aura, damage scope). Pure data + 1 radius verify. | none |
| #40 | medium | retire windup_burst prototype (migrate recipe/refs + save migration). Must follow fuse. | blocked on #36 |

Ready to route with no dep: **#35, #37, #38, #39** — all pure-logic (no Godot MCP). Recommended first: **#35** (unblocks #36).

### 2.4 Still raw (future synthesis threads)

- **Identity extraction table** (7 candidates) → becomes the element-wave design; resolves the D1 magnitude gate that's parking fleet↔enduring + the reserve buff/debuff branch.
- **Augment extraction table** (20+8 candidates). Not started.

---

## PART 3 — PROCESS & WORKFLOW

### 3.1 Multi-agent issue/PR workflow

Authoritative rule = **Rule 6** in `CLAUDE.md` / `.agents/AGENTS.md` (kept in sync between both). This is the rationale/history behind it.

**The three agents:**
- **Claude CLI** (this agent) — internal implementer; has Godot MCP + Blender MCP.
- **Google Gemini via Antigravity CLI** — also an internal implementer with the same MCP access. Original author of the AGENTS.md convention.
- **Hermes** (+ free coding LLM models) — coding-only "external"/"weaker" implementer, no MCP/editor access, runs from a separate clone driven by the user.

**Routing is the user's call** — no scope/implementer labels; the user hands issues to agents directly. The MCP-only rules are the real guard against a no-MCP agent picking up editor work.

**Labels (issues only, not PRs):**
- **Stage** (mutually exclusive, always swap never stack): `submitted` → `ready` → `in-progress` → `needs-review`; `blocked` stackable (note the blocking issue # in the body); rework path swaps `needs-review` → `changes-requested`.
- **Complexity** (`complexity-low/medium/high`): a routing *floor* = minimum agent capability. Exactly one per issue; multiple or none → treat as `complexity-high` (fail-safe upward).
- **Terminal**: `finished` (PR merged) / `rejected` (dumped). Applying either removes all other labels. If both somehow land, treat as `rejected`.

**Handoffs are the agent's job** (each runs the `gh issue edit` swap itself):
- Claim: `ready`/`changes-requested` → `in-progress` + a one-line "starting work" comment. **Claiming comes first, before branching or reading code.**
- Push PR: `in-progress` → `needs-review`.
- Review sends back: `needs-review` → `changes-requested`.
- Merge: remove stage labels, add `finished`. Reject: remove all, add `rejected`.

**Branching (git flow):** only after claiming, branch off `develop` as `feature/issue-<number>-<slug>`. **PRs target `develop`, not `main`.**

**Board sync:** `.github/workflows/sync-project-status.yml` maps labels → board Status (triggers on label/close/reopen). The board view is filtered `-label:finished -label:rejected`, so a terminal label drops the card automatically.

**Review mandate:** no PR merges without a review; an agent must never review/merge its own PR unless the user explicitly authorizes it for that task.

**Auto-close gap + fix (2026-07-07):** GitHub's native `Closes #N` only fires on merge to the **default branch (`main`)**; git-flow PRs target `develop`, so merged issues stayed open. Fix: a step in `sync-project-status.yml` closes the issue when a terminal label is applied (`finished` → completed, `rejected` → not planned). ⚠️ `issues:`-triggered workflows run from the **default-branch copy** of the file, so the step only fires once it's on `main`.

### 3.2 Config-file facts (confirmed 2026-07-01, don't re-derive)

- **Claude Code**: `@path` imports are eager, loaded at launch. `CLAUDE.md` can import via `@.agents/AGENTS.md`. Broken `@path` imports fail **silently** (literal `@filename` left unexpanded — no error). Always verify the target path exists relative to the importing file.
- **Hermes**: precedence is first-match-wins, **exclusive**: `.hermes.md` → `AGENTS.md` → `CLAUDE.md` → `.cursorrules`. No `.hermes.md` exists (deliberately — creating one would make Hermes ignore `AGENTS.md` entirely).
- **Antigravity CLI**: reads `AGENTS.md`/`.agents/` natively.
- Net: `AGENTS.md` is the single converged source of truth for all three agents.

### 3.3 Issue-writing discipline

Keep issues **lean** — reference existing patterns, give acceptance criteria + touch points, not literal code or exhaustive test cases. Rationale: if a spec fully constrains the implementation, you've done the work yourself and delegation is pointless. Applies to pre-handoff verification too (catch an obvious scope-blocker, not an exhaustive audit).

**Exception — this does NOT apply to reference docs** (`docs/technical-*.md`, `docs/design-*.md`): those are reused across every future issue, so full/careful speccing is correctly amortized.

**Spec-lever mapping (2026-07-04 lesson):** when writing a binding implementation spec from design tables, walk **every row** and name the **exact spec field** that carries each lever, per targeting shape. A field existing on the runtime type ≠ the authoring type being able to reach it. Watch double-duty fields (`Range` = reach on Entity/Position but AoE radius on Self). Any lever without a named field = a spec gap; fix before creating the issue. (This gap once shipped a wrong PR that had to be sent back.)

### 3.4 Documentation habits

- **Justification lines**: when recording a load-bearing design decision (design-stats.md deliberate absences, brainstorm resolutions, matrix rows), add an explicit italic `*Justification: …*` line where a real rationale exists. A rule without its written why gets eroded later. Don't force it on trivial decisions.
- **`game-design-direction.md` is a scratchpad** for half-baked/open ideas only — never a resolved-decisions log. Resolved → formalise into the GDD and remove; rejected → remove, no trace. The GDD holds resolved decisions exclusively.

### 3.5 Tooling rules (Claude-specific)

- **Godot MCP Pro is the only way to do editor work** (nodes, scenes, particles, animations, materials, shaders). Never raw `.tscn`/`.tres` edits, GDScript workarounds, or C# `_Ready()` construction. After any `.tscn`/`.tres` change, call `reload_project`.
- **If Godot MCP times out or is unresponsive, STOP and tell the user immediately** — no workarounds (they can corrupt scenes). Wait for the connection to be restored.
- **Never run/query the live editor without explicit approval** each time (`play_scene`, screenshots, `get_output_log`, `get_editor_errors`, `get_scene_tree`, etc.). `reload_project` after an edit is the only exception.
- **"Done" = a clean build, nothing more.** Screenshots/playtesting/log inspection are optional extras needing separate approval.
- **UI styling** goes through `assets/ui/game_theme.tres` via MCP theme tools (`set_theme_color`, `set_theme_stylebox`, `set_theme_font_size`, `set_theme_constant`). Procedural C# boxes / custom meshes are a last resort — discuss first.
- **Blender work** always via Blender MCP (`execute_blender_code`), never headless `blender --background --python` for edits. Call `get_scene_info` first to verify connection.
- **Placeholder VFX**: clone `src/vfx/effect1.tscn` as a plain file, rename, wire it in — never author bespoke effects. The user styles real effects later.

### 3.6 Standing behavioural rules from the user

- **Conflicting/ambiguous instructions → STOP and ask.** If any message contains internally contradictory or ambiguous instructions, never silently pick the interpretation that lets you keep acting. Momentum from prior fast "yes, do it" turns is never a reason to guess. (Triggered by "create the issue, don't execute it" → the assistant executed anyway. User generalized the correction to *any* conflict.)
- **No changes without explicit approval.** Propose a plan, discuss, wait for approval, implement — one item at a time when resolving a list. Format the approval prompt in **bold**.
- **Bug fixes are strictly localized** to the reported bug — change only the causing line(s); no refactors/renames/model changes. If a related flaw is spotted, report it and wait.
- **Consistency-check hook**: when `[CONSISTENCY CHECK FAILED]` fires, don't add a pre-tool-call mention — put a single note in the post-read summary naming the specific differing line.

---

## Recent shipped work (context for where things stand)

- **Wave 1 fully shipped** (issues #23–#27 merged). Playtest gate dropped by user decision.
- **Pre-alpha declared**; versioning scheme locked; docs de-versioned (~120 sites, 17 files).
- **Release v0.1.0-prealpha shipped** (issue #32 → PR #33 → develop→main PR #34, admin override, user-authorized). `release-on-main.yml` cut the tagged pre-release automatically.
- **Element wave Phase 2**: Magic→Fire migration (#51), delivery-pool refactor (#49), Fire/Cold/Lightning identities + signature ailments (#52), per-element enemy resistances (#53/#57), crit itemization (#50/#56) — all merged to `develop`.

### Open at snapshot time (2026-07-07)
- **PR #58** → `develop`: auto-close-on-terminal-label workflow + AGENTS.md note. Awaiting peer review (can't self-merge).
- **PR #59** → `main`: cherry-pick of the workflow file only, to activate auto-close immediately. Conflict-free but blocked by `main` branch protection — pending a decision on whether to `--admin` override or get an approval.
