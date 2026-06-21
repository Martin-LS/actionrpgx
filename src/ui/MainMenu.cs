using Godot;

namespace Godot2.Ui;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        GetNode<Button>("MenuCard/VBox/PlayButton").Pressed += OnPlayPressed;
    }

    private void OnPlayPressed()
    {
        GetTree().ChangeSceneToFile("res://src/ui/account_screen.tscn");
    }
}
