using Godot;

namespace ActionRpgX.Hud;

public partial class Hud : CanvasLayer
{
    private ProgressBar _healthBar  = null!;
    private ProgressBar _focusBar   = null!;
    private ProgressBar _shieldBar  = null!;
    private ProgressBar _xpBar      = null!;
    private Label       _levelLabel = null!;
    private Label       _timerLabel = null!;
    private Label       _coinLabel  = null!;
    private ColorRect   _hitFlash   = null!;

    private Run.RunSession? _session;

    private sealed class SkillCell
    {
        public ProgressBar    Bar     = null!;
        public PanelContainer Panel   = null!;
        public float          Cooldown;
        public float          Elapsed;
    }
    private readonly SkillCell[] _skillCells = new SkillCell[5];
    private SkillCell? _dodgeCell;
    private readonly bool[] _slotIsAuraActive = new bool[5];

    public override void _Ready()
    {
        _healthBar  = GetNode<ProgressBar>("Control/HealthBar");
        _focusBar   = GetNode<ProgressBar>("Control/FocusBar");
        _shieldBar  = GetNode<ProgressBar>("Control/ShieldBar");
        _xpBar      = GetNode<ProgressBar>("Control/XpBar");
        _levelLabel = GetNode<Label>("Control/LevelLabel");
        _timerLabel = GetNode<Label>("Control/TimerLabel");
        _coinLabel  = GetNode<Label>("Control/CoinLabel");
        _hitFlash   = GetNode<ColorRect>("Control/HitFlash");

        StyleBar(_healthBar, new Color(0.8f,  0.15f, 0.15f));
        StyleBar(_focusBar,  new Color(0.2f,  0.5f,  0.95f)); // blue
        StyleBar(_shieldBar, new Color(0.55f, 0.8f,  1.0f));  // light blue
        StyleBar(_xpBar,     new Color(0.1f,  0.7f,  0.2f));

        var player = GetTree().GetFirstNodeInGroup("player") as Player.PlayerController;
        if (player != null)
        {
            _healthBar.MaxValue = player.MaxHealth;
            _healthBar.Value    = player.CurrentHealth;
            _xpBar.MaxValue     = player.XpToNextLevel;
            _xpBar.Value        = player.CurrentXp;
            _levelLabel.Text    = $"Level {player.Level}";

            player.HealthChanged += OnHealthChanged;
            player.FocusChanged  += OnFocusChanged;
            player.ShieldChanged += OnShieldChanged;
            player.XpChanged     += OnXpChanged;
            player.LeveledUp     += OnLeveledUp;
            player.PlayerHit     += OnPlayerHit;
            player.DodgeFired    += OnDodgeFired;

            _focusBar.MaxValue  = player.MaxFocus;
            _focusBar.Value     = player.CurrentFocus;
            _shieldBar.MaxValue = 1.0; // normalized; updated on first ShieldChanged
            _shieldBar.Value    = 0.0;

            var weaponController = player.GetNodeOrNull<Weapon.WeaponController>("Weapon");
            weaponController?.Connect(Weapon.WeaponController.SignalName.SkillFired,
                Callable.From<int, float, string>(OnSkillFired));
            weaponController?.Connect(Weapon.WeaponController.SignalName.AuraToggled,
                Callable.From<int, bool>(OnAuraToggled));
        }

        BuildSkillBar();

        _session = GetParent().GetNodeOrNull<Run.RunSession>("RunSession");
        if (_session != null)
            _session.CoinChanged += OnCoinChanged;

        _coinLabel.Text  = "Coins: 0";
        _timerLabel.Text = "0:00";
    }

    private void BuildSkillBar()
    {
        var container = GetNode<Control>("Control");

        var skillBar = new HBoxContainer();
        skillBar.AddThemeConstantOverride("separation", 6);
        skillBar.AnchorLeft      = 0.5f;
        skillBar.AnchorRight     = 0.5f;
        skillBar.AnchorTop       = 1.0f;
        skillBar.AnchorBottom    = 1.0f;
        skillBar.GrowHorizontal  = Control.GrowDirection.Both;
        skillBar.OffsetBottom    = -20f;
        skillBar.OffsetTop       = -90f;

        for (int i = 0; i < 5; i++)
        {
            var bar = new ProgressBar
            {
                MaxValue          = 1.0,
                Value             = 1.0,
                ShowPercentage    = false,
                FillMode          = 3, // bottom-to-top
                CustomMinimumSize = new Vector2(64f, 64f),
                SizeFlagsHorizontal = Control.SizeFlags.Fill,
                SizeFlagsVertical   = Control.SizeFlags.Fill,
            };
            StyleSkillBar(bar);

            var cell = new PanelContainer { CustomMinimumSize = new Vector2(64f, 64f) };
            cell.AddChild(bar);
            skillBar.AddChild(cell);

            _skillCells[i] = new SkillCell { Bar = bar, Panel = cell, Cooldown = 0f, Elapsed = 0f };
        }

        // Add a visual separation gap for the Dodge slot
        var spacer = new Control { CustomMinimumSize = new Vector2(16f, 0f) };
        skillBar.AddChild(spacer);

        // Build Dodge cell
        var dodgeBar = new ProgressBar
        {
            MaxValue          = 1.0,
            Value             = 1.0,
            ShowPercentage    = false,
            FillMode          = 3, // bottom-to-top
            CustomMinimumSize = new Vector2(64f, 64f),
            SizeFlagsHorizontal = Control.SizeFlags.Fill,
            SizeFlagsVertical   = Control.SizeFlags.Fill,
        };
        StyleSkillBar(dodgeBar);

        // Distinct styling: Cool slate/grey fill for dodge roll
        var fill = new StyleBoxFlat { BgColor = new Color(0.45f, 0.45f, 0.45f) };
        dodgeBar.AddThemeStyleboxOverride("fill", fill);

        var dodgeLabel = new Label
        {
            Text = "Space",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        dodgeLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.7f));
        dodgeLabel.AddThemeFontSizeOverride("font_size", 12);

        var dodgeCell = new PanelContainer { CustomMinimumSize = new Vector2(64f, 64f) };
        dodgeCell.AddChild(dodgeBar);
        dodgeCell.AddChild(dodgeLabel);
        skillBar.AddChild(dodgeCell);

        _dodgeCell = new SkillCell { Bar = dodgeBar, Panel = dodgeCell, Cooldown = 0f, Elapsed = 0f };

        container.AddChild(skillBar);
    }

    public override void _Process(double delta)
    {
        if (_session != null)
        {
            int secs = (int)_session.ElapsedTime;
            _timerLabel.Text = $"{secs / 60}:{secs % 60:D2}";
        }

        for (int i = 0; i < _skillCells.Length; i++)
        {
            var cell = _skillCells[i];
            if (cell == null || cell.Cooldown <= 0f || cell.Elapsed >= cell.Cooldown) continue;
            cell.Elapsed = Mathf.Min(cell.Cooldown, cell.Elapsed + (float)delta);
            cell.Bar.Value = cell.Elapsed / cell.Cooldown;
        }

        if (_dodgeCell != null && _dodgeCell.Cooldown > 0f && _dodgeCell.Elapsed < _dodgeCell.Cooldown)
        {
            _dodgeCell.Elapsed = Mathf.Min(_dodgeCell.Cooldown, _dodgeCell.Elapsed + (float)delta);
            _dodgeCell.Bar.Value = _dodgeCell.Elapsed / _dodgeCell.Cooldown;
        }
    }

    private void OnPlayerHit()
    {
        _hitFlash.Color = new Color(0.8f, 0.05f, 0.05f, 0.3f);
        var tween = CreateTween();
        tween.TweenProperty(_hitFlash, "color:a", 0f, 0.15f);
    }

    private void OnHealthChanged(float newHealth) => _healthBar.Value = newHealth;

    private void OnFocusChanged(float current, float max)
    {
        _focusBar.MaxValue = max;
        _focusBar.Value    = current;
    }

    private void OnShieldChanged(float current, float max)
    {
        _shieldBar.MaxValue = max;
        _shieldBar.Value    = current;
    }

    private void OnSkillFired(int slotIndex, float cooldown, string _delivery)
    {
        if (slotIndex < 0 || slotIndex >= _skillCells.Length) return;
        var cell = _skillCells[slotIndex];
        if (cell == null) return;
        if (_slotIsAuraActive[slotIndex]) return; // aura ticks don't look like cooldowns
        cell.Cooldown  = cooldown;
        cell.Elapsed   = 0f;
        cell.Bar.Value = 0.0;
    }

    private void OnAuraToggled(int slotIndex, bool active)
    {
        if (slotIndex < 0 || slotIndex >= _skillCells.Length) return;
        _slotIsAuraActive[slotIndex] = active;
        var cell = _skillCells[slotIndex];
        if (cell == null) return;
        if (active)
        {
            var amber = new StyleBoxFlat { BgColor = new Color("#E88A28") };
            cell.Bar.AddThemeStyleboxOverride("fill",       amber);
            cell.Bar.AddThemeStyleboxOverride("background", amber);
        }
        else
        {
            cell.Bar.AddThemeStyleboxOverride("fill",       new StyleBoxFlat { BgColor = new Color(0.3f, 0.55f, 0.9f) });
            cell.Bar.AddThemeStyleboxOverride("background", new StyleBoxFlat { BgColor = new Color(0.15f, 0.15f, 0.15f) });
            cell.Bar.Value = 1.0;
            cell.Cooldown  = 0f;
            cell.Elapsed   = 0f;
        }
    }

    private void OnDodgeFired(float cooldown)
    {
        if (_dodgeCell == null) return;
        _dodgeCell.Cooldown  = cooldown;
        _dodgeCell.Elapsed   = 0f;
        _dodgeCell.Bar.Value = 0.0;
    }

    private void OnXpChanged(int currentXp, int xpToNextLevel)
    {
        _xpBar.MaxValue = xpToNextLevel;
        _xpBar.Value    = currentXp;
    }

    private void OnLeveledUp(int newLevel) => _levelLabel.Text = $"Level {newLevel}";

    private void OnCoinChanged(int total) => _coinLabel.Text = $"Coins: {total}";

    private static void StyleBar(ProgressBar bar, Color fillColor)
    {
        var fill = new StyleBoxFlat { BgColor = fillColor };
        var bg   = new StyleBoxFlat { BgColor = new Color(0.1f, 0.1f, 0.1f) };
        bar.AddThemeStyleboxOverride("fill", fill);
        bar.AddThemeStyleboxOverride("background", bg);
    }

    private static void StyleSkillBar(ProgressBar bar)
    {
        var fill = new StyleBoxFlat { BgColor = new Color(0.3f, 0.55f, 0.9f) };
        var bg   = new StyleBoxFlat { BgColor = new Color(0.15f, 0.15f, 0.15f) };
        bar.AddThemeStyleboxOverride("fill", fill);
        bar.AddThemeStyleboxOverride("background", bg);
    }
}
