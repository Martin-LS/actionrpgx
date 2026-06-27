# Step 3 — Export GLB
# Run this after 01_decimate.py and 02_style.py.
# Set CHARACTER_NAME before running — this becomes the filename.

CHARACTER_NAME = 'enemy_grunt'  # snake_case, no extension, no spaces

# -----------------------------------------------------------------------

import bpy
import os

blend_path = bpy.data.filepath
if not blend_path:
    raise RuntimeError("Save the .blend file before exporting.")

# src/ → characters/ → models/ → assets/ → project root
blend_dir    = os.path.dirname(blend_path)           # .../src/
chars_dir    = os.path.dirname(blend_dir)             # .../characters/
models_dir   = os.path.dirname(chars_dir)             # .../models/
assets_dir   = os.path.dirname(models_dir)            # .../assets/
project_root = os.path.dirname(assets_dir)            # project root

output_path = os.path.join(
    project_root, 'assets', 'models', 'characters', f'{CHARACTER_NAME}.glb'
)

bpy.ops.export_scene.gltf(
    filepath=output_path,
    use_visible=True,
    export_apply=True,
    export_yup=True,
    export_animations=True,
    export_nla_strips=True,
    export_cameras=False,
    export_lights=False,
)

print(f"Exported: {output_path}")
