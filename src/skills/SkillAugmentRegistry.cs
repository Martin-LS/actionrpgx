using System.Collections.Generic;

namespace ActionRpgX.Skills;

public static class SkillAugmentRegistry
{
    private static readonly Dictionary<string, SkillAugmentData> _all = new()
    {
        ["slow"]            = new SkillAugmentData("slow",            "Slow",            new string[] { }, EotId: "slow"),
        ["critical_strike"] = new SkillAugmentData("critical_strike", "Critical Strike", new string[] { }, EotId: null),
        ["magic_damage"]    = new SkillAugmentData("magic_damage",    "Magic Damage",    new string[] { }, EotId: null, ConflictGroup: "damage_type"),
    };

    public static SkillAugmentData?             Get(string id) => _all.TryGetValue(id, out var s) ? s : null;
    public static IEnumerable<SkillAugmentData> GetAll()       => _all.Values;
}
