namespace ActionRpgX.Stats;

public readonly struct StatModifier
{
    public readonly StatId        Stat;
    public readonly ModifierType  Type;
    public readonly float         Value;
    public readonly ModifierSource Source;
    public readonly string         SourceId;

    public StatModifier(StatId stat, ModifierType type, float value, ModifierSource source, string sourceId = "")
    {
        Stat     = stat;
        Type     = type;
        Value    = value;
        Source   = source;
        SourceId = sourceId;
    }
}
