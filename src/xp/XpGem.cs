using Godot;

namespace Godot1.Xp;

public partial class XpGem : Area3D
{
    [Export] public int Value = 5;

    private static readonly Texture2D ItemTex =
        GD.Load<Texture2D>("res://assets/kenney_topdown_rpg/Roguelike Base Pack/Spritesheet/roguelikeSheet_transparent.png");

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        AddChild(new Sprite3D
        {
            Texture       = ItemTex,
            RegionEnabled = true,
            RegionRect    = new Rect2(697, 408, 16, 16),
            PixelSize     = 1.5f,
            Billboard     = BaseMaterial3D.BillboardModeEnum.Enabled,
            Transparent   = true,
            AlphaCut      = SpriteBase3D.AlphaCutMode.Discard,
        });
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Player.PlayerController pc)
        {
            pc.CollectXp(Value);
            QueueFree();
        }
    }
}
