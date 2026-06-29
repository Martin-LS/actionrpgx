# Voxel Character Spec

Design spec for all humanoid characters built using the voxel construction approach.

---

## Core Principles

- **Construction:** rectangular boxes only, positioned on the voxel grid. No curves, no bevels, no smooth shading. Perceived curves are achieved by stair-stepping boxes (voxelated staircase approximation).
- **Surface:** vertex colors per face during blockout. Final characters use a shared 64×64 texture (Minecraft-style UV layout) with pixel art detail on key faces.
- **Subdivision:** none — geometry stays low poly. The voxel appearance comes from the grid proportions and surface treatment, not from subdividing parts into individual unit cubes.

---

## Voxel Unit

`1 voxel = 1/15 m ≈ 0.0667 m`

This unit was derived from the existing `player.blend` model — the head (0.6667 m) divides cleanly to 10 voxels, and all major body parts land on integer or half-integer voxel counts at this scale.

---

## Base Mesh — Part Dimensions

All dimensions in voxels. Total height: **36 voxels = 2.4 m** (T-pose including neck).

| Part | Voxels W × D × H | Metres W × D × H |
|---|---|---|
| Head | 6 × 5 × 6 | 0.400 × 0.333 × 0.400 |
| Neck | 4 × 4 × 2 | 0.267 × 0.267 × 0.133 |
| Torso | 10 × 5 × 11 | 0.667 × 0.333 × 0.733 |
| Hips | 7 × 4 × 4 | 0.467 × 0.267 × 0.267 |
| Upper Arm (×2) | 6 × 3 × 3 | 0.400 × 0.200 × 0.200 |
| Lower Arm (×2) | 5 × 2 × 2 | 0.333 × 0.133 × 0.133 |
| Hand (×2) | 2 × 4 × 3 | 0.133 × 0.267 × 0.200 |
| Upper Leg (×2) | 4 × 4 × 6 | 0.267 × 0.267 × 0.400 |
| Lower Leg (×2) | 2 × 2 × 5 | 0.133 × 0.133 × 0.333 |
| Foot (×2) | 4 × 5 × 2 | 0.267 × 0.333 × 0.133 |

**Height stack (bottom → top):** Foot(2) + LowerLeg(5) + UpperLeg(6) + Hips(4) + Torso(11) + Neck(2) + Head(6) = **36 voxels**

**T-pose arm span:** 2 × (Torso half-width 5 + UpperArm 6 + LowerArm 5 + Hand 2) = **36 voxels = 2.4 m**

---

## Armour Layer (planned — head and body only)

Armour is built as a separate layer of boxes placed on top of the base mesh, on the same voxel grid. The base body is never hidden — armour pieces are always additive.

### Head armour

Extra boxes overlaid on the head cube:
- **Visor slab** — a flat 1-voxel-deep plate covering the front face of the head (e.g. 6 × 1 × 3, centred on the eye region)
- **Ear guards** — small side plates (e.g. 1 × 2 × 4) on the left and right faces
- **Crest / ridge** — a thin strip along the top (e.g. 2 × 6 × 1 or stepped)

### Body armour

Extra boxes overlaid on the torso:
- **Chest plate** — a 1-voxel-thick slab on the front face of the torso (e.g. 8 × 1 × 9)
- **Shoulder pauldrons** — boxes sitting on top of the upper arms, wider than the arm cross-section (e.g. 5 × 4 × 2 per side)
- **Belt** — a thin horizontal band at the hip join (e.g. 7 × 4 × 1)

### Design constraint

Keep the base torso depth (5 voxels = 0.333 m) shallow enough that a 1-voxel chest plate overlay does not make the character look implausibly thick. Pauldrons should overhang the arm slightly (wider than the 3-voxel arm depth) to read as separate pieces.

### Workflow

Armour pieces are separate named objects in the `.blend` file, parented to the same bones as the body parts they overlay (`Head`, `Chest`). They are merged into the same per-bone export object at the pre-export stage.

---

## Texture Spec

### Format

- **Size:** 64×64 PNG
- **Layout:** Minecraft-style — each body part gets a rectangular region; each face of that part maps to its own sub-region within it
- **Single shared texture** per character — one material, one draw call
- **No filtering** — import with nearest-neighbour (point) sampling in Godot so pixels stay crisp

### Face detail priority

| Face | Detail |
|---|---|
| Head front | Eyes (2 small pixel squares), nose (1 pixel), hairline (top edge pixels) |
| Head top | Flat hair color — no detail |
| Head back / sides | Flat skin/hair color — no detail |
| Torso front | Subtle chest marking (1–2 shades, simple shape) |
| Torso top | Flat cloth color — visible from camera but no detail needed |
| All limb faces | Flat color only — too small and in motion to read detail |

### Color palette (dark fantasy, muted tones)

| Zone | Color description | Hex (approx) |
|---|---|---|
| Skin | Desaturated warm tan — slightly ashen | `#c4a882` |
| Hair | Dark brown, near-black | `#2e1f0f` |
| Eyes | Pale grey or dull amber | `#9e8b6a` |
| Tunic / torso | Deep muted burgundy | `#5c2a2a` |
| Chest marking | One shade lighter than tunic — subtle | `#7a3d3d` |
| Trousers | Dark charcoal | `#2a2a30` |
| Boots | Near-black dark brown | `#1a1208` |
| Neck / hands | Matches skin | `#c4a882` |

---

## Blender Construction Notes

- **Grid snap:** set Blender's snap increment to `1/15 m = 0.0667 m` so all placements stay on the voxel grid
- **One object per body part** during authoring — easier to reposition and weight-paint
- **Join at export time** (see `technical-assets.md` — Pre-Export Merge section)
- **Flat shade** all parts before export
- **No bevel modifier, no subdivision modifier** — ever
