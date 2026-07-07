using System;
using System.Collections.Generic;

namespace ActionRpgX.Skills;

public static class FormRegistry
{
    private static readonly Dictionary<string, FormData> All = new()
    {
        ["reserve_heavy"] = new FormData(
            Id: "reserve_heavy",
            PrototypeId: "self_aura",
            TierTrack: TierTrack.RadiusUp,
            Range: BalanceConfig.Forms.SelfAuraHeavyRange,
            FocusCost: BalanceConfig.Forms.SelfAuraHeavyFocusReservation,
            Cooldown: BalanceConfig.Forms.SelfAuraHeavyCooldown,
            TickRate: BalanceConfig.Forms.SelfAuraHeavyCooldown
        ),
        ["reserve_light"] = new FormData(
            Id: "reserve_light",
            PrototypeId: "self_aura",
            TierTrack: TierTrack.FocusCostDown,
            Range: BalanceConfig.Forms.SelfAuraLightRange,
            FocusCost: BalanceConfig.Forms.SelfAuraLightFocusReservation,
            Cooldown: BalanceConfig.Forms.SelfAuraLightCooldown,
            TickRate: BalanceConfig.Forms.SelfAuraLightCooldown
        ),
        // Buff/debuff scope: reservation ↔ magnitude (EotSlice), vs the damage scope's reservation ↔ radius above.
        ["reserve_heavy_magnitude"] = new FormData(
            Id: "reserve_heavy_magnitude",
            PrototypeId: "self_aura",
            TierTrack: TierTrack.EotMagnitudeUp,
            Range: BalanceConfig.Forms.SelfAuraMagnitudeHeavyRange,
            FocusCost: BalanceConfig.Forms.SelfAuraMagnitudeHeavyFocusReservation,
            EotSlice: BalanceConfig.Forms.SelfAuraMagnitudeHeavyEotSlice,
            Cooldown: BalanceConfig.Forms.SelfAuraMagnitudeHeavyCooldown,
            TickRate: BalanceConfig.Forms.SelfAuraMagnitudeHeavyCooldown
        ),
        ["reserve_light_magnitude"] = new FormData(
            Id: "reserve_light_magnitude",
            PrototypeId: "self_aura",
            TierTrack: TierTrack.FocusCostDown,
            Range: BalanceConfig.Forms.SelfAuraMagnitudeLightRange,
            FocusCost: BalanceConfig.Forms.SelfAuraMagnitudeLightFocusReservation,
            EotSlice: BalanceConfig.Forms.SelfAuraMagnitudeLightEotSlice,
            Cooldown: BalanceConfig.Forms.SelfAuraMagnitudeLightCooldown,
            TickRate: BalanceConfig.Forms.SelfAuraMagnitudeLightCooldown
        ),
        ["swarm"] = new FormData(
            Id: "swarm",
            PrototypeId: "stackable_zone",
            TierTrack: TierTrack.StackLimitUp,
            Cooldown: BalanceConfig.Forms.StackableZoneSwarmCooldown,
            ZoneRadius: BalanceConfig.Forms.StackableZoneSwarmZoneRadius,
            StackLimit: BalanceConfig.Forms.StackableZoneSwarmStackLimit
        ),
        ["singular"] = new FormData(
            Id: "singular",
            PrototypeId: "stackable_zone",
            TierTrack: TierTrack.RadiusUp,
            Cooldown: BalanceConfig.Forms.StackableZoneSingularCooldown,
            ZoneRadius: BalanceConfig.Forms.StackableZoneSingularZoneRadius,
            Duration: BalanceConfig.Forms.StackableZoneSingularDuration,
            StackLimit: BalanceConfig.Forms.StackableZoneSingularStackLimit
        ),
        ["trap_swarm"] = new FormData(
            Id: "trap_swarm",
            PrototypeId: "triggered_zone_burst",
            TierTrack: TierTrack.StackLimitUp,
            Cooldown: BalanceConfig.Forms.TriggeredZoneBurstSwarmCooldown,
            TriggerRadius: BalanceConfig.Forms.TriggeredZoneBurstSwarmTriggerRadius,
            ZoneRadius: BalanceConfig.Forms.TriggeredZoneBurstSwarmZoneRadius,
            StackLimit: BalanceConfig.Forms.TriggeredZoneBurstSwarmStackLimit
        ),
        ["trap_singular"] = new FormData(
            Id: "trap_singular",
            PrototypeId: "triggered_zone_burst",
            TierTrack: TierTrack.RadiusUp,
            Cooldown: BalanceConfig.Forms.TriggeredZoneBurstSingularCooldown,
            TriggerRadius: BalanceConfig.Forms.TriggeredZoneBurstSingularTriggerRadius,
            ZoneRadius: BalanceConfig.Forms.TriggeredZoneBurstSingularZoneRadius,
            StackLimit: BalanceConfig.Forms.TriggeredZoneBurstSingularStackLimit
        ),
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
        ["blast"] = new FormData(
            Id: "blast",
            PrototypeId: "fixed_zone_burst",
            TierTrack: TierTrack.CooldownDown,
            Cooldown: BalanceConfig.Forms.BlastCooldown
        ),
        ["fuse"] = new FormData(
            Id: "fuse",
            PrototypeId: "fixed_zone_burst",
            TierTrack: TierTrack.RadiusUp,
            Cooldown: BalanceConfig.Forms.FuseCooldown,
            WindUp: BalanceConfig.Forms.FuseWindUp,
            ZoneRadius: BalanceConfig.Forms.FuseZoneRadius
        ),
        ["echo"] = new FormData(
            Id: "echo",
            PrototypeId: "fixed_zone_burst",
            TierTrack: TierTrack.RadiusUp,
            Cooldown: BalanceConfig.Forms.EchoCooldown,
            SubHits: BalanceConfig.Forms.EchoSubHits
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
        ["ramp"] = new FormData(
            Id: "ramp",
            PrototypeId: "self_channeled_tick",
            TierTrack: TierTrack.RampSpeedUp,
            Range: BalanceConfig.Forms.SpinRange,
            TickRate: BalanceConfig.Forms.RampInitialTickRate,
            FocusCost: BalanceConfig.Forms.RampFocusCost,
            RampSpeed: 1.0f
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
        ),
        ["fleet"] = new FormData(
            Id: "fleet",
            PrototypeId: "entity_debuff",
            TierTrack: TierTrack.EotMagnitudeUp,
            Cooldown: BalanceConfig.Skills.EntityDebuffCooldown,
            EotSlice: BalanceConfig.Forms.FleetSliceFactor
        ),
        ["enduring"] = new FormData(
            Id: "enduring",
            PrototypeId: "entity_debuff",
            TierTrack: TierTrack.EotDurationUp,
            Cooldown: BalanceConfig.Skills.EntityDebuffCooldown,
            EotSlice: BalanceConfig.Forms.EnduringSliceFactor
        )
    };

    public static FormData? Get(string id) => All.TryGetValue(id, out var f) ? f : null;

    public static IEnumerable<FormData> GetAll() => All.Values;
}
