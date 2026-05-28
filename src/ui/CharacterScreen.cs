using Godot;
using Godot1.Items;

namespace Godot1.Ui;

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
            GetTree().ChangeSceneToFile("res://src/ui/character_select.tscn");
        GetNode<Button>("VBox/Buttons/StartRunButton").Pressed += () =>
            GetTree().ChangeSceneToFile("res://main.tscn");

        _weaponBtn.Pressed += () => OpenPicker(ItemSlot.Weapon);
        _armorBtn.Pressed  += () => OpenPicker(ItemSlot.Armor);
        _accBtn.Pressed    += () => OpenPicker(ItemSlot.Accessory);

        _manager   = GetNode<Character.CharacterManager>("/root/CharacterManager");
        _character = _manager.SelectedCharacter!;
        if (_character == null)
        {
            GetTree().ChangeSceneToFile("res://src/ui/character_select.tscn");
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        var (hp, spd, dmg) = _character.BaseStats();
        int levelsAboveOne = _character.CurrentLevel - 1;
        int   totalHp  = hp  + levelsAboveOne * 5;
        float totalDmg = dmg + levelsAboveOne;

        _nameLabel.Text  = _character.Name;
        _typeLabel.Text  = _character.Type.ToString();
        _levelLabel.Text = $"Level {_character.CurrentLevel}   XP: {_character.CurrentXp}";
        _statsLabel.Text = $"HP {totalHp}   Speed {spd:F0}   Damage {totalDmg:F0}\nRuns: {_character.RunsCompleted}   Coins: {_character.CoinBank}   Crafting ⚙: {_character.CraftingCurrency1}";

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

        if (_character.OwnedItemIds.Count == 0)
        {
            _inventoryList.AddChild(new Label { Text = "No items yet — earn them by defeating enemies." });
            return;
        }

        foreach (var id in _character.OwnedItemIds)
        {
            var item = ItemRegistry.Get(id);
            if (item == null) continue;

            string equippedTag = _character.EquippedItems.TryGetValue(item.Slot.ToString(), out var eid) && eid == id
                ? $" [equipped]"
                : "";

            string statParts = "";
            if (item.BonusHp    != 0)   statParts += $" HP {item.BonusHp:+#;-#;0}";
            if (item.BonusSpeed != 0f)  statParts += $" Spd {item.BonusSpeed:+#;-#;0}";
            if (item.BonusDamage != 0f) statParts += $" Dmg {item.BonusDamage:+#;-#;0}";

            var label = new Label
            {
                Text = $"[{item.Slot}] {item.Name}{statParts}{equippedTag}"
            };
            _inventoryList.AddChild(label);
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
