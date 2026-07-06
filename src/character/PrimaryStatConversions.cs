namespace ActionRpgX.Character;

// All rates TBD — Balancer-owned.
// Damage scales by DELIVERY, not damage type: Str→Melee, Dex→Ranged, Int→Spell
// (design-stats.md delivery-scaling, 2026-07-06). Melee/Spell multipliers calibrated
// so Warrior str=20 → 1.5× and Mage int=20 → 1.5×, matching the old archetype table
// at level 1; Ranged mirrors them so a Dex build scales ranged the same way.
public static class PrimaryStatConversions
{
    // Str → derived stats
    public const float StrToMeleeDamageMultiplier = 0.075f;
    public const float StrToMaxHp                 = 2f;
    public const float StrToPhysResistance        = 0.002f;
    public const float StrToCritDamage            = 0.01f;

    // Dex → derived stats
    public const float DexToRangedDamageMultiplier = 0.075f;
    public const float DexToCritChance             = 0.003f;
    public const float DexToEvasion                = 0.002f;

    // Int → derived stats
    public const float IntToSpellDamageMultiplier = 0.075f;
    public const float IntToMaxFocus              = 2f;
    public const float IntToMagResistance         = 0.002f;
    public const float IntToFocusRegen            = 0.1f;
}
