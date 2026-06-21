namespace Godot2.Eot;

public class EotInstance
{
    public string DefinitionId  { get; set; } = "";
    public float  TimeRemaining { get; set; }
    public float  TickTimer     { get; set; }
    public float  CritMultiplier { get; set; } = 1.0f;
}
