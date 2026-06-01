"""
Print existing actions and their frame ranges from player.blend.
Run via: blender --background --python tools/inspect_player_anims.py
"""
import bpy

bpy.ops.wm.open_mainfile(filepath=r"C:\work\my\github\godot1\assets\models\characters\player.blend")

for action in bpy.data.actions:
    fr = action.frame_range
    print(f"  action: {action.name}  frames={fr[0]:.0f}-{fr[1]:.0f}")
    for fc in action.fcurves:
        print(f"    {fc.data_path}[{fc.array_index}]  keys={len(fc.keyframe_points)}")
