> **SESSION START — BEFORE ANYTHING ELSE:** The `SessionStart` hook automatically checks consistency between this file and `.agents/AGENTS.md`. If an inconsistency is reported in the system-reminder, note it briefly in plain text and continue.

# CLAUDE.md — actionrpgx

## Project Overview

Top-down action RPG in the vein of Diablo and Path of Exile — deliberate, build-driven combat with deep skill and item systems. **Fully craft-driven:** items, skills, and maps are all obtained through crafting. Enemies drop only crafting materials; there are no direct item drops. The crafting system is the progression engine. Godot 4.6, C#, Forward Plus renderer. 3D world with custom voxel-art style characters, perspective camera.

At the start of every session: first run the **Consistency Check** below, then read `docs/index.md` to orient, then read `docs/todo-technical.md` per the **Session-Start Todo Behaviour** section below. Read `docs/technical-tips.md` before any 3D asset, animation, or bone work. Read `docs/color-scheme.md` before any visual, UI, VFX, or material work.

---

## Project Rules

### 0. AI Operational Guidelines

- **Do not apply any changes or edits to files without explicit user approval.**
- Always propose a plan and discuss the issues/changes first.
- When resolving a list of tasks or issues, proceed strictly **one-by-one** (propose/discuss, wait for approval, implement, repeat).
- When waiting for user approval, format the approval prompt in bold: **e.g. "Waiting for your approval to proceed."**

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
- Read `docs/color-scheme.md` before any theme work.
- Procedural or custom solutions (C# mesh construction, hardcoded styleboxes in code) are a **last resort**. Before going that route, stop and discuss with the user to confirm there is no standard Godot themed control that achieves the same result.

### 5. Animation

- **Always use Godot MCP to set up AnimationTree** in the editor (e.g., `create_animation_tree`, `add_state_machine_state`, `add_state_machine_transition`). Never construct AnimationTree nodes, states, or transitions in C# code.
  - AnimationTree built dynamically in C# `_Ready()` silently breaks (`Travel()` fails, `GetCurrentNode()` never updates). Pre-creating the node in the scene via MCP/editor fixes this.
- **C# code limits**: C# code is limited to setting `animTree.AnimPlayer = animTree.GetPathTo(animPlayer)`, setting `animTree.Active = true`, and calling `Travel()` for state changes. Use `GetCurrentNode()` for state checks — no manual bool flags.
- **Loop modes**: Set animation loop modes at runtime in C# (GLB import silently ignores `.import` subresource loop flags). Do not call `AnimationPlayer.Play()` directly when AnimationTree is active.

---

## Session-Start Todo Behaviour

When reading `docs/todo-technical.md` at session start:
1. **Report** any items already ticked off / marked done — list them and flag for cleanup. Do not edit the file.
2. **List** a few open items for situational awareness.
3. **Recommend** one open item to tackle next — only if the recommendation is obvious from the todo text alone, with no code or doc scanning required. If not obvious, skip the recommendation entirely.
4. **No file edits** — cleanup only happens if the user explicitly approves.

---

## Tools (Claude Specific)

- **Godot MCP Pro** is connected — use `mcp__godot-mcp-pro__*` tools to inspect/modify the live editor.
- MCP tools are auto-approved globally.
- Proactively use `play_scene`, `get_game_screenshot`, `get_output_log`, and `get_editor_errors` to verify changes work before reporting done.
- When debugging runtime behaviour (animation, physics, signals, gameplay logic): invoke `/godot-debug` via the Skill tool to read the log file before drawing conclusions — do not guess from code alone.
- **Godot MCP Pro is the only way to do editor work.** Any task that involves creating or modifying nodes, scenes, particles, animations, materials, shaders, or any other editor resource must be done via MCP tools — never via GDScript workarounds, never by writing raw `.tscn`/`.tres` file content, never by constructing editor objects in C# `_Ready()`. If an MCP tool exists for the task, use it. If one does not exist — or if it times out or is unresponsive — stop and discuss with the user before trying another approach.
- **After every `.tscn` or `.tres` file change, call `mcp__godot-mcp-pro__reload_project` immediately** so the editor picks up the change without prompting the user to reload manually.

---

## Blender Work (Claude Specific)

- **Always use Blender MCP** (`mcp__blender__execute_blender_code`) for any editing of Blender files — modifying animations, keyframes, meshes, or exporting GLBs. Never use headless Python scripts (`blender --background --python`) to edit or save blend files.
- Running a Python script solely to open a `.blend` file is fine as a loading step. The rule is about edits.
- **Before executing any Blender MCP code, call `get_scene_info` as the very first action to verify connection.** If it fails or returns a default scene, stop immediately and tell the user to get Blender MCP working before writing any code. Do not fall back to headless scripts.

---

## Consistency Check

On session start, run the consistency script to check for differences and update the HUD:
```powershell
pwsh -File C:/work/my/github/actionrpgx/.claude/check_consistency.ps1
```

If any inconsistency is found, report it before doing anything else using this format:

\e[31m[INCONSISTENCY] Section: [section name] — [describe the difference].\e[0m

Do not proceed with any session-start tasks until the user has acknowledged the inconsistency.

**Maintenance note:** Any changes to the **Project Rules** and **Session-Start Todo Behaviour** sections must use normalised formatting — backticks for file paths, no `file:///` links — so the text comparison remains mechanical.
