using Godot;
using Godot2.Character;

namespace Godot2.Ui;

// Coin-funded upgrades removed from design — this panel is a stub pending redesign.
public partial class MetaUpgradesPanel : Panel
{
    private Label         _coinLabel    = null!;
    private VBoxContainer _upgradesVBox = null!;

    public override void _Ready()
    {
        _coinLabel    = GetNode<Label>        ("VBox/CoinLabel");
        _upgradesVBox = GetNode<VBoxContainer>("VBox/UpgradesVBox");

        GetNode<Button>("VBox/CloseButton").Pressed += () => Visible = false;

        Visible = false;
    }

    public void Refresh(CharacterData _)
    {
        _coinLabel.Text = "Coin upgrades: coming soon";
        foreach (Node child in _upgradesVBox.GetChildren())
            child.QueueFree();
    }
}
