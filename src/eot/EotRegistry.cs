using System.Collections.Generic;

namespace Godot1.Eot;

public static class EotRegistry
{
    private static readonly Dictionary<string, EotData> _all = new()
    {
        ["slow"] = new EotData(
            Id:           "slow",
            Name:         "Slow",
            ApplyChance:  BalanceConfig.Eots.SlowApplyChance,
            Duration:     BalanceConfig.Eots.SlowDuration,
            IsDamageEot:  false,
            SlowFraction: BalanceConfig.Eots.SlowFraction
        ),
        ["burn"] = new EotData(
            Id:           "burn",
            Name:         "Burn",
            ApplyChance:  BalanceConfig.Eots.BurnApplyChance,
            Duration:     BalanceConfig.Eots.BurnDuration,
            IsDamageEot:  true,
            TickRate:     BalanceConfig.Eots.BurnTickRate,
            DamagePerTick: BalanceConfig.Eots.BurnDamagePerTick
        ),
    };

    public static EotData?             Get(string id) => _all.TryGetValue(id, out var e) ? e : null;
    public static IEnumerable<EotData> GetAll()       => _all.Values;
}
