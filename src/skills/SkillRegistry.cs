using System.Collections.Generic;
using ActionRpgX.Items;

namespace ActionRpgX.Skills;

public static class SkillRegistry
{
    private static readonly Dictionary<string, SkillData> All = new()
    {
        // --- Player-facing prototypes ---

        ["entity_burst"] = new SkillData(
            "entity_burst", "Entity Burst", SkillType.Active,
            Tags: System.Array.Empty<string>(),
            Cooldown: BalanceConfig.Skills.EntityBurstCooldown,
            FocusCost: BalanceConfig.Focus.EntityBurstFocusCost,
            IconPath: "res://assets/icons/items/battle_axe.png",
            Description: "Proves Entity targeting and weapon-adaptive delivery. Universal starter — fires at locked target using equipped weapon.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Entity,
            DamagePattern: SkillDamagePattern.Burst),

        ["self_channeled_tick"] = new SkillData(
            "self_channeled_tick", "Self Channeled Tick", SkillType.Channeled,
            Tags: new[] { "Melee", "AoE" },
            Cooldown: BalanceConfig.Skills.SelfChanneledTickCooldown, Range: BalanceConfig.Skills.SelfChanneledTickRange,
            FocusCost: BalanceConfig.Focus.SelfChanneledTickFocusCostPerSec,
            Description: "Proves Channeled skill type with Self targeting. Continuous ticking damage while held; drains Focus over time.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Self,
            DamagePattern: SkillDamagePattern.Tick,
            TickRate: BalanceConfig.Skills.SelfChanneledTickCooldown),

        ["self_duration_tick"] = new SkillData(
            "self_duration_tick", "Self Duration Tick", SkillType.Active,
            Tags: new[] { "AoE" },
            Cooldown: BalanceConfig.Skills.SelfDurationTickCooldown, Range: BalanceConfig.Skills.SelfDurationTickRange,
            FocusCost: BalanceConfig.Focus.SelfDurationTickFocusCost,
            Description: "Proves Active Self skill with ticking damage over a fixed duration. Activate → ticks damage in radius for duration → cooldown.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Self,
            DamagePattern: SkillDamagePattern.Tick,
            Duration: BalanceConfig.Skills.SelfDurationTickDuration,
            DamageType: DamageType.Fire,
            TickRate: BalanceConfig.Skills.SelfDurationTickCooldown),

        ["self_burst"] = new SkillData(
            "self_burst", "Self Burst", SkillType.Active,
            Tags: new[] { "AoE" },
            Cooldown: BalanceConfig.Skills.SelfBurstCooldown, Range: BalanceConfig.Skills.SelfBurstRange,
            FocusCost: BalanceConfig.Focus.SelfBurstFocusCost,
            Description: "Proves Active Self burst. Instant explosion centered on player; flat Focus cost.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Self,
            DamagePattern: SkillDamagePattern.Burst),

        // --- Engine proof prototypes ---

        ["tracked_tick"] = new SkillData(
            "tracked_tick", "Tracked Tick", SkillType.Active,
            Tags: new[] { "AoE" },
            Cooldown: BalanceConfig.Skills.TrackedTickCooldown,
            FocusCost: BalanceConfig.Focus.TrackedTickFocusCost,
            Description: "Attaches a ticking damage zone to the locked enemy. Zone follows the enemy and expires when they die. Proves ZoneTracksEntity and entity death expiry.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Entity,
            DamagePattern: SkillDamagePattern.Tick,
            ZoneTracksEntity: true,
            Duration: BalanceConfig.Skills.TrackedTickDuration,
            ZoneRadius: BalanceConfig.Skills.TrackedTickZoneRadius,
            TickRate: BalanceConfig.Skills.TrackedTickRate),

        ["triggered_zone_burst"] = new SkillData(
            "triggered_zone_burst", "Triggered Zone Burst", SkillType.Active,
            Tags: new[] { "AoE" },
            Cooldown: BalanceConfig.Skills.TriggeredZoneBurstCooldown,
            Range: BalanceConfig.Skills.TriggeredZoneBurstRange,
            FocusCost: BalanceConfig.Focus.TriggeredZoneBurstFocusCost,
            Description: "Places a dormant trap; arms after 0.5s then fires once when an enemy enters the trigger radius. Proves TriggerRadius/ArmTime/TriggerCount.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Position,
            DamagePattern: SkillDamagePattern.Burst,
            StackLimit: 3,
            Duration: BalanceConfig.Skills.TriggeredZoneBurstDuration,
            ZoneRadius: BalanceConfig.Skills.TriggeredZoneBurstZoneRadius,
            TriggerRadius: BalanceConfig.Skills.TriggeredZoneBurstTriggerRadius,
            ArmTime: BalanceConfig.Skills.TriggeredZoneBurstArmTime,
            TriggerCount: 1),

        ["stackable_zone"] = new SkillData(
            "stackable_zone", "Stackable Zone", SkillType.Active,
            Tags: new[] { "AoE" },
            Cooldown: BalanceConfig.Skills.StackableZoneCooldown,
            Range: BalanceConfig.Skills.StackableZoneRange,
            FocusCost: BalanceConfig.Focus.StackableZoneFocusCost,
            Description: "Each cast places an independent ticking zone; up to 3 active simultaneously. A 4th cast despawns the oldest. Proves StackLimit and oldest-despawn on cap.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Position,
            DamagePattern: SkillDamagePattern.Tick,
            StackLimit: 3,
            Duration: BalanceConfig.Skills.StackableZoneDuration,
            ZoneRadius: BalanceConfig.Skills.StackableZoneZoneRadius,
            TickRate: BalanceConfig.Skills.StackableZoneRate),

        ["entity_debuff"] = new SkillData(
            "entity_debuff", "Entity Debuff", SkillType.Active,
            Tags: System.Array.Empty<string>(),
            Cooldown: BalanceConfig.Skills.EntityDebuffCooldown,
            FocusCost: BalanceConfig.Focus.EntityDebuffFocusCost,
            Description: "Applies slow to locked target for 6s with no damage. Proves Entity targeting with pure debuff output.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Entity,
            DamagePattern: SkillDamagePattern.None,
            DebuffEotId: "slow"),

        ["fixed_zone_burst"] = new SkillData(
            "fixed_zone_burst", "Fixed Zone Burst", SkillType.Active,
            Tags: new[] { "AoE" },
            Cooldown: BalanceConfig.Skills.FixedZoneBurstCooldown,
            Range: BalanceConfig.Skills.FixedZoneBurstRange,
            FocusCost: BalanceConfig.Focus.FixedZoneBurstFocusCost,
            Description: "Instant explosion at locked target's position. Proves Position targeting resolves to enemy location, not player.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Position,
            DamagePattern: SkillDamagePattern.Burst,
            StackLimit: 1,
            Duration: 0f,
            ZoneRadius: BalanceConfig.Skills.FixedZoneBurstZoneRadius),

        ["fixed_zone_tick"] = new SkillData(
            "fixed_zone_tick", "Fixed Zone Tick", SkillType.Active,
            Tags: new[] { "AoE" },
            Cooldown: BalanceConfig.Skills.FixedZoneTickCooldown,
            Range: BalanceConfig.Skills.FixedZoneTickRange,
            FocusCost: BalanceConfig.Focus.FixedZoneTickFocusCost,
            Description: "Persistent ticking zone at locked target's position. Proves Position targeting with duration and tick damage.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Position,
            DamagePattern: SkillDamagePattern.Tick,
            StackLimit: 1,
            Duration: BalanceConfig.Skills.FixedZoneTickDuration,
            ZoneRadius: BalanceConfig.Skills.FixedZoneTickZoneRadius,
            TickRate: BalanceConfig.Skills.FixedZoneTickRate),

        ["self_aura"] = new SkillData(
            "self_aura", "Self Aura", SkillType.Aura,
            Tags: System.Array.Empty<string>(),
            Cooldown: BalanceConfig.Skills.SelfAuraCooldown, Range: BalanceConfig.Skills.SelfAuraRange,
            FocusCost: BalanceConfig.Focus.SelfAuraFocusReservation,
            Description: "Proves Aura toggle + Focus reservation mechanic. Toggle on → reserves Focus and pulses effect each tick. Toggle off → unreserves Focus.",
            Kind: SkillKind.Prototype,
            TargetingShape: SkillTargetingShape.Self,
            DamagePattern: SkillDamagePattern.Tick,
            TickRate: BalanceConfig.Skills.SelfAuraCooldown),
    };

    public static SkillData? Get(string id) => All.TryGetValue(id, out var s) ? s : null;
    public static IEnumerable<SkillData> GetAll() => All.Values;

    public static bool ValidateCombo(SkillData proto, FormData form, IdentityData identity, out string? errorMessage)
    {
        errorMessage = null;

        if (form.PrototypeId != proto.Id)
        {
            errorMessage = $"Form '{form.Id}' belongs to prototype '{form.PrototypeId}', not '{proto.Id}'.";
            return false;
        }

        if (form.Range != null && proto.TargetingShape != SkillTargetingShape.Self)
        {
            errorMessage = $"Form '{form.Id}' sets Range, but its prototype '{proto.Id}' is not Self-targeting.";
            return false;
        }

        var dummyPreset = new PresetData(
            Id: "dummy",
            Name: "Dummy",
            PrototypeId: proto.Id,
            FormId: form.Id,
            IdentityId: identity.Id,
            IconPath: ""
        );
        var composed = SkillComposer.Compose(proto, form, identity, dummyPreset);

        if (composed.DamagePattern != SkillDamagePattern.None && !string.IsNullOrEmpty(composed.DebuffEotId) && composed.Type != SkillType.Aura)
        {
            errorMessage = $"Skill '{composed.Id}' has DebuffEotId set ('{composed.DebuffEotId}') but DamagePattern is {composed.DamagePattern} (must be None).";
            return false;
        }
        if (composed.DamagePattern == SkillDamagePattern.Tick && composed.TickRate <= 0f)
        {
            errorMessage = $"Skill '{composed.Id}' has DamagePattern.Tick but TickRate is {composed.TickRate} (must be > 0).";
            return false;
        }

        return true;
    }

    static SkillRegistry()
    {
        // Named skills are no longer pre-composed into the registry — the craft
        // wizard composes them per-instance as flattened snapshots. Only the
        // prototypes below live here. Validate them for internal consistency.
        foreach (var skill in All.Values)
        {
            if (skill.DamagePattern != SkillDamagePattern.None && !string.IsNullOrEmpty(skill.DebuffEotId) && skill.Type != SkillType.Aura)
            {
                throw new System.InvalidOperationException(
                    $"Skill '{skill.Id}' has DebuffEotId set ('{skill.DebuffEotId}') but DamagePattern is {skill.DamagePattern} (must be None).");
            }
            if (skill.DamagePattern == SkillDamagePattern.Tick && skill.TickRate <= 0f)
            {
                throw new System.InvalidOperationException(
                    $"Skill '{skill.Id}' has DamagePattern.Tick but TickRate is {skill.TickRate} (must be > 0).");
            }
        }
    }
}
