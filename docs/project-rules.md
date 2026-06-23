# Project Rules & Guidelines

This document contains project-wide guidelines for coding, game mechanics conventions, animation rigs, and tool usage. All AI assistants must read and follow these rules.

---

## 1. Scope Rules

* **Bug fixes are strictly localised to the reported bug.** Change only the line(s) directly causing the issue. Do not refactor, rename, restructure, or clean up anything else — not in the same file, not in related files.
* **If a related flaw or issue is spotted while investigating, stop, report it to the user, and wait for instruction before touching it.**
* **Do not change data models, method signatures, or architecture as part of a bug fix unless explicitly asked.**

---

## 2. Character Handedness

The player character is **right-handed** — weapons attach to **`Hand_R`** in code.

* **Why**: `stickman.glb` uses standard Mixamo bone naming from the character's own perspective. 
  * `Hand_R` = character's anatomical right = visual right hand (screen-right from top-down camera).
  * `Hand_L` = character's anatomical left = visual left.
  * Confirmed at runtime: `Hand_L` produced a weapon on the left side of screen; `Hand_R` is correct.
  * *Note: the old `player_character.glb` model had the opposite convention due to an unusual Blender orientation and is now retired.*

* **Weapon attachment is per-weapon-type**:
  * Sword, wand → `Hand_R` (right hand; right-arm swing animation `melee_right_atack`)
  * Bow → `Hand_L` (left hand holds the bow; left-arm sweep animation `melee_left_atack`)
  * Animation dominant arm: `Hand_R` channels (melee swing)

---

## 3. Blender Bone Rotation Direction (player.blend)

The character faces **-Y** in Blender (standard convention). Bone local X rotation direction depends on which way the bone points at rest — **downward and upward bones are opposite**:

| Bone group | Examples | Negative X | Positive X |
|---|---|---|---|
| Downward (-Z) | UpperLeg, LowerLeg, Foot, UpperArm, LowerArm, Hand | **Forward** (-Y world) | Backward |
| Upward (+Z) | Hips, Spine, Chest, Neck, Head | Backward | **Forward** |

* **empirical tail-position test**: Before keying any new bone type, apply a +35° X test rotation in Blender, read `bone.tail` world Y, then undo. If Y went positive → positive X is backward → use negative X for forward motion. Verify first.
* **Review**: Do not export to Godot until the user has reviewed and approved the animation in Blender.

---

## 4. UI Theming

All UI styling goes through `assets/ui/game_theme.tres`. 
* Use MCP theme tools (`set_theme_color`, `set_theme_stylebox`, `set_theme_font_size`, `set_theme_constant`) to modify it. 
* Read `docs/color-scheme.md` before any theme work.
* Procedural or custom solutions (C# mesh construction, hardcoded styleboxes in code) are a **last resort**. Before going that route, stop and discuss with the user to confirm there is no standard Godot themed control that achieves the same result.

---

## 5. Animation

* **Always use Godot MCP to set up AnimationTree** in the editor (e.g., `create_animation_tree`, `add_state_machine_state`, and `add_state_machine_transition`). Never construct AnimationTree nodes, states, or transitions in C# code.
  * AnimationTree built dynamically in C# `_Ready()` silently breaks (`Travel()` fails, `GetCurrentNode()` never updates). Pre-creating the node in the scene via MCP/editor fixes this.
* **C# code limits**: C# code is limited to setting `animTree.AnimPlayer = animTree.GetPathTo(animPlayer)`, setting `animTree.Active = true`, and calling `Travel()` for state changes. Use `GetCurrentNode()` for state checks — no manual bool flags.
* **Loop modes**: Set animation loop modes at runtime in C# (GLB import silently ignores `.import` subresource loop flags). Do not call `AnimationPlayer.Play()` directly when AnimationTree is active.
