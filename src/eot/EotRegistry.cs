using System.Collections.Generic;

namespace ActionRpgX.Eot;

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

        // Signature ailments (element wave) — one per identity, applied innately at flatten time.
        ["bleed"] = new EotData(
            Id:            "bleed",
            Name:          "Bleed",
            ApplyChance:   BalanceConfig.Eots.SignatureApplyChance,
            Duration:      BalanceConfig.Eots.BleedDuration,
            IsDamageEot:   true,
            TickRate:      BalanceConfig.Eots.BleedTickRate,
            DamagePerTick: BalanceConfig.Eots.BleedDamagePerTick
        ),
        ["burn"] = new EotData(
            Id:            "burn",
            Name:          "Burn",
            ApplyChance:   BalanceConfig.Eots.SignatureApplyChance,
            Duration:      BalanceConfig.Eots.BurnDuration,
            IsDamageEot:   true,
            TickRate:      BalanceConfig.Eots.BurnTickRate,
            DamagePerTick: BalanceConfig.Eots.BurnDamagePerTick
        ),
        ["chill"] = new EotData(
            Id:           "chill",
            Name:         "Chill",
            ApplyChance:  BalanceConfig.Eots.SignatureApplyChance,
            Duration:     BalanceConfig.Eots.ChillDuration,
            IsDamageEot:  false,
            SlowFraction: BalanceConfig.Eots.ChillSlowFraction
        ),
        ["shock"] = new EotData(
            Id:             "shock",
            Name:           "Shock",
            ApplyChance:    BalanceConfig.Eots.SignatureApplyChance,
            Duration:       BalanceConfig.Eots.ShockDuration,
            IsDamageEot:    false,
            DamageTakenAmp: BalanceConfig.Eots.ShockDamageTakenAmp
        ),
    };

    public static EotData?             Get(string id) => _all.TryGetValue(id, out var e) ? e : null;
    public static IEnumerable<EotData> GetAll()       => _all.Values;
}
