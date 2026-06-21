using System.Collections.Generic;

namespace Godot2.Skills;

public static class SkillAugmentRegistry
{
    private static readonly Dictionary<string, SkillAugmentData> _all = new()
    {
        ["splash"]           = new SkillAugmentData("splash",           "Splash",           new[] { "Melee" },   EotId: null),
        ["pierce"]           = new SkillAugmentData("pierce",           "Pierce",           new[] { "Range" },  EotId: null),
        ["slow"]             = new SkillAugmentData("slow",             "Slow",             new[] { "Attack" },  EotId: "slow"),
        ["critical_strike"]  = new SkillAugmentData("critical_strike",  "Critical Strike",  new[] { "Attack" },  EotId: null),
        ["magic_damage"]     = new SkillAugmentData("magic_damage",     "Magic Damage",     new[] { "Attack" },  EotId: null, ConflictGroup: "damage_type"),
    };

    public static SkillAugmentData?             Get(string id) => _all.TryGetValue(id, out var s) ? s : null;
    public static IEnumerable<SkillAugmentData> GetAll()       => _all.Values;
}
