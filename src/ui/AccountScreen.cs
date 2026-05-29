using Godot;
using Godot1.Items;
using Godot1.Stats;

namespace Godot1.Ui;

public partial class AccountScreen : Control
{
    // Left panel
    private Label         _inventoryInfo = null!;
    private GridContainer _inventoryGrid = null!;

    // Characters tab — roster view
    private Control       _rosterView    = null!;
    private VBoxContainer _characterList = null!;
    private Control       _createPanel   = null!;
    private LineEdit      _nameInput     = null!;
    private Character.CharacterType _pendingType = Character.CharacterType.Warrior;

    // Characters tab — character view
    private Control       _characterView = null!;
    private Label         _nameLabel     = null!;
    private Label         _typeLabel     = null!;
    private Label         _levelLabel    = null!;
    private Label         _statsLabel    = null!;
    private Button        _weaponBtn     = null!;
    private Button        _armorBtn      = null!;
    private Button        _accBtn        = null!;

    // Crafting tab
    private Button _craftWeaponBtn    = null!;
    private Button _craftArmorBtn     = null!;
    private Button _craftAccessoryBtn = null!;

    private Character.CharacterManager _manager = null!;

    private const string RosterBase    = "VBox/HSplit/RightPanel/TabContainer/Characters/RosterView";
    private const string CharViewBase  = "VBox/HSplit/RightPanel/TabContainer/Characters/CharacterView";
    private const string CraftingBase  = "VBox/HSplit/RightPanel/TabContainer/Crafting";

    public override void _Ready()
    {
        _manager = GetNode<Character.CharacterManager>("/root/CharacterManager");

        // Left panel
        _inventoryInfo = GetNode<Label>        ("VBox/HSplit/LeftPanel/InventoryInfo");
        _inventoryGrid = GetNode<GridContainer>("VBox/HSplit/LeftPanel/InventoryGrid");

        // Characters tab — roster
        _rosterView    = GetNode<Control>      ($"{RosterBase}");
        _characterList = GetNode<VBoxContainer>($"{RosterBase}/Scroll/CharacterList");
        _createPanel   = GetNode<Control>      ($"{RosterBase}/CreatePanel");
        _nameInput     = GetNode<LineEdit>     ($"{RosterBase}/CreatePanel/VBox/NameInput");

        GetNode<Button>($"{RosterBase}/NewCharacterButton").Pressed                   += () => _createPanel.Visible = true;
        GetNode<Button>($"{RosterBase}/CreatePanel/VBox/WarriorBtn").Pressed          += () => _pendingType = Character.CharacterType.Warrior;
        GetNode<Button>($"{RosterBase}/CreatePanel/VBox/RogueBtn").Pressed            += () => _pendingType = Character.CharacterType.Rogue;
        GetNode<Button>($"{RosterBase}/CreatePanel/VBox/MageBtn").Pressed             += () => _pendingType = Character.CharacterType.Mage;
        GetNode<Button>($"{RosterBase}/CreatePanel/VBox/ConfirmBtn").Pressed          += OnConfirmCreate;
        GetNode<Button>($"{RosterBase}/CreatePanel/VBox/CancelBtn").Pressed           += () => _createPanel.Visible = false;

        // Characters tab — character view
        _characterView = GetNode<Control>($"{CharViewBase}");
        _nameLabel     = GetNode<Label>  ($"{CharViewBase}/VBox/NameLabel");
        _typeLabel     = GetNode<Label>  ($"{CharViewBase}/VBox/TypeLabel");
        _levelLabel    = GetNode<Label>  ($"{CharViewBase}/VBox/LevelLabel");
        _statsLabel    = GetNode<Label>  ($"{CharViewBase}/VBox/StatsLabel");
        _weaponBtn     = GetNode<Button> ($"{CharViewBase}/VBox/GearPanel/WeaponSlotButton");
        _armorBtn      = GetNode<Button> ($"{CharViewBase}/VBox/GearPanel/ArmorSlotButton");
        _accBtn        = GetNode<Button> ($"{CharViewBase}/VBox/GearPanel/AccessorySlotButton");

        _weaponBtn.Pressed += () => OpenPicker(ItemSlot.Weapon);
        _armorBtn.Pressed  += () => OpenPicker(ItemSlot.Armor);
        _accBtn.Pressed    += () => OpenPicker(ItemSlot.Accessory);

        GetNode<Button>($"{CharViewBase}/VBox/Buttons/ChangeCharacterButton").Pressed += () =>
        {
            _manager.SelectCharacter("");
            ShowRoster();
        };
        GetNode<Button>($"{CharViewBase}/VBox/Buttons/StartRunButton").Pressed += () =>
            GetTree().ChangeSceneToFile("res://main.tscn");

        // Crafting tab
        _craftWeaponBtn    = GetNode<Button>($"{CraftingBase}/VBox/CraftWeaponButton");
        _craftArmorBtn     = GetNode<Button>($"{CraftingBase}/VBox/CraftArmorButton");
        _craftAccessoryBtn = GetNode<Button>($"{CraftingBase}/VBox/CraftAccessoryButton");

        _craftWeaponBtn.Pressed    += () => { _manager.AddItemToInventory("iron_sword");   Refresh(); };
        _craftArmorBtn.Pressed     += () => { _manager.AddItemToInventory("leather_vest"); Refresh(); };
        _craftAccessoryBtn.Pressed += () => { _manager.AddItemToInventory("swift_ring");   Refresh(); };

        GetNode<Button>("VBox/BackButton").Pressed += () =>
            GetTree().ChangeSceneToFile("res://src/ui/main_menu.tscn");

        Refresh();
    }

    private void Refresh()
    {
        RefreshInventory();
        if (_manager.SelectedCharacter != null)
            ShowCharacter();
        else
            ShowRoster();
    }

    private void ShowRoster()
    {
        _rosterView.Visible    = true;
        _characterView.Visible = false;
        _createPanel.Visible   = false;
        RefreshRoster();
    }

    private void ShowCharacter()
    {
        _rosterView.Visible    = false;
        _characterView.Visible = true;
        RefreshCharacter();
    }

    private void RefreshRoster()
    {
        foreach (Node child in _characterList.GetChildren())
            child.QueueFree();

        foreach (var c in _manager.GetAll())
        {
            var hbox = new HBoxContainer();
            var selectBtn = new Button
            {
                Text = $"{c.Name}  [{c.Type}]  Lv.{c.CurrentLevel}  Runs: {c.RunsCompleted}",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            string capturedId = c.Id;
            selectBtn.Pressed += () =>
            {
                _manager.SelectCharacter(capturedId);
                ShowCharacter();
            };
            hbox.AddChild(selectBtn);

            var deleteBtn = new Button { Text = "X" };
            deleteBtn.Pressed += () =>
            {
                _manager.Delete(capturedId);
                RefreshRoster();
                RefreshInventory();
            };
            hbox.AddChild(deleteBtn);
            _characterList.AddChild(hbox);
        }
    }

    private void RefreshCharacter()
    {
        var c     = _manager.SelectedCharacter!;
        var stats = c.BuildStatBlock();

        _nameLabel.Text  = c.Name;
        _typeLabel.Text  = c.Type.ToString();
        _levelLabel.Text = $"Level {c.CurrentLevel}   XP: {c.CurrentXp}";
        _statsLabel.Text = $"HP {(int)stats.Get(StatId.MaxHp)}   Speed {stats.Get(StatId.Speed):F0}   Damage {stats.Get(StatId.Damage):F0}\nRuns: {c.RunsCompleted}";

        RefreshSlotButton(_weaponBtn, c, ItemSlot.Weapon,    "Weapon");
        RefreshSlotButton(_armorBtn,  c, ItemSlot.Armor,     "Armor");
        RefreshSlotButton(_accBtn,    c, ItemSlot.Accessory, "Accessory");
    }

    private static void RefreshSlotButton(Button btn, Character.CharacterData c, ItemSlot slot, string slotName)
    {
        btn.AddThemeConstantOverride("icon_max_width", 40);

        if (c.EquippedItems.TryGetValue(slot.ToString(), out var id))
        {
            var item = ItemRegistry.Get(id);
            if (item != null)
            {
                btn.Text = $"{slotName}: {item.Name}";
                btn.Icon = !string.IsNullOrEmpty(item.IconPath)
                    ? GD.Load<Texture2D>(item.IconPath)
                    : null;
                return;
            }
        }

        btn.Icon = null;
        btn.Text = $"{slotName}: Empty";
    }

    private void RefreshInventory()
    {
        var profile = _manager.Profile;
        _inventoryInfo.Text = $"{profile.OwnedItemIds.Count} / {Character.ProfileData.MaxInventory}   Coins: {profile.CoinBank}   Crafting: {profile.CraftingCurrency1}";

        foreach (Node child in _inventoryGrid.GetChildren())
            child.QueueFree();

        for (int i = 0; i < Character.ProfileData.MaxInventory; i++)
        {
            var btn = new Button
            {
                CustomMinimumSize = new Vector2(60, 60),
            };

            if (i < profile.OwnedItemIds.Count)
            {
                var id   = profile.OwnedItemIds[i];
                var item = ItemRegistry.Get(id);
                if (item != null)
                {
                    if (!string.IsNullOrEmpty(item.IconPath))
                    {
                        btn.Icon        = GD.Load<Texture2D>(item.IconPath);
                        btn.ExpandIcon  = true;
                    }
                    else
                    {
                        btn.Text = item.Name;
                        btn.AddThemeFontSizeOverride("font_size", 10);
                    }
                    btn.TooltipText = BuildTooltip(item);
                }
            }
            else
            {
                btn.Modulate = new Color(1f, 1f, 1f, 0.3f);
                btn.Disabled = true;
            }

            _inventoryGrid.AddChild(btn);
        }

        bool full = profile.OwnedItemIds.Count >= Character.ProfileData.MaxInventory;
        if (_craftWeaponBtn != null)
        {
            _craftWeaponBtn.Disabled    = full;
            _craftArmorBtn.Disabled     = full;
            _craftAccessoryBtn.Disabled = full;
        }
    }

    private static string BuildTooltip(ItemData item)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"{item.Name}  [{item.Slot}]");
        if (item.BonusHp    != 0)   sb.Append($"\nHP {item.BonusHp:+#;-#;0}");
        if (item.BonusSpeed != 0f)  sb.Append($"\nSpeed {item.BonusSpeed:+#;-#;0}");
        if (item.BonusDamage != 0f) sb.Append($"\nDamage {item.BonusDamage:+#;-#;0}");
        return sb.ToString();
    }

    private void OnConfirmCreate()
    {
        var name = _nameInput.Text.Trim();
        if (name.Length == 0) return;
        _manager.Create(name, _pendingType);
        _nameInput.Text = "";
        _createPanel.Visible = false;
        RefreshRoster();
        RefreshInventory();
    }

    private void OpenPicker(ItemSlot slot)
    {
        var pickerScene = GD.Load<PackedScene>("res://src/ui/item_picker_panel.tscn");
        var picker      = pickerScene.Instantiate<ItemPickerPanel>();
        picker.Init(_manager, _manager.SelectedCharacter!, slot, () => Refresh());
        AddChild(picker);
    }
}
