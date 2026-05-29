using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Godot1.Items;

public static class ItemRegistry
{
    public static readonly IReadOnlyDictionary<string, ItemData> All =
        new Dictionary<string, ItemData>
        {
            // Weapons
            ["iron_sword"]      = new("iron_sword",      "Iron Sword",      ItemSlot.Weapon,    0,   0f,  3f, "res://assets/icons/items/iron_sword.png"),
            ["battle_axe"]      = new("battle_axe",      "Battle Axe",      ItemSlot.Weapon,    0, -15f,  6f, "res://assets/icons/items/battle_axe.png"),
            ["enchanted_blade"] = new("enchanted_blade",  "Enchanted Blade", ItemSlot.Weapon,   10,   0f,  2f, "res://assets/icons/items/enchanted_blade.png"),
            // Armor
            ["leather_vest"]    = new("leather_vest",    "Leather Vest",    ItemSlot.Armor,    20,   0f,  0f, "res://assets/icons/items/leather_vest.png"),
            ["chain_mail"]      = new("chain_mail",      "Chain Mail",      ItemSlot.Armor,    40, -10f,  0f, "res://assets/icons/items/chain_mail.png"),
            ["mage_robe"]       = new("mage_robe",       "Mage Robe",       ItemSlot.Armor,    15,  15f,  0f, "res://assets/icons/items/mage_robe.png"),
            // Accessories
            ["swift_ring"]      = new("swift_ring",      "Swift Ring",      ItemSlot.Accessory, 0,  20f,  0f, "res://assets/icons/items/swift_ring.png"),
            ["vitality_charm"]  = new("vitality_charm",  "Vitality Charm",  ItemSlot.Accessory,30,   0f,  0f, "res://assets/icons/items/vitality_charm.png"),
            ["war_band"]        = new("war_band",        "War Band",        ItemSlot.Accessory,10,   0f,  2f, "res://assets/icons/items/war_band.png"),
        };

    public static ItemData? Get(string id) => All.GetValueOrDefault(id);

    public static IEnumerable<ItemData> ForSlot(ItemSlot slot) =>
        All.Values.Where(i => i.Slot == slot);

    public static ItemData RandomDrop()
    {
        var values = All.Values.ToArray();
        return values[GD.RandRange(0, values.Length - 1)];
    }
}
