"""
Exaggerates the attack arm swing in player.blend for top-down camera visibility.
Changes:
  - UpperArm_R X rotation scaled 1.5x (bigger forward/backward arc)
  - UpperArm_R Z rotation added (horizontal sweep visible from above)
  - Chest X rotation scaled 1.5x (more visible body twist)
Re-exports player.glb.
Run via: blender --background --python tools/exaggerate_attack_swing.py
"""
import bpy
import math

BLENDER = "C:/Program Files/Blender Foundation/Blender 5.1/blender.exe"
BLEND   = r"C:\work\my\github\godot1\assets\models\characters\player.blend"
OUT     = r"C:\work\my\github\godot1\assets\models\characters\player.glb"

bpy.ops.wm.open_mainfile(filepath=BLEND)

attack = bpy.data.actions.get("attack")
if not attack:
    print("ERROR: 'attack' action not found")
    raise SystemExit(1)

# Collect all fcurves from the layered action
fcurves = []
for layer in attack.layers:
    for strip in layer.strips:
        for cb in strip.channelbags:
            for fc in cb.fcurves:
                fcurves.append((fc.data_path, fc.array_index, fc))

def get_fc(path, idx):
    for dp, ai, fc in fcurves:
        if dp == path and ai == idx:
            return fc
    return None

def set_keyframe(fc, frame, value):
    for kp in fc.keyframe_points:
        if int(kp.co[0]) == frame:
            kp.co[1] = value
            return
    fc.keyframe_points.insert(frame, value)

# --- UpperArm_R X rotation: scale non-zero keyframes by 1.5x ---
fc_ur_x = get_fc('pose.bones["UpperArm_R"].rotation_euler', 0)
if fc_ur_x:
    for kp in fc_ur_x.keyframe_points:
        if kp.co[1] != 0.0:
            kp.co[1] *= 1.5
    print("UpperArm_R[0] scaled 1.5x")
    for kp in fc_ur_x.keyframe_points:
        print(f"  frame {int(kp.co[0])}: {round(kp.co[1], 4)}")
else:
    print("WARNING: UpperArm_R[0] not found")

# --- UpperArm_R Z rotation: new horizontal-sweep channel ---
# Arm cocks back at frame 8, sweeps forward at frame 18, recovers by frame 39
# Values in radians (negative = arm swings back/outward, positive = forward/inward)
arm_z_keys = [(0, 0.0), (8, -0.35), (18, 0.70), (30, 0.17), (39, 0.0)]

fc_ur_z = get_fc('pose.bones["UpperArm_R"].rotation_euler', 2)
if fc_ur_z:
    # Clear existing keyframes
    while fc_ur_z.keyframe_points:
        fc_ur_z.keyframe_points.remove(fc_ur_z.keyframe_points[0])

for frame, val in arm_z_keys:
    # Need to insert via the armature object in pose mode — use key_insert instead
    pass

# Insert Z rotation keyframes via the pose bone directly
arm_obj = next(o for o in bpy.data.objects if o.type == 'ARMATURE')
bpy.context.view_layer.objects.active = arm_obj
bpy.ops.object.mode_set(mode='POSE')

arm_obj.animation_data.action = attack
bone_ur = arm_obj.pose.bones["UpperArm_R"]

for frame, val in arm_z_keys:
    bpy.context.scene.frame_set(frame)
    bone_ur.rotation_euler[2] = val
    bone_ur.keyframe_insert(data_path="rotation_euler", index=2, frame=frame)

print("UpperArm_R[2] horizontal sweep added:")
for frame, val in arm_z_keys:
    print(f"  frame {frame}: {round(val, 4)} rad ({round(math.degrees(val), 1)}°)")

# --- Chest X rotation: scale non-zero keyframes by 1.5x ---
bpy.ops.object.mode_set(mode='OBJECT')

# Re-collect fcurves after modifications
fcurves = []
for layer in attack.layers:
    for strip in layer.strips:
        for cb in strip.channelbags:
            for fc in cb.fcurves:
                fcurves.append((fc.data_path, fc.array_index, fc))

fc_chest_x = get_fc('pose.bones["Chest"].rotation_euler', 0)
if fc_chest_x:
    for kp in fc_chest_x.keyframe_points:
        if kp.co[1] != 0.0:
            kp.co[1] *= 1.5
    print("Chest[0] scaled 1.5x")
    for kp in fc_chest_x.keyframe_points:
        print(f"  frame {int(kp.co[0])}: {round(kp.co[1], 4)}")
else:
    print("WARNING: Chest[0] not found")

# --- Save blend file ---
bpy.ops.wm.save_mainfile(filepath=BLEND)
print("Saved player.blend")

# --- Export GLB ---
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.gltf(
    filepath=OUT,
    use_selection=False,
    export_format='GLB',
    export_yup=True,
)
print(f"Exported: {OUT}")
