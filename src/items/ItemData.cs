namespace Godot1.Items;

public record ItemData(
    string    Id,
    string    Name,
    ItemSlot  Slot,
    int       BonusHp,
    float     BonusSpeed,
    float     BonusDamage
);
