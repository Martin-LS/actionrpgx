using Godot;
using Godot1.Items;
using Godot1.Stats;

namespace Godot1.Ui;

// Kept for compatibility — the account_screen.tscn supersedes this scene.
public partial class CharacterScreen : Control
{
    private Label         _nameLabel     = null!;
    private Label         _typeLabel     = null!;
    private Label         _levelLabel    = null!;
    private Label         _statsLabel    = null!;
    private Button        _weaponBtn     = null!;
    private Button        _armorBtn      = null!;
    private Button        _accBtn        = null!;
    private VBoxContainer _inventoryList = null!;

    private Character.CharacterManager _manager   = null!;
    private Character.CharacterData    _character = null!;

    public override void _Ready()
    {
        _nameLabel     = GetNode<Label>        ("VBox/NameLabel");
        _typeLabel     = GetNode<Label>        ("VBox/TypeLabel");
        _levelLabel    = GetNode<Label>        ("VBox/LevelLabel");
        _statsLabel    = GetNode<Label>        ("VBox/StatsLabel");
        _weaponBtn     = GetNode<Button>       ("VBox/GearPanel/WeaponSlotButton");
        _armorBtn      = GetNode<Button>       ("VBox/GearPanel/ArmorSlotButton");
        _accBtn        = GetNode<Button>       ("VBox/GearPanel/AccessorySlotButton");
        _inventoryList = GetNode<VBoxContainer>("VBox/InventoryPanel/InventoryScroll/InventoryList");

        GetNode<Button>("VBox/Buttons/BackButton").Pressed     += () =>
            GetTree().ChangeSceneToFile("res://src/ui/account_screen.tscn");
        GetNode<Button>("VBox/Buttons/StartRunButton").Pressed += () =>
            GetTree().ChangeSceneToFile("res://main.tscn");

        _weaponBtn.Pressed += () => OpenPicker(ItemSlot.Weapon);
        _armorBtn.Pressed  += () => OpenPicker(ItemSlot.Armor);
        _accBtn.Pressed    += () => OpenPicker(ItemSlot.Accessory);

        _manager   = GetNode<Character.CharacterManager>("/root/CharacterManager");
        _character = _manager.SelectedCharacter!;
        if (_character == null)
        {
            GetTree().ChangeSceneToFile("res://src/ui/account_screen.tscn");
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        var stats     = _character.BuildStatBlock();
        int   totalHp  = (int)stats.Get(StatId.MaxHp);
        float totalSpd = stats.Get(StatId.Speed);
        float totalDmg = stats.Get(StatId.Damage);

        _nameLabel.Text  = _character.Name;
        _typeLabel.Text  = _character.Type.ToString();
        _levelLabel.Text = $"Level {_character.CurrentLevel}   XP: {_character.CurrentXp}";
        _statsLabel.Text = $"HP {totalHp}   Speed {totalSpd:F0}   Damage {totalDmg:F0}\n" +
                           $"Runs: {_character.RunsCompleted}   " +
                           $"Coins: {_manager.Profile.CoinBank}   " +
                           $"Crafting: {_manager.Profile.CraftingCurrency1}";

        RefreshGearSlots();
        RefreshInventory();
    }

    private void RefreshGearSlots()
    {
        _weaponBtn.Text = SlotLabel(ItemSlot.Weapon,    "Weapon");
        _armorBtn.Text  = SlotLabel(ItemSlot.Armor,     "Armor");
        _accBtn.Text    = SlotLabel(ItemSlot.Accessory, "Accessory");
    }

    private string SlotLabel(ItemSlot slot, string slotName)
    {
        if (_character.EquippedItems.TryGetValue(slot.ToString(), out var id))
        {
            var item = ItemRegistry.Get(id);
            return item != null ? $"{slotName}: {item.Name}" : $"{slotName}: (unknown)";
        }
        return $"{slotName}: Empty";
    }

    private void RefreshInventory()
    {
        foreach (Node child in _inventoryList.GetChildren())
            child.QueueFree();

        var ownedIds = _manager.Profile.OwnedItemIds;
        if (ownedIds.Count == 0)
        {
            _inventoryList.AddChild(new Label { Text = "No items yet." });
            return;
        }

        foreach (var id in ownedIds)
        {
            var item = ItemRegistry.Get(id);
            if (item == null) continue;

            string equippedTag = _character.EquippedItems.TryGetValue(item.Slot.ToString(), out var eid) && eid == id
                ? " [equipped]"
                : "";

            string statParts = "";
            if (item.BonusHp    != 0)   statParts += $" HP {item.BonusHp:+#;-#;0}";
            if (item.BonusSpeed != 0f)  statParts += $" Spd {item.BonusSpeed:+#;-#;0}";
            if (item.BonusDamage != 0f) statParts += $" Dmg {item.BonusDamage:+#;-#;0}";

            _inventoryList.AddChild(new Label
            {
                Text = $"[{item.Slot}] {item.Name}{statParts}{equippedTag}"
            });
        }
    }

    private void OpenPicker(ItemSlot slot)
    {
        var pickerScene = GD.Load<PackedScene>("res://src/ui/item_picker_panel.tscn");
        var picker      = pickerScene.Instantiate<ItemPickerPanel>();
        picker.Init(_manager, _character, slot, () => Refresh());
        AddChild(picker);
    }
}
