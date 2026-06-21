using Godot;

namespace ActionRpgX.Items;

public static class ItemTier
{
    public const int Common   = 1;
    public const int Uncommon = 2;
    public const int Rare     = 3;
    public const int Max      = Rare;

    public static string Label(int tier) => tier switch
    {
        Common   => "Common",
        Uncommon => "Uncommon",
        Rare     => "Rare",
        _        => "Unknown",
    };

    public static Color BorderColor(int tier) => tier switch
    {
        Common   => new Color("#4A5560"),
        Uncommon => new Color("#6B8090"),
        Rare     => new Color("#A07810"),
        _        => new Color("#4A5560"),
    };
}
