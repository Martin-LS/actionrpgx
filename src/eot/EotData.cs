using ActionRpgX.Items;

namespace ActionRpgX.Eot;

public record EotData(
    string     Id,
    string     Name,
    float      ApplyChance,
    float      Duration,
    bool       IsDamageEot,
    float      TickRate              = 0f,
    float      DamagePerTick         = 0f,
    float      DamagePerTickFraction = 0f,
    float      SlowFraction          = 0f,
    float      DamageTakenAmp        = 0f,               // fractional increase to all damage the target takes (Shock)
    DamageType DamageType            = DamageType.Physical, // resist channel this EoT's tick damage rolls against
    int        MaxStacks             = 1                  // independent concurrent instances allowed (Poison stacks; signature ailments refresh in place)
);
