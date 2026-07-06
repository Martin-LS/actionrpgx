using System;
using System.Collections.Generic;

namespace ActionRpgX.Skills;

public static class FormRegistry
{
    private static readonly Dictionary<string, FormData> All = new()
    {
        ["salvo"] = new FormData(
            Id: "salvo",
            PrototypeId: "entity_burst",
            TierTrack: TierTrack.CooldownDown,
            Cooldown: BalanceConfig.Forms.SalvoCooldown,
            FocusCost: BalanceConfig.Forms.SalvoFocusCost,
            SubHits: BalanceConfig.Forms.SalvoSubHits
        ),
        ["swift"] = new FormData(
            Id: "swift",
            PrototypeId: "entity_burst",
            TierTrack: TierTrack.CooldownDown,
            Cooldown: BalanceConfig.Forms.SwiftCooldown,
            FocusCost: BalanceConfig.Forms.SwiftFocusCost
        ),
        ["heavy"] = new FormData(
            Id: "heavy",
            PrototypeId: "entity_burst",
            TierTrack: TierTrack.FocusCostDown,
            Cooldown: BalanceConfig.Forms.HeavyCooldown,
            FocusCost: BalanceConfig.Forms.HeavyFocusCost,
            WindUp: BalanceConfig.Forms.HeavyWindUp
        ),
        ["nova"] = new FormData(
            Id: "nova",
            PrototypeId: "self_burst",
            TierTrack: TierTrack.CooldownDown,
            Cooldown: BalanceConfig.Forms.NovaCooldown,
            Range: BalanceConfig.Forms.NovaRange
        ),
        ["quake"] = new FormData(
            Id: "quake",
            PrototypeId: "self_burst",
            TierTrack: TierTrack.RadiusUp,
            Cooldown: BalanceConfig.Forms.QuakeCooldown,
            WindUp: BalanceConfig.Forms.QuakeWindUp,
            Range: BalanceConfig.Forms.QuakeRange
        ),
        ["storm"] = new FormData(
            Id: "storm",
            PrototypeId: "fixed_zone_tick",
            TierTrack: TierTrack.RadiusUp,
            Cooldown: BalanceConfig.Forms.StormCooldown,
            ZoneRadius: BalanceConfig.Forms.StormZoneRadius,
            Duration: BalanceConfig.Forms.StormDuration,
            TickRate: BalanceConfig.Forms.StormTickRate
        ),
        ["floor"] = new FormData(
            Id: "floor",
            PrototypeId: "fixed_zone_tick",
            TierTrack: TierTrack.CooldownDown,
            Cooldown: BalanceConfig.Forms.FloorCooldown,
            ZoneRadius: BalanceConfig.Forms.FloorZoneRadius,
            Duration: BalanceConfig.Forms.FloorDuration,
            TickRate: BalanceConfig.Forms.FloorTickRate,
            FocusCost: BalanceConfig.Forms.FloorFocusCost
        ),
        ["spin"] = new FormData(
            Id: "spin",
            PrototypeId: "self_channeled_tick",
            TierTrack: TierTrack.TickRateUp,
            Range: BalanceConfig.Forms.SpinRange,
            TickRate: BalanceConfig.Forms.SpinTickRate,
            FocusCost: BalanceConfig.Forms.SpinFocusCost
        ),
        ["vortex"] = new FormData(
            Id: "vortex",
            PrototypeId: "self_channeled_tick",
            TierTrack: TierTrack.FocusCostDown,
            Range: BalanceConfig.Forms.VortexRange,
            TickRate: BalanceConfig.Forms.VortexTickRate,
            FocusCost: BalanceConfig.Forms.VortexFocusCost
        )
    };

    public static FormData? Get(string id) => All.TryGetValue(id, out var f) ? f : null;

    public static IEnumerable<FormData> GetAll() => All.Values;
}
