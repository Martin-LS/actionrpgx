namespace ActionRpgX.Skills;

// FormData.cs — budget-lever overrides; null = inherit the prototype's value
public record FormData(
    string    Id,            // "swift", "heavy", "nova", "quake", "storm", "floor", "spin", "vortex"
    string    PrototypeId,   // which prototype this form applies to
    TierTrack TierTrack,
    float?    Cooldown   = null,
    float?    FocusCost  = null,
    float?    WindUp     = null,
    float?    Duration   = null,
    float?    ZoneRadius = null,
    float?    TickRate   = null,
    float?    Range      = null   // ONLY on forms of Self-targeting prototypes
);
