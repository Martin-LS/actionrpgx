using Godot;
using System.Collections.Generic;
using System.Linq;
using Godot1.Items;

namespace Godot1.Character;

public partial class CharacterManager : Node
{
    private const string SavePath = "user://save.json";

    private List<CharacterData> _characters = new();

    public ProfileData Profile { get; private set; } = new();
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

    private void SeedStarterGear(CharacterData c)
    {
        var (weapon, armor, accessory) = c.Type switch
        {
            CharacterType.Warrior => ("iron_sword",      "chain_mail",    "war_band"),
            CharacterType.Rogue   => ("iron_sword",      "leather_vest",  "swift_ring"),
            CharacterType.Mage    => ("enchanted_blade", "mage_robe",     "vitality_charm"),
            _                     => ("iron_sword",      "leather_vest",  "war_band"),
        };

        // Starter items go straight into gear slots — not the shared inventory pool.
        c.EquippedItems[ItemSlot.Weapon.ToString()]    = weapon;
        c.EquippedItems[ItemSlot.Armor.ToString()]     = armor;
        c.EquippedItems[ItemSlot.Accessory.ToString()] = accessory;
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
        Profile.CoinBank          += coinsEarned;
        Profile.CraftingCurrency1 += craftingCurrency1Earned;
        Save();
    }

    public bool AddItemToInventory(string itemId)
    {
        if (Profile.OwnedItemIds.Count >= ProfileData.MaxInventory) return false;
        Profile.OwnedItemIds.Add(itemId);
        Save();
        return true;
    }

    public void EquipItem(string characterId, ItemSlot slot, string itemId)
    {
        var c = _characters.FirstOrDefault(x => x.Id == characterId);
        if (c == null || !Profile.OwnedItemIds.Contains(itemId)) return;

        // Return the currently-equipped item to the inventory pool.
        if (c.EquippedItems.TryGetValue(slot.ToString(), out var oldId))
            Profile.OwnedItemIds.Add(oldId);

        Profile.OwnedItemIds.Remove(itemId);
        c.EquippedItems[slot.ToString()] = itemId;
        Save();
    }

    // Returns false if the inventory is full and the item cannot be returned.
    public bool UnequipItem(string characterId, ItemSlot slot)
    {
        var c = _characters.FirstOrDefault(x => x.Id == characterId);
        if (c == null || !c.EquippedItems.TryGetValue(slot.ToString(), out var itemId)) return false;
        if (Profile.OwnedItemIds.Count >= ProfileData.MaxInventory) return false;

        c.EquippedItems.Remove(slot.ToString());
        Profile.OwnedItemIds.Add(itemId);
        Save();
        return true;
    }

    // Removes an item from wherever it lives — inventory or any character's gear slot.
    public void DeleteItem(string itemId)
    {
        Profile.OwnedItemIds.Remove(itemId);
        foreach (var c in _characters)
        {
            var key = c.EquippedItems.FirstOrDefault(kv => kv.Value == itemId).Key;
            if (key != null) c.EquippedItems.Remove(key);
        }
        Save();
    }

    private void Save()
    {
        var ownedArr = new Godot.Collections.Array();
        foreach (var id in Profile.OwnedItemIds) ownedArr.Add(id);

        var profileDict = new Godot.Collections.Dictionary
        {
            ["coinBank"]          = Profile.CoinBank,
            ["craftingCurrency1"] = Profile.CraftingCurrency1,
            ["ownedItemIds"]      = ownedArr,
        };

        var charList = new Godot.Collections.Array();
        foreach (var c in _characters)
        {
            var equippedDict = new Godot.Collections.Dictionary();
            foreach (var kv in c.EquippedItems) equippedDict[kv.Key] = kv.Value;

            charList.Add(new Godot.Collections.Dictionary
            {
                ["id"]            = c.Id,
                ["name"]          = c.Name,
                ["type"]          = c.Type.ToString(),
                ["runsCompleted"] = c.RunsCompleted,
                ["currentLevel"]  = c.CurrentLevel,
                ["currentXp"]     = c.CurrentXp,
                ["equippedItems"] = equippedDict,
            });
        }

        var root = new Godot.Collections.Dictionary
        {
            ["profile"]    = profileDict,
            ["characters"] = charList,
        };

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

        if (root.ContainsKey("profile") && root["profile"].Obj is Godot.Collections.Dictionary pd)
        {
            Profile.CoinBank          = pd.ContainsKey("coinBank")          ? System.Convert.ToInt32(pd["coinBank"].Obj)          : 0;
            Profile.CraftingCurrency1 = pd.ContainsKey("craftingCurrency1") ? System.Convert.ToInt32(pd["craftingCurrency1"].Obj) : 0;
            if (pd.ContainsKey("ownedItemIds") && pd["ownedItemIds"].Obj is Godot.Collections.Array arr)
                Profile.OwnedItemIds = arr.Select(v => v.ToString()!).ToList();
        }

        if (!root.ContainsKey("characters") || root["characters"].Obj is not Godot.Collections.Array list) return;

        _characters.Clear();
        foreach (var item in list)
        {
            if (item.Obj is not Godot.Collections.Dictionary gd) continue;
            var d = new Dictionary<string, object?>();
            foreach (var kv in gd)
            {
                string key = kv.Key.ToString()!;
                object? val = kv.Value.Obj;

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

        // Migrate old saves: equipped items must not also sit in the inventory pool.
        foreach (var c in _characters)
            foreach (var id in c.EquippedItems.Values)
                Profile.OwnedItemIds.Remove(id);
    }
}
