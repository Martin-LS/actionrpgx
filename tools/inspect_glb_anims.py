"""
Imports a GLB and lists its embedded animations.
Run via: blender --background --python tools/inspect_glb_anims.py
"""
import bpy, sys

GLB = r"C:\work\my\github\godot1\assets\models\characters\player.glb"

bpy.ops.import_scene.gltf(filepath=GLB)

for action in bpy.data.actions:
    fr = action.frame_range
    print(f"  ANIM: {action.name}  frames={fr[0]:.0f}-{fr[1]:.0f}")
