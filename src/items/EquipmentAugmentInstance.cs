namespace ActionRpgX.Items;

public class EquipmentAugmentInstance
{
    public string Id           { get; set; } = System.Guid.NewGuid().ToString();
    public string DefinitionId { get; set; } = "";
    public int Tier            { get; set; } = 1;
    public int TriggerChance   { get; set; } = 15; // percentage

    public EquipmentAugmentData? Definition => EquipmentAugmentRegistry.Get(DefinitionId);
}
