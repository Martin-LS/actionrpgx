"""
Compresses attack animation keyframe times by 55% (39 frames → ~22 frames).
Result plays in ~0.73s at 30fps instead of 1.3s — more visible snap at camera distance.
All bone values stay the same; only the frame positions change.
Run via: blender --background --python tools/compress_attack_timing.py
"""
import bpy

BLEND = r"C:\work\my\github\godot1\assets\models\characters\player.blend"
OUT   = r"C:\work\my\github\godot1\assets\models\characters\player.glb"

SCALE = 0.55  # new_frame = round(old_frame * SCALE)

bpy.ops.wm.open_mainfile(filepath=BLEND)

attack = bpy.data.actions.get("attack")
if not attack:
    print("ERROR: 'attack' action not found")
    raise SystemExit(1)

for layer in attack.layers:
    for strip in layer.strips:
        for cb in strip.channelbags:
            for fc in cb.fcurves:
                for kp in fc.keyframe_points:
                    original = kp.co[0]
                    compressed = round(original * SCALE)
                    kp.co[0] = float(compressed)
                    kp.handle_left.x  = round(kp.handle_left.x  * SCALE)
                    kp.handle_right.x = round(kp.handle_right.x * SCALE)

print(f"Attack keyframes compressed to {SCALE*100:.0f}% of original timing.")
print(f"New duration: ~{round(39 * SCALE)} frames ({round(39 * SCALE / 30, 2)}s at 30fps)")

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
