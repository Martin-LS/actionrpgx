"""
Recreates the idle breathing animation in player.blend using the Blender 5.1
layered action API, then re-exports player.glb.
Idle: Chest rises 0.06 units local-Y at frame 20, UpperArm_L/R lift 3° at frame 20.
All back to rest at frame 0 and 39 (loop point).
"""
import bpy
import math

BLEND = r"C:\work\my\github\godot1\assets\models\characters\player.blend"
OUT   = r"C:\work\my\github\godot1\assets\models\characters\player.glb"

bpy.ops.wm.open_mainfile(filepath=BLEND)

arm_obj = next(o for o in bpy.data.objects if o.type == 'ARMATURE')
bpy.context.view_layer.objects.active = arm_obj

# Remove old idle action if it exists
old = bpy.data.actions.get("idle")
if old:
    bpy.data.actions.remove(old)
    print("Removed old idle action")

# Create new layered action
idle_action = bpy.data.actions.new(name="idle")
arm_obj.animation_data_create()
arm_obj.animation_data.action = idle_action

bpy.ops.object.mode_set(mode='POSE')

def key(bone_name, data_path, index, frame, value):
    b = arm_obj.pose.bones[bone_name]
    bpy.context.scene.frame_set(frame)
    if data_path == "location":
        b.location[index] = value
        b.keyframe_insert(data_path="location", index=index, frame=frame)
    elif data_path == "rotation_euler":
        b.rotation_euler[index] = value
        b.keyframe_insert(data_path="rotation_euler", index=index, frame=frame)

# Rest frames at 0 and 39
for bone in ["Chest", "UpperArm_L", "UpperArm_R"]:
    b = arm_obj.pose.bones[bone]
    for frame in [0, 39]:
        bpy.context.scene.frame_set(frame)
        b.location = (0, 0, 0)
        b.rotation_euler = (0, 0, 0)
        b.keyframe_insert(data_path="location", frame=frame)
        b.keyframe_insert(data_path="rotation_euler", frame=frame)

# Frame 20: inhale peak
# Chest rises in local Y
key("Chest", "location", 1, 20, 0.06)

# UpperArm_L/R lift slightly (X rotation = 3°)
lift = math.radians(3)
key("UpperArm_L", "rotation_euler", 0, 20, lift)
key("UpperArm_R", "rotation_euler", 0, 20, lift)

bpy.ops.object.mode_set(mode='OBJECT')

# Verify
for layer in idle_action.layers:
    for strip in layer.strips:
        for cb in strip.channelbags:
            for fc in cb.fcurves:
                print(f"  idle: {fc.data_path}[{fc.array_index}]  keys={len(fc.keyframe_points)}")

print(f"Idle action created: frames {idle_action.frame_range[0]:.0f}-{idle_action.frame_range[1]:.0f}")
print(f"Actions in file: {[a.name for a in bpy.data.actions]}")

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
