using Godot;
using System.Collections.Generic;
using ActionRpgX.Eot;

namespace ActionRpgX.Weapon;

public partial class FixedZoneTick : Node3D
{
    public float              Damage;
    public Items.DamageType   DmgType;
    public float              Radius;
    public float              Duration;
    public float              TickInterval;
    public List<(string Id, float Chance)> EotIds = new();
    public float              CritMultiplier = 1f;

    private float _elapsed;
    private float _nextTick;

    public override void _Ready()
    {
        _nextTick = TickInterval;
    }

    public override void _Process(double delta)
    {
        _elapsed += (float)delta;

        _nextTick -= (float)delta;
        if (_nextTick <= 0f)
        {
            Tick();
            _nextTick = TickInterval;
        }

        if (Duration > 0f && _elapsed >= Duration)
            QueueFree();
    }

    private void Tick()
    {
        bool isCrit = CritMultiplier > 1f;
        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not Enemies.EnemyController enemy || enemy.IsQueuedForDeletion()) continue;
            if (GlobalPosition.DistanceTo(enemy.GlobalPosition) > Radius) continue;
            enemy.TakeDamage(Damage, DmgType, isCrit);
            foreach (var (eotId, chance) in EotIds)
            {
                var eot = EotRegistry.Get(eotId);
                if (eot != null && GD.Randf() < chance)
                    enemy.ApplyEot(eot, CritMultiplier);
            }
        }
    }
}
