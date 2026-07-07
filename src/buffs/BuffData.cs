using ActionRpgX.Stats;

namespace ActionRpgX.Buffs;

public record BuffModifier(StatId Stat, ModifierType Type, float Value);

public record BuffData(
    string         Id,
    string         Name,
    BuffModifier[] Modifiers,
    float?         Duration = null
);
