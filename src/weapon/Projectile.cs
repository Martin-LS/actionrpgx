using Godot;

namespace Godot1.Weapon;

public partial class Projectile : Area3D
{
    public float Damage;
    public float Speed = 500f;
    public float MaxRange = 600f;

    private Vector3 _direction;
    private float _traveled;

    public void Initialize(Vector3 direction, float damage)
    {
        _direction = direction.Normalized();
        Damage = damage;
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        AddChild(new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = 5f, Height = 10f },
        });
    }

    public override void _PhysicsProcess(double delta)
    {
        var step = _direction * Speed * (float)delta;
        GlobalPosition += step;
        _traveled += step.Length();

        if (_traveled >= MaxRange)
            QueueFree();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Enemies.EnemyController enemy)
        {
            enemy.TakeDamage((int)Damage);
            QueueFree();
        }
    }
}
