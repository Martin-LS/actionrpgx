# Project Rules — actionrpgx

## Project Overview

Top-down action RPG in the vein of Diablo and Path of Exile — deliberate, build-driven combat with deep skill and item systems. **Fully craft-driven:** items, skills, and maps are all obtained through crafting. Enemies drop only crafting materials; there are no direct item drops. The crafting system is the progression engine. Godot 4.6, C#, Forward Plus renderer. 3D world with custom voxel-art style characters, perspective camera.

At the start of every session: read [docs/index.md](file:///C:/work/my/github/actionrpgx/docs/index.md) to orient, read [docs/todo-technical.md](file:///C:/work/my/github/actionrpgx/docs/todo-technical.md) and tick off anything completed. Read [docs/project-rules.md](file:///C:/work/my/github/actionrpgx/docs/project-rules.md) before any design or architectural decision or bug fix. Read [docs/technical-tips.md](file:///C:/work/my/github/actionrpgx/docs/technical-tips.md) before any 3D asset, animation, or bone work. Read [docs/color-scheme.md](file:///C:/work/my/github/actionrpgx/docs/color-scheme.md) before any visual, UI, VFX, or material work.

## Tools and MCP Integration (Antigravity Specific)

- **Godot MCP Pro** is connected — use the `call_mcp_tool` tool with `ServerName: "godot-mcp-pro"` to inspect/modify the live editor.
- Proactively use `play_scene`, `get_game_screenshot`, `get_output_log`, and `get_editor_errors` via MCP to verify changes work before reporting done.
- When debugging runtime behaviour (animation, physics, signals, gameplay logic): read the log file before drawing conclusions — do not guess from code alone.
- **Godot MCP Pro is the only way to do editor work.** Any task that involves creating or modifying nodes, scenes, particles, animations, materials, shaders, or any other editor resource must be done via MCP tools — never via GDScript workarounds, never by writing raw `.tscn`/`.tres` file content, never by constructing editor objects in C# `_Ready()`. If an MCP tool exists for the task, use it. If one does not exist — or if it times out or is unresponsive — stop and discuss with the user before trying another approach.
- **After every `.tscn` or `.tres` file change, call `reload_project` via `godot-mcp-pro` immediately** so the editor picks up the change without prompting the user to reload manually.

## Blender Work (Antigravity Specific)

- **Always use Blender MCP** (`ServerName: "blender"`, tool `execute_blender_code`) via `call_mcp_tool` for any editing of Blender files — modifying animations, keyframes, meshes, or exporting GLBs. Never use headless Python scripts (`blender --background --python`) to edit or save blend files.
- Running a Python script solely to open a `.blend` file is fine as a loading step. The rule is about edits.
- **Before executing any Blender MCP code, call `get_scene_info` as the very first action to verify connection.** If it fails or returns a default scene, stop immediately and tell the user to get Blender MCP working before writing any code. Do not fall back to headless scripts.

## Code Search and Traversal Preferences

- **Always prefer using `ast-grep` (`sg`)** over standard `ripgrep` (`rg`) or the built-in `grep_search` tool when searching, traversing, or analyzing codebase source code.
- You do not need to wait for explicit user instructions to use `ast-grep` when performing code-related work; utilize it automatically as your default tool for code analysis.
- For all non-code searches (e.g., scanning documentation, markdown guides, log files, or raw text configuration files), you may fall back to standard `ripgrep` (`rg` or `grep_search`).

## AI Operational Guidelines

- **Do not apply any changes or edits to files without explicit user approval.**
- Always propose a plan and discuss the issues/changes first.
- When resolving a list of tasks or issues, proceed strictly **one-by-one** (propose/discuss, wait for approval, implement, repeat).
