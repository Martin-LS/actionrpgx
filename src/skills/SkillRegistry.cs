using System.Collections.Generic;

namespace Godot1.Skills;

public static class SkillRegistry
{
    private static readonly Dictionary<string, SkillData> All = new()
    {
        ["strike"] = new SkillData(
            "strike", "Strike", SkillType.Active,
            Tags: new[] { "Attack" },
            Cooldown: BalanceConfig.Skills.StrikeCooldown, Range: BalanceConfig.Skills.StrikeRange,
            IconPath: "res://assets/icons/items/battle_axe.png"),
    };

    public static SkillData? Get(string id) => All.TryGetValue(id, out var s) ? s : null;
}
