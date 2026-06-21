using Godot;

namespace Godot2.Ui;

public partial class RunEndOverlay : CanvasLayer
{
    private Label  _titleLabel  = null!;
    private Label  _statsLabel  = null!;
    private Button _returnButton = null!;

    public override void _Ready()
    {
        _titleLabel   = GetNode<Label>  ("Overlay/Panel/VBox/TitleLabel");
        _statsLabel   = GetNode<Label>  ("Overlay/Panel/VBox/StatsLabel");
        _returnButton = GetNode<Button> ("Overlay/Panel/VBox/ReturnButton");

        _returnButton.Pressed += OnReturn;

        var session = GetParent().GetNodeOrNull("RunSession");
        session?.Connect("RunEnded", Callable.From<bool, int, float>(ShowResult));

        Visible = false;
    }

    private void ShowResult(bool won, int levelReached, float elapsed)
    {
        _titleLabel.Text = won ? "Victory!" : "You Died";

        int min = (int)elapsed / 60;
        int sec = (int)elapsed % 60;
        var session = GetParent().GetNodeOrNull<Run.RunSession>("RunSession");
        int coins  = session?.CoinsEarned ?? 0;
        int crafting = session?.CraftingCurrency1Earned ?? 0;
        _statsLabel.Text = $"Level reached:      {levelReached}\nTime survived:      {min}m {sec:D2}s\nCoins earned:       {coins}\nCrafting currency:  {crafting}";

        Visible = true;
        GetTree().Paused = true;
    }

    private void OnReturn()
    {
        var manager = GetNode<Character.CharacterManager>("/root/CharacterManager");
        var player  = GetTree().GetFirstNodeInGroup("player") as Player.PlayerController;
        var session = GetParent().GetNodeOrNull<Run.RunSession>("RunSession");
        manager.RecordRunCompletion(
            player?.Level ?? 1,
            player?.CurrentXp ?? 0,
            session?.CoinsEarned ?? 0,
            session?.CraftingCurrency1Earned ?? 0);

        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://src/ui/character_screen.tscn");
    }
}
