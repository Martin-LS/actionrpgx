using Godot;
using System.Collections.Generic;

namespace Godot1.World;

public partial class DungeonGenerator : Node3D
{
    private GridMap _gridMap = null!;
    private readonly List<Vector3> _floorPositions = new();

    private const int   AHalf = 12;       // 24×24 cell arena, -12 to +11
    private const float CL    = 4f;       // cell size local
    private const float SC    = 9f;       // node scale
    private const float CW    = CL * SC;  // cell world size = 36

    public Vector3 SpawnPosition { get; private set; } = Vector3.Zero;

    public override void _Ready()
    {
        var lib = GD.Load<MeshLibrary>("res://assets/models/environment/dungeon/dungeon_library.tres");
        if (lib == null) { GD.PrintErr("DungeonGenerator: dungeon_library.tres not found"); return; }

        _gridMap = new GridMap
        {
            MeshLibrary = lib,
            CellSize    = new Vector3(CL, CL, CL),
            Scale       = new Vector3(SC, SC, SC),
        };
        AddChild(_gridMap);

        BuildFloor();
        BuildWalls();
        AddPerimeterCollision();
        ScatterProps();

        var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (player != null)
            player.GlobalPosition = SpawnPosition;
    }

    private void BuildFloor()
    {
        for (int x = -AHalf; x < AHalf; x++)
            for (int z = -AHalf; z < AHalf; z++)
            {
                _gridMap.SetCellItem(new Vector3I(x, 0, z), 0);
                _floorPositions.Add(CellToWorld(x, z));
            }
    }

    private void BuildWalls()
    {
        var wallScene   = GD.Load<PackedScene>("res://assets/models/environment/dungeon/wall.gltf");
        var cornerScene = GD.Load<PackedScene>("res://assets/models/environment/dungeon/wall_corner.gltf");
        if (wallScene == null || cornerScene == null) { GD.PrintErr("DungeonGenerator: wall scenes not found"); return; }

        // Straight walls along each edge, excluding the 4 corner cells
        // North (z=-AHalf): faces south (+Z into room) — rotation 0
        for (int x = -AHalf + 1; x < AHalf - 1; x++)
            PlaceProp(wallScene, CellToWorld(x, -AHalf), 0f);

        // South (z=AHalf-1): faces north (-Z into room) — rotation PI
        for (int x = -AHalf + 1; x < AHalf - 1; x++)
            PlaceProp(wallScene, CellToWorld(x, AHalf - 1), Mathf.Pi);

        // West (x=-AHalf): faces east (+X into room) — rotation PI/2
        for (int z = -AHalf + 1; z < AHalf - 1; z++)
            PlaceProp(wallScene, CellToWorld(-AHalf, z), Mathf.Pi * 0.5f);

        // East (x=AHalf-1): faces west (-X into room) — rotation 3*PI/2
        for (int z = -AHalf + 1; z < AHalf - 1; z++)
            PlaceProp(wallScene, CellToWorld(AHalf - 1, z), Mathf.Pi * 1.5f);

        // Four corners
        PlaceProp(cornerScene, CellToWorld(-AHalf,    -AHalf    ), 0f);
        PlaceProp(cornerScene, CellToWorld(AHalf - 1, -AHalf    ), Mathf.Pi * 0.5f);
        PlaceProp(cornerScene, CellToWorld(AHalf - 1, AHalf - 1 ), Mathf.Pi);
        PlaceProp(cornerScene, CellToWorld(-AHalf,    AHalf - 1 ), Mathf.Pi * 1.5f);
    }

    private void AddPerimeterCollision()
    {
        var body    = new StaticBody3D();
        AddChild(body);
        float half  = AHalf * CW;     // 432
        float span  = half * 2 + CW;  // 900 (covers wall cells too)

        // One large box per side, placed outside the floor area
        AddBox(body, new Vector3(0, 0, -(half + CW * 0.5f)), new Vector3(span, 100f, CW));
        AddBox(body, new Vector3(0, 0,  (half + CW * 0.5f)), new Vector3(span, 100f, CW));
        AddBox(body, new Vector3(-(half + CW * 0.5f), 0, 0), new Vector3(CW, 100f, span));
        AddBox(body, new Vector3( (half + CW * 0.5f), 0, 0), new Vector3(CW, 100f, span));
    }

    private static void AddBox(StaticBody3D body, Vector3 pos, Vector3 size)
    {
        var cs = new CollisionShape3D { Shape = new BoxShape3D { Size = size } };
        cs.Position = pos;
        body.AddChild(cs);
    }

    private void ScatterProps()
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();

        var pillarScene = GD.Load<PackedScene>("res://assets/models/environment/dungeon/pillar.gltf");
        var barrelScene = GD.Load<PackedScene>("res://assets/models/environment/dungeon/barrel_large.gltf");
        var crateScene  = GD.Load<PackedScene>("res://assets/models/environment/dungeon/crate_large.gltf");
        var torchScene  = GD.Load<PackedScene>("res://assets/models/environment/dungeon/torch_lit.gltf");

        // Pillars in a loose grid, skipping the central 3×3 area
        if (pillarScene != null)
            for (int gx = -2; gx <= 2; gx++)
                for (int gz = -2; gz <= 2; gz++)
                {
                    if (Mathf.Abs(gx) <= 1 && Mathf.Abs(gz) <= 1) continue;
                    int cx = gx * 5 + rng.RandiRange(-1, 1);
                    int cz = gz * 5 + rng.RandiRange(-1, 1);
                    if (cx > -AHalf + 1 && cx < AHalf - 2 && cz > -AHalf + 1 && cz < AHalf - 2)
                        PlaceProp(pillarScene, CellToWorld(cx, cz), 0f);
                }

        // Barrels and crates scattered across the interior
        int inner = AHalf - 3;
        if (barrelScene != null)
            for (int i = 0; i < 12; i++)
                PlaceProp(barrelScene,
                    CellToWorld(rng.RandiRange(-inner, inner), rng.RandiRange(-inner, inner)),
                    rng.Randf() * Mathf.Tau);

        if (crateScene != null)
            for (int i = 0; i < 12; i++)
                PlaceProp(crateScene,
                    CellToWorld(rng.RandiRange(-inner, inner), rng.RandiRange(-inner, inner)),
                    rng.Randf() * Mathf.Tau);

        // Torches along the inside of the wall border, every 5 cells
        if (torchScene != null)
        {
            float torchY = 0.395f * SC; // lift so base sits flush with floor top
            for (int x = -AHalf + 3; x < AHalf - 2; x += 5)
            {
                PlaceProp(torchScene, CellToWorld(x, -AHalf + 1) + new Vector3(0, torchY, 0), 0f);
                PlaceProp(torchScene, CellToWorld(x,  AHalf - 2) + new Vector3(0, torchY, 0), Mathf.Pi);
            }
            for (int z = -AHalf + 3; z < AHalf - 2; z += 5)
            {
                PlaceProp(torchScene, CellToWorld(-AHalf + 1, z) + new Vector3(0, torchY, 0), Mathf.Pi * 0.5f);
                PlaceProp(torchScene, CellToWorld( AHalf - 2, z) + new Vector3(0, torchY, 0), Mathf.Pi * 1.5f);
            }
        }
    }

    private void PlaceProp(PackedScene scene, Vector3 worldPos, float yRot)
    {
        var node = scene.Instantiate<Node3D>();
        node.Scale    = new Vector3(SC, SC, SC);
        node.Rotation = new Vector3(0f, yRot, 0f);
        AddChild(node);
        node.GlobalPosition = worldPos;
    }

    private Vector3 CellToWorld(int x, int z)
        => _gridMap.ToGlobal(_gridMap.MapToLocal(new Vector3I(x, 0, z)));

    public Vector3 GetSpawnPointNear(Vector3 reference, float minDist)
    {
        if (_floorPositions.Count == 0) return reference;
        var candidates = new List<int>();
        for (int i = 0; i < _floorPositions.Count; i++)
            if (_floorPositions[i].DistanceTo(reference) >= minDist)
                candidates.Add(i);
        if (candidates.Count > 0)
            return _floorPositions[candidates[(int)(GD.Randi() % (uint)candidates.Count)]];
        return _floorPositions[(int)(GD.Randi() % (uint)_floorPositions.Count)];
    }
}
