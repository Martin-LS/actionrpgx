"""
Import player.glb and print all fcurve channels on the idle animation.
Run via: blender --background --python tools/inspect_idle_fcurves.py
"""
import bpy

bpy.ops.import_scene.gltf(filepath=r"C:\work\my\github\godot1\assets\models\characters\player.glb")

for action in bpy.data.actions:
    if action.name != "idle":
        continue
    print(f"=== {action.name} ===")
    # Try old API (Blender 4.x)
    if hasattr(action, 'fcurves'):
        for fc in action.fcurves:
            print(f"  {fc.data_path}[{fc.array_index}]  keys={[round(k.co.y,4) for k in fc.keyframe_points]}")
    else:
        # Blender 5.x layered actions
        for layer in action.layers:
            for strip in layer.strips:
                if hasattr(strip, 'channelbags'):
                    for bag in strip.channelbags:
                        for fc in bag.fcurves:
                            print(f"  {fc.data_path}[{fc.array_index}]  keys={[round(k.co.y,4) for k in fc.keyframe_points]}")
