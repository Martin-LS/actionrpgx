using System.Collections.Generic;
using System.Linq;
using Godot1.Items;

namespace Godot1.Character;

public class CharacterData
{
    public string Id { get; set; } = System.Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public CharacterType Type { get; set; } = CharacterType.Warrior;
    public int RunsCompleted { get; set; } = 0;

    public int CurrentLevel { get; set; } = 1;
    public int CurrentXp { get; set; } = 0;

    public int CoinBank { get; set; } = 0;
    public int CraftingCurrency1 { get; set; } = 0;

    public int BonusMaxHealth { get; set; } = 0;
    public float BonusSpeed { get; set; } = 0f;
    public float BonusDamage { get; set; } = 0f;

    public List<string> OwnedItemIds { get; set; } = new();
    public Dictionary<string, string> EquippedItems { get; set; } = new(); // slot name → item ID

    public (int MaxHealth, float Speed, float Damage) BaseStats()
    {
        var (hp, spd, dmg) = Type switch
        {
            CharacterType.Warrior => (150, 170f, 20f),
            CharacterType.Rogue   => (80,  260f, 15f),
            CharacterType.Mage    => (100, 200f, 35f),
            _                     => (100, 200f, 20f),
        };
        hp  += BonusMaxHealth;
        spd += BonusSpeed;
        dmg += BonusDamage;

        foreach (var (_, id) in EquippedItems)
        {
            var item = ItemRegistry.Get(id);
            if (item == null) continue;
            hp  += item.BonusHp;
            spd += item.BonusSpeed;
            dmg += item.BonusDamage;
        }

        return (hp, spd, dmg);
    }

    public Dictionary<string, object?> ToDict() => new()
    {
        ["id"]             = Id,
        ["name"]           = Name,
        ["type"]           = Type.ToString(),
        ["runsCompleted"]  = RunsCompleted,
        ["currentLevel"]   = CurrentLevel,
        ["currentXp"]      = CurrentXp,
        ["coinBank"]            = CoinBank,
        ["craftingCurrency1"]   = CraftingCurrency1,
        ["bonusMaxHealth"] = BonusMaxHealth,
        ["bonusSpeed"]     = BonusSpeed,
        ["bonusDamage"]    = BonusDamage,
        ["ownedItemIds"]   = OwnedItemIds,
        ["equippedItems"]  = EquippedItems,
    };

    public static CharacterData FromDict(Dictionary<string, object?> d) => new()
    {
        Id             = (string)d["id"]!,
        Name           = (string)d["name"]!,
        Type           = System.Enum.Parse<CharacterType>((string)d["type"]!),
        RunsCompleted  = System.Convert.ToInt32(d["runsCompleted"]),
        CurrentLevel   = d.ContainsKey("currentLevel") ? System.Convert.ToInt32(d["currentLevel"]) : 1,
        CurrentXp      = d.ContainsKey("currentXp")    ? System.Convert.ToInt32(d["currentXp"])    : 0,
        CoinBank           = d.ContainsKey("coinBank")           ? System.Convert.ToInt32(d["coinBank"])           : 0,
        CraftingCurrency1  = d.ContainsKey("craftingCurrency1") ? System.Convert.ToInt32(d["craftingCurrency1"]) : 0,
        BonusMaxHealth = System.Convert.ToInt32(d["bonusMaxHealth"]),
        BonusSpeed     = System.Convert.ToSingle(d["bonusSpeed"]),
        BonusDamage    = System.Convert.ToSingle(d["bonusDamage"]),
        OwnedItemIds   = d.ContainsKey("ownedItemIds") && d["ownedItemIds"] is System.Collections.IEnumerable raw
            ? raw.Cast<object>().Select(x => x.ToString()!).ToList()
            : new(),
        EquippedItems  = d.ContainsKey("equippedItems") && d["equippedItems"] is Dictionary<string, object?> eq
            ? eq.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "")
            : new(),
    };
}
