"""
Print all bones animated in the attack action to see what's available.
"""
import bpy

bpy.ops.wm.open_mainfile(filepath=r"C:\work\my\github\godot1\assets\models\characters\player.blend")

attack = bpy.data.actions.get("attack")
print(f"attack: frames {attack.frame_range[0]:.0f}-{attack.frame_range[1]:.0f}")
for layer in attack.layers:
    for strip in layer.strips:
        for cb in strip.channelbags:
            for fc in cb.fcurves:
                vals = [(int(kp.co[0]), round(kp.co[1], 3)) for kp in fc.keyframe_points]
                print(f"  {fc.data_path}[{fc.array_index}]  {vals}")
