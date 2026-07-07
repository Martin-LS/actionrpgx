namespace ActionRpgX.Eot;

public class EotInstance
{
    public string DefinitionId  { get; set; } = "";
    public float  TimeRemaining { get; set; }
    public float  TickTimer     { get; set; }
    public float  CritMultiplier { get; set; } = 1.0f;
    public float  SlowFraction   { get; set; }
    public float  DamageTakenAmp { get; set; }
    public float  DamagePerTick  { get; set; }
}
