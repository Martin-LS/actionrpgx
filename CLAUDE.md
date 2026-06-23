# CLAUDE.md — actionrpgx

## Project Overview

Top-down action RPG in the vein of Diablo and Path of Exile — deliberate, build-driven combat with deep skill and item systems. **Fully craft-driven:** items, skills, and maps are all obtained through crafting. Enemies drop only crafting materials; there are no direct item drops. The crafting system is the progression engine. Godot 4.6, C#, Forward Plus renderer. 3D world with custom voxel-art style characters, perspective camera.

At the start of every session: read `docs/index.md` to orient, read `docs/todo-tech.md` and tick off anything completed. Read `docs/project-rules.md` before any design or architectural decision or bug fix. Read `docs/tech-tips.md` before any 3D asset, animation, or bone work. Read `docs/color-scheme.md` before any visual, UI, VFX, or material work.

## Tools (Claude Specific)

- **Godot MCP Pro** is connected — use `mcp__godot-mcp-pro__*` tools to inspect/modify the live editor.
- MCP tools are auto-approved globally.
- Proactively use `play_scene`, `get_game_screenshot`, `get_output_log`, and `get_editor_errors` to verify changes work before reporting done.
- When debugging runtime behaviour (animation, physics, signals, gameplay logic): invoke `/godot-debug` via the Skill tool to read the log file before drawing conclusions — do not guess from code alone.
- **Godot MCP Pro is the only way to do editor work.** Any task that involves creating or modifying nodes, scenes, particles, animations, materials, shaders, or any other editor resource must be done via MCP tools — never via GDScript workarounds, never by writing raw `.tscn`/`.tres` file content, never by constructing editor objects in C# `_Ready()`. If an MCP tool exists for the task, use it. If one does not exist — or if it times out or is unresponsive — stop and discuss with the user before trying another approach.
- **After every `.tscn` or `.tres` file change, call `mcp__godot-mcp-pro__reload_project` immediately** so the editor picks up the change without prompting the user to reload manually.

## Blender Work (Claude Specific)

- **Always use Blender MCP** (`mcp__blender__execute_blender_code`) for any editing of Blender files — modifying animations, keyframes, meshes, or exporting GLBs. Never use headless Python scripts (`blender --background --python`) to edit or save blend files.
- Running a Python script solely to open a `.blend` file is fine as a loading step. The rule is about edits.
- **Before executing any Blender MCP code, call `get_scene_info` as the very first action to verify connection.** If it fails or returns a default scene, stop immediately and tell the user to get Blender MCP working before writing any code. Do not fall back to headless scripts.
