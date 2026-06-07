# Tech Tips — Hard-Won Lessons

Lessons learned the expensive way. Read before touching animations, bones, or 3D assets.

---

## Blender → Godot coordinate system

| Blender | Godot (after Y-up GLB export) |
|---|---|
| +X | +X |
| +Y (forward) | -Z (forward) |
| +Z (up) | +Y (up) |

**Rule**: if a mesh looks sideways or vertical in Godot but correct in Blender, the axes are wrong.  
**Fix**: rotate the mesh in Blender to compensate, then re-export. Do not fix with code transforms.

---

## Blender mesh origins must be at the grip/pivot point

When you export a mesh as GLB, Godot creates a node at the object's **origin**, not at the mesh's geometric center. If the origin is at world `(0,0,0)` but the mesh geometry is at `(-2, 0, 0)`, the Godot node will be at `(0,0,0)` but the mesh will render 2 units to the side.

**Fix in Blender**: `Object → Set Origin → Origin to Geometry` (or manually place origin at the intended attach point, e.g. the sword grip).  
**Check in Godot**: print `MeshInstance3D.get_aabb().position` — if it's far from `(0,0,0)` the origin is wrong.

---

## BoneAttachment3D: the child node inherits the bone's full rotation

A `BoneAttachment3D` child node's world axes = bone's local axes in world space. The mesh must be oriented in **bone-local space**, not world space.

For `Hand_R` bone (Godot rest pose):
- Bone local X = world right `(1,0,0)`
- Bone local Y = world down `(0,-1,0)`
- Bone local Z = world forward `(0,0,-1)`

A sword blade along the child node's local **-Z** will map to world **+Z** (backward, toward camera). Orient the blade along local **+Z** to point forward.

**Don't fight bone rotation with position-only tracking** — that removes rotation from the attachment entirely and the weapon won't swing during animation.

---

## Bone names get `_2` suffix when a mesh node has the same name

Godot's GLB importer appends `_2` to a bone name if a `MeshInstance3D` in the same scene shares that name (e.g. a mesh named `Hand_R` causes the bone to be renamed `Hand_R_2`).

`Skeleton3D.FindBone("Hand_R")` returns -1 in this case. Always use a fallback:

```csharp
private static int FindBone(Skeleton3D skeleton, string name)
{
    int idx = skeleton.FindBone(name);
    return idx >= 0 ? idx : skeleton.FindBone(name + "_2");
}
```

---

## AnimationPlayer autoplay survives `Stop()` in `_Ready()`

Godot re-triggers autoplay via a deferred notification after `_Ready()` completes. Calling `_animPlayer.Stop()` directly in `_Ready()` has no effect.

**Fix**:
```csharp
_animPlayer.Autoplay = "";           // clear the property
_animPlayer.CallDeferred("stop");    // runs after all deferred notifications
```

Also set loop mode at runtime — the `.import` file `_subresources` loop flags are silently ignored:
```csharp
_animPlayer.GetAnimation("run").LoopMode    = Animation.LoopModeEnum.Linear;
_animPlayer.GetAnimation("attack").LoopMode = Animation.LoopModeEnum.None;
```

---

## AnimationTree must be set up in the Godot editor, not in C# `_Ready`

Creating an `AnimationTree` dynamically in C# `_Ready()` causes `Travel()` and `Start()` to silently fail — `GetCurrentNode()` never updates from the initial state.

**Fix**: Create the `AnimationTree` node in the Godot editor (or via MCP). In `_Ready`, only configure its `AnimPlayer` path and set `Active = true`:

```csharp
var animTree = GetNodeOrNull<AnimationTree>("AnimationTree");
if (animTree != null)
{
    animTree.AnimPlayer = animTree.GetPathTo(animPlayer); // animPlayer found via FindChild
    animTree.Active = true;
    // Do NOT fetch _smPlayback here — AnimationTree hasn't processed a frame yet (see below)
}
```

When the AnimationTree node pre-exists in the scene, `Travel()` works correctly and `GetCurrentNode()` reflects the live state.

**Setting AnimPlayer path at runtime**: When the AnimationPlayer is inside a dynamically-loaded GLB, use `animTree.GetPathTo(animPlayer)` to resolve the correct path — do not hardcode it.

**State machine setup** (via MCP `add_state_machine_state` / `add_state_machine_transition`):
- States: `idle`, `run`, `attack`
- `Start → idle`: advance_mode=auto
- `idle ↔ run`: advance_mode=enabled (Travel-driven)
- `idle → attack`, `run → attack`: advance_mode=enabled
- `attack → idle`: switch_mode=at_end, advance_mode=auto (auto-returns when clip finishes)

**C# usage**:
- `_PhysicsProcess`: if current != "attack" → Travel("run") or Travel("idle")
- `OnSkillFired`: Travel("attack")
- No bool flag needed — state machine tracks state

**`AnimationNodeStateMachinePlayback` must be fetched lazily — never in `_Ready()`**

`animTree.Get("parameters/playback")` returns an empty Variant in `_Ready()` because the AnimationTree hasn't processed its first frame yet. The cast to `AnimationNodeStateMachinePlayback` silently produces `null`, making every subsequent `_smPlayback?.Travel(...)` call a no-op with no error.

Fix: initialise lazily at the top of `_PhysicsProcess`:

```csharp
if (_smPlayback == null)
{
    var at = GetNodeOrNull<AnimationTree>("AnimationTree");
    if (at != null)
        _smPlayback = at.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();
}
```

Use `.As<T>()` rather than an explicit cast — it returns `null` on type mismatch instead of throwing.

**Smoothing transitions with xfade_time**: Each transition has an `xfade_time` (seconds) that crossfades between the two clips. Default is 0 (instant cut). Set it via `execute_editor_script` — iterate by index because `find_transition(from, to)` can silently return -1:

```gdscript
var sm = EditorInterface.get_edited_scene_root().get_node("AnimationTree").tree_root
# indices match sm.get_transition_count(); use get_transition_from/to to confirm order
var xfade = {1: 0.2, 2: 0.2, 3: 0.1, 4: 0.1, 5: 0.2}  # skip 0 (Start→idle)
for i in xfade:
    sm.get_transition(i).xfade_time = xfade[i]
EditorInterface.save_scene()
```

Recommended values for idle/run/attack: idle↔run = 0.2s (smooth stop/start), →attack = 0.1s (snappy), attack→idle = 0.2s (clean landing).

---

## Godot log file location (Windows)

```
%APPDATA%\Godot\app_userdata\<project_name>\logs\godot.log
```

Enable in Project Settings: `debug/file_logging/enable_file_logging = true`.

The MCP output panel (`get_output_log`) sometimes lags or shows stale data. Read the file directly via PowerShell for reliable results:

```powershell
Get-Content "$env:APPDATA\Godot\app_userdata\godot1\logs\godot.log" | Select-Object -Last 100
# or filter:
Get-Content "$env:APPDATA\Godot\app_userdata\godot1\logs\godot.log" | Select-String "ANIM"
```

Use the `/godot-debug` skill (`.claude/commands/godot-debug.md`) to automate this.

---

## New files copied into the project must be scanned before Godot can load them

Godot only knows about files it has imported (`.import` sidecar exists). Files copied in via PowerShell/Explorer after the editor last opened will fail with `No loader found for resource` at runtime — even if the path is correct.

**Fix**: trigger a filesystem scan from the editor or via MCP:

```gdscript
EditorInterface.get_resource_filesystem().scan()
```

Wait ~3 seconds, then verify `.import` files appeared beside the new assets. This is distinct from reimporting — reimport is for existing tracked files; scan is for files the editor has never seen.

---

## GLB reimport after Blender re-export

After re-exporting a `.glb` from Blender, Godot caches the old version. The game will run with the old mesh until you force a reimport.

**Fastest method** (via MCP):
```gdscript
var fs = EditorInterface.get_resource_filesystem()
fs.scan()
fs.reimport_files(["res://assets/models/equipment/weapon_sword.glb"])
```

Alternatively delete the `.import` file and let Godot regenerate on next editor focus.

---

## BoneAttachment3D vs `GetBoneGlobalPose` position tracking

| | BoneAttachment3D | Per-frame position tracking |
|---|---|---|
| Position | ✅ follows bone | ✅ follows bone |
| Rotation | ✅ follows bone | ❌ not applied |
| Weapon swings during attack | ✅ | ❌ |
| Complexity | Low | Medium |
| When to use | Equipment, always | Never (was a workaround) |

**Use `BoneAttachment3D` for all equipment.** Position-only tracking was a dead-end workaround for a mesh-origin problem that should be fixed in Blender instead.

---

## Blender NLA workflow for game animations

Each action must **start at frame 1**. Never offset keyframes to match the NLA strip position (e.g. run at 100, attack at 140) — that only works for cutscene timelines and makes Blender's preview a mess.

Correct setup:
- Each action: keyframes at frames 1–N
- NLA strips: all placed at frame 1, each on its own track
- To preview one animation: click the ★ (solo) icon on its NLA track
- Strip extrapolation: set to `NOTHING` on all strips so they don't hold/bleed outside their range
- `use_nla` must be `True` and `animation_data.action` must be `None` for NLA strips to evaluate cleanly

---

## Raising a character arm overhead (UpperArm_R Z rotation)

For UpperArm_R (bone pointing down in rest, character facing -Y):

| Z rotation | Elbow position |
|---|---|
| 0° | hanging at side (rest) |
| −90° | horizontal, pointing right |
| −120° | above shoulder height |
| −135° | at ear/head height |
| −150° | nearly vertical overhead |

For an overhand throw wind-up with hand near the ear: **Z ≈ −135°** on UpperArm_R combined with X ≈ +20° (arm slightly back).

UpperArm_L mirrors this: **+Z** extends the arm outward to the left.

---

## Shoulder twist for throw animations (Chest/Spine/Hips Y rotation)

Y rotation on upward bones (Chest, Spine, Hips) is a **roll around the bone's own axis** — it twists the torso without moving the spine tip. This is the shoulder twist axis.

Empirically verified for Chest:
- **+Y** → right shoulder moves BACK (+Y world) = throw wind-up
- **−Y** → right shoulder moves FORWARD (−Y world) = throw release

Without shoulder twist, a throw animation looks like a forward poke from the front view. Always add Chest/Spine/Hips Y rotation to make a throw readable from any angle.

Typical values: Chest ±20–25°, Spine ±12–15°, Hips ±6–8°.

---

## Mixamo FBX export — lessons learned

### Hair causes stray neck geometry

Hair meshes (`Hair_SideL`, `Hair_SideR`) have vertices that extend down to shoulder/neck height (Z ≈ 2.09) even though visually they sit on the head. After joining all meshes and exporting, these create a floating geometry cluster between the head and torso in the FBX. Mixamo and other tools flag this as an error.

**Fix:** Delete all hair objects before merging for Mixamo export. Hair is not needed for auto-rigging and can be added back after retargeting.

---

### Surgical face deletion makes mesh integrity worse

If you identify problem faces (non-manifold, interior, duplicate) and delete them selectively after the mesh has already been joined and posed, you will create new open boundary edges where those faces used to be. Trying to fix 8 non-manifold edges this way created 36.

**Fix:** Restart from the source `.blend` with the offending objects removed before any merge or T-pose step. Clean source → merge → clean → T-pose, in that order. Never fix geometry after T-pose.

---

### Duplicate faces at mesh join points

When separate box-geometry meshes are joined (`object.join()`), their touching cap faces become co-planar duplicates — e.g. the top face of Torso and the bottom face of Head both exist at the same Z. These show as non-manifold in FBX viewers and Mixamo.

**Fix:** Always run `bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.001)` immediately after joining, followed by interior face deletion and `recalc_face_normals`. Do this before the T-pose step.

---

### T-pose rotation threshold for `player.blend`

The arm/torso boundary is at X = ±0.51. Torso verts end at X = ±0.50; arm verts start at X = ±0.55. Using a threshold of `0.51` correctly isolates arm verts without touching torso geometry. If the character proportions change, recheck this gap by printing the X distribution at shoulder height (Z 1.5–2.5) before transforming.
