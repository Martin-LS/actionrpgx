using Godot;
using System.Linq;
using Godot1.Crafting;
using Godot1.Items;

namespace Godot1.Ui;

public partial class AccountScreen : Control
{
    // Characters tab — roster view
    private VBoxContainer _characterList = null!;
    private Control       _createPanel   = null!;
    private LineEdit      _nameInput     = null!;
    private Character.CharacterType _pendingType = Character.CharacterType.Warrior;

    // Crafting tab
    private VBoxContainer _craftVBox = null!;

    private Character.CharacterManager _manager = null!;

    private const string RosterBase   = "VBox/HSplit/RightPanel/TabContainer/Characters/RosterView";
    private const string CraftingBase = "VBox/HSplit/RightPanel/TabContainer/Crafting";

    public override void _Ready()
    {
        _manager = GetNode<Character.CharacterManager>("/root/CharacterManager");

        // Characters tab — roster
        _characterList = GetNode<VBoxContainer>($"{RosterBase}/Scroll/CharacterList");
        _createPanel   = GetNode<Control>      ($"{RosterBase}/CreatePanel");
        _nameInput     = GetNode<LineEdit>     ($"{RosterBase}/CreatePanel/VBox/NameInput");

        var confirmBtn = GetNode<Button>($"{RosterBase}/CreatePanel/VBox/ConfirmBtn");
        confirmBtn.Disabled = true;
        _nameInput.TextChanged += text => confirmBtn.Disabled = text.Trim().Length == 0;

        GetNode<Button>($"{RosterBase}/NewCharacterButton").Pressed          += () => _createPanel.Visible = true;
        GetNode<Button>($"{RosterBase}/CreatePanel/VBox/WarriorBtn").Pressed += () => _pendingType = Character.CharacterType.Warrior;
        GetNode<Button>($"{RosterBase}/CreatePanel/VBox/RogueBtn").Pressed   += () => _pendingType = Character.CharacterType.Rogue;
        GetNode<Button>($"{RosterBase}/CreatePanel/VBox/MageBtn").Pressed    += () => _pendingType = Character.CharacterType.Mage;
        confirmBtn.Pressed                                                    += OnConfirmCreate;
        GetNode<Button>($"{RosterBase}/CreatePanel/VBox/CancelBtn").Pressed  += () => _createPanel.Visible = false;

        // Crafting tab
        _craftVBox = GetNode<VBoxContainer>($"{CraftingBase}/VBox");

        GetNode<Button>("VBox/BackButton").Pressed += () =>
            GetTree().ChangeSceneToFile("res://src/ui/main_menu.tscn");

        Refresh();
    }

    private void Refresh()
    {
        RefreshRoster();
        RefreshCrafting();
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
                GetTree().ChangeSceneToFile("res://src/ui/character_screen.tscn");
            };
            hbox.AddChild(selectBtn);

            var deleteBtn = new Button { Text = "X" };
            deleteBtn.Pressed += () =>
            {
                _manager.Delete(capturedId);
                Refresh();
            };
            hbox.AddChild(deleteBtn);
            _characterList.AddChild(hbox);
        }
    }

    private void RefreshCrafting()
    {
        foreach (Node child in _craftVBox.GetChildren())
            child.QueueFree();

        var matsLabel = new Label
        {
            Text = $"Common materials: {_manager.Profile.GetMaterial("crafting_common")}",
        };
        _craftVBox.AddChild(matsLabel);

        bool inventoryFull = _manager.Profile.OwnedItemIds.Count >= Character.ProfileData.MaxInventory;

        foreach (var recipe in RecipeRegistry.All.Values)
        {
            var item = ItemRegistry.Get(recipe.OutputItemId);
            if (item == null) continue;

            string costText = string.Join(", ", recipe.MaterialCosts.Select(kv =>
                $"{kv.Value}× {(kv.Key == "crafting_common" ? "Common" : kv.Key)}"));
            bool canAfford = recipe.MaterialCosts.All(kv => _manager.Profile.GetMaterial(kv.Key) >= kv.Value);

            var btn = new Button
            {
                Text     = $"{item.Name}  —  {costText}",
                Disabled = !canAfford || inventoryFull,
            };
            string capturedId = recipe.Id;
            btn.Pressed += () => { _manager.CraftItem(capturedId); Refresh(); };
            _craftVBox.AddChild(btn);
        }
    }

    private static string BuildTooltip(ItemData item)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"{item.Name}  [{item.Slot}]");
        if (item.BonusHp            != 0)   sb.Append($"\nHP {item.BonusHp:+#;-#;0}");
        if (item.BonusSpeed         != 0f)  sb.Append($"\nSpeed {item.BonusSpeed:+#;-#;0}");
        if (item.SkillBonus         != 0f)  sb.Append($"\nSkill Bonus {item.SkillBonus:+#;-#;0}");
        if (item.DamageReduction    != 0f)  sb.Append($"\nDamage Reduction {item.DamageReduction:P0}");
        if (item.PhysicalResistance != 0f)  sb.Append($"\nPhys. Resist {item.PhysicalResistance:P0}");
        return sb.ToString();
    }

    private void OnConfirmCreate()
    {
        var name = _nameInput.Text.Trim();
        if (name.Length == 0) return;
        _manager.Create(name, _pendingType);
        _nameInput.Text      = "";
        _createPanel.Visible = false;
        Refresh();
    }
}
