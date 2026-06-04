"""
Print attack action fcurves for arm bones (Blender 5.1 layered action API).
"""
import bpy

bpy.ops.wm.open_mainfile(filepath=r"C:\work\my\github\godot1\assets\models\characters\player.blend")

attack = bpy.data.actions.get("attack")
if not attack:
    print("ERROR: no 'attack' action found")
else:
    print(f"attack: frames {attack.frame_range[0]:.0f}-{attack.frame_range[1]:.0f}  legacy={attack.is_action_legacy}")
    for layer in attack.layers:
        for strip in layer.strips:
            for cb in strip.channelbags:
                for fc in cb.fcurves:
                    if any(b in fc.data_path for b in ["UpperArm", "Forearm", "Hand_R", "Chest"]):
                        vals = [(int(kp.co[0]), round(kp.co[1], 4)) for kp in fc.keyframe_points]
                        print(f"  {fc.data_path}[{fc.array_index}]  {vals}")
