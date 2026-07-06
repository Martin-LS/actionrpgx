# Docs Index

Quick reference for every doc in this folder. Load this first each session to know what to read and when.

---

## Project Status

- **Phase: pre-alpha** — core systems are still being designed and built (skill roster, element wave, enemy variety, economy). Declared exit criterion for **alpha**: feature-complete — every major system exists in some form; work shifts from "build systems" to "fill and fix." Phase transitions are declared decisions, recorded here.
- **Version: `0.1.0-prealpha`** — mirrors `config/version` in `project.godot` (the source of truth). SemVer `0.MINOR.PATCH`: MINOR bumps when a milestone ships to `main` (tagged `v<version>`), PATCH for fix-only releases. `1.0.0` = release. The phase rides as the pre-release suffix; it is not encoded in the numbers.
- **Active dev vocabulary** — all current wave/direction labels target pre-alpha: **wave 1** (composition model — shipped), the **element wave** (next major skill wave), and the **D1–D4 directions**. Docs never state the version number; they describe current truth in plain terms (no v1/v2 scope tags).

---

## Session Start — Always Read

| File | When to read |
|------|--------------|
| [technical-todo.md](technical-todo.md) | Every session — check what's pending, tick off anything completed |

---

## Rules & Style — Read Before Relevant Work

| File | When to read |
|------|--------------|
| [visuals-style.md](visuals-style.md) | Before any visual, UI, VFX, material, or lighting work — full color palette with hex values |
| [technical-tips.md](technical-tips.md) | Before any 3D asset, animation, or bone work — hard-won Blender→Godot lessons |

---

## Game Design (GDD)

| File | Contents |
|------|----------|
| [design-mechanics.md](design-mechanics.md) | Core loop, combat, archetypes, enemies, focus system, damage types, run structure, hit feedback |
| [design-stats.md](design-stats.md) | **Authoritative** stats doc — six-surface model, Active-surface catalogues (character/skill/item), Stat Ownership Matrix (governing); other docs defer here |
| [design-stats-deferred.md](design-stats-deferred.md) | Cold companion to design-stats.md — Deferred/Parked stat surfaces (enemy, map, hit model, reward, speed family, niche list); load only for push-back substance or surface promotion |
| [design-skills.md](design-skills.md) | Skill design rules, all 11 skill prototypes, AoE and targeting rules, delivery types |
| [design-augments.md](design-augments.md) | Skill Augments and Equipment Augments — current augment list, trigger system, resolution order, prototypes |
| [design-progression.md](design-progression.md) | Meta-progression, gear slots (Weapon/Hat/Body/Ring/Skills), item tiers, currencies, inventory tabs |
| [design-map.md](design-map.md) | Map design — biomes, chunk types, procedural assembly, map attributes |
| [design-ui.md](design-ui.md) | Screen layouts and HUD — screen hierarchy, Loadout tab layout, HUD elements, pause menu |
| [design-ui-mechanics.md](design-ui-mechanics.md) | Interaction model — all click flows for Skills, Augments, and Equipment tabs/slots/modify panels |

---

## Technical

| File | Contents |
|------|----------|
| [technical-scene.md](technical-scene.md) | Scene architecture, signals, C# conventions, rendering setup, save layers, camera |
| [technical-systems.md](technical-systems.md) | Data types (`GearItemInstance`, `SkillInstance`, etc.), save format, crafting pipeline, combat system internals |
| [technical-map.md](technical-map.md) | Map generation runtime — `DungeonGenerator`, `MapData`, chunk placement algorithm, seed reproducibility |
| [technical-assets.md](technical-assets.md) | 3D asset pipeline — visual style spec, model authoring rules, chamfer convention, bone/rig requirements |

---

## Scratchpad & Reference

| File | Contents |
|------|----------|
| [design-skill-system-brainstorming.md](design-skill-system-brainstorming.md) | OPEN brainstorm for the skill system's open questions — roster expansion, element wave, augment candidates; resolved items move to design-skills.md |
| [design-directions.md](design-directions.md) | Parked design discussions — open but unresolved ideas; resolved items are removed and moved to design docs |
| [design-ideas.md](design-ideas.md) | Uncommitted design concepts parking lot — nothing here is scheduled or spec'd |
| [ref-arpg-skills.md](ref-arpg-skills.md) | Cross-game ARPG skill reference — PoE2 (EA 0.5.0) + D4 (Season 14) + The Last Epoch (2024) for prototype extraction; design inspiration only, adapt concepts not names |
| [ref-arpg-damage-types.md](ref-arpg-damage-types.md) | Cross-game damage-type & ailment reference (D4/PoE2/LE/Grim Dawn/No Rest for the Wicked) — sourcing for the element wave (roster, per-type ailments, resist model); design inspiration only |
