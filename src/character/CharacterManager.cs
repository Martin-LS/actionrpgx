using Godot;
using System.Collections.Generic;
using System.Linq;
using ActionRpgX.Crafting;
using ActionRpgX.Items;
using ActionRpgX.Skills;

namespace ActionRpgX.Character;

public partial class CharacterManager : Node
{
    private const string SavePath = "user://save.json";

    private List<CharacterData> _characters = new();

    public ProfileData    Profile           { get; private set; } = new();
    public CharacterData? SelectedCharacter { get; private set; }

    public override void _Ready() => Load();

    public IReadOnlyList<CharacterData> GetAll() => _characters;

    public void SetupTestScenario()
    {
        var chars = GetAll();
        CharacterData c;
        if (chars.Count > 0)
        {
            c = chars[0];
            SelectCharacter(c.Id);
        }
        else
        {
            c = Create("TestHero", CharacterType.Warrior);
            SelectCharacter(c.Id);
        }

        Profile.Materials["crafting_common"] = 100;

        if (Profile.OwnedSkillInstances.Count == 0)
        {
            CraftSkillItem("recipe_entity_burst");
        }

        var skillInst = Profile.OwnedSkillInstances[0];
        EquipSkill(c.Id, 0, skillInst.Id);

        if (Profile.OwnedSkillAugmentInstances.Count == 0)
        {
            CraftSkillAugmentItem("recipe_slow");
        }
        
        Save();
    }

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
        var (weapon, hat, body, boots, ring) = c.Type switch
        {
            CharacterType.Warrior => ("sword_t1", "heavy_hat_t1",  "heavy_body_t1",  "heavy_boots_t1",  "ring_t1"),
            CharacterType.Rogue   => ("bow_t1",   "medium_hat_t1", "medium_body_t1", "medium_boots_t1", "ring_t1"),
            CharacterType.Mage    => ("wand_t1",  "medium_hat_t1", "medium_body_t1", "medium_boots_t1", "ring_t1"),
            _                     => ("sword_t1", "heavy_hat_t1",  "heavy_body_t1",  "heavy_boots_t1",  "ring_t1"),
        };

        c.EquippedGear[ItemSlot.Weapon.ToString()] = new GearItemInstance { DefinitionId = weapon };
        c.EquippedGear[ItemSlot.Hat.ToString()]    = new GearItemInstance { DefinitionId = hat };
        c.EquippedGear[ItemSlot.Body.ToString()]   = new GearItemInstance { DefinitionId = body };
        c.EquippedGear[ItemSlot.Boots.ToString()]  = new GearItemInstance { DefinitionId = boots };
        c.EquippedGear[ItemSlot.Ring.ToString()]   = new GearItemInstance { DefinitionId = ring };

        var skillInst = new SkillItemInstance { DefinitionId = "entity_burst" };

        Profile.OwnedSkillInstances.Add(skillInst);
        c.SlottedSkillInstanceIds = new List<string> { skillInst.Id, "", "", "", "" };
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
        SelectedCharacter.CurrentLevel = finalLevel;
        SelectedCharacter.CurrentXp    = finalXp;
        Profile.CoinBank += coinsEarned;
        Profile.AddMaterial("crafting_common", craftingCurrency1Earned);
        Save();
    }

    // ── Skill slot helpers ────────────────────────────────────────────────────

    private int CountSlottedSkills()
    {
        var ids = new System.Collections.Generic.HashSet<string>();
        foreach (var c in _characters)
            foreach (var id in c.SlottedSkillInstanceIds)
                if (!string.IsNullOrEmpty(id)) ids.Add(id);
        return ids.Count;
    }

    public int GetUnslottedSkillCount()
    {
        return Profile.OwnedSkillInstances.Count - CountSlottedSkills();
    }


    // ── Instance lookups ──────────────────────────────────────────────────────

    public GearItemInstance? FindGearInstance(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var owned = Profile.OwnedGearInstances.FirstOrDefault(g => g.Id == id);
        if (owned != null) return owned;
        foreach (var c in _characters)
        {
            if (c.EquippedGear.Values.FirstOrDefault(g => g.Id == id) is { } eq)
                return eq;
        }
        return null;
    }

    public SkillItemInstance? FindSkillInstance(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return Profile.OwnedSkillInstances.FirstOrDefault(s => s.Id == id);
    }

    public SkillAugmentInstance? FindSkillAugmentInstance(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return Profile.OwnedSkillAugmentInstances.FirstOrDefault(s => s.Id == id);
    }

    public EquipmentAugmentInstance? FindEquipmentAugmentInstance(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return Profile.OwnedEquipmentAugmentInstances.FirstOrDefault(a => a.Id == id);
    }

    // ── Gear inventory ────────────────────────────────────────────────────────

    public CraftResult CraftGearItem(string recipeId)
    {
        var recipe = RecipeRegistry.Get(recipeId);
        if (recipe == null) return CraftResult.InsufficientMaterials;

        foreach (var (matId, qty) in recipe.MaterialCosts)
            if (Profile.GetMaterial(matId) < qty)
                return CraftResult.InsufficientMaterials;

        if (Profile.OwnedGearInstances.Count >= ProfileData.MaxInventory)
            return CraftResult.InventoryFull;

        foreach (var (matId, qty) in recipe.MaterialCosts)
            Profile.Materials[matId] -= qty;

        Profile.OwnedGearInstances.Add(new GearItemInstance { DefinitionId = recipe.OutputItemId });
        Save();
        return CraftResult.Success;
    }

    public void EquipItem(string characterId, ItemSlot slot, string instanceId)
    {
        var c    = _characters.FirstOrDefault(x => x.Id == characterId);
        var inst = Profile.OwnedGearInstances.FirstOrDefault(g => g.Id == instanceId);
        if (c == null || inst == null) return;

        if (c.EquippedGear.TryGetValue(slot.ToString(), out var old))
            Profile.OwnedGearInstances.Add(old);

        Profile.OwnedGearInstances.Remove(inst);
        c.EquippedGear[slot.ToString()] = inst;
        Save();
    }

    public bool UnequipItem(string characterId, ItemSlot slot)
    {
        var c = _characters.FirstOrDefault(x => x.Id == characterId);
        if (c == null || !c.EquippedGear.TryGetValue(slot.ToString(), out var inst)) return false;
        if (Profile.OwnedGearInstances.Count >= ProfileData.MaxInventory) return false;

        c.EquippedGear.Remove(slot.ToString());
        Profile.OwnedGearInstances.Add(inst);
        Save();
        return true;
    }

    public void DeleteGearItem(string instanceId)
    {
        Profile.OwnedGearInstances.RemoveAll(g => g.Id == instanceId);
        foreach (var c in _characters)
        {
            var key = c.EquippedGear.FirstOrDefault(kv => kv.Value.Id == instanceId).Key;
            if (key != null) c.EquippedGear.Remove(key);
        }
        Save();
    }

    public CraftResult UpgradeGearItem(string instanceId)
    {
        var inst = FindGearInstance(instanceId);
        if (inst == null || inst.Tier >= ItemTier.Max)  return CraftResult.InsufficientMaterials;
        if (Profile.GetMaterial("crafting_common") < 1) return CraftResult.InsufficientMaterials;
        Profile.Materials["crafting_common"] -= 1;
        inst.Tier++;
        Save();
        return CraftResult.Success;
    }

    // ── Skill inventory ───────────────────────────────────────────────────────

    public CraftResult CraftSkillItem(string recipeId)
    {
        var recipe = RecipeRegistry.Get(recipeId);
        if (recipe == null) return CraftResult.InsufficientMaterials;

        foreach (var (matId, qty) in recipe.MaterialCosts)
            if (Profile.GetMaterial(matId) < qty)
                return CraftResult.InsufficientMaterials;

        int unslotted = Profile.OwnedSkillInstances.Count - CountSlottedSkills();
        if (unslotted >= ProfileData.MaxInventory)
            return CraftResult.InventoryFull;

        foreach (var (matId, qty) in recipe.MaterialCosts)
            Profile.Materials[matId] -= qty;

        Profile.OwnedSkillInstances.Add(new SkillItemInstance { DefinitionId = recipe.OutputItemId });
        Save();
        return CraftResult.Success;
    }

    public void EquipSkill(string charId, int slotIndex, string instanceId)
    {
        var c = _characters.FirstOrDefault(x => x.Id == charId);
        if (c == null) return;
        if (FindSkillInstance(instanceId) == null) return;
        while (c.SlottedSkillInstanceIds.Count <= slotIndex)
            c.SlottedSkillInstanceIds.Add("");
        c.SlottedSkillInstanceIds[slotIndex] = instanceId;
        while (c.SlotAutoActivate.Count <= slotIndex)
            c.SlotAutoActivate.Add(true);
        c.SlotAutoActivate[slotIndex] = true;
        Save();
    }

    public bool UnequipSkillSlot(string charId, int slotIndex)
    {
        var c = _characters.FirstOrDefault(x => x.Id == charId);
        if (c == null || slotIndex >= c.SlottedSkillInstanceIds.Count) return false;
        if (string.IsNullOrEmpty(c.SlottedSkillInstanceIds[slotIndex])) return false;
        int unslotted = Profile.OwnedSkillInstances.Count - CountSlottedSkills();
        if (unslotted >= ProfileData.MaxInventory) return false;
        c.SlottedSkillInstanceIds[slotIndex] = "";
        Save();
        return true;
    }

    public void DeleteSkillItem(string instanceId)
    {
        Profile.OwnedSkillInstances.RemoveAll(s => s.Id == instanceId);
        foreach (var c in _characters)
            for (int i = 0; i < c.SlottedSkillInstanceIds.Count; i++)
                if (c.SlottedSkillInstanceIds[i] == instanceId)
                    c.SlottedSkillInstanceIds[i] = "";
        Save();
    }

    public void DeleteSkillAugmentItem(string instanceId)
    {
        Profile.OwnedSkillAugmentInstances.RemoveAll(a => a.Id == instanceId);
        foreach (var s in Profile.OwnedSkillInstances)
        {
            for (int i = 0; i < s.SocketedSkillAugmentIds.Count; i++)
                if (s.SocketedSkillAugmentIds[i] == instanceId)
                    s.SocketedSkillAugmentIds[i] = "";
        }
        Save();
    }

    public void DeleteEquipmentAugmentItem(string instanceId)
    {
        Profile.OwnedEquipmentAugmentInstances.RemoveAll(a => a.Id == instanceId);
        foreach (var g in Profile.OwnedGearInstances)
        {
            for (int i = 0; i < g.SocketedEquipmentAugmentIds.Count; i++)
                if (g.SocketedEquipmentAugmentIds[i] == instanceId)
                    g.SocketedEquipmentAugmentIds[i] = "";
        }
        foreach (var c in _characters)
        {
            foreach (var g in c.EquippedGear.Values)
            {
                for (int i = 0; i < g.SocketedEquipmentAugmentIds.Count; i++)
                    if (g.SocketedEquipmentAugmentIds[i] == instanceId)
                        g.SocketedEquipmentAugmentIds[i] = "";
            }
        }
        Save();
    }

    public void DeleteSkillPermanently(string charId, int slotIndex)
    {
        var c = _characters.FirstOrDefault(x => x.Id == charId);
        if (c == null || slotIndex >= c.SlottedSkillInstanceIds.Count) return;
        var instanceId = c.SlottedSkillInstanceIds[slotIndex];
        c.SlottedSkillInstanceIds[slotIndex] = "";
        if (!string.IsNullOrEmpty(instanceId))
            DeleteSkillItem(instanceId);
        else
            Save();
    }

    public CraftResult UpgradeSkillItem(string instanceId)
    {
        var inst = FindSkillInstance(instanceId);
        if (inst == null || inst.Tier >= ItemTier.Max)  return CraftResult.InsufficientMaterials;
        if (Profile.GetMaterial("crafting_common") < 1) return CraftResult.InsufficientMaterials;
        Profile.Materials["crafting_common"] -= 1;
        inst.Tier++;
        Save();
        return CraftResult.Success;
    }

    // ── Skill Augment inventory ───────────────────────────────────────────────

    public CraftResult CraftSkillAugmentItem(string recipeId)
    {
        var recipe = RecipeRegistry.Get(recipeId);
        if (recipe == null) return CraftResult.InsufficientMaterials;

        foreach (var (matId, qty) in recipe.MaterialCosts)
            if (Profile.GetMaterial(matId) < qty)
                return CraftResult.InsufficientMaterials;

        if (Profile.OwnedSkillAugmentInstances.Count >= ProfileData.MaxInventory)
            return CraftResult.InventoryFull;

        foreach (var (matId, qty) in recipe.MaterialCosts)
            Profile.Materials[matId] -= qty;

        Profile.OwnedSkillAugmentInstances.Add(new SkillAugmentInstance 
        { 
            DefinitionId = recipe.OutputItemId,
            TriggerChance = new System.Random().Next(10, 31)
        });
        Save();
        return CraftResult.Success;
    }

    public CraftResult SocketSkillAugment(string skillInstanceId, int slotIndex, string augmentInstanceId)
    {
        var skill = FindSkillInstance(skillInstanceId);
        var augment = FindSkillAugmentInstance(augmentInstanceId);
        if (skill == null || augment == null) return CraftResult.InsufficientMaterials;
        if (slotIndex >= skill.MaxSkillAugmentSlots) return CraftResult.InsufficientMaterials;

        var newGroup = augment.Definition?.ConflictGroup;
        for (int i = 0; i < skill.SocketedSkillAugmentIds.Count; i++)
        {
            if (i == slotIndex) continue;
            var otherAugId = skill.SocketedSkillAugmentIds[i];
            if (string.IsNullOrEmpty(otherAugId)) continue;
            var otherAug = FindSkillAugmentInstance(otherAugId);
            if (otherAug == null) continue;

            if (otherAug.DefinitionId == augment.DefinitionId)
                return CraftResult.InsufficientMaterials;

            if (newGroup != null && otherAug.Definition?.ConflictGroup == newGroup)
                return CraftResult.InsufficientMaterials;
        }

        while (skill.SocketedSkillAugmentIds.Count <= slotIndex)
            skill.SocketedSkillAugmentIds.Add("");
        skill.SocketedSkillAugmentIds[slotIndex] = augmentInstanceId;
        Save();
        return CraftResult.Success;
    }

    public void RemoveSkillAugment(string skillInstanceId, int slotIndex)
    {
        var skill = FindSkillInstance(skillInstanceId);
        if (skill == null || slotIndex >= skill.SocketedSkillAugmentIds.Count) return;
        skill.SocketedSkillAugmentIds[slotIndex] = "";
        Save();
    }

    public CraftResult UpgradeSkillAugment(string instanceId)
    {
        var inst = FindSkillAugmentInstance(instanceId);
        if (inst == null || inst.Tier >= Items.ItemTier.Max) return CraftResult.InsufficientMaterials;
        if (Profile.GetMaterial("crafting_common") < 1) return CraftResult.InsufficientMaterials;
        Profile.Materials["crafting_common"] -= 1;
        inst.Tier++;
        Save();
        return CraftResult.Success;
    }

    public CraftResult RerollSkillAugment(string instanceId)
    {
        var inst = FindSkillAugmentInstance(instanceId);
        if (inst == null) return CraftResult.InsufficientMaterials;
        if (Profile.GetMaterial("crafting_common") < 1) return CraftResult.InsufficientMaterials;
        Profile.Materials["crafting_common"] -= 1;
        int min = 10 * inst.Tier;
        int max = 30 * inst.Tier;
        inst.TriggerChance = new System.Random().Next(min, max + 1);
        Save();
        return CraftResult.Success;
    }

    // ── Equipment Augment inventory ───────────────────────────────────────────

    public CraftResult CraftEquipmentAugmentItem(string recipeId)
    {
        var recipe = RecipeRegistry.Get(recipeId);
        if (recipe == null) return CraftResult.InsufficientMaterials;

        foreach (var (matId, qty) in recipe.MaterialCosts)
            if (Profile.GetMaterial(matId) < qty)
                return CraftResult.InsufficientMaterials;

        if (Profile.OwnedEquipmentAugmentInstances.Count >= ProfileData.MaxInventory)
            return CraftResult.InventoryFull;

        foreach (var (matId, qty) in recipe.MaterialCosts)
            Profile.Materials[matId] -= qty;

        Profile.OwnedEquipmentAugmentInstances.Add(new EquipmentAugmentInstance 
        { 
            DefinitionId = recipe.OutputItemId,
            TriggerChance = new System.Random().Next(10, 31)
        });
        Save();
        return CraftResult.Success;
    }

    public CraftResult SocketEquipmentAugment(string gearInstanceId, int slotIndex, string augmentInstanceId)
    {
        var gear    = FindGearInstance(gearInstanceId);
        var augment = FindEquipmentAugmentInstance(augmentInstanceId);
        if (gear == null || augment == null) return CraftResult.InsufficientMaterials;
        if (slotIndex >= gear.MaxEquipmentAugSlots) return CraftResult.InsufficientMaterials;

        var augmentDef = EquipmentAugmentRegistry.Get(augment.DefinitionId);
        if (augmentDef == null) return CraftResult.InsufficientMaterials;



        for (int i = 0; i < gear.SocketedEquipmentAugmentIds.Count; i++)
        {
            if (i == slotIndex) continue;
            if (FindEquipmentAugmentInstance(gear.SocketedEquipmentAugmentIds[i])?.DefinitionId == augment.DefinitionId)
                return CraftResult.InsufficientMaterials;
        }

        while (gear.SocketedEquipmentAugmentIds.Count <= slotIndex)
            gear.SocketedEquipmentAugmentIds.Add("");
        gear.SocketedEquipmentAugmentIds[slotIndex] = augmentInstanceId;
        Save();
        return CraftResult.Success;
    }

    public void RemoveEquipmentAugment(string gearInstanceId, int slotIndex)
    {
        var gear = FindGearInstance(gearInstanceId);
        if (gear == null || slotIndex >= gear.SocketedEquipmentAugmentIds.Count) return;
        gear.SocketedEquipmentAugmentIds[slotIndex] = "";
        Save();
    }

    public CraftResult UpgradeEquipmentAugment(string instanceId)
    {
        var inst = FindEquipmentAugmentInstance(instanceId);
        if (inst == null || inst.Tier >= Items.ItemTier.Max) return CraftResult.InsufficientMaterials;
        if (Profile.GetMaterial("crafting_common") < 1) return CraftResult.InsufficientMaterials;
        Profile.Materials["crafting_common"] -= 1;
        inst.Tier++;
        Save();
        return CraftResult.Success;
    }

    public CraftResult RerollEquipmentAugment(string instanceId)
    {
        var inst = FindEquipmentAugmentInstance(instanceId);
        if (inst == null) return CraftResult.InsufficientMaterials;
        if (Profile.GetMaterial("crafting_common") < 1) return CraftResult.InsufficientMaterials;
        Profile.Materials["crafting_common"] -= 1;
        int min = 10 * inst.Tier;
        int max = 30 * inst.Tier;
        inst.TriggerChance = new System.Random().Next(min, max + 1);
        Save();
        return CraftResult.Success;
    }

    public void SetSlotAutoActivate(string charId, int slotIndex, bool value)
    {
        var c = _characters.FirstOrDefault(x => x.Id == charId);
        if (c == null) return;
        while (c.SlotAutoActivate.Count <= slotIndex)
            c.SlotAutoActivate.Add(true);
        c.SlotAutoActivate[slotIndex] = value;
        Save();
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    private static Godot.Collections.Dictionary GearInstToDict(GearItemInstance g)
    {
        var augArr = new Godot.Collections.Array();
        foreach (var aid in g.SocketedEquipmentAugmentIds) augArr.Add(aid);
        return new Godot.Collections.Dictionary
        {
            ["id"]                          = g.Id,
            ["defId"]                       = g.DefinitionId,
            ["tier"]                        = g.Tier,
            ["socketedEquipmentAugmentIds"] = augArr,
        };
    }

    private static Godot.Collections.Dictionary SkillInstToDict(SkillItemInstance s)
    {
        var augArr = new Godot.Collections.Array();
        foreach (var aid in s.SocketedSkillAugmentIds) augArr.Add(aid);
        return new Godot.Collections.Dictionary
        {
            ["id"]                     = s.Id,
            ["defId"]                  = s.DefinitionId,
            ["tier"]                   = s.Tier,
            ["socketedSkillAugmentIds"] = augArr,
        };
    }

    private static Godot.Collections.Dictionary SkillAugInstToDict(SkillAugmentInstance s) => new()
    {
        ["id"]            = s.Id,
        ["defId"]         = s.DefinitionId,
        ["tier"]          = s.Tier,
        ["triggerChance"] = s.TriggerChance,
    };

    private static Godot.Collections.Dictionary EquipAugInstToDict(EquipmentAugmentInstance a) => new()
    {
        ["id"]            = a.Id,
        ["defId"]         = a.DefinitionId,
        ["tier"]          = a.Tier,
        ["triggerChance"] = a.TriggerChance,
    };

    private static GearItemInstance DictToGearInst(Godot.Collections.Dictionary d)
    {
        var rawDefId = d["defId"].ToString()!;
        var inst = new GearItemInstance
        {
            Id           = d["id"].ToString()!,
            DefinitionId = OldToNew.GetValueOrDefault(rawDefId, rawDefId),
            Tier         = System.Convert.ToInt32(d["tier"].Obj),
        };
        if (d.ContainsKey("socketedEquipmentAugmentIds") &&
            d["socketedEquipmentAugmentIds"].Obj is Godot.Collections.Array arr)
            foreach (var v in arr) inst.SocketedEquipmentAugmentIds.Add(v.ToString()!);
        return inst;
    }

    private static SkillItemInstance DictToSkillInst(Godot.Collections.Dictionary d)
    {
        var inst = new SkillItemInstance
        {
            Id           = d["id"].ToString()!,
            DefinitionId = MigrateSkillId(d["defId"].ToString()!),
            Tier         = d.ContainsKey("tier") ? System.Convert.ToInt32(d["tier"].Obj) : 1,
        };

        // Read new key; fall back to old key for saves written before the Support→SkillAugment rename
        string socketKey = d.ContainsKey("socketedSkillAugmentIds") ? "socketedSkillAugmentIds" : "socketedSupportInstanceIds";
        if (d.ContainsKey(socketKey) && d[socketKey].Obj is Godot.Collections.Array arr)
            foreach (var v in arr)
                inst.SocketedSkillAugmentIds.Add(v.ToString()!);
        return inst;
    }

    private static SkillAugmentInstance DictToSkillAugInst(Godot.Collections.Dictionary d) => new()
    {
        Id            = d["id"].ToString()!,
        DefinitionId  = d["defId"].ToString()!,
        Tier          = d.ContainsKey("tier") ? System.Convert.ToInt32(d["tier"].Obj) : 1,
        TriggerChance = d.ContainsKey("triggerChance") ? System.Convert.ToInt32(d["triggerChance"].Obj) : 15,
    };

    private static EquipmentAugmentInstance DictToEquipAugInst(Godot.Collections.Dictionary d) => new()
    {
        Id            = d["id"].ToString()!,
        DefinitionId  = d["defId"].ToString()!,
        Tier          = d.ContainsKey("tier") ? System.Convert.ToInt32(d["tier"].Obj) : 1,
        TriggerChance = d.ContainsKey("triggerChance") ? System.Convert.ToInt32(d["triggerChance"].Obj) : 15,
    };

    public void Save()
    {
        var gearArr = new Godot.Collections.Array();
        foreach (var g in Profile.OwnedGearInstances)             gearArr.Add(GearInstToDict(g));

        var skillArr = new Godot.Collections.Array();
        foreach (var s in Profile.OwnedSkillInstances)            skillArr.Add(SkillInstToDict(s));

        var skillAugArr = new Godot.Collections.Array();
        foreach (var s in Profile.OwnedSkillAugmentInstances)     skillAugArr.Add(SkillAugInstToDict(s));

        var equipAugArr = new Godot.Collections.Array();
        foreach (var a in Profile.OwnedEquipmentAugmentInstances) equipAugArr.Add(EquipAugInstToDict(a));

        var matsDict = new Godot.Collections.Dictionary();
        foreach (var kv in Profile.Materials) matsDict[kv.Key] = kv.Value;

        var profileDict = new Godot.Collections.Dictionary
        {
            ["coinBank"]                       = Profile.CoinBank,
            ["materials"]                      = matsDict,
            ["ownedGearInstances"]             = gearArr,
            ["ownedSkillInstances"]            = skillArr,
            ["ownedSkillAugmentInstances"]     = skillAugArr,
            ["ownedEquipmentAugmentInstances"] = equipAugArr,
        };

        var charList = new Godot.Collections.Array();
        foreach (var c in _characters)
        {
            var equippedGearDict = new Godot.Collections.Dictionary();
            foreach (var kv in c.EquippedGear) equippedGearDict[kv.Key] = GearInstToDict(kv.Value);

            var slottedArr = new Godot.Collections.Array();
            foreach (var sid in c.SlottedSkillInstanceIds) slottedArr.Add(sid);

            var autoActivateArr = new Godot.Collections.Array();
            foreach (var b in c.SlotAutoActivate) autoActivateArr.Add(b);

            charList.Add(new Godot.Collections.Dictionary
            {
                ["id"]                      = c.Id,
                ["name"]                    = c.Name,
                ["type"]                    = c.Type.ToString(),
                ["runsCompleted"]           = c.RunsCompleted,
                ["currentLevel"]            = c.CurrentLevel,
                ["currentXp"]              = c.CurrentXp,
                ["equippedGear"]            = equippedGearDict,
                ["slottedSkillInstanceIds"] = slottedArr,
                ["slotAutoActivate"]        = autoActivateArr,
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
        if (!FileAccess.FileExists(SavePath))
        {
            Profile.AddMaterial("crafting_common", 10);
            return;
        }
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        if (file == null) return;

        var parsed = Json.ParseString(file.GetAsText());
        if (parsed.Obj is not Godot.Collections.Dictionary root) return;

        // ── Profile ──────────────────────────────────────────────────────────
        if (root.ContainsKey("profile") && root["profile"].Obj is Godot.Collections.Dictionary pd)
        {
            Profile.CoinBank = pd.ContainsKey("coinBank") ? System.Convert.ToInt32(pd["coinBank"].Obj) : 0;

            if (pd.ContainsKey("materials") && pd["materials"].Obj is Godot.Collections.Dictionary md)
                foreach (var kv in md)
                    Profile.Materials[kv.Key.ToString()!] = System.Convert.ToInt32(kv.Value.Obj);
            else if (pd.ContainsKey("craftingCurrency1"))
                Profile.AddMaterial("crafting_common", System.Convert.ToInt32(pd["craftingCurrency1"].Obj));

            if (pd.ContainsKey("ownedGearInstances") && pd["ownedGearInstances"].Obj is Godot.Collections.Array ga)
                foreach (var v in ga)
                    if (v.Obj is Godot.Collections.Dictionary gd) Profile.OwnedGearInstances.Add(DictToGearInst(gd));

            if (pd.ContainsKey("ownedSkillInstances") && pd["ownedSkillInstances"].Obj is Godot.Collections.Array sa)
                foreach (var v in sa)
                    if (v.Obj is Godot.Collections.Dictionary sd) Profile.OwnedSkillInstances.Add(DictToSkillInst(sd));

            // Read new key first; fall back to old key for saves written before the Support→SkillAugment rename
            string augKey = pd.ContainsKey("ownedSkillAugmentInstances") ? "ownedSkillAugmentInstances" : "ownedSupportInstances";
            if (pd.ContainsKey(augKey) && pd[augKey].Obj is Godot.Collections.Array spa)
                foreach (var v in spa)
                    if (v.Obj is Godot.Collections.Dictionary spd) Profile.OwnedSkillAugmentInstances.Add(DictToSkillAugInst(spd));

            if (pd.ContainsKey("ownedEquipmentAugmentInstances") && pd["ownedEquipmentAugmentInstances"].Obj is Godot.Collections.Array eaa)
                foreach (var v in eaa)
                    if (v.Obj is Godot.Collections.Dictionary ead) Profile.OwnedEquipmentAugmentInstances.Add(DictToEquipAugInst(ead));

            // Migrate: old ownedItemIds (string list) → GearItemInstance list
            if (pd.ContainsKey("ownedItemIds") && pd["ownedItemIds"].Obj is Godot.Collections.Array oldItems)
                foreach (var v in oldItems)
                {
                    string defId = v.ToString()!;
                    if (ItemRegistry.Get(defId) != null)
                        Profile.OwnedGearInstances.Add(new GearItemInstance { DefinitionId = defId });
                }

            // Migrate: old ownedSkillIds (string list) → SkillItemInstance list
            if (pd.ContainsKey("ownedSkillIds") && pd["ownedSkillIds"].Obj is Godot.Collections.Array oldSkills)
                foreach (var v in oldSkills)
                    Profile.OwnedSkillInstances.Add(new SkillItemInstance { DefinitionId = MigrateSkillId(v.ToString()!) });
        }

        // ── Characters ───────────────────────────────────────────────────────
        if (!root.ContainsKey("characters") || root["characters"].Obj is not Godot.Collections.Array list) return;

        _characters.Clear();
        foreach (var item in list)
        {
            if (item.Obj is not Godot.Collections.Dictionary gd) continue;

            var c = new CharacterData
            {
                Id            = gd.ContainsKey("id")            ? gd["id"].ToString()!                                     : System.Guid.NewGuid().ToString(),
                Name          = gd.ContainsKey("name")          ? gd["name"].ToString()!                                   : "",
                Type          = gd.ContainsKey("type")          ? System.Enum.Parse<CharacterType>(gd["type"].ToString()!) : CharacterType.Warrior,
                RunsCompleted = gd.ContainsKey("runsCompleted") ? System.Convert.ToInt32(gd["runsCompleted"].Obj)          : 0,
                CurrentLevel  = gd.ContainsKey("currentLevel")  ? System.Convert.ToInt32(gd["currentLevel"].Obj)          : 1,
                CurrentXp     = gd.ContainsKey("currentXp")     ? System.Convert.ToInt32(gd["currentXp"].Obj)             : 0,
            };

            if (gd.ContainsKey("equippedGear") && gd["equippedGear"].Obj is Godot.Collections.Dictionary eqGd)
                foreach (var kv in eqGd)
                    if (kv.Value.Obj is Godot.Collections.Dictionary instDict)
                    {
                        string slotKey = MigrateSlotKey(kv.Key.ToString()!);
                        c.EquippedGear[slotKey] = DictToGearInst(instDict);
                    }

            // Migrate old format: equippedItems (slot → definition ID)
            if (gd.ContainsKey("equippedItems") && gd["equippedItems"].Obj is Godot.Collections.Dictionary oldEq)
                foreach (var kv in oldEq)
                {
                    string slot  = kv.Key.ToString()!;
                    string defId = kv.Value.ToString()!;
                    if (!c.EquippedGear.ContainsKey(slot))
                    {
                        string mapped = OldToNew.GetValueOrDefault(defId, defId);
                        if (ItemRegistry.Get(mapped) != null)
                            c.EquippedGear[slot] = new GearItemInstance { DefinitionId = mapped };
                    }
                }

            if (gd.ContainsKey("slottedSkillInstanceIds") && gd["slottedSkillInstanceIds"].Obj is Godot.Collections.Array slotArr)
                c.SlottedSkillInstanceIds = slotArr.Select(v => v.ToString()!).ToList();

            // Migrate old format: slottedSkillIds (definition IDs)
            if (gd.ContainsKey("slottedSkillIds") && gd["slottedSkillIds"].Obj is Godot.Collections.Array oldSlots
                && c.SlottedSkillInstanceIds.Count == 0)
            {
                var defIdToInst = new Dictionary<string, SkillItemInstance>();
                foreach (var v in oldSlots)
                {
                    string defId = v.ToString()!;
                    if (string.IsNullOrEmpty(defId)) continue;
                    if (!defIdToInst.ContainsKey(defId))
                    {
                        var inst = new SkillItemInstance { DefinitionId = MigrateSkillId(defId) };
                        defIdToInst[defId] = inst;
                        Profile.OwnedSkillInstances.Add(inst);
                    }
                }
                c.SlottedSkillInstanceIds = oldSlots.Select(v =>
                {
                    string defId = v.ToString()!;
                    return string.IsNullOrEmpty(defId) ? "" : defIdToInst.GetValueOrDefault(defId)?.Id ?? "";
                }).ToList();
            }

            while (c.SlottedSkillInstanceIds.Count < 5)
                c.SlottedSkillInstanceIds.Add("");

            if (gd.ContainsKey("slotAutoActivate") && gd["slotAutoActivate"].Obj is Godot.Collections.Array aaArr)
                c.SlotAutoActivate = aaArr.Select(v => System.Convert.ToBoolean(v.Obj)).ToList();
            while (c.SlotAutoActivate.Count < 5)
                c.SlotAutoActivate.Add(true);

            _characters.Add(c);
        }
    }

    private static string MigrateSlotKey(string oldKey) => oldKey switch
    {
        "Armor"     => "Body",
        "Accessory" => "Ring",
        _           => oldKey,
    };

    private static string MigrateSkillId(string oldId) => oldId switch
    {
        "attack_melee"           => "entity_burst",
        "attack_ranged_physical" => "entity_burst",
        "attack_ranged_magic"    => "entity_burst",
        "arrow"                  => "entity_burst",
        "bolt"                   => "entity_burst",
        "strike"                 => "entity_burst",
        "cyclone"                => "self_channeled_tick",
        "nova"                   => "self_burst",
        "damage_aura"            => "self_duration_tick",
        _                        => oldId,
    };

    private static readonly Dictionary<string, string> OldToNew = new()
    {
        ["iron_sword"]      = "sword_t1",
        ["battle_axe"]      = "sword_t1",
        ["enchanted_blade"] = "wand_t1",
        ["leather_vest"]    = "light_body_t1",
        ["chain_mail"]      = "heavy_body_t1",
        ["mage_robe"]       = "medium_body_t1",
        ["light_armor_t1"]  = "light_body_t1",
        ["medium_armor_t1"] = "medium_body_t1",
        ["heavy_armor_t1"]  = "heavy_body_t1",
        ["swift_ring"]      = "ring_t1",
        ["vitality_charm"]  = "ring_t1",
        ["war_band"]        = "ring_t1",
        ["accessory_t1"]    = "ring_t1",
    };
}
