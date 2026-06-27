# Technical Design Document — 3D Asset Pipeline

> **Build pipeline** — the how-to reference for authoring, rigging, animating, and exporting all 3D assets. Covers Blender workflow, export settings, rig spec, animation clips, and asset libraries.
> For the visual design spec (construction rules, colors, geometry, VFX style) see `docs/visuals-style.md`.

---

## Building a New Asset — Checklist

Follow this for every new asset regardless of type (character, enemy, prop, tree, rock, building). Visual rules (game voxel size, silhouette rules) are in `docs/visuals-style.md`.

1. **Build with 0.025 m game voxels** — place individual voxels to fill each body region. No other primitive types, no bevels, no smooth surfaces. Each distinct bone region + material zone is its own named Blender object. Follow the naming convention: `{bone_lowercase}_{material_zone}` — e.g. `head_skin`, `head_hair`, `chest_cloth`, `foot_r_boot`.
2. **Assign one flat-colour material per voxel group object** — pick from `docs/visuals-style.md`. No textures, no gradients, no vertex colours.
3. **Shade flat** — select all objects, right-click → Shade Flat. Confirm `shade_smooth` is off.
4. **Check silhouette from top-down** — the game camera is elevated and angled (~45° pitch). Confirm the asset reads clearly as a silhouette. Most visible faces are tops (head top, shoulder tops, roof).
5. **Run the pre-export merge** — see Pre-Export Merge section below. Non-destructive: the source `.blend` is unchanged.
6. **Export** — follow the Export Settings section below.

**Player `.blend` is the visual reference.** When in doubt, open `player.blend` and match what you see.

---

## Colour Palette Per Character

Each character has its own palette. Record it here when authoring so future modifications match exactly.

| Character | Skin | Hair | Top | Bottom | Shoes |
|---|---|---|---|---|---|
| Player | `#f5c9a0` | `#d4a96a` | `#a8d8ea` (light blue) | `#d4e8c2` (light green) | `#f4a261` (warm orange) |
| Enemy (generic) | `#c8b8a2` | `#8b7355` | `#e8c4c4` (light red) | `#c4c4e8` (light purple) | `#b8d4b8` (light grey-green) |
| Enemy (skeleton) | `#e8e0d0` (bone) | — (no hair) | `#c8bfae` (bone dark, ribs/joints) | `#b0a898` (joint accent) | `#c8bfae` (flat foot) |

*(Placeholder palette — light flat colours across the board. Replace with final colour scheme once decided.)*

---

## Rig — Standard Bone Set

> **Base file: `assets/models/characters/player_prototype.blend`** — this is the canonical rigged humanoid source. When creating any new humanoid character (skeleton, zombie, humanoid boss, NPC, etc.), **duplicate `player_prototype.blend`** as the starting point. It already has the correct armature, bone hierarchy, weight painting, and rest pose. Do not build a new rig from scratch.

> **Status: Implemented for `player.glb`.** Enemy models are still unrigged static meshes. The spec below applies to all future humanoid characters.

Every humanoid character uses this exact bone hierarchy and naming. Do not deviate — animation sharing and code lookups depend on consistent names.

```
Root
└── Hips
	├── Spine
	│   ├── Chest
	│   │   ├── Neck
	│   │   │   └── Head
	│   │   ├── UpperArm_L
	│   │   │   └── LowerArm_L
	│   │   │       └── Hand_L
	│   │   └── UpperArm_R
	│   │       └── LowerArm_R
	│   │           └── Hand_R
	├── UpperLeg_L
	│   └── LowerLeg_L
	│       └── Foot_L
	└── UpperLeg_R
		└── LowerLeg_R
			└── Foot_R
```

- `Root` sits at world origin (0,0,0), Y-up
- `Hips` is the physical centre of mass, all locomotion drives from here
- All bones point along their local Y axis
- Weight painting: each body-part mesh is weight-painted 100% to its corresponding bone (no blending — blocky characters have no deformation zones)

---

## Animation Clips

> **Status: Partial — player only.** `run` (looping) and `attack` (one-shot) clips exist in `player.glb`. `AnimationPlayer` is wired in `PlayerController` — run plays while moving, attack triggers on `SkillFired`. `idle`, `walk`, `hit`, `death` are not yet authored. Enemy models have no animations. The table below is the full target spec.

These are the clip names the C# code will reference. Every character must have all clips that apply to their type. Names are case-sensitive.

| Clip | Frames | Loop | Description |
|---|---|---|---|
| `idle` | 0–59 | Yes | Subtle weight shift or gentle bob — can be near-static |
| `walk` | 60–99 | Yes | Arms swing opposite to legs, hips bob slightly |
| `run` | 100–139 | Yes | Faster walk — exaggerate arm/leg swing |
| `attack` | 140–179 | No | Weapon arm swings forward and returns |
| `hit` | 180–199 | No | Brief recoil — character rocks back |
| `death` | 200–239 | No | Falls or collapses — stays down at last frame |

All animations live as named Actions in Blender's NLA editor and are exported into the GLB in one pass.

Frame rate: **30 fps** (set in Blender scene before animating).

---

## Modular Equipment Visuals

The character model is a base body with equipment meshes added on top. Each equipped item maps to one or more meshes parented to the armature. No base body geometry is ever hidden — all equipment pieces are additive.

### Slot → Visual mapping

| Game slot | Visual pieces | Bone attached to |
|---|---|---|
| Armour (Heavy) | Chest plate (wide shoulders) + Full helm block | `Chest`, `Head` |
| Armour (Medium) | Leather vest (slight shoulder pads) + Hood/coif | `Chest`, `Head` |
| Armour (Light) | Thin padded vest + Cloth cap (low profile) | `Chest`, `Head` |
| No armour | Base body only | — |
| Weapon (Sword) | Short rectangular blade mesh | `Hand_R` |
| Weapon (Bow) | Bow frame mesh | `Hand_L` |
| Weapon (Wand) | Thin staff/rod mesh | `Hand_R` |
| Accessory | No visual representation | — |

Armour category drives both chest and hat together — equipping Heavy armour shows the heavy chest piece **and** the heavy helm. They are one asset (one GLB), not two independent pieces.

### Asset files

| Asset | File |
|---|---|
| Heavy armour | `assets/models/equipment/armour_heavy.glb` + `.blend` |
| Medium armour | `assets/models/equipment/armour_medium.glb` + `.blend` |
| Light armour | `assets/models/equipment/armour_light.glb` + `.blend` |
| Sword | `assets/models/equipment/weapon_sword.glb` + `.blend` |
| Bow | `assets/models/equipment/weapon_bow.glb` + `.blend` |
| Wand | `assets/models/equipment/weapon_wand.glb` + `.blend` |

### Runtime behaviour (Godot)

- On equip: instantiate the equipment GLB as a child of the character's skeleton node, attach to the named bone
- On unequip: remove that child node
- Equipment meshes carry no armature of their own — they are static meshes riding the character's bones
- All animations continue unchanged — equipment pieces move with the bones they're attached to

---

## Mixamo Export Pipeline

Use Mixamo to auto-rig a new character and download animation packs. Follow this checklist every time — deviating from it causes mesh errors on upload.

### Step-by-step (via Blender MCP)

1. **Set Blender scene units to Metric / Meters** — Mixamo silently mis-scales characters exported from scenes with any other unit setting. Check `Scene Properties → Units → Unit System = Metric, Length = Meters` before any other step.

2. **Open the source `.blend`** — use the version that still has the armature intact (before rig removal).

3. **Delete hair objects** — `Hair_SideL`, `Hair_SideR`, `Hair_Back`, `Hair_Top` must be removed before merging. Hair geometry extends down to shoulder/neck height and creates stray faces in the FBX between head and torso.

4. **Remove rig and animations**
   - Set armature pose position to `REST`
   - Apply armature modifier on every mesh (bakes rest pose into geometry)
   - Delete the Armature object
   - Clear animation data from all objects + purge orphaned datablocks

5. **Merge all remaining meshes into one** — Mixamo's auto-rigger **requires** a single mesh object. Multiple separate objects cause the rigging phase to fail with no useful error message.

   - **Destructive workflow** (when the source `.blend` is a throwaway or a copy): select all mesh objects, `bpy.ops.object.join()`, rename result `PlayerCharacter`.
   - **Stickman / modular-mesh workflow** (source `.blend` keeps separate pieces): duplicate all mesh objects (`bpy.ops.object.duplicate()`), join the duplicates, remove their armature modifiers, export with `use_selection=True`, then delete the duplicates. The source `.blend` is untouched.

   After joining, also remove any remaining armature modifiers from the joined mesh — they are not needed for the Mixamo upload and can interfere.

   **Every body region must be connected.** If the Head box has a gap to the Torso (no Neck piece), the voxel remesh produces a disconnected head island. Mixamo cannot place the Chin marker and fails with "please place all markers." Add a Neck mesh piece that overlaps slightly into both the Head and Torso so the remesh merges them into one solid.

6. **Clean the mesh** (in this order — do not skip steps or reorder):
   - `remove_doubles` (dist=0.001) — eliminates duplicate verts from the join
   - Delete loose verts and edges
   - Delete interior faces (faces where all edges are shared by >2 faces)
   - `recalc_face_normals` — fix inverted normals

7. **Apply T-pose** — rotate arm vertices to horizontal at shoulder pivots. For `player.blend`:
   - Threshold: `|X| > 0.51` separates arm verts from torso (torso ends at X=±0.5, arms start at X=±0.55)
   - Left shoulder pivot: `(0.620, 0.0, 2.100)`, rotation `Ry(-90°)`
   - Right shoulder pivot: `(-0.620, 0.0, 2.100)`, rotation `Ry(+90°)`
   - Run health check after: must show 0 non-manifold edges before proceeding

8. **Set origin to base of mesh** — `Object → Set Origin → Origin to Geometry` then manually move the origin to foot level (Z=0). Mixamo places the character on the floor using the origin; a mid-body origin pushes the rig markers off and produces a worse auto-rig result.

9. **Apply all transforms** — `bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)`. Scale must be `(1,1,1)` in the exported file.

10. **Export as FBX**:
   ```python
   bpy.ops.export_scene.fbx(
	   filepath=r"<project>/assets/models/characters/<name>_mixamo.fbx",
	   use_selection=True,   # export only the joined mesh, not the whole scene
	   apply_unit_scale=True,
	   apply_scale_options='FBX_SCALE_NONE',
	   bake_space_transform=False,
	   object_types={'MESH'},
	   use_mesh_modifiers=True,
	   add_leaf_bones=False,
	   path_mode='COPY',
	   embed_textures=False,
	   axis_forward='-Z',
	   axis_up='Y',
	   mesh_smooth_type='FACE',
   )
   ```

11. **Verify** — import the FBX back into a fresh Blender scene (enable **Automatic Bone Orientation** in the FBX importer) and confirm: single mesh, T-pose visible from front orthographic, 0 non-manifold edges.

### Output file naming

`<name>_mixamo.fbx` alongside the source `.blend` — e.g. `player_mixamo.fbx`. Do not commit these FBX files to git; they are throwaway upload artefacts.

### After Mixamo

Mixamo returns a rigged FBX (with its own skeleton) plus downloaded animation FBX files. Retarget those animations to the project's standard bone set using the constraint-based bake method below.

#### Bone name mapping

| Mixamo bone | Our bone |
|---|---|
| `Hips` | `Hips` |
| `Spine` | `Spine` |
| `Spine1` | `Chest` |
| `Neck` | `Neck` |
| `Head` | `Head` |
| `LeftArm` | `UpperArm_L` |
| `LeftForeArm` | `LowerArm_L` |
| `LeftHand` | `Hand_L` |
| `RightArm` | `UpperArm_R` |
| `RightForeArm` | `LowerArm_R` |
| `RightHand` | `Hand_R` |
| `LeftUpLeg` | `UpperLeg_L` |
| `LeftLeg` | `LowerLeg_L` |
| `LeftFoot` | `Foot_L` |
| `RightUpLeg` | `UpperLeg_R` |
| `RightLeg` | `LowerLeg_R` |
| `RightFoot` | `Foot_R` |

**Discarded Mixamo bones:**
- `Spine2` — map `Spine1` → `Chest`, discard `Spine2` F-curves
- `LeftShoulder` / `RightShoulder` — clavicle bones, discard
- `LeftToeBase` / `RightToeBase` — toe bones, discard

**`mixamorig:` prefix:** Mixamo FBX downloads often prefix all bone names with `mixamorig:` (e.g. `mixamorig:Hips`). Strip this prefix before mapping.

**`Root` bone:** Our skeleton has a `Root` bone above `Hips`. Mixamo has no `Root`. `Root` will have no animation data — this is expected, not a bug.

#### Retargeting method (constraint-based bake)

For each animation FBX:

1. Import animation FBX into Blender (enable **Automatic Bone Orientation** in the FBX importer) — creates a Mixamo armature
2. On our stickman armature, add `Copy Rotation` constraints on each bone targeting the corresponding Mixamo bone; add `Copy Location` on `Hips` only
3. `Object → Animation → Bake Action` (Visual Keying ON, Clear Constraints ON, frame range = animation range)
4. Name the baked action (e.g. `run`, `attack`)
5. Push action to NLA, delete the Mixamo armature

For batch imports (e.g. the Capoeira pack's 39 clips), script this loop via Blender MCP.

#### Upper/lower body blending

Animations support Godot `AnimationTree` bone-mask blending (e.g. `run` lower body + `attack` upper body). When authoring clips ensure:
- `run` has upper body keyframes (hold idle pose)
- `attack` has lower body keyframes (hold run pose)

Missing channels on either half snap to T-pose during blending.

#### Output

One GLB per character — mesh + armature + all clips as NLA strips. Use the standard GLB export settings below.

### Capoeira Pack

Downloaded from Mixamo. Stored at `C:\work\my\assets\Capoeira Pack\`. **Not committed to git.** Contains the rigged player character and 39 animation clips.

| File | Notes |
|---|---|
| `player_mixamo.fbx` | Rigged character — Mixamo skeleton, use as retarget source |

**Ginga (footwork — movement base):**
`ginga forward`, `ginga backward`, `ginga sideways 1/2`, `ginga sideways to au`, `ginga variation 1/2/3`

**Esquiva (evasions/dodges):**
`esquiva 1/2/3/4/5`

**Kicks / attacks:**
`armada`, `armada to esquiva`, `bencao`, `chapa 2`, `chapa giratoria 2`, `chapa-giratoria`, `chapaeu de couro`, `martelo 2/3`, `martelo do chau`, `martelo do chau sem mao`, `meia lua de compasso`, `meia lua de compasso back`, `meia lua de frente`, `pontera`, `queshada 1/2`

**Acrobatics:**
`au`, `au to role`, `macaco side`

**Sweeps / ground:**
`rasteira 1/2`, `troca 1`

**General capoeira idles:**
`capoeira`, `capoeira (2)`, `capoeira (3)`

Copy individual FBX files into `assets/models/characters/animations/` when bringing a clip into the project.

---

## Pre-Export Merge

Run this via `execute_blender_code` before every GLB export. It is **non-destructive** — the source `.blend` is never modified. Individual voxels remain intact in the saved file committed to git.

### What it does

For each named voxel group object in the scene:
1. Duplicate the object
2. Join all voxels in the duplicate into one mesh
3. Remove doubles (`merge_distance = 0.001`)
4. Delete interior faces (faces where all edges are shared by more than two faces)
5. Recalculate normals

Export the GLB from the duplicates (`use_selection=True`), then delete them.

### Naming convention

Voxel group objects follow `{bone_lowercase}_{material_zone}`:

| Bone | Lowercase prefix | Example objects |
|---|---|---|
| `Head` | `head` | `head_skin`, `head_hair`, `head_eyes` |
| `Chest` | `chest` | `chest_cloth`, `chest_metal` |
| `UpperArm_R` | `upperarm_r` | `upperarm_r_cloth` |
| `LowerArm_L` | `lowerarm_l` | `lowerarm_l_skin` |
| `Hand_R` | `hand_r` | `hand_r_skin`, `hand_r_glove` |
| `UpperLeg_L` | `upperleg_l` | `upperleg_l_cloth` |
| `Foot_R` | `foot_r` | `foot_r_boot` |

The merge script uses the prefix to resolve which bone the merged mesh should be weighted to. All objects sharing the same bone prefix are weighted 100% to that bone — no blending.

### Script template

```python
import bpy

duplicates = []

for obj in bpy.data.objects:
	if obj.type != 'MESH':
        continue
	bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.duplicate()
    dup = bpy.context.active_object
    duplicates.append(dup)
    # Join voxels (single-object case is already joined; multi-child case needs select-all children first)
	bpy.ops.object.mode_set(mode='EDIT')
	bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.remove_doubles(threshold=0.001)
    bpy.ops.mesh.delete_loose()
    bpy.ops.mesh.select_interior_faces()
	bpy.ops.mesh.delete(type='FACE')
    bpy.ops.mesh.normals_make_consistent(inside=False)
	bpy.ops.object.mode_set(mode='OBJECT')

# Export GLB from duplicates only
bpy.ops.object.select_all(action='DESELECT')
for dup in duplicates:
    dup.select_set(True)

# (then call export_scene.gltf with use_selection=True — see Export Settings below)

# Cleanup
for dup in duplicates:
    bpy.data.objects.remove(dup, do_unlink=True)
```

---

## Export Settings (Blender → GLB)

Run via `execute_blender_code` with these exact flags:

```python
bpy.ops.export_scene.gltf(
	filepath="<godot_project>/assets/models/characters/<name>.glb",
	export_format='GLB',
	export_apply=True,          # apply modifiers
	export_yup=True,            # Y-up for Godot
	export_animations=True,
	export_nla_strips=True,     # export all NLA Actions as separate clips
	export_frame_range=False,   # use NLA strip ranges, not scene range
	export_skins=True,
	export_all_influences=False,
	export_def_bones=False,
	export_materials='EXPORT',
	export_normals=True,
	use_selection=False,
	export_cameras=False,
	export_lights=False,
)
```

---

## File & Folder Conventions

```
assets/
  models/
	characters/
	  player_prototype.blend   ← canonical rigged humanoid base — duplicate this for all new humanoid chars
	  player.glb
	  player.blend
	  enemy_<type>.glb
	  enemy_<type>.blend
	equipment/
	  weapon_<type>.glb
	  weapon_<type>.blend
	  armour_<category>.glb
	  armour_<category>.blend
	props/
	  <name>.glb
	  <name>.blend
```

- One GLB per character, containing mesh + armature + all animation clips
- Blender source files (`.blend`) **are** committed alongside the GLB — they are the editable source
- Godot import settings (`.import` files) **are** committed — configure loop flags etc. once and keep them

---

## Godot Import Settings (per GLB)

After first import, configure via the Import dock:

| Setting | Value |
|---|---|
| Scale | 1.0 (model is authored to correct scale) |
| Animation → Storage | Built-in |
| Animation → FPS | 30 |
| Loop clips | `idle`, `walk`, `run` → **Loop** on; `attack`, `hit`, `death` → **Loop** off |
| Skins | On |

Commit the `.import` file after configuring — Godot regenerates it from source if deleted, which loses the loop settings.

---

## EffectBlocks VFX Pack

EffectBlocks by Bukkbeek — pre-built Godot 4 VFX scenes. Full pack extracted at `C:\work\my\assets\effectblocks\`. **Not committed to git.** The `PolyBlocks/` folder is copied into the project root (`res://PolyBlocks/`) and **is** committed — it contains the scenes and assets the game references directly.

When a new effect is needed, copy the relevant scene from the extracted pack or from `res://PolyBlocks/EffectBlocks/assets/<category>/` if already present.

### Packs available

| Pack | Folder |
|---|---|
| EffectBlocks v4 | `effectblocks\EffectBlocks v4\` |
| EffectBlocks v3 | `effectblocks\PolyBlocks_EffectBlocks_v3\` |
| EffectBlocks PixelRenderer v2 | `effectblocks\PolyBlocks_EffectBlocks_PixelRenderer_v2\` |

### In-project contents (`res://PolyBlocks/EffectBlocks/`)

| Category | Scenes | Notes |
|---|---|---|
| `assets/impacts/` | `impact_1` – `impact_8` | Hit flash effects — billboard sparkles, circles, spikes |
| `assets/attacks/` | `attack_crystal`, `attack_earth`, `attack_fire` | Larger area attack effects |
| `assets/explosions/` | various | Explosion blasts |
| `assets/fire/` | various | Fire/burn effects |
| `assets/energy/` | various | Magic/energy effects |
| `source_files/scripts/` | `impacts.gd`, `impact_single.gd`, etc. | Scripts referenced by effect scenes — do not move |
| `source_files/textures/` | `sparkle.png`, `circle_1.png`, etc. | Textures referenced by effect scenes — do not move |

### Triggering effects — two script patterns

**`impact_single.gd`** (root IS `GPUParticles3D` — impacts 3–8):
```csharp
var fx = scene.Instantiate<GpuParticles3D>();
// modify ProcessMaterial before AddChild
fx.Call("activate_effects");  // sets self.emitting = true
```

**`impacts.gd`** (root is `Node3D`, children Effect1/Effect2/Light — impacts 1–2):
```csharp
var fx = scene.Instantiate<Node3D>();
fx.Call("activate_effects");  // sets Effect1.emitting, Effect2.emitting, tweens Light
```

**Scale note** — `Node3D.Scale` does not affect rendered particle size. Set `ProcessMaterial.ScaleMin/Max` directly (always `.Duplicate()` the material first). Current hit effect uses `ScaleMin = 40, ScaleMax = 80`.

---

## KayKit Asset Library

KayKit packs are used for prototyping. They live outside the project at `C:\work\my\assets\kaykit\` and are **not committed to git**.

When a KayKit asset is needed, copy just the relevant GLB (and texture PNG if needed) into the appropriate `assets/` subfolder. Only those copied files are tracked by git.

Pre-release, open the corresponding `.blend` from the library in Blender MCP to customise before re-exporting.

### Packs available

| Pack | Folder | Contents |
|---|---|---|
| Adventurers 2.0 SOURCE | `KayKit_Adventurers_2.0_SOURCE/` | 9 characters (Knight, Barbarian, Mage, Ranger, Rogue, Rogue_Hooded, Druid, Engineer, Barbarian_Large) + all props/weapons + alt textures A/B/C + `.blend` source files |
| Adventurers 2.0 FREE | `KayKit_Adventurers_2.0_FREE/` | 5 base characters only, no source blends — subset of SOURCE |
| Adventurers 2.0 EXTRA | `KayKit_Adventurers_2.0_EXTRA/` | Same as SOURCE minus `.blend` files — redundant if SOURCE is present |
| Skeletons 1.1 SOURCE | `KayKit_Skeletons_1.1_SOURCE/` | 6 characters (Warrior, Mage, Rogue, Minion, Golem, Necromancer) + all weapons/accessories + textures A/B + `.blend` source files. Also includes Rig_Medium_General/MovementBasic and Rig_Large_General/MovementBasic GLBs |
| Skeletons 1.1 EXTRA | `KayKit_Skeletons_1.1_EXTRA/` | Same as SOURCE minus `.blend` files — redundant if SOURCE is present |
| Skeletons 1.1 FREE | `KayKit_Skeletons_1.1_FREE/` | 4 characters (Warrior, Mage, Rogue, Minion) — subset of SOURCE, no Golem/Necromancer, no `.blend` |
| Character Animations 1.1 | `KayKit_Character_Animations_1.1/` | Full animation library for Rig_Medium and Rig_Large |
| **Dungeon Remastered 1.1 SOURCE** | `KayKit_DungeonRemastered_1.1_SOURCE/` | **Planned map/dungeon tileset.** Full modular dungeon tile set + single `.blend` source file (`Dungeon_Remastered_1.1_Source.blend`). Files are `.gltf` + `.bin` pairs (not `.glb`) — Godot imports them the same way |
| Dungeon Remastered 1.1 EXTRA | `KayKit_DungeonRemastered_1.1_EXTRA/` | SOURCE content + extra props (bar furniture, bookcases, mimic chest, round tables, rocks, scaffold pieces, etc.) — no `.blend` |
| Dungeon Remastered 1.1 FREE | `KayKit_DungeonRemastered_1.1_FREE/` | Base tile set only, no `.blend`, no extra props — subset of EXTRA |

### Dungeon Remastered tileset

**This is the planned tile source for all maps and dungeons.**

Modular stone/wood dungeon tiles designed for top-down placement. One shared texture (`dungeon_texture.png`) plus 6 alt schemes (Golden, Black & White, Sepia A/B, Night A/B).

| Category | Examples |
|---|---|
| Floors | `floor_tile_small/large`, `floor_dirt_*`, `floor_wood_*`, `floor_tile_big_grate`, `floor_tile_big_spikes` |
| Walls | `wall`, `wall_corner`, `wall_doorway`, `wall_arched`, `wall_window_*`, `wall_gated`, scaffold variants |
| Stairs | `stairs`, `stairs_long`, `stairs_wide`, `stairs_wood`, modular stair pieces |
| Props | `chest`, `barrel`, `torch_lit`, `candle_lit`, `pillar`, `column`, `table_*`, `shelf_*` |
| Decorative | `banner_*` (6 colors × multiple patterns), `coin`, `key`, `sword_shield` |

**Import note**: tiles use `.gltf` + `.bin` pairs. Copy both files when bringing a tile into the project — Godot needs the `.bin` alongside the `.gltf` or import will fail.

### Skeleton characters (Rig_Medium unless noted)

| File | Notes |
|---|---|
| `Skeleton_Warrior.glb` | Sword+shield skeleton — Rig_Medium |
| `Skeleton_Mage.glb` | Staff skeleton — Rig_Medium |
| `Skeleton_Rogue.glb` | Dagger skeleton — Rig_Medium |
| `Skeleton_Minion.glb` | Small unarmed skeleton — Rig_Medium |
| `Skeleton_Golem.glb` | Large bone golem — **Rig_Large** (use Rig_Large_* animations) |
| `Necromancer.glb` | Hooded spellcaster — Rig_Medium (uses same animations as Adventurers) |

Textures: `skeleton_texture_A.png` (bone/grey), `skeleton_texture_B.png` (darker variant).

Weapons are separate GLBs (`Skeleton_Axe.glb`, `Skeleton_Blade.glb`, `Skeleton_Scythe.glb`, etc.) — attach to skeleton's `Hand_R` bone via `BoneAttachment3D`.

### Animation files (Rig_Medium, used by all Adventurers and Skeletons)

| File | Clips |
|---|---|
| `Rig_Medium_General.glb` | `Idle_A/B`, `Death_A/B`, `Hit_A/B`, `Spawn_Air/Ground`, `Interact`, `PickUp`, `Throw`, `Use_Item` |
| `Rig_Medium_MovementBasic.glb` | `Running_A/B`, `Walking_A/B/C`, `Jump_Start/Idle/Land/Full_Short/Full_Long` |
| `Rig_Medium_MovementAdvanced.glb` | Dodge (4 dirs), strafe, sneak, crouch, crawl, walk backwards, running with bow/rifle |
| `Rig_Medium_CombatMelee.glb` | 1H/2H/dual-wield attacks (chop, slice, stab, spin), block, unarmed (kick, punch) |
| `Rig_Medium_CombatRanged.glb` | Bow, 1H/2H gun, magic (spellcast, summon) |
| `Rig_Medium_Special.glb` | Skeleton-specific: awaken, idle, taunt, walk, death, resurrect, spawn |
| `Rig_Medium_Simulation.glb` | Sit, lie down, cheer, wave (NPC flavour) |
| `Rig_Medium_Tools.glb` | Chop, dig, fish, hammer, pickaxe, saw, lockpick |
