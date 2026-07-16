using System.Collections.Generic;
using ActionRpgX.Stats;

namespace ActionRpgX.Buffs;

public static class BuffRegistry
{
    private static readonly Dictionary<string, BuffData> _all = new()
    {
        ["steel_wall"] = new BuffData(
            Id: "steel_wall",
            Name: "Steel Wall",
            Modifiers: new[] { new BuffModifier(StatId.PhysicalResistance, ModifierType.FlatAdd, 0.15f) }
        ),
        ["haste"] = new BuffData(
            Id: "haste",
            Name: "Haste",
            Modifiers: new[] { new BuffModifier(StatId.Speed, ModifierType.FlatAdd, 25f) }
        ),
        ["focus_shield"] = new BuffData(
            Id: "focus_shield",
            Name: "Focus Shield",
            Modifiers: new[] { new BuffModifier(StatId.MaxFocus, ModifierType.FlatAdd, 30f) }
        )
    };

    public static BuffData?             Get(string id) => _all.TryGetValue(id, out var b) ? b : null;
    public static IEnumerable<BuffData> GetAll()       => _all.Values;
}
