using System.Collections.Generic;

namespace ActionRpgX.Items;

public static class EquipmentAugmentRegistry
{
    private static readonly Dictionary<string, EquipmentAugmentData> _all = new()
    {
        ["retaliation"] = new("retaliation", "Retaliation"),
        ["fortify"]     = new("fortify",     "Fortify"),
        ["dash_reflex"] = new("dash_reflex", "Dash Reflex"),
        ["ghost_step"]  = new("ghost_step",  "Ghost Step"),
        ["mending"]     = new("mending",     "Mending"),
    };

    public static EquipmentAugmentData?             Get(string id) => _all.TryGetValue(id, out var a) ? a : null;
    public static IEnumerable<EquipmentAugmentData> GetAll()       => _all.Values;
}
