using Godot;

namespace ActionRpgX.Xp;

public partial class XpShard : Area3D
{
    [Export] public int Value = BalanceConfig.Pickups.XpShardValue;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        CollisionLayer = 0; // Pickups don't need a collision layer
        CollisionMask = 2;  // Only detect Player (2)
        AddChild(new MeshInstance3D
        {
            Mesh             = new BoxMesh { Size = new Vector3(10f, 10f, 10f) },
            Position         = new Vector3(0f, 5f, 0f),
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.2f, 0.9f, 0.3f) },
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
