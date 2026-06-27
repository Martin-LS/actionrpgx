# Asset Pipeline

> End-to-end pipeline for taking a character asset from concept to a usable scene in Godot. Covers the full chain: silhouette → ComfyUI → Blender cleanup → Blender styling → Godot scene setup.
>
> **Scope:** Characters only for now (player + humanoid enemies). Props, environment tiles, and weapons follow the same pipeline with fewer steps — to be specced after the character pipeline is proven.
>
> **Who does what:**
> - **You (the user)** — silhouette prep, ComfyUI generation, Mixamo upload
> - **Cheap model (Cline + OpenRouter, e.g. Qwen/DeepSeek/Hermes)** — Blender cleanup, Blender styling, mechanical Godot scene setup
> - **Claude** — Godot game logic wiring, AnimationTree, script connections, anything that touches C# code

---

## Step 1 — Silhouette Prep

Draw a front view and a side view silhouette of the character. Black fill on white background.

**Tools:** Paint, Figma, Photoshop, anything that exports PNG.

**Rules:**
- Match the silhouette archetype from `docs/visuals-style.md` (e.g. demons wide and low, undead tall and thin)
- Keep proportions roughly consistent with the Character Proportions table in `docs/visuals-style.md`
- Front and side views should be the same scale

> **Note:** The silhouette is your primary design control. The AI will faithfully follow the shape you give it — invest time here and you'll save iteration rounds later. A rough sketch is fine; clean edges matter more than artistic polish.

---

## Step 2 — ComfyUI → 3D Mesh (Hunyuan3D-2)

Feed the front + side silhouette pair into ComfyUI using the Hunyuan3D-2 image-to-3D node.

**Input:** Front view PNG + side view PNG
**Output:** GLB or OBJ mesh with baked texture

**Settings to use:**
- Model: Hunyuan3D-2
- Input mode: multi-view (front + side)
- Resolution: default or 256 — higher resolution is not needed at this poly count

> **Note:** Multi-view input (front + side simultaneously) is much more predictable than single-image input. With a single image, the AI guesses the back and side — with two views it has direct constraints. Expect acceptable output in 1–2 generations rather than 5–6.
>
> **Iterating:** If the output isn't right, two options: (1) refine your silhouette and re-run, or (2) take a render/screenshot of the output, paint over it to adjust, and feed it back in as a new input image. Option 1 is cleaner; option 2 is faster for small shape tweaks.
>
> **The generated texture can be ignored** — it gets replaced with flat palette colors in Step 4.

---

## Step 3 — Blender Cleanup

**Handled by: cheap model (Cline + OpenRouter)**

Import the generated mesh into Blender and clean it up to spec.

**Steps:**
1. Import GLB/OBJ into Blender
2. Apply Decimate modifier to hit the poly count target (see table below)
3. Apply flat shading (right-click → Shade Flat)
4. Fix any broken geometry (non-manifold edges, isolated vertices)
5. Delete the generated texture — it is not needed

**Poly count targets:**

| Character type | Target (triangles) |
|---|---|
| Player | 800 |
| Common enemy | 400 |
| Boss | 1200 |

> **Note for agent:** Use the Decimate modifier in Collapse mode. Target ratio = desired tris ÷ current tris. Apply the modifier after confirming the count. Flat shading is applied per-object: select all mesh objects, right-click in the viewport, Shade Flat. Check for non-manifold geometry with Edit Mode → Select → Select All by Trait → Non Manifold — delete any loose verts or edges found.
>
> **Note:** These counts are intentionally low. Flat shading and the top-down camera angle mean the game reads well at Roblox-level detail (~300–500 tris). The player has a higher budget only because it's on screen constantly and benefits from slightly more limb definition.

---

## Step 4 — Blender Styling

**Handled by: cheap model (Cline + OpenRouter)**

Apply flat palette colors per bone region and fit the mesh to the shared armature.

### 4a — Color Assignment

For each bone region, apply a flat Principled BSDF material using the colors from `docs/visuals-style.md`. The mapping below is the default for a humanoid character — adjust per character type (e.g. an undead enemy has no skin tone, a demon may have different cloth colors).

| Bone region | Default material zone | Color reference |
|---|---|---|
| Head | skin | Skin tone from `visuals-style.md` |
| Chest / UpperArm | cloth (shirt) | Shirt color |
| LowerArm / Hand | skin | Skin tone |
| Hips / UpperLeg / LowerLeg | cloth (pants) | Pants color |
| Foot | boot | Boot color |

> **Note for agent:** Use `mat.use_nodes = True` with a Principled BSDF node. Set Roughness = 1.0, Metallic = 0.0. Do not use `use_nodes = False` — it does not export color to GLB. Assign one material per region; do not share materials between regions (makes it easy to change individual colors later).
>
> **Note:** The color scheme in `docs/visuals-style.md` is a working placeholder and will be revised. When it is updated, re-run this step on any affected characters — the pipeline is non-destructive so the source `.blend` retains the mesh and rig unchanged.

### 4b — Armature Fitting

Fit the cleaned mesh to the shared armature from `assets/models/characters/src/player.blend`.

**Steps:**
1. Append the armature from `player.blend` into the current file
2. Scale and position the mesh to match the armature proportions
3. Parent the mesh to the armature with automatic weights (`Ctrl+P → Armature Deform with Automatic Weights`)
4. Review weight painting — check that limb regions deform correctly with no cross-contamination between bones
5. Correct any obvious weight painting errors (e.g. foot verts influenced by spine bone)

> **Note for agent:** Automatic weights work well for humanoid meshes at this poly count — manual weight painting is usually not needed. If a region deforms badly, use Weight Paint mode to clean the affected verts: paint weight = 1.0 to the correct bone, weight = 0.0 to the incorrect bone. Check deformation by posing the armature in Pose Mode.
>
> **Note:** All humanoid characters share this one armature so that Mixamo animations retarget to every character without additional work. Do not modify the armature — mesh must fit to it, not the other way around.

---

## Step 5 — Mixamo Auto-Rig (You)

> **Handled by: you** — Mixamo requires a browser login.

Upload the styled, fitted mesh to [mixamo.com](https://www.mixamo.com):

1. Export a T-pose GLB from Blender (mesh only, no armature — Mixamo generates its own)
2. Upload to Mixamo → Auto-Rigger
3. Place the joint markers (chin, wrists, elbows, groin)
4. Download with the Mixamo rig as FBX

Then retarget the Mixamo rig to the shared armature in Blender:

1. Import the Mixamo FBX alongside the shared armature
2. Add `COPY_ROTATION` constraints from each shared armature bone to its corresponding `mixamorig:` bone
3. Bake the pose in Pose Mode (Pose → Animation → Bake Action)
4. Push the baked action to the NLA editor

> **Note:** All Mixamo animations use the same `mixamorig:` bone naming regardless of which character model was uploaded. This means the retargeting steps above are identical for every character — do it once per new animation clip, and it works across all humanoids.

---

## Step 6 — Export GLB

**Handled by: cheap model (Cline + OpenRouter)**

Export the final character (mesh + shared armature + animations) as GLB.

**File locations:**
- Source `.blend`: `assets/models/characters/src/{character_name}.blend`
- Exported GLB: `assets/models/characters/{character_name}.glb`

**Export settings:**
```python
bpy.ops.export_scene.gltf(
    filepath="...",
    use_visible=True,
    export_apply=True,
    export_yup=True,
    export_animations=True,
    export_nla_strips=True,
    export_cameras=False,
    export_lights=False,
)
```

> **Note for agent:** Commit both the `.blend` source and the `.glb` export to git. The `.blend` is the source of truth — the `.glb` is derived but committed so Godot can import it without requiring Blender to be installed.

---

## Step 7 — Godot Scene Setup

**Mechanical setup: cheap model (Cline + OpenRouter)**
**Game logic wiring: Claude**

### 7a — Mechanical setup (cheap model)

1. Import the GLB — Godot auto-imports on file drop into `assets/models/characters/`
2. Create a new scene at the correct path (see File Locations below)
3. Add the imported GLB as a child node (MeshInstance3D + Skeleton3D)
4. Add a CollisionShape3D with a CapsuleShape3D sized to the character
5. Add an AnimationTree node — wire it to the shared animation player

> **Note for agent:** Use Godot MCP Pro tools for all scene setup (`add_node`, `add_scene_instance`, `setup_collision`, `create_animation_tree`). Never edit `.tscn` files directly. After any `.tscn` change call `reload_project` so the editor picks it up.

### 7b — Game logic wiring (Claude)

- Connect AnimationTree to `EnemyController.cs` (or `PlayerController.cs` for the player)
- Set correct physics layers for the character type
- Wire any signals (hit detection, death, etc.)
- Verify the scene runs correctly in-game

### File Locations

| Asset type | Scene path |
|---|---|
| Player | `src/player/player.tscn` |
| Enemy | `src/enemies/{enemy_name}.tscn` |

---

## Quick Reference — Checklist

- [ ] Front + side silhouette drawn and exported as PNG
- [ ] Hunyuan3D-2 output looks acceptable (correct shape, no major artifacts)
- [ ] Mesh decimated to poly count target
- [ ] Flat shading applied
- [ ] Flat palette materials assigned per bone region
- [ ] Mesh fitted to shared armature, weights checked
- [ ] Mixamo retarget done for required animation clips
- [ ] GLB exported to `assets/models/characters/{name}.glb`
- [ ] `.blend` source committed to `assets/models/characters/src/{name}.blend`
- [ ] Godot scene created at correct path
- [ ] CollisionShape sized correctly
- [ ] AnimationTree wired and tested in-game
