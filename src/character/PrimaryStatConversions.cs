namespace ActionRpgX.Character;

// All rates TBD — Balancer-owned.
// Str/Int damage multipliers calibrated so Warrior str=20 → 1.5× and Mage int=20 → 1.5×,
// matching the old archetype multiplier table at level 1.
public static class PrimaryStatConversions
{
    // Str → derived stats
    public const float StrToPhysDamageMultiplier = 0.075f;
    public const float StrToMaxHp                = 2f;
    public const float StrToPhysResistance       = 0f;
    public const float StrToCritDamage           = 0.01f;

    // Dex → derived stats
    public const float DexToCritChance = 0.003f;
    public const float DexToEvasion    = 0.002f;

    // Int → derived stats
    public const float IntToMagDamageMultiplier = 0.075f;
    public const float IntToMaxFocus            = 2f;
    public const float IntToMagResistance       = 0f;
    public const float IntToFocusRegen          = 0.1f;
}
