namespace ActionRpgX.Skills;

public static class SkillComposer
{
    public static SkillData Compose(SkillData proto, FormData form, IdentityData identity, PresetData preset) =>
        proto with {
            Id         = preset.Id,
            Name       = preset.Name,
            Kind       = SkillKind.Normal,
            BasedOn    = proto.Id,
            DamageType = identity.DamageType,
            IconPath   = preset.IconPath,
            Cooldown   = form.Cooldown   ?? proto.Cooldown,
            FocusCost  = form.FocusCost  ?? proto.FocusCost,
            WindUp     = form.WindUp     ?? proto.WindUp,
            Duration   = form.Duration   ?? proto.Duration,
            ZoneRadius = form.ZoneRadius ?? proto.ZoneRadius,
            TickRate   = form.TickRate   ?? proto.TickRate,
            Range      = form.Range      ?? proto.Range,
            VfxKey     = identity.VfxKey
        };
}
