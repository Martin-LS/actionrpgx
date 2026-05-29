namespace Godot1.Enemies;

public record EnemyData(
    string EnemyType,
    int    SpriteRow,
    float  BaseSpeed,
    int    BaseHealth,
    int    ContactDamage,
    float  DamageInterval = 1f
);

public static class EnemyRegistry
{
    public static readonly EnemyData Standard = new("standard", 6, 260f, 1, 10);
    public static readonly EnemyData Runner   = new("runner",   4, 400f, 1,  8);
    public static readonly EnemyData Tank     = new("tank",     2, 160f, 1, 18);
}
