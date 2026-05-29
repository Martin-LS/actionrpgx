using Godot;

namespace Godot1;

public partial class CheckerBackground : Node3D
{
    private const string SheetPath = "res://assets/kenney_topdown_rpg/Roguelike Base Pack/Spritesheet/roguelikeSheet_transparent.png";
    private const int TileSize = 16;
    private const int TileStep = 17; // 16px tile + 1px spacing

    // Ground tiles decoded from Kenney's TMX sample map Ground/terrain layer
    private static readonly Vector2I[] GroundTiles =
    {
        new(2, 0), new(3, 0), new(4, 0), new(5, 0),
        new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1), new(5, 1),
        new(1, 2), new(2, 2), new(3, 2), new(4, 2),
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

        // Each run picks a primary tile type; 15% chance of variation per tile
        var primary = GroundTiles[rng.RandiRange(0, GroundTiles.Length - 1)];

        for (int ty = 0; ty < mapTiles; ty++)
        {
            for (int tx = 0; tx < mapTiles; tx++)
            {
                var tile = rng.Randf() < 0.85f
                    ? primary
                    : GroundTiles[rng.RandiRange(0, GroundTiles.Length - 1)];

                mapImage.BlitRect(
                    sheetImage,
                    new Rect2I(tile.X * TileStep, tile.Y * TileStep, TileSize, TileSize),
                    new Vector2I(tx * TileSize, ty * TileSize));
            }
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
