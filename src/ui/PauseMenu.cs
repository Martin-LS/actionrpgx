using Godot;

namespace Godot1.Ui;

public partial class PauseMenu : CanvasLayer
{
    public override void _Ready()
    {
        Visible = false;

        GetNode<Button>("Panel/VBox/ResumeButton").Pressed      += Toggle;
        GetNode<Button>("Panel/VBox/HBox/EndRunButton").Pressed += EndRun;

        var debugSection = GetNode<VBoxContainer>("Panel/VBox/DebugSection");
        var separator    = GetNode<HSeparator>("Panel/VBox/DebugSeparator");

        if (!OS.IsDebugBuild())
        {
            debugSection.Hide();
            separator.Hide();
            return;
        }

        var speedLabel  = GetNode<Label>("Panel/VBox/DebugSection/SpeedRow/SpeedLabel");
        var speedSlider = GetNode<HSlider>("Panel/VBox/DebugSection/SpeedRow/SpeedSlider");
        var rangeToggle = GetNode<CheckBox>("Panel/VBox/DebugSection/RangeToggle");

        var player = GetTree().GetFirstNodeInGroup("player") as Player.PlayerController;
        if (player != null)
        {
            speedSlider.Value = player.Speed;
            speedLabel.Text   = $"Speed: {(int)player.Speed}";
            speedSlider.ValueChanged += val =>
            {
                player.Speed    = (float)val;
                speedLabel.Text = $"Speed: {(int)val}";
            };

            rangeToggle.ButtonPressed = OS.HasFeature("editor");
            rangeToggle.Toggled += on => player.SetRangeIndicatorVisible(on);
        }
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("ui_cancel"))
        {
            Toggle();
            GetViewport().SetInputAsHandled();
        }
    }

    private void Toggle()
    {
        Visible = !Visible;
        GetTree().Paused = Visible;
    }

    private void EndRun()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://src/ui/character_screen.tscn");
    }
}
