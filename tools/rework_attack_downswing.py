"""
Replaces the attack animation with an overhead downswing.
Motion: arm raises overhead (frames 0-10), hammers down (10-17), follow-through + return (17-39).
Run via: blender --background --python tools/rework_attack_downswing.py
"""
import bpy
import math

BLEND = r"C:\work\my\github\godot1\assets\models\characters\player.blend"
OUT   = r"C:\work\my\github\godot1\assets\models\characters\player.glb"

bpy.ops.wm.open_mainfile(filepath=BLEND)

arm_obj = next(o for o in bpy.data.objects if o.type == 'ARMATURE')
bpy.context.view_layer.objects.active = arm_obj

attack = bpy.data.actions.get("attack")
if not attack:
    print("ERROR: 'attack' action not found")
    raise SystemExit(1)

# Assign action and enter pose mode
arm_obj.animation_data_create()
arm_obj.animation_data.action = attack
bpy.ops.object.mode_set(mode='POSE')

def key(bone_name, axis, frame, value):
    b = arm_obj.pose.bones[bone_name]
    bpy.context.scene.frame_set(frame)
    b.rotation_euler[axis] = value
    b.keyframe_insert(data_path="rotation_euler", index=axis, frame=frame)

# --- Clear all existing keyframes by removing fcurves from channelbags ---
bpy.ops.object.mode_set(mode='OBJECT')
for layer in attack.layers:
    for strip in layer.strips:
        for cb in strip.channelbags:
            # collect and remove all fcurves
            to_remove = list(cb.fcurves)
            for fc in to_remove:
                cb.fcurves.remove(fc)

bpy.ops.object.mode_set(mode='POSE')

# Reset all bones to rest at frame 0 and 39 (loop points)
for bone_name in ["UpperArm_R", "LowerArm_R", "Hand_R",
                   "UpperArm_L", "LowerArm_L", "Chest"]:
    b = arm_obj.pose.bones[bone_name]
    for frame in [0, 39]:
        bpy.context.scene.frame_set(frame)
        b.rotation_euler = (0.0, 0.0, 0.0)
        b.keyframe_insert(data_path="rotation_euler", frame=frame)

# -------------------------------------------------------------------
# OVERHEAD DOWNSWING DESIGN (all values in radians, bone-local space)
#
# UpperArm_R axis 0 (X): positive = arm raised up/back, negative = arm forward/down
# LowerArm_R axis 0 (X): negative = elbow bends (arm coils), positive = elbow extends
# Chest      axis 0 (X): negative = lean back, positive = lean forward
#
# Timing:
#   0      : rest
#   6      : slight pre-raise starts
#   10     : ARM OVERHEAD (fully raised, elbow bent, coiled)
#   17     : STRIKE (arm hammers down — fast 7-frame window)
#   25     : follow-through (arm extended forward/down)
#   32     : beginning of recovery
#   39     : rest
# -------------------------------------------------------------------

# UpperArm_R — the main vertical arc
for frame, val in [(0, 0.0), (6, 0.4), (10, 1.65), (17, -1.35), (25, -0.7), (32, -0.15), (39, 0.0)]:
    key("UpperArm_R", 0, frame, val)

# UpperArm_R Z — tiny outward flare when raised, neutral on strike
for frame, val in [(0, 0.0), (10, 0.25), (17, 0.0), (39, 0.0)]:
    key("UpperArm_R", 2, frame, val)

# LowerArm_R — elbow coils up then extends hard on strike
for frame, val in [(0, 0.0), (10, -0.65), (17, 0.25), (25, 0.1), (39, 0.0)]:
    key("LowerArm_R", 0, frame, val)

# Hand_R — wrist cocks back overhead, snaps forward on impact
for frame, val in [(0, 0.0), (10, 0.45), (17, -0.35), (25, 0.0), (39, 0.0)]:
    key("Hand_R", 0, frame, val)

# Chest — leans back on raise, lunges forward into strike
for frame, val in [(0, 0.0), (10, -0.2), (17, 0.5), (25, 0.25), (32, 0.08), (39, 0.0)]:
    key("Chest", 0, frame, val)

# UpperArm_L — counterbalances: goes forward as right arm raises, back as it strikes
for frame, val in [(0, 0.0), (10, -0.45), (17, 0.35), (25, 0.1), (39, 0.0)]:
    key("UpperArm_L", 0, frame, val)

# LowerArm_L — slight bend to match natural counterbalance
for frame, val in [(0, 0.0), (10, 0.2), (17, -0.15), (39, 0.0)]:
    key("LowerArm_L", 0, frame, val)

bpy.ops.object.mode_set(mode='OBJECT')

print("Attack action redesigned as overhead downswing.")
print("Keyframe summary (UpperArm_R X):")
for layer in attack.layers:
    for strip in layer.strips:
        for cb in strip.channelbags:
            for fc in cb.fcurves:
                if "UpperArm_R" in fc.data_path and fc.array_index == 0:
                    for kp in fc.keyframe_points:
                        deg = math.degrees(kp.co[1])
                        print(f"  frame {int(kp.co[0])}: {round(kp.co[1],3)} rad ({round(deg,1)}°)")

bpy.ops.wm.save_mainfile(filepath=BLEND)
print("Saved player.blend")

bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.gltf(
    filepath=OUT,
    use_selection=False,
    export_format='GLB',
    export_yup=True,
)
print(f"Exported: {OUT}")
