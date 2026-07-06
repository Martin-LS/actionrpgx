using ActionRpgX.Items;

namespace ActionRpgX.Skills;

// IdentityData.cs
public record IdentityData(
    string     Id,        // "physical", "fire", "cold", "lightning"
    DamageType DamageType,
    string     VfxKey,        // consumed by the VFX issue; "" allowed until then
    string?    SignatureEotId // innate signature ailment for this element (element wave); null = none
);
