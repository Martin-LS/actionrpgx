using Godot;

namespace Godot1;

public partial class CheckerBackground : Node3D
{
    private const string SheetPath = "res://assets/kenney_topdown_rpg/Roguelike Base Pack/Spritesheet/roguelikeSheet_transparent.png";
    private const int TileSize = 16;
    private const int TileStep = 17; // 16px tile + 1px spacing

    // Confirmed clean flat-ground center tiles (col=5, block center row).
    private static readonly Vector2I[] TerrainTypes =
    {
        new(5,  1),  // green grass (confirmed)
        new(5, 16),  // grey stone  (confirmed)
    };

    // Water-dominant tiles from the water+grass autotile block (rows 0-2).
    // Used to fill random pool patches on top of the base terrain.
    private static readonly Vector2I[] WaterTiles =
    {
        new(0, 1), new(1, 1), new(2, 1), new(3, 1),
    };

    public override void _Ready()
    {
        var sheetImage = Image.LoadFromFile(ProjectSettings.GlobalizePath(SheetPath));
        sheetImage.Convert(Image.Format.Rgba8);

        const int mapTiles = 128;
        const int imgSize  = mapTiles * TileSize; // 2048×2048 px

        var mapImage = Image.CreateEmpty(imgSize, imgSize, false, Image.Format.Rgba8);
        var rng = new RandomNumberGenerator();
        rng.Randomize();

        // Fill base terrain
        var chosen = TerrainTypes[rng.RandiRange(0, TerrainTypes.Length - 1)];
        var baseSrc = new Rect2I(chosen.X * TileStep, chosen.Y * TileStep, TileSize, TileSize);
        for (int ty = 0; ty < mapTiles; ty++)
            for (int tx = 0; tx < mapTiles; tx++)
                mapImage.BlitRect(sheetImage, baseSrc, new Vector2I(tx * TileSize, ty * TileSize));

        // Scatter random water pools on top.
        // Half are placed near spawn centre (tile 64,64) so at least some are always visible.
        int numPools = rng.RandiRange(6, 12);
        int cx = mapTiles / 2;
        for (int p = 0; p < numPools; p++)
        {
            int poolW = rng.RandiRange(4, 10);
            int poolH = rng.RandiRange(4, 10);
            int px, py;
            if (p < numPools / 2)
            {
                // Near-centre — within ±20 tiles of spawn (guaranteed visible)
                px = Mathf.Clamp(cx + rng.RandiRange(-20, 20) - poolW / 2, 2, mapTiles - poolW - 2);
                py = Mathf.Clamp(cx + rng.RandiRange(-20, 20) - poolH / 2, 2, mapTiles - poolH - 2);
            }
            else
            {
                // Anywhere on the map for further variety
                px = rng.RandiRange(2, mapTiles - poolW - 2);
                py = rng.RandiRange(2, mapTiles - poolH - 2);
            }
            var wt   = WaterTiles[rng.RandiRange(0, WaterTiles.Length - 1)];
            var wSrc = new Rect2I(wt.X * TileStep, wt.Y * TileStep, TileSize, TileSize);
            for (int wy = 0; wy < poolH; wy++)
                for (int wx = 0; wx < poolW; wx++)
                    mapImage.BlitRect(sheetImage, wSrc, new Vector2I((px + wx) * TileSize, (py + wy) * TileSize));
        }

        var texture = ImageTexture.CreateFromImage(mapImage);

        var mat = new StandardMaterial3D
        {
            AlbedoTexture = texture,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            ShadingMode   = BaseMaterial3D.ShadingModeEnum.Unshaded,
        };

        var plane = new PlaneMesh { Size = new Vector2(imgSize, imgSize) };
        plane.Material = mat;
        AddChild(new MeshInstance3D { Mesh = plane });
    }
}
