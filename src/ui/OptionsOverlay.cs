using Godot;
using ActionRpgX.Player;
using ActionRpgX.Weapon;
using ActionRpgX.Character;

namespace Godot1.Ui;

public partial class OptionsOverlay : CanvasLayer
{
    private Button _globalButton = null!;
    private Control _menuControl = null!;
    private VBoxContainer _buttonList = null!;

    public override void _Ready()
    {
        _globalButton = GetNode<Button>("GlobalButton");
        _menuControl = GetNode<Control>("MenuControl");
        _buttonList = GetNode<VBoxContainer>("MenuControl/PanelContainer/VBoxContainer/ButtonList");

        _menuControl.Visible = false;
        GetTree().Paused = false;

        _globalButton.Pressed += () => Toggle();
        UpdateGlobalButtonVisibility();
    }

    public override void _Process(double delta)
    {
        UpdateGlobalButtonVisibility();
    }

    private void UpdateGlobalButtonVisibility()
    {
        if (_menuControl.Visible)
        {
            _globalButton.Visible = false;
            return;
        }

        string currentScene = GetTree().CurrentScene?.SceneFilePath ?? "";
        bool isMainMenu = currentScene == "res://src/ui/main_menu.tscn";
        _globalButton.Visible = !isMainMenu;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            string currentScene = GetTree().CurrentScene?.SceneFilePath ?? "";
            if (currentScene == "res://src/ui/main_menu.tscn") return;

            Toggle();
            GetViewport().SetInputAsHandled();
        }
    }

    private void Toggle()
    {
        _menuControl.Visible = !_menuControl.Visible;
        GetTree().Paused = _menuControl.Visible;
        UpdateGlobalButtonVisibility();

        if (_menuControl.Visible)
            RebuildMenu();
    }

    private void RebuildMenu()
    {
        ClearButtonList();

        string currentScene = GetTree().CurrentScene?.SceneFilePath ?? "";
        bool isInRun = currentScene == "res://main.tscn";

        if (isInRun)
        {
            Button resumeBtn = new Button();
            resumeBtn.Text = "Resume";
            resumeBtn.Pressed += () => Toggle();
            _buttonList.AddChild(resumeBtn);

            HBoxContainer endRunBox = new HBoxContainer();
            endRunBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            Button endRunBtn = new Button();
            endRunBtn.Text = "End Run";
            endRunBtn.Pressed += () => EndRun();
            endRunBox.AddChild(endRunBtn);

            Label warningLabel = new Label();
            warningLabel.Text = "All progress from this run will be lost.";
            warningLabel.Modulate = new Color("#E85050");
            endRunBox.AddChild(warningLabel);

            _buttonList.AddChild(endRunBox);
        }
        else
        {
            Button closeBtn = new Button();
            closeBtn.Text = "Close";
            closeBtn.Pressed += () => Toggle();
            _buttonList.AddChild(closeBtn);
        }

        if (OS.IsDebugBuild())
        {
            _buttonList.AddChild(new HSeparator());

            Button debugBtn = new Button();
            debugBtn.Text = "Debug Options";
            debugBtn.Pressed += () => RebuildDebugMenu();
            _buttonList.AddChild(debugBtn);
        }
    }

    private void RebuildDebugMenu()
    {
        ClearButtonList();

        Button backBtn = new Button();
        backBtn.Text = "← Back";
        backBtn.Pressed += () => RebuildMenu();
        _buttonList.AddChild(backBtn);

        _buttonList.AddChild(new HSeparator());

        string currentScene = GetTree().CurrentScene?.SceneFilePath ?? "";
        bool isInRun = currentScene == "res://main.tscn";

        if (isInRun)
        {
            var player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
            var weapon = player?.GetNodeOrNull<WeaponController>("Weapon");

            if (player != null)
            {
                CheckBox rangeToggle = new CheckBox();
                rangeToggle.Text = "Range Indicator";
                rangeToggle.ButtonPressed = player.RangeIndicatorVisible;
                rangeToggle.Toggled += on => player.SetRangeIndicatorVisible(on);
                _buttonList.AddChild(rangeToggle);

                CheckBox godToggle = new CheckBox();
                godToggle.Text = "God Mode";
                godToggle.ButtonPressed = player.GodMode;
                godToggle.Toggled += on => player.GodMode = on;
                _buttonList.AddChild(godToggle);
            }

            if (weapon != null)
            {
                _buttonList.AddChild(new HSeparator());

                Label autoCastLabel = new Label();
                autoCastLabel.Text = "Skill Auto-cast";
                _buttonList.AddChild(autoCastLabel);

                HBoxContainer row = new HBoxContainer();
                for (int i = 0; i < 5; i++)
                {
                    int slotIndex = i;
                    VBoxContainer col = new VBoxContainer();

                    Label numLabel = new Label();
                    numLabel.Text = (i + 1).ToString();
                    numLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    col.AddChild(numLabel);

                    CheckBox cb = new CheckBox();
                    cb.ButtonPressed = weapon.GetSlotAutoActivate(slotIndex);
                    cb.Toggled += on => weapon.SetSlotAutoActivate(slotIndex, on);
                    col.AddChild(cb);

                    row.AddChild(col);
                }
                _buttonList.AddChild(row);
            }

            _buttonList.AddChild(new HSeparator());
        }

        Button addMatsBtn = new Button();
        addMatsBtn.Text = "Add Materials";
        addMatsBtn.Pressed += () =>
        {
            var manager = GetNode<CharacterManager>("/root/CharacterManager");
            manager.Profile?.AddMaterial("crafting_common", 500);
        };
        _buttonList.AddChild(addMatsBtn);
    }

    private void ClearButtonList()
    {
        foreach (Node child in _buttonList.GetChildren())
            child.QueueFree();
    }

    private void EndRun()
    {
        GetTree().Paused = false;
        _menuControl.Visible = false;
        GetTree().ChangeSceneToFile("res://src/ui/character_screen.tscn");
    }
}
