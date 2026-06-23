namespace ActionRpgX.Skills;

public class SkillAugmentInstance
{
    public string Id           { get; set; } = System.Guid.NewGuid().ToString();
    public string DefinitionId { get; set; } = "";
    public int Tier            { get; set; } = 1;
    public int TriggerChance   { get; set; } = 15; // percentage

    public SkillAugmentData? Definition => SkillAugmentRegistry.Get(DefinitionId);
}
