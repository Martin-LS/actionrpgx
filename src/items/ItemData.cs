namespace Godot2.Items;

public record ItemData(
    string         Id,
    string         Name,
    ItemSlot       Slot,
    string         IconPath            = "",
    // Weapon fields
    float          WeaponRange         = 0f,
    string         PreferredDelivery   = "",
    float          BaseDamage          = 0f,
    DamageType     BaseDamageType      = DamageType.Physical,
    float          DamageBonus         = 0f,  // % bonus to damage of this weapon's type (e.g. 0.10 = +10%)
    float          CritChanceBonus     = 0f,  // flat crit chance added by weapon identity
    // Armor fields
    ArmorCategory  ArmorCategory      = ArmorCategory.None,
    int            BonusHp            = 0,
    float          BonusSpeed         = 0f,
    float          DamageReduction    = 0f,
    float          RangeModifier      = 0f,
    // Accessory fields
    float          PhysicalResistance = 0f
)
{
    public string[] Tags { get; init; } = System.Array.Empty<string>();
}
