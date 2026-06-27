# Step 2 — Assign Flat Palette Materials by Bone Region
# Run this after 01_decimate.py and after the mesh is fitted to the shared armature.
# Each mesh object gets one flat Principled BSDF material determined by its dominant bone.
#
# COLOR NOTE: These colors are placeholders from the initial build. The full color scheme
# in docs/visuals-style.md will be revised — update PALETTE here when that happens.

# Linear color values (Blender uses linear, not sRGB hex)
PALETTE = {
    'skin':  (0.91, 0.72, 0.54, 1.0),
    'shirt': (0.10, 0.18, 0.38, 1.0),
    'pants': (0.18, 0.13, 0.09, 1.0),
    'boot':  (0.08, 0.05, 0.03, 1.0),
}

# Bone name → material zone
# Adjust per character type — e.g. an undead enemy might map Head → 'pants' color for grey skin
BONE_ZONE = {
    'Head':       'skin',
    'Neck':       'skin',
    'Chest':      'shirt',
    'Spine':      'shirt',
    'UpperArm_L': 'shirt',
    'UpperArm_R': 'shirt',
    'LowerArm_L': 'skin',
    'LowerArm_R': 'skin',
    'Hand_L':     'skin',
    'Hand_R':     'skin',
    'Hips':       'pants',
    'UpperLeg_L': 'pants',
    'UpperLeg_R': 'pants',
    'LowerLeg_L': 'pants',
    'LowerLeg_R': 'pants',
    'Foot_L':     'boot',
    'Foot_R':     'boot',
}

# -----------------------------------------------------------------------

import bpy

# Create one material per zone
zone_mats = {}
for zone, color in PALETTE.items():
    mat = bpy.data.materials.new(name=f"Zone_{zone}")
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Roughness"].default_value = 1.0
    bsdf.inputs["Metallic"].default_value = 0.0
    zone_mats[zone] = mat

unresolved = []

for obj in bpy.data.objects:
    if obj.type != 'MESH':
        continue

    # Find dominant bone via average vertex group weight
    vg_totals = {}
    for vg in obj.vertex_groups:
        if vg.name not in BONE_ZONE:
            continue
        total = 0.0
        count = 0
        for v in obj.data.vertices:
            for g in v.groups:
                if g.group == vg.index:
                    total += g.weight
                    count += 1
        if count > 0:
            vg_totals[vg.name] = total / count

    if not vg_totals:
        print(f"WARNING: {obj.name} — no recognised bone weights, skipping")
        unresolved.append(obj.name)
        continue

    dominant = max(vg_totals, key=vg_totals.get)
    zone = BONE_ZONE[dominant]
    mat = zone_mats[zone]

    obj.data.materials.clear()
    obj.data.materials.append(mat)
    print(f"{obj.name} → {dominant} → {zone}")

if unresolved:
    print(f"\nUnresolved objects (no bone weights): {unresolved}")
    print("These need manual material assignment or weight painting before export.")
else:
    print("\nDone — all objects styled.")
