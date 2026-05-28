using Godot;
using System.Collections.Generic;
using System.Linq;
using Godot1.Items;
using Godot1.Meta;

namespace Godot1.Character;

public partial class CharacterManager : Node
{
    private const string SavePath = "user://characters.json";

    private List<CharacterData> _characters = new();

    public CharacterData? SelectedCharacter { get; private set; }

    public override void _Ready() => Load();

    public IReadOnlyList<CharacterData> GetAll() => _characters;

    public CharacterData Create(string name, CharacterType type)
    {
        var c = new CharacterData { Name = name, Type = type };
        SeedStarterGear(c);
        _characters.Add(c);
        Save();
        return c;
    }

    private static void SeedStarterGear(CharacterData c)
    {
        var (weapon, armor, accessory) = c.Type switch
        {
            CharacterType.Warrior => ("iron_sword",      "chain_mail",   "war_band"),
            CharacterType.Rogue   => ("iron_sword",      "leather_vest", "swift_ring"),
            CharacterType.Mage    => ("enchanted_blade", "mage_robe",    "vitality_charm"),
            _                     => ("iron_sword",      "leather_vest", "war_band"),
        };

        c.OwnedItemIds.Add(weapon);
        c.OwnedItemIds.Add(armor);
        c.OwnedItemIds.Add(accessory);
        c.EquippedItems[Items.ItemSlot.Weapon.ToString()]    = weapon;
        c.EquippedItems[Items.ItemSlot.Armor.ToString()]     = armor;
        c.EquippedItems[Items.ItemSlot.Accessory.ToString()] = accessory;
    }

    public void Delete(string id)
    {
        _characters.RemoveAll(c => c.Id == id);
        if (SelectedCharacter?.Id == id)
            SelectedCharacter = null;
        Save();
    }

    public void SelectCharacter(string id) =>
        SelectedCharacter = _characters.FirstOrDefault(c => c.Id == id);

    public void RecordRunCompletion(int finalLevel, int finalXp, int coinsEarned, int craftingCurrency1Earned = 0)
    {
        if (SelectedCharacter == null) return;
        SelectedCharacter.RunsCompleted++;
        SelectedCharacter.CurrentLevel     = finalLevel;
        SelectedCharacter.CurrentXp        = finalXp;
        SelectedCharacter.CoinBank        += coinsEarned;
        SelectedCharacter.CraftingCurrency1 += craftingCurrency1Earned;
        Save();
    }

    public bool PurchaseUpgrade(string characterId, MetaUpgradeType type)
    {
        var c = _characters.FirstOrDefault(x => x.Id == characterId);
        if (c == null) return false;

        int level = type switch
        {
            MetaUpgradeType.MaxHealth => c.BonusMaxHealth / 10,
            MetaUpgradeType.Speed     => (int)(c.BonusSpeed / 10f),
            MetaUpgradeType.Damage    => (int)(c.BonusDamage / 2f),
            _                         => 0
        };

        if (level >= 5) return false;
        int cost = (level + 1) * 50;
        if (c.CoinBank < cost) return false;

        c.CoinBank -= cost;
        switch (type)
        {
            case MetaUpgradeType.MaxHealth: c.BonusMaxHealth += 10;  break;
            case MetaUpgradeType.Speed:     c.BonusSpeed     += 10f; break;
            case MetaUpgradeType.Damage:    c.BonusDamage    += 2f;  break;
        }
        Save();
        return true;
    }

    public void AddItemToInventory(string characterId, string itemId)
    {
        var c = _characters.FirstOrDefault(x => x.Id == characterId);
        if (c == null) return;
        if (!c.OwnedItemIds.Contains(itemId))
            c.OwnedItemIds.Add(itemId);
        Save();
    }

    public void EquipItem(string characterId, ItemSlot slot, string itemId)
    {
        var c = _characters.FirstOrDefault(x => x.Id == characterId);
        if (c == null || !c.OwnedItemIds.Contains(itemId)) return;
        c.EquippedItems[slot.ToString()] = itemId;
        Save();
    }

    public void UnequipItem(string characterId, ItemSlot slot)
    {
        var c = _characters.FirstOrDefault(x => x.Id == characterId);
        if (c == null) return;
        c.EquippedItems.Remove(slot.ToString());
        Save();
    }

    private void Save()
    {
        var list = new Godot.Collections.Array();
        foreach (var c in _characters)
        {
            var ownedArr = new Godot.Collections.Array();
            foreach (var id in c.OwnedItemIds) ownedArr.Add(id);

            var equippedDict = new Godot.Collections.Dictionary();
            foreach (var kv in c.EquippedItems) equippedDict[kv.Key] = kv.Value;

            var gd = new Godot.Collections.Dictionary
            {
                ["id"]             = c.Id,
                ["name"]           = c.Name,
                ["type"]           = c.Type.ToString(),
                ["runsCompleted"]  = c.RunsCompleted,
                ["currentLevel"]   = c.CurrentLevel,
                ["currentXp"]      = c.CurrentXp,
                ["coinBank"]           = c.CoinBank,
                ["craftingCurrency1"]  = c.CraftingCurrency1,
                ["bonusMaxHealth"] = c.BonusMaxHealth,
                ["bonusSpeed"]     = c.BonusSpeed,
                ["bonusDamage"]    = c.BonusDamage,
                ["ownedItemIds"]   = ownedArr,
                ["equippedItems"]  = equippedDict,
            };
            list.Add(gd);
        }

        var root = new Godot.Collections.Dictionary { ["characters"] = list };
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        file?.StoreString(Json.Stringify(root));
    }

    private void Load()
    {
        if (!FileAccess.FileExists(SavePath)) return;
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        if (file == null) return;

        var parsed = Json.ParseString(file.GetAsText());
        if (parsed.Obj is not Godot.Collections.Dictionary root) return;
        if (root["characters"].Obj is not Godot.Collections.Array list) return;

        _characters.Clear();
        foreach (var item in list)
        {
            if (item.Obj is not Godot.Collections.Dictionary gd) continue;
            var d = new Dictionary<string, object?>();
            foreach (var kv in gd)
            {
                string key = kv.Key.ToString()!;
                object? val = kv.Value.Obj;

                // Deserialize ownedItemIds array → List<string>
                if (key == "ownedItemIds" && val is Godot.Collections.Array arr)
                {
                    d[key] = arr.Select(v => v.ToString()!).ToList();
                    continue;
                }

                // Deserialize equippedItems dict → Dictionary<string, object?>
                if (key == "equippedItems" && val is Godot.Collections.Dictionary eqGd)
                {
                    var eq = new Dictionary<string, object?>();
                    foreach (var ekv in eqGd)
                        eq[ekv.Key.ToString()!] = ekv.Value.Obj;
                    d[key] = eq;
                    continue;
                }

                d[key] = val;
            }
            _characters.Add(CharacterData.FromDict(d));
        }
    }
}
