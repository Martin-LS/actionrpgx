using ActionRpgX.Items;

namespace ActionRpgX.Skills;

// IdentityData.cs
public record IdentityData(
    string     Id,        // "physical", "magic"
    DamageType DamageType,
    string     VfxKey     // consumed by the VFX issue; "" allowed until then
);
