"""
List all actions in player.blend and what bones they touch.
"""
import bpy

bpy.ops.wm.open_mainfile(filepath=r"C:\work\my\github\godot1\assets\models\characters\player.blend")

for action in bpy.data.actions:
    is_legacy = getattr(action, 'is_action_legacy', True)
    print(f"action: {action.name}  legacy={is_legacy}  frames={action.frame_range[0]:.0f}-{action.frame_range[1]:.0f}")
    if is_legacy:
        for fc in action.fcurves:
            print(f"  {fc.data_path}[{fc.array_index}]  keys={len(fc.keyframe_points)}")
    else:
        for layer in action.layers:
            for strip in layer.strips:
                for cb in strip.channelbags:
                    for fc in cb.fcurves:
                        print(f"  {fc.data_path}[{fc.array_index}]  keys={len(fc.keyframe_points)}")
