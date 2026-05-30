using System.Collections.Generic;
using Godot1.Items;

namespace Godot1.Skills;

public static class SkillRegistry
{
    private static readonly Dictionary<string, SkillData> All = new()
    {
        ["attack_melee"] = new SkillData(
            "attack_melee", "Strike", SkillType.Active,
            SkillCategory.Melee, Cooldown: 0.8f, Range: 200f,
            IconPath: "res://assets/icons/items/battle_axe.png"),

        ["attack_ranged_physical"] = new SkillData(
            "attack_ranged_physical", "Arrow", SkillType.Active,
            SkillCategory.RangedPhysical, Cooldown: 0.8f, Range: 400f,
            IconPath: "res://assets/icons/items/war_band.png"),

        ["attack_ranged_magic"] = new SkillData(
            "attack_ranged_magic", "Bolt", SkillType.Active,
            SkillCategory.RangedMagic, Cooldown: 0.8f, Range: 400f,
            IconPath: "res://assets/icons/items/enchanted_blade.png"),
    };

    public static SkillData? Get(string id) => All.TryGetValue(id, out var s) ? s : null;
}
