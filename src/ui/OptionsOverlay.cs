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

    private const string DebugSettingsPath = "user://debug_settings.json";
    public bool RangeIndicatorEnabled { get; private set; } = false;
    public bool GodModeEnabled { get; private set; } = false;
    public bool TargetIndicatorEnabled { get; private set; } = true;
    public bool DebugCollisionsEnabled { get; private set; } = false;

    public override void _Ready()
    {
        _globalButton = GetNode<Button>("GlobalButton");
        _menuControl = GetNode<Control>("MenuControl");
        _buttonList = GetNode<VBoxContainer>("MenuControl/PanelContainer/VBoxContainer/ButtonList");

        _menuControl.Visible = false;
        GetTree().Paused = false;

        _globalButton.Pressed += () => Toggle();
        UpdateGlobalButtonVisibility();
        LoadDebugSettings();
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

    private void LoadDebugSettings()
    {
        if (!FileAccess.FileExists(DebugSettingsPath)) return;
        using var file = FileAccess.Open(DebugSettingsPath, FileAccess.ModeFlags.Read);
        if (file == null) return;

        var text = file.GetAsText();
        var parsed = Json.ParseString(text);
        if (parsed.Obj is Godot.Collections.Dictionary dict)
        {
            RangeIndicatorEnabled = dict.ContainsKey("rangeIndicator") && System.Convert.ToBoolean(dict["rangeIndicator"].Obj);
            GodModeEnabled = dict.ContainsKey("godMode") && System.Convert.ToBoolean(dict["godMode"].Obj);
            TargetIndicatorEnabled = !dict.ContainsKey("targetIndicator") || System.Convert.ToBoolean(dict["targetIndicator"].Obj);
            DebugCollisionsEnabled = dict.ContainsKey("debugCollisions") && System.Convert.ToBoolean(dict["debugCollisions"].Obj);
            GetTree().DebugCollisionsHint = DebugCollisionsEnabled;
        }
    }

    private void SaveDebugSettings()
    {
        var dict = new Godot.Collections.Dictionary
        {
            ["rangeIndicator"] = RangeIndicatorEnabled,
            ["godMode"] = GodModeEnabled,
            ["targetIndicator"] = TargetIndicatorEnabled,
            ["debugCollisions"] = DebugCollisionsEnabled
        };
        using var file = FileAccess.Open(DebugSettingsPath, FileAccess.ModeFlags.Write);
        file?.StoreString(Json.Stringify(dict));
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
                rangeToggle.ButtonPressed = RangeIndicatorEnabled;
                rangeToggle.Toggled += on =>
                {
                    RangeIndicatorEnabled = on;
                    player.SetRangeIndicatorVisible(on);
                    SaveDebugSettings();
                };
                _buttonList.AddChild(rangeToggle);

                CheckBox targetToggle = new CheckBox();
                targetToggle.Text = "Target Indicator";
                targetToggle.ButtonPressed = TargetIndicatorEnabled;
                targetToggle.Toggled += on =>
                {
                    TargetIndicatorEnabled = on;
                    player.SetTargetIndicatorVisible(on);
                    SaveDebugSettings();
                };
                _buttonList.AddChild(targetToggle);

                CheckBox godToggle = new CheckBox();
                godToggle.Text = "God Mode";
                godToggle.ButtonPressed = GodModeEnabled;
                godToggle.Toggled += on =>
                {
                    GodModeEnabled = on;
                    player.GodMode = on;
                    SaveDebugSettings();
                };
                _buttonList.AddChild(godToggle);

                CheckBox collisionToggle = new CheckBox();
                collisionToggle.Text = "Show Collision Shapes";
                collisionToggle.ButtonPressed = DebugCollisionsEnabled;
                collisionToggle.Toggled += on =>
                {
                    DebugCollisionsEnabled = on;
                    ToggleDebugCollisions(on);
                    SaveDebugSettings();
                };
                _buttonList.AddChild(collisionToggle);
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
                    cb.Toggled += on =>
                    {
                        weapon.SetSlotAutoActivate(slotIndex, on);
                        var manager = GetNode<CharacterManager>("/root/CharacterManager");
                        var c = manager.SelectedCharacter;
                        if (c != null && slotIndex < c.SlotAutoActivate.Count)
                        {
                            c.SlotAutoActivate[slotIndex] = on;
                            manager.Save();
                        }
                    };
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

    private void ToggleDebugCollisions(bool on)
    {
        GetTree().DebugCollisionsHint = on;
        UpdateDebugCollisionsRecursive(GetTree().Root);
    }

    private void UpdateDebugCollisionsRecursive(Node node)
    {
        if (node is CollisionShape3D shape)
        {
            bool original = shape.Disabled;
            shape.Disabled = !original;
            shape.Disabled = original;
        }
        foreach (Node child in node.GetChildren())
        {
            UpdateDebugCollisionsRecursive(child);
        }
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
