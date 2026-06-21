using Godot;
using Godot2.Player;

namespace Godot2.Run;

public partial class RunSession : Node
{
    [Export] public float RunDuration = 300f;
    [Export] public int   MapLevel    = 1;

    [Signal] public delegate void RunEndedEventHandler(bool won, int levelReached, float elapsed);
    [Signal] public delegate void CoinChangedEventHandler(int total);

    public int CoinsEarned { get; private set; }
    public int CraftingCurrency1Earned { get; private set; }
    public float ElapsedTime => _elapsed;

    private float _elapsed;
    private bool  _ended;

    public override void _Ready()
    {
        var player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
        if (player != null)
            player.PlayerDied += () => EndRun(false);
    }

    public override void _Process(double delta)
    {
        if (_ended) return;
        _elapsed += (float)delta;
        if (_elapsed >= RunDuration)
            EndRun(true);
    }

    public void AddCoin(int amount)
    {
        CoinsEarned += amount;
        EmitSignal(SignalName.CoinChanged, CoinsEarned);
    }

    public void AddCraftingCurrency1(int amount) => CraftingCurrency1Earned += amount;

    private void EndRun(bool won)
    {
        if (_ended) return;
        _ended = true;

        var player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
        int level = player?.Level ?? 1;
        EmitSignal(SignalName.RunEnded, won, level, _elapsed);
    }
}
