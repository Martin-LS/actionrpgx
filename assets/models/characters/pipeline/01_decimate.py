# Step 1 — Decimate + Flat Shade + Clean Geometry
# Run this in Blender via execute_blender_code after importing the ComfyUI mesh.
# Set CHARACTER_TYPE to match the character being processed.

CHARACTER_TYPE = 'enemy'  # 'player' | 'enemy' | 'boss'

TARGET_TRIS = {
    'player': 800,
    'enemy':  400,
    'boss':   1200,
}

# -----------------------------------------------------------------------

import bpy

target = TARGET_TRIS[CHARACTER_TYPE]

for obj in bpy.data.objects:
    if obj.type != 'MESH':
        continue

    current_tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    print(f"{obj.name}: {current_tris} tris")

    if current_tris <= target:
        print(f"  already at or under target, skipping")
        continue

    ratio = target / current_tris
    mod = obj.modifiers.new("Decimate", 'DECIMATE')
    mod.ratio = max(ratio, 0.01)

    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier="Decimate")
    obj.select_set(False)

    final_tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    print(f"  decimated to {final_tris} tris")

# Flat shading — select mesh objects only
bpy.ops.object.select_all(action='DESELECT')
for obj in bpy.data.objects:
    if obj.type == 'MESH':
        obj.select_set(True)
bpy.ops.object.shade_flat()
print("Flat shading applied.")

# Clean loose geometry
for obj in bpy.data.objects:
    if obj.type != 'MESH':
        continue
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.delete_loose()
    bpy.ops.object.mode_set(mode='OBJECT')

bpy.ops.object.select_all(action='DESELECT')
print("Done — decimate, flat shade, clean geometry complete.")
