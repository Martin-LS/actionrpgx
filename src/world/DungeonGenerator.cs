using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ActionRpgX.Enemies;
using ActionRpgX.Run;

namespace ActionRpgX.World;

public partial class DungeonGenerator : Node3D
{
    [Signal] public delegate void MapReadyEventHandler();

    private static readonly PackedScene EnemyScene =
        GD.Load<PackedScene>("res://src/enemies/enemy.tscn");

    private readonly List<Vector3>           _floorPositions = new();
    private readonly List<EnemyController>   _placedEnemies  = new();

    private const float RoomSize       = 400f;
    private const float CorridorWidth  = 90f;
    private const float CorridorLength = 160f;
    private const float GridStep       = RoomSize + CorridorLength;
    private const float FloorThick     = 2f;
    private const float WallHeight     = 200f;
    private const float WallThick      = 8f;

    // Side indices: 0=N(-Z)  1=S(+Z)  2=E(+X)  3=W(-X)
    private static readonly int[] Dx = { 0,  0, 1, -1 };
    private static readonly int[] Dz = { 1, -1, 0,  0 };
    // When moving in direction di, which side of the SOURCE room faces the dest?
    private static readonly int[] SrcSide  = { 1, 0, 2, 3 };
    // Which side of the DEST room faces back to source?
    private static readonly int[] DestSide = { 0, 1, 3, 2 };

    public Vector3 SpawnPosition { get; private set; } = Vector3.Zero;

    public override void _Ready()
    {
        var mapData = RunConfig.Pending ?? MapData.GenerateRandom();
        var rng     = new Random(mapData.Seed);

        var rooms     = new List<(int gx, int gz)> { (0, 0) };
        var corridors = new List<(float cx, float cz, float w, float d)>();
        var occupied  = new HashSet<(int, int)> { (0, 0) };

        // Track which sides of each room have a corridor opening
        // connectedSides[(gx,gz)][side] = true means that side has a corridor
        var connectedSides = new Dictionary<(int, int), bool[]>();
        connectedSides[(0, 0)] = new bool[4];

        for (int i = 1; i < mapData.ChunkCount; i++)
        {
            bool placed   = false;
            var roomOrder = Shuffled(rng, rooms.Count);
            foreach (int ri in roomOrder)
            {
                if (placed) break;
                var (rx, rz) = rooms[ri];
                var dirOrder = Shuffled(rng, 4);
                foreach (int di in dirOrder)
                {
                    int nx = rx + Dx[di], nz = rz + Dz[di];
                    if (occupied.Contains((nx, nz))) continue;

                    occupied.Add((nx, nz));
                    rooms.Add((nx, nz));
                    connectedSides[(nx, nz)] = new bool[4];

                    connectedSides[(rx, rz)][SrcSide[di]]  = true;
                    connectedSides[(nx, nz)][DestSide[di]] = true;

                    float rcx = rx * GridStep, rcz = rz * GridStep;
                    float ncx = nx * GridStep, ncz = nz * GridStep;
                    bool  zDir = di < 2;
                    corridors.Add((
                        (rcx + ncx) / 2f,
                        (rcz + ncz) / 2f,
                        zDir ? CorridorWidth  : CorridorLength,
                        zDir ? CorridorLength : CorridorWidth
                    ));

                    placed = true;
                    break;
                }
            }
        }

        SpawnPosition = new Vector3(0f, 0f, 0f);

        var floorBody = new StaticBody3D();
        var wallBody  = new StaticBody3D();
        AddChild(floorBody);
        AddChild(wallBody);

        var floorMat = new StandardMaterial3D { AlbedoColor = new Color("#2e2618") };

        foreach (var (gx, gz) in rooms)
        {
            float cx = gx * GridStep, cz = gz * GridStep;
            AddFloorPatch(cx, cz, RoomSize, RoomSize, floorMat, floorBody);
            AddRoomWalls(cx, cz, connectedSides[(gx, gz)], wallBody);
            _floorPositions.Add(new Vector3(cx, 0f, cz));
        }

        foreach (var (cx, cz, w, d) in corridors)
        {
            AddFloorPatch(cx, cz, w, d, floorMat, floorBody);
            AddCorridorSideWalls(cx, cz, w, d, wallBody);
        }

        ScatterObstacles(rooms, rng);
        PlaceEnemies(rooms, mapData, rng);
        ComputeClusters();
        ConnectEnemySignals();

        var enemiesToAdd = _placedEnemies.ToList();
        Callable.From(() => { foreach (var e in enemiesToAdd) AddChild(e); }).CallDeferred();

        var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (player != null)
            player.GlobalPosition = SpawnPosition;

        BakeNavmesh();
    }

    private void BakeNavmesh()
    {
        var navMesh = new NavigationMesh();
        navMesh.AgentHeight   = 50f;
        navMesh.AgentRadius   = 16f; // slightly > enemy collision sphere radius (14) for wall clearance
        navMesh.AgentMaxClimb = 5f;
        navMesh.CellSize      = 4f;
        navMesh.CellHeight    = 4f;
        navMesh.Set("parsed_geometry_type", 2); // 2 = Both (mesh instances + static colliders)

        // Explicitly parse geometry from DungeonMap (this) so all floors/walls/obstacles are included.
        // Default BakeNavigationMesh() scans the region's own children (empty) — this avoids that trap.
        var sourceData = new NavigationMeshSourceGeometryData3D();
        NavigationServer3D.ParseSourceGeometryData(navMesh, sourceData, this);
        NavigationServer3D.BakeFromSourceGeometryData(navMesh, sourceData);

        var region = new NavigationRegion3D { NavigationMesh = navMesh };
        AddChild(region);

        // Deferred so all _Ready() subscribers have connected before the signal fires
        Callable.From(() =>
        {
            foreach (var e in _placedEnemies) e.SetIdle();
            EmitSignal(SignalName.MapReady);
        }).CallDeferred();
    }

    private void AddFloorPatch(float cx, float cz, float w, float d,
                               StandardMaterial3D mat, StaticBody3D body)
    {
        var pos = new Vector3(cx, -FloorThick / 2f, cz);

        var mi = new MeshInstance3D
        {
            Mesh             = new BoxMesh { Size = new Vector3(w, FloorThick, d) },
            MaterialOverride = mat,
            Position         = pos,
        };
        AddChild(mi);

        body.AddChild(new CollisionShape3D
        {
            Shape    = new BoxShape3D { Size = new Vector3(w, FloorThick, d) },
            Position = pos,
        });
    }

    // Walls on all 4 sides of a room; sides with corridors get a gap opening.
    private static void AddRoomWalls(float cx, float cz, bool[] connected, StaticBody3D body)
    {
        float half  = RoomSize / 2f;
        float cHalf = CorridorWidth / 2f;
        float segLen = (RoomSize - CorridorWidth) / 2f;

        // N (-Z)
        if (!connected[0])
            AddWall(body, new Vector3(cx, WallHeight / 2f, cz - half - WallThick / 2f),
                          new Vector3(RoomSize + WallThick * 2, WallHeight, WallThick));
        else
        {
            AddWall(body, new Vector3(cx - cHalf - segLen / 2f, WallHeight / 2f, cz - half - WallThick / 2f),
                          new Vector3(segLen, WallHeight, WallThick));
            AddWall(body, new Vector3(cx + cHalf + segLen / 2f, WallHeight / 2f, cz - half - WallThick / 2f),
                          new Vector3(segLen, WallHeight, WallThick));
        }

        // S (+Z)
        if (!connected[1])
            AddWall(body, new Vector3(cx, WallHeight / 2f, cz + half + WallThick / 2f),
                          new Vector3(RoomSize + WallThick * 2, WallHeight, WallThick));
        else
        {
            AddWall(body, new Vector3(cx - cHalf - segLen / 2f, WallHeight / 2f, cz + half + WallThick / 2f),
                          new Vector3(segLen, WallHeight, WallThick));
            AddWall(body, new Vector3(cx + cHalf + segLen / 2f, WallHeight / 2f, cz + half + WallThick / 2f),
                          new Vector3(segLen, WallHeight, WallThick));
        }

        // E (+X)
        if (!connected[2])
            AddWall(body, new Vector3(cx + half + WallThick / 2f, WallHeight / 2f, cz),
                          new Vector3(WallThick, WallHeight, RoomSize + WallThick * 2));
        else
        {
            AddWall(body, new Vector3(cx + half + WallThick / 2f, WallHeight / 2f, cz - cHalf - segLen / 2f),
                          new Vector3(WallThick, WallHeight, segLen));
            AddWall(body, new Vector3(cx + half + WallThick / 2f, WallHeight / 2f, cz + cHalf + segLen / 2f),
                          new Vector3(WallThick, WallHeight, segLen));
        }

        // W (-X)
        if (!connected[3])
            AddWall(body, new Vector3(cx - half - WallThick / 2f, WallHeight / 2f, cz),
                          new Vector3(WallThick, WallHeight, RoomSize + WallThick * 2));
        else
        {
            AddWall(body, new Vector3(cx - half - WallThick / 2f, WallHeight / 2f, cz - cHalf - segLen / 2f),
                          new Vector3(WallThick, WallHeight, segLen));
            AddWall(body, new Vector3(cx - half - WallThick / 2f, WallHeight / 2f, cz + cHalf + segLen / 2f),
                          new Vector3(WallThick, WallHeight, segLen));
        }
    }

    // Invisible walls along the narrow sides of a corridor.
    private static void AddCorridorSideWalls(float cx, float cz, float w, float d, StaticBody3D body)
    {
        bool zDir = w < d;
        if (zDir)
        {
            AddWall(body, new Vector3(cx - w / 2f - WallThick / 2f, WallHeight / 2f, cz),
                          new Vector3(WallThick, WallHeight, d));
            AddWall(body, new Vector3(cx + w / 2f + WallThick / 2f, WallHeight / 2f, cz),
                          new Vector3(WallThick, WallHeight, d));
        }
        else
        {
            AddWall(body, new Vector3(cx, WallHeight / 2f, cz - d / 2f - WallThick / 2f),
                          new Vector3(w, WallHeight, WallThick));
            AddWall(body, new Vector3(cx, WallHeight / 2f, cz + d / 2f + WallThick / 2f),
                          new Vector3(w, WallHeight, WallThick));
        }
    }

    private static void AddWall(StaticBody3D body, Vector3 pos, Vector3 size)
    {
        body.AddChild(new CollisionShape3D
        {
            Shape    = new BoxShape3D { Size = size },
            Position = pos,
        });
    }

    private void ScatterObstacles(List<(int gx, int gz)> rooms, Random rng)
    {
        var stumpMat = new StandardMaterial3D { AlbedoColor = new Color("#2e1008") };
        var rockMat  = new StandardMaterial3D { AlbedoColor = new Color("#3a3028") };
        var logMat   = new StandardMaterial3D { AlbedoColor = new Color("#3c2818") };

        foreach (var (gx, gz) in rooms)
        {
            if (gx == 0 && gz == 0) continue;

            float cx   = gx * GridStep;
            float cz   = gz * GridStep;
            float half = RoomSize / 2f * 0.65f;

            for (int i = 0; i < rng.Next(1, 4); i++)
                PlaceObstacle(RandomPoint(rng, cx, cz, half), new Vector3(18, 55, 18), stumpMat);

            for (int i = 0; i < rng.Next(1, 3); i++)
                PlaceObstacle(RandomPoint(rng, cx, cz, half), new Vector3(38, 28, 38), rockMat);

            if (rng.Next(2) == 0)
                PlaceObstacle(RandomPoint(rng, cx, cz, half), new Vector3(15, 18, 90), logMat,
                              (float)(rng.NextDouble() * Math.PI));
        }
    }

    private void PlaceObstacle(Vector3 floorPos, Vector3 size, StandardMaterial3D mat, float yRot = 0f)
    {
        var pos = new Vector3(floorPos.X, size.Y / 2f, floorPos.Z);

        AddChild(new MeshInstance3D
        {
            Mesh             = new BoxMesh { Size = size },
            MaterialOverride = mat,
            Position         = pos,
            Rotation         = new Vector3(0f, yRot, 0f),
        });

        var sb = new StaticBody3D { Position = pos, Rotation = new Vector3(0f, yRot, 0f) };
        sb.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
        AddChild(sb);
    }

    private static Vector3 RandomPoint(Random rng, float cx, float cz, float half)
        => new Vector3(
            cx + (float)(rng.NextDouble() * 2 - 1) * half,
            0f,
            cz + (float)(rng.NextDouble() * 2 - 1) * half);

    private static List<int> Shuffled(Random rng, int count)
    {
        var list = new List<int>(count);
        for (int i = 0; i < count; i++) list.Add(i);
        for (int i = count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    private void PlaceEnemies(List<(int gx, int gz)> rooms, MapData mapData, Random rng)
    {
        int poolTotal = 0;
        foreach (var e in mapData.EnemyPool) poolTotal += e.Count;
        if (poolTotal <= 0) poolTotal = 1;

        const float Margin     = 60f;
        const float MinSpacing = 80f;
        float half = RoomSize / 2f - Margin;

        foreach (var (gx, gz) in rooms)
        {
            if (gx == 0 && gz == 0) continue; // skip player spawn room

            float cx    = gx * GridStep;
            float cz    = gz * GridStep;
            int   count = rng.Next(mapData.MinEnemiesPerRoom, mapData.MaxEnemiesPerRoom + 1);
            var   spots = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                Vector3 pos   = Vector3.Zero;
                bool    found = false;
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    float px = cx + (float)(rng.NextDouble() * 2 - 1) * half;
                    float pz = cz + (float)(rng.NextDouble() * 2 - 1) * half;
                    pos = new Vector3(px, 0f, pz);
                    bool tooClose = false;
                    foreach (var s in spots)
                        if (s.DistanceTo(pos) < MinSpacing) { tooClose = true; break; }
                    if (!tooClose) { found = true; break; }
                }
                if (!found) continue;

                var entry = PickPoolEntry(mapData.EnemyPool, poolTotal, rng);
                var enemy = EnemyScene.Instantiate<EnemyController>();
                ApplyEnemyEntry(enemy, entry, mapData.Level);
                enemy.Position = pos;
                _placedEnemies.Add(enemy);
                spots.Add(pos);
            }
        }
    }

    private static EnemyPoolEntry PickPoolEntry(List<EnemyPoolEntry> pool, int total, Random rng)
    {
        int roll = rng.Next(total);
        int acc  = 0;
        foreach (var e in pool)
        {
            acc += e.Count;
            if (roll < acc) return e;
        }
        return pool[0];
    }

    private static void ApplyEnemyEntry(EnemyController enemy, EnemyPoolEntry entry, int mapLevel)
    {
        var data = EnemyRegistry.Get(entry.EnemyType);
        enemy.Speed              = data.BaseSpeed + entry.SpeedBonus;
        enemy.MaxHealth          = data.BaseHealth + entry.HpBonus;
        enemy.ContactDamage      = data.ContactDamage + entry.DamageBonus;
        enemy.DamageInterval     = data.DamageInterval;
        enemy.PhysicalResistance = data.PhysicalResistance + entry.ArmorBonus * 0.01f;
        enemy.MagicResistance    = data.MagicResistance;
        enemy.ModelPath          = data.ModelPath;
        enemy.MapLevel           = mapLevel;
    }

    private void ComputeClusters()
    {
        float radius  = BalanceConfig.Enemies.ClusterProximityRadiusTiles * GameScale.TileSize;
        int   nextId  = 0;
        var   visited = new HashSet<int>();

        for (int i = 0; i < _placedEnemies.Count; i++)
        {
            if (visited.Contains(i)) continue;
            int id    = nextId++;
            var queue = new Queue<int>();
            queue.Enqueue(i);
            visited.Add(i);
            _placedEnemies[i].ClusterId = id;

            while (queue.Count > 0)
            {
                int cur = queue.Dequeue();
                for (int j = 0; j < _placedEnemies.Count; j++)
                {
                    if (visited.Contains(j)) continue;
                    if (_placedEnemies[cur].Position.DistanceTo(_placedEnemies[j].Position) <= radius)
                    {
                        visited.Add(j);
                        _placedEnemies[j].ClusterId = id;
                        queue.Enqueue(j);
                    }
                }
            }
        }
    }

    private void ConnectEnemySignals()
    {
        var session = GetParent().GetNodeOrNull<RunSession>("RunSession");
        if (session == null) return;
        session.SetTotalEnemies(_placedEnemies.Count);
        foreach (var enemy in _placedEnemies)
            enemy.Died += session.OnEnemyDied;
    }
}
