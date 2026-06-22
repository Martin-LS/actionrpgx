using System.Collections.Generic;

namespace ActionRpgX.Character;

public static class PrimaryStatGainRegistry
{
    public record PrimaryStatGains(
        float StrBase, float StrPerLevel,
        float DexBase, float DexPerLevel,
        float IntBase, float IntPerLevel);

    // All values TBD — Balancer-owned.
    // Calibrated so primary stats at level 1 reproduce old archetype multiplier output.
    private static readonly Dictionary<CharacterType, PrimaryStatGains> _gains = new()
    {
        [CharacterType.Warrior] = new(20f, 3f,    5f, 0.5f,  7f, 0.5f),
        [CharacterType.Rogue]   = new(13f, 1.5f, 15f, 2f,    7f, 0.5f),
        [CharacterType.Mage]    = new( 7f, 0.5f,  8f, 1f,   20f, 3f),
    };

    public static PrimaryStatGains Get(CharacterType type) =>
        _gains.TryGetValue(type, out var g) ? g : _gains[CharacterType.Warrior];
}
