# Tech To-Do

Code, systems, UI, and art tasks only. Design decisions live in the GDD files.

---
c2go

## Tech Spec Gaps — GDD designed, no technical spec written yet

Items where the GDD defines the design but no corresponding technical specification exists in the tech docs. Write the spec before implementing.

### UI

> **(v1 — postponed)**

- [ ] Spec Run Results Overlay content — what data is displayed (XP gained, level reached, materials earned, coins, win/lose state) and how it is populated from RunSession

---

## Systems / Features

- [x] Spec and implement re-roll for v1 (all random).

> **Not in scope.**

- [ ] Map selection screen — currently one hardcoded arena; no selection or variety
- [ ] Boss mechanic — run win condition triggers on timer expiry; boss is unimplemented

---

## Art / Assets

> **Not in scope.**

- [ ] Hollow Dark Forest assets — floor tile, tree trunk wall, wall corner (Blender); replace placeholder box geometry in DungeonGenerator
- [ ] **Roll animation** *(v2+)* — No roll clip exists in the player GLB; characters slide while dodging. Needs rig animation in Blender.
