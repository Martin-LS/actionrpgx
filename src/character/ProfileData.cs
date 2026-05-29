using System.Collections.Generic;

namespace Godot1.Character;

public class ProfileData
{
    public const int MaxInventory = 10;

    public int CoinBank { get; set; } = 0;
    public int CraftingCurrency1 { get; set; } = 0;
    public List<string> OwnedItemIds { get; set; } = new();
}
