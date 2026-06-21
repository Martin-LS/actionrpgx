using Godot;

namespace ActionRpgX;

public partial class CameraFollow : Camera3D
{
    public override void _Ready()
    {
        Projection = ProjectionType.Perspective;
        Fov = 45f;
        Position = new Vector3(0f, 180f, 100f);
        RotationDegrees = new Vector3(-60f, 0f, 0f);
    }
}
