# Project Rules — actionrpgx

## Project Overview

Top-down action RPG in the vein of Diablo and Path of Exile — deliberate, build-driven combat with deep skill and item systems. **Fully craft-driven:** items, skills, and maps are all obtained through crafting. Enemies drop only crafting materials; there are no direct item drops. The crafting system is the progression engine. Godot 4.6, C#, Forward Plus renderer. 3D world with custom voxel-art style characters, perspective camera.

At the start of every session: read [docs/index.md](file:///C:/work/my/github/actionrpgx/docs/index.md) to orient. Read [docs/technical-tips.md](file:///C:/work/my/github/actionrpgx/docs/technical-tips.md) before any 3D asset, animation, or bone work. Read [docs/visuals-style.md](file:///C:/work/my/github/actionrpgx/docs/visuals-style.md) before any visual, UI, VFX, or material work. Read [docs/technical-coding.md](file:///C:/work/my/github/actionrpgx/docs/technical-coding.md) before any registry/gameplay-logic work (items, recipes, skills, augments). Check `.agents/skills/` for available project skills (e.g. `worker-issues`) and follow their instructions when the matching task comes up, even if your tool's own skill-loading mechanism doesn't auto-register them.

---

## Project Rules

### 0. AI Operational Guidelines

- **Do not apply any changes or edits to files without explicit user approval.**
- Always propose a plan and discuss the issues/changes first.
- When resolving a list of tasks or issues, proceed strictly **one-by-one** (propose/discuss, wait for approval, implement, repeat).
- When waiting for user approval, format the approval prompt in bold: **e.g. "Waiting for your approval to proceed."**
- **If a user message contains conflicting or ambiguous instructions, stop and ask for clarification — never silently resolve it in whichever direction lets you proceed.** This applies even mid-session, even after a run of fast "yes, do it" exchanges — momentum from prior turns is never a reason to guess instead of asking.

### 1. Scope Rules

- **Bug fixes are strictly localised to the reported bug.** Change only the line(s) directly causing the issue. Do not refactor, rename, restructure, or clean up anything else — not in the same file, not in related files.
- **If a related flaw or issue is spotted while investigating, stop, report it to the user, and wait for instruction before touching it.**
- **Do not change data models, method signatures, or architecture as part of a bug fix unless explicitly asked.**

### 2. Character Handedness

The player character is **right-handed** — weapons attach to **`Hand_R`** in code.

- **Why**: `stickman.glb` uses standard Mixamo bone naming from the character's own perspective.
  - `Hand_R` = character's anatomical right = visual right hand (screen-right from top-down camera).
  - `Hand_L` = character's anatomical left = visual left.
  - Confirmed at runtime: `Hand_L` produced a weapon on the left side of screen; `Hand_R` is correct.
  - *Note: the old `player_character.glb` model had the opposite convention due to an unusual Blender orientation and is now retired.*
- **Weapon attachment is per-weapon-type**:
  - Sword, wand → `Hand_R` (right hand; right-arm swing animation `melee_right_atack`)
  - Bow → `Hand_L` (left hand holds the bow; left-arm sweep animation `melee_left_atack`)
  - Animation dominant arm: `Hand_R` channels (melee swing)

### 3. Blender Bone Rotation Direction (player.blend)

The character faces **-Y** in Blender (standard convention). Bone local X rotation direction depends on which way the bone points at rest — **downward and upward bones are opposite**:

| Bone group | Examples | Negative X | Positive X |
|---|---|---|---|
| Downward (-Z) | UpperLeg, LowerLeg, Foot, UpperArm, LowerArm, Hand | **Forward** (-Y world) | Backward |
| Upward (+Z) | Hips, Spine, Chest, Neck, Head | Backward | **Forward** |

- **Empirical tail-position test**: Before keying any new bone type, apply a +35° X test rotation in Blender, read `bone.tail` world Y, then undo. If Y went positive → positive X is backward → use negative X for forward motion. Verify first.
- **Review**: Do not export to Godot until the user has reviewed and approved the animation in Blender.

### 4. UI Theming

All UI styling goes through `assets/ui/game_theme.tres`.
- Use MCP theme tools (`set_theme_color`, `set_theme_stylebox`, `set_theme_font_size`, `set_theme_constant`) to modify it.
- Read `docs/visuals-style.md` before any theme work.
- Procedural or custom solutions (C# mesh construction, hardcoded styleboxes in code) are a **last resort**. Before going that route, stop and discuss with the user to confirm there is no standard Godot themed control that achieves the same result.

### 5. Animation

- **Always use Godot MCP to set up AnimationTree** in the editor (e.g., `create_animation_tree`, `add_state_machine_state`, `add_state_machine_transition`). Never construct AnimationTree nodes, states, or transitions in C# code.
  - AnimationTree built dynamically in C# `_Ready()` silently breaks (`Travel()` fails, `GetCurrentNode()` never updates). Pre-creating the node in the scene via MCP/editor fixes this.
- **C# code limits**: C# code is limited to setting `animTree.AnimPlayer = animTree.GetPathTo(animPlayer)`, setting `animTree.Active = true`, and calling `Travel()` for state changes. Use `GetCurrentNode()` for state checks — no manual bool flags.
- **Loop modes**: Set animation loop modes at runtime in C# (GLB import silently ignores `.import` subresource loop flags). Do not call `AnimationPlayer.Play()` directly when AnimationTree is active.

### 6. Multi-Agent Issue Workflow

- Feature/bug work is designed collaboratively and written up as a GitHub issue before implementation begins. The user routes issues to agents directly (separate clones per agent) — issue labels describe scope, they don't need to be self-detected.
- **Hermes only works `scope-logic` issues** (pure C#/logic pluggable into existing scene nodes/signals: combat math, registries, crafting rules, save/load). Anything touching `.tscn`/`.tres`, nodes, AnimationTree, UI theming, or Blender/animation (Rules 3–5) is out of scope for Hermes — if a `scope-logic` issue turns out to need this, stop and flag it rather than attempting a workaround.
- `scope-split` issues get the editor/scene wiring done first, with a separate follow-up `scope-logic` issue opened for Hermes.
- **Labels** (GitHub, issues only — not PRs): Stage — `submitted` → `ready` → `in-progress` → `needs-review`, with `blocked` stackable on any stage (note the blocking issue number in the body). If review sends the work back for rework, swap `needs-review` → `changes-requested` (distinct from `ready` — it's rework, not a fresh pickup) until an agent claims it again. Scope — `scope-logic` / `scope-editor` / `scope-split`. There is no Implementer/role label — who claims, implements, or reviews any issue is entirely the user's call, not something the label scheme enforces. `scope-logic`/`scope-editor` describe what the *work* requires (e.g. whether Godot MCP editor access is needed), not who's allowed to pick it up; if the user routes an editor-requiring issue to an agent without MCP access, that's on the user, not a guard the labels are meant to catch.
- **Terminal labels — `finished` / `rejected`**: `finished` (PR merged) or `rejected` (solution or issue itself was dumped — pure rework needed, or the issue was ill-defined and should be replaced by a new one; distinct from `changes-requested`, which is incremental) can apply from *any* stage, not just at the end of the normal flow — an issue can be `rejected` the moment it's created (ill-defined), or `finished` immediately (it was already solved elsewhere). Whenever either applies, the agent removes all other labels and adds exactly one of the two. No precedence needed among the active stage labels if they're ever mistakenly stacked — that's just a sign the handoff was done wrong, go check the issue history. The one exception: if `finished` and `rejected` both end up on the same issue (a labeling mistake, since merged code can't retroactively be dumped), treat it as `rejected`. The kanban board's view is filtered to exclude both terminal labels (`-label:finished -label:rejected`), so adding either instantly drops the card off the board — no Status field to sync, no automation needed.
- **Claiming an issue is mandatory and comes first** — before creating a branch, before reading code, before anything else. Since all agents share the one human GitHub account, ownership is transparent, not enforced by GitHub, so this step is the only signal the user gets that work has started: swap `ready` (or `changes-requested`) → `in-progress` on the issue and post a one-line comment naming itself (e.g. "Claude starting work"). This also keeps pickup lists like `worker-issues` from surfacing an issue another agent already claimed. An agent that creates a branch or touches code without having done this first is out of process — flag it.
- **Branching (git flow)**: only after claiming, branch off `develop` as `feature/issue-<number>-<slug>` (slug = short kebab-case of the issue title), e.g. `feature/issue-1-boots-equipment-slot`. PRs target `develop`, not `main`.
- **Label handoffs are the agent's job, not the user's.** Stage labels are mutually exclusive — always swap, never stack. There's no GitHub Action enforcing this; each agent runs the `gh issue edit` swap itself at every handoff point so the user never has to touch labels manually:
  - Claim (start work): the implementer (whoever picks up the issue) swaps `ready` / `changes-requested` → `in-progress`.
  - Commit & push for PR (finish work): the PR creator (same implementer) swaps `in-progress` → `needs-review`.
  - Review sends work back: the reviewer swaps `needs-review` → `changes-requested`.
  - Review merges the PR: whoever merges (the assigned reviewer, or the user) removes all stage labels and adds `finished`.
  - Review rejects the solution/issue outright: whoever rejects removes all stage labels and adds `rejected`.
- **Issue-writing discipline: keep issues lean.** Reference existing patterns instead of deriving the full solution; give acceptance criteria and touch points (which files/registries need entries), not literal code or exhaustive test cases. A spec detailed enough to fully constrain the implementation should just be implemented directly instead. This applies to pre-handoff verification too — check enough to catch an obvious scope-blocker (e.g. a hardcoded list that would make "pure logic" actually need scene work), not an exhaustive audit of every reference to the thing being changed. **This "keep it lean" discipline applies to issues only** — reference docs (`docs/technical-*.md`) are the opposite case and should be specced out properly and completely, since they're reused across every future issue rather than spent once.
- **PR review**: the user assigns a reviewer per PR (no fixed rule) — the assigned reviewer checks scope adherence, matched intent against the issue, and that any tests are meaningful, not just green CI. Whoever completes the review (merge or reject) is also responsible for the resulting terminal label swap — see above.

---

## Tools and MCP Integration (Antigravity Specific)

- **Godot MCP Pro** is connected — use the `call_mcp_tool` tool with `ServerName: "godot-mcp-pro"` to inspect/modify the live editor.
- Proactively use `play_scene`, `get_game_screenshot`, `get_output_log`, and `get_editor_errors` via MCP to verify changes work before reporting done.
- When debugging runtime behaviour (animation, physics, signals, gameplay logic): read the log file before drawing conclusions — do not guess from code alone.
- **Godot MCP Pro is the only way to do editor work.** Any task that involves creating or modifying nodes, scenes, particles, animations, materials, shaders, or any other editor resource must be done via MCP tools — never via GDScript workarounds, never by writing raw `.tscn`/`.tres` file content, never by constructing editor objects in C# `_Ready()`. If an MCP tool exists for the task, use it. If one does not exist — or if it times out or is unresponsive — stop and discuss with the user before trying another approach.
- **After every `.tscn` or `.tres` file change, call `reload_project` via `godot-mcp-pro` immediately** so the editor picks up the change without prompting the user to reload manually.

---

## Blender Work (Antigravity Specific)

- **Always use Blender MCP** (`ServerName: "blender"`, tool `execute_blender_code`) via `call_mcp_tool` for any editing of Blender files — modifying animations, keyframes, meshes, or exporting GLBs. Never use headless Python scripts (`blender --background --python`) to edit or save blend files.
- Running a Python script solely to open a `.blend` file is fine as a loading step. The rule is about edits.
- **Before executing any Blender MCP code, call `get_scene_info` as the very first action to verify connection.** If it fails or returns a default scene, stop immediately and tell the user to get Blender MCP working before writing any code. Do not fall back to headless scripts.

---

## Code Search and Traversal Preferences

- **Always prefer using `ast-grep` (`sg`)** over standard `ripgrep` (`rg`) or the built-in `grep_search` tool when searching, traversing, or analyzing codebase source code.
- You do not need to wait for explicit user instructions to use `ast-grep` when performing code-related work; utilize it automatically as your default tool for code analysis.
- For all non-code searches (e.g., scanning documentation, markdown guides, log files, or raw text configuration files), you may fall back to standard `ripgrep` (`rg` or `grep_search`).
