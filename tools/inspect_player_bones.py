"""
Print all bone names and their rest pose positions from player.blend.
Run via: blender --background --python tools/inspect_player_bones.py
"""
import bpy

bpy.ops.wm.open_mainfile(filepath=r"C:\work\my\github\godot1\assets\models\characters\player.blend")

arm = next((o for o in bpy.data.objects if o.type == 'ARMATURE'), None)
if not arm:
    print("No armature found")
else:
    print(f"Armature: {arm.name}")
    for bone in arm.data.bones:
        h = bone.head_local
        print(f"  {bone.name:30s}  head=({h.x:.3f}, {h.y:.3f}, {h.z:.3f})")
