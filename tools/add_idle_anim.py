"""
Adds a breathing idle animation to player.blend and re-exports player.glb.
Breathing: Chest rises 0.06 units over 20 frames, falls back over next 20.
Shoulders (UpperArm_L/R) rise slightly with the chest via tiny Z rotation.
Run via: blender --background --python tools/add_idle_anim.py
"""
import bpy
import math
import os

BLEND = r"C:\work\my\github\godot1\assets\models\characters\player.blend"
OUT   = r"C:\work\my\github\godot1\assets\models\characters\player.glb"

bpy.ops.wm.open_mainfile(filepath=BLEND)

arm_obj = next(o for o in bpy.data.objects if o.type == 'ARMATURE')
bpy.context.view_layer.objects.active = arm_obj
arm_obj.select_set(True)

# Create the idle action and assign it
idle_action = bpy.data.actions.new(name="idle")
arm_obj.animation_data_create()
arm_obj.animation_data.action = idle_action

bpy.ops.object.mode_set(mode='POSE')

chest = arm_obj.pose.bones["Chest"]
ul    = arm_obj.pose.bones["UpperArm_L"]
ur    = arm_obj.pose.bones["UpperArm_R"]

# Helper: set a pose bone to rest and insert keyframes
def key_rest(bone, frame):
    bone.location     = (0, 0, 0)
    bone.rotation_euler = (0, 0, 0)
    bone.keyframe_insert(data_path="location",       frame=frame)
    bone.keyframe_insert(data_path="rotation_euler", frame=frame)

def key_inhale(frame):
    # Chest rises in local Y (bone points up, so local Y ≈ world Z)
    chest.location = (0, 0.06, 0)
    chest.keyframe_insert(data_path="location", frame=frame)
    # Shoulders lift: rotate UpperArm slightly back (local X rotation)
    lift = math.radians(3)
    ul.rotation_euler = (lift, 0, 0)
    ul.keyframe_insert(data_path="rotation_euler", frame=frame)
    ur.rotation_euler = (lift, 0, 0)
    ur.keyframe_insert(data_path="rotation_euler", frame=frame)

# Frame 0: rest
key_rest(chest, 0)
key_rest(ul, 0)
key_rest(ur, 0)
# Frame 20: inhale peak
key_inhale(20)
# Frame 39: rest again (loop point)
key_rest(chest, 39)
key_rest(ul, 39)
key_rest(ur, 39)

# Smooth interpolation on all fcurves
bpy.ops.object.mode_set(mode='OBJECT')

print("Idle action created. Exporting GLB...")

bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.gltf(
    filepath=OUT,
    use_selection=False,
    export_format='GLB',
    export_yup=True,
)
print(f"Exported: {OUT}")
