using System.Collections.Generic;
using ActionRpgX.Items;

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
            DamagePerTick: BalanceConfig.Eots.BleedDamagePerTick,
            DamageType:    DamageType.Physical
        ),
        ["burn"] = new EotData(
            Id:            "burn",
            Name:          "Burn",
            ApplyChance:   BalanceConfig.Eots.SignatureApplyChance,
            Duration:      BalanceConfig.Eots.BurnDuration,
            IsDamageEot:   true,
            TickRate:      BalanceConfig.Eots.BurnTickRate,
            DamagePerTick: BalanceConfig.Eots.BurnDamagePerTick,
            DamageType:    DamageType.Fire
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
        ["poison"] = new EotData(
            Id:            "poison",
            Name:          "Poison",
            ApplyChance:   BalanceConfig.Eots.SignatureApplyChance,
            Duration:      BalanceConfig.Eots.PoisonDuration,
            IsDamageEot:   true,
            TickRate:      BalanceConfig.Eots.PoisonTickRate,
            DamagePerTick: BalanceConfig.Eots.PoisonDamagePerTick,
            DamageType:    DamageType.Poison,
            MaxStacks:     BalanceConfig.Eots.PoisonMaxStacks
        ),
        ["decay"] = new EotData(
            Id:                    "decay",
            Name:                  "Decay",
            ApplyChance:           BalanceConfig.Eots.SignatureApplyChance,
            Duration:              BalanceConfig.Eots.DecayDuration,
            IsDamageEot:           true,
            TickRate:              BalanceConfig.Eots.DecayTickRate,
            DamagePerTickFraction: BalanceConfig.Eots.DecayDamageFraction,
            DamageType:            DamageType.Void,
            MaxStacks:             1
        ),
    };

    public static EotData?             Get(string id) => _all.TryGetValue(id, out var e) ? e : null;
    public static IEnumerable<EotData> GetAll()       => _all.Values;
}
