using Godot;

namespace Godot1.Ui;

public partial class AccountScreen : Control
{
    private VBoxContainer _characterList = null!;
    private Control       _createPanel   = null!;
    private LineEdit      _nameInput     = null!;
    private Character.CharacterType _pendingType = Character.CharacterType.Warrior;

    private Character.CharacterManager _manager = null!;

    private const string RosterBase = "VBox/HSplit/RightPanel/TabContainer/Characters/RosterView";

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

        GetNode<Button>("VBox/BackButton").Pressed += () =>
            GetTree().ChangeSceneToFile("res://src/ui/main_menu.tscn");

        Refresh();
    }

    private void Refresh()
    {
        RefreshRoster();
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
