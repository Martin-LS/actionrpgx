namespace Godot2;

public static class GameScale
{
    // 1 tile in world units. Derived from DungeonGenerator: CL (4) × SC (9) = 36.
    // All gameplay distances (WeaponRange, RangeModifier) are authored in tiles and
    // multiplied by this constant at runtime. Update here if the map tile scale changes.
    public const float TileSize = 36f;
}
