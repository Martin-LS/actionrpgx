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
            FocusCost: BalanceConfig.Focus.StrikeFocusCost,
            IconPath: "res://assets/icons/items/battle_axe.png"),

        ["cyclone"] = new SkillData(
            "cyclone", "Cyclone", SkillType.Channeled,
            Tags: new[] { "Melee", "Attack" },
            Cooldown: BalanceConfig.Skills.CycloneCooldown, Range: BalanceConfig.Skills.CycloneRange,
            FocusCost: BalanceConfig.Focus.CycloneFocusCostPerSec),

        ["nova"] = new SkillData(
            "nova", "Nova", SkillType.Active,
            Tags: new[] { "Attack", "Burst" },
            Cooldown: BalanceConfig.Skills.NovaCooldown, Range: BalanceConfig.Skills.NovaRange,
            FocusCost: BalanceConfig.Focus.NovaFocusCost),

        ["damage_aura"] = new SkillData(
            "damage_aura", "Damage Aura", SkillType.Aura,
            Tags: new[] { "Aura" },
            Cooldown: BalanceConfig.Skills.DamageAuraCooldown, Range: BalanceConfig.Skills.DamageAuraRange,
            FocusCost: BalanceConfig.Focus.DamageAuraReservation),
    };

    public static SkillData? Get(string id) => All.TryGetValue(id, out var s) ? s : null;
}
