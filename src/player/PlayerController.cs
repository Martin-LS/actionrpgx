using Godot;
using System.Collections.Generic;
using ActionRpgX.Skills;
using ActionRpgX;
using ActionRpgX.Enemies;

namespace ActionRpgX.Player;

public partial class PlayerController : CharacterBody3D
{
    [Signal] public delegate void HealthChangedEventHandler(float newHealth);
    [Signal] public delegate void PlayerDiedEventHandler();
    [Signal] public delegate void PlayerHitEventHandler();
    [Signal] public delegate void XpChangedEventHandler(int currentXp, int xpToNextLevel);
    [Signal] public delegate void LeveledUpEventHandler(int newLevel);
    [Signal] public delegate void FocusChangedEventHandler(float current, float max);
    [Signal] public delegate void ShieldChangedEventHandler(float current, float max);
    [Signal] public delegate void DamageTakenEventHandler(float effectiveDamage, bool isMagic);
    [Signal] public delegate void DodgeFiredEventHandler(float cooldown);

    [Export] public float Speed = 200f;
    [Export] public int MaxHealth = 100;

    public float DamageReduction    { get; private set; }
    public float PhysicalResistance { get; private set; }
    public float MagicResistance    { get; private set; }
    private float _evasion;
    public float EffectiveRange     { get; private set; }

    private float _rangeBuffBonus; // flat tile bonus from active range buffs (e.g. Shout)

    private static readonly PackedScene TargetIndicatorScene =
        GD.Load<PackedScene>("res://src/vfx/target_indicator.tscn");

    private static readonly PackedScene SwingVfxScene =
        GD.Load<PackedScene>("res://PolyBlocks/EffectBlocks/assets/impacts/impact_5.tscn");

    private Node3D? _targetIndicator;
    private Node3D? _aimReticle;

    private Stats.StatBlock _statBlock = new();
    private Character.CharacterData? _charData;
    private Node3D _model = null!;
    private AnimationNodeStateMachinePlayback? _moveSm;
    private AnimationTree? _animTree;
    private AnimationPlayer? _animPlayer;

    public bool  GodMode       { get; set; }
    public float CurrentHealth { get; private set; }
    public float CurrentFocus  { get; private set; }
    public float MaxFocus      { get; private set; }
    public int Level { get; private set; } = 1;
    public int CurrentXp { get; private set; }
    public int XpToNextLevel { get; private set; } = 20;

    public EnemyController? LockedTarget  { get; private set; }
    public Vector3          TargetPosition { get; private set; }

    private float _focusRegen;
    private float _totalReserved;
    private float _currentFocusShield;
    private float _maxFocusShield;

    private float _yaw;
    private const float RotationSpeed = 20f;

    private MeshInstance3D? _rangeIndicator;

    // Equipment augment state
    private readonly Dictionary<string, float> _activeAugmentChances = new();
    private bool  _fortifyActive;
    private float _dashReflexTimer;
    private float _ghostStepTimer;
    private float _mendingTimer;

    // Dodge state
    private bool    _isDodging = false;
    private float   _dodgeTimer = 0f;
    private float   _dodgeCooldownTimer = 0f;
    private Vector3 _dodgeDirection;

    public override void _Ready()
    {
        var manager = GetNodeOrNull<Character.CharacterManager>("/root/CharacterManager");

        if (manager?.SelectedCharacter != null)
        {
            var c = manager.SelectedCharacter;
            _charData  = c;
            _statBlock = c.BuildStatBlock();

            MaxHealth          = (int)_statBlock.Get(Stats.StatId.MaxHp);
            Speed              = _statBlock.Get(Stats.StatId.Speed);
            PhysicalResistance = _statBlock.Get(Stats.StatId.PhysicalResistance);
            MagicResistance    = _statBlock.Get(Stats.StatId.MagicResistance);
            _evasion           = _statBlock.Get(Stats.StatId.Evasion);
            MaxFocus           = _statBlock.Get(Stats.StatId.MaxFocus);
            _focusRegen        = _statBlock.Get(Stats.StatId.FocusRegen);
            CurrentFocus       = MaxFocus;

            _maxFocusShield     = MaxFocus * BalanceConfig.Focus.ShieldFraction;
            _currentFocusShield = _maxFocusShield;

            Level         = c.CurrentLevel;
            CurrentXp     = c.CurrentXp;
            XpToNextLevel = ComputeXpToNextLevel(Level);

            var weapon = GetEquippedItem(c, Items.ItemSlot.Weapon);
            var hat    = GetEquippedItem(c, Items.ItemSlot.Hat);
            var body   = GetEquippedItem(c, Items.ItemSlot.Body);

            DamageReduction = (hat?.DamageReduction ?? 0f) + (body?.DamageReduction ?? 0f);

            var weaponController = GetNodeOrNull<Weapon.WeaponController>("Weapon");
            ApplyWeaponDamage(weaponController, weapon);
            RecalculateEffectiveRange();
            weaponController?.SetPreferredDelivery(weapon?.PreferredDelivery ?? "Melee");

            for (int i = 0; i < 5 && i < c.SlottedSkillInstanceIds.Count; i++)
            {
                var instanceId = c.SlottedSkillInstanceIds[i];
                if (string.IsNullOrEmpty(instanceId)) continue;
                var instance = manager.FindSkillInstance(instanceId);
                var skill    = instance?.Definition;
                if (skill == null) continue;

                var augmentEots   = new List<(string Id, float Chance)>();
                bool  hasMagicDamage = false;
                float critChanceBonus = 0f;
                if (instance != null)
                {
                    var activeAugments = Skills.AugmentResolver.Resolve(instance.SocketedSkillAugmentIds, manager.FindSkillAugmentInstance);
                    foreach (var augInst in activeAugments)
                    {
                        if (augInst.DefinitionId == "magic_damage")    hasMagicDamage = true;
                        if (augInst.DefinitionId == "critical_strike") critChanceBonus += augInst.TriggerChance / 100f;
                        var eotId = augInst.Definition?.EotId;
                        if (eotId != null) augmentEots.Add((eotId, augInst.TriggerChance / 100f));
                    }
                }

                weaponController?.SetSlot(i, skill, augmentEots, hasMagicDamage, critChanceBonus);
                bool autoActivate = i < c.SlotAutoActivate.Count ? c.SlotAutoActivate[i] : true;
                weaponController?.SetSlotAutoActivate(i, autoActivate);
            }

            // Seed equipment augments from all equipped gear
            _activeAugmentChances.Clear();
            foreach (var kvp in c.EquippedGear)
            {
                foreach (var augInstId in kvp.Value.SocketedEquipmentAugmentIds)
                {
                    if (string.IsNullOrEmpty(augInstId)) continue;
                    var augInst = manager.FindEquipmentAugmentInstance(augInstId);
                    if (augInst?.DefinitionId != null)
                        _activeAugmentChances[augInst.DefinitionId] = augInst.TriggerChance / 100f;
                }
            }
        }
        else
        {
            _statBlock.SetBase(Stats.StatId.MaxHp,         MaxHealth);
            _statBlock.SetBase(Stats.StatId.Speed,          Speed);
            _statBlock.SetBase(Stats.StatId.PhysicalDamage, 20f);
            _statBlock.SetBase(Stats.StatId.MagicDamage,    0f);
            XpToNextLevel = ComputeXpToNextLevel(Level);
            var wc = GetNodeOrNull<Weapon.WeaponController>("Weapon");
            wc?.SetDamage(20f, 0f);
            wc?.SetGlobalCritChance(0f);
            wc?.SetCritMultiplier(BalanceConfig.SkillAugments.CritMultiplier);
            wc?.SetRange(1.5f * GameScale.TileSize);
            wc?.SetPreferredDelivery("Melee");
            var fallback = SkillRegistry.Get("entity_burst");
            if (fallback != null) wc?.SetSlot(0, fallback);

            MaxFocus            = BalanceConfig.Focus.WarriorMaxFocus;
            _focusRegen         = BalanceConfig.Focus.WarriorRegenPerSec;
            CurrentFocus        = MaxFocus;
            _maxFocusShield     = MaxFocus * BalanceConfig.Focus.ShieldFraction;
            _currentFocusShield = _maxFocusShield;
        }

        _mendingTimer = 3.0f;

        GlobalPosition = Vector3.Zero;
        CurrentHealth  = MaxHealth;
        AddToGroup("player");

        var visuals = new Node3D();
        visuals.Scale = new Vector3(9f, 9f, 9f);
        AddChild(visuals);
        _model = visuals;

        string modelPath = "res://assets/models/characters/player.glb";
        var playerModel = GD.Load<PackedScene>(modelPath).Instantiate<Node3D>();
        visuals.AddChild(playerModel);

        var animPlayer = playerModel.FindChild("AnimationPlayer", true, false) as AnimationPlayer;
        if (animPlayer == null)
        {
            animPlayer = new AnimationPlayer();
            playerModel.AddChild(animPlayer);
        }

        if (animPlayer.HasAnimation("breathing_idle"))
            animPlayer.GetAnimation("breathing_idle").LoopMode = Animation.LoopModeEnum.Linear;
        if (animPlayer.HasAnimation("run"))
        {
            var runAnim = animPlayer.GetAnimation("run");
            runAnim.LoopMode = Animation.LoopModeEnum.Linear;
            // Strip root motion: zero X/Z on Hips position track so the animation plays in place
            for (int i = 0; i < runAnim.GetTrackCount(); i++)
            {
                if (runAnim.TrackGetPath(i).ToString().Contains("Hips") &&
                    runAnim.TrackGetType(i) == Animation.TrackType.Position3D)
                {
                    for (int k = 0; k < runAnim.TrackGetKeyCount(i); k++)
                    {
                        var pos = (Vector3)runAnim.TrackGetKeyValue(i, k);
                        runAnim.TrackSetKeyValue(i, k, new Vector3(0f, pos.Y, 0f));
                    }
                }
            }
        }

        _animPlayer = animPlayer;

        var animTree = GetNodeOrNull<AnimationTree>("AnimationTree");
        if (animTree != null)
        {
            animTree.AnimPlayer = animTree.GetPathTo(animPlayer);
            animTree.Active = true;
            _animTree = animTree;
        }

        var skeleton = playerModel.FindChild("Skeleton3D", true, false) as Skeleton3D;
        if (skeleton != null)
        {
            string? weaponDefId = _charData != null
                ? GetEquippedItem(_charData, Items.ItemSlot.Weapon)?.Id
                : "sword_t1";
            string? weaponPath = GetWeaponModelPath(weaponDefId);
            if (weaponPath != null)
            {
                var weaponScene = GD.Load<PackedScene>(weaponPath);
                if (weaponScene != null)
                {
                    var weaponRoot = weaponScene.Instantiate<Node3D>();
                    visuals.AddChild(weaponRoot);
                    string attachBone = weaponDefId == "bow_t1" ? "Hand_L" : "Hand_R";
                    AttachWeaponToSkeleton(weaponRoot, skeleton, attachBone);
                }
            }
        }

        GetNodeOrNull<Weapon.WeaponController>("Weapon")?.Connect(
            Weapon.WeaponController.SignalName.SkillFired,
            Callable.From<int, float, string>(OnSkillFired));


        float indicatorRadius = EffectiveRange > 0f ? EffectiveRange : 1.5f * GameScale.TileSize;
        _rangeIndicator = CreateRangeIndicator(indicatorRadius);
        AddChild(_rangeIndicator);
        _rangeIndicator.Visible = false;

        if (TargetIndicatorScene != null)
        {
            _targetIndicator = TargetIndicatorScene.Instantiate<Node3D>();
            _targetIndicator.Visible = false;
            GetTree().Root.CallDeferred(Node.MethodName.AddChild, _targetIndicator);
        }

        _aimReticle = CreateAimReticle();
        _aimReticle.Visible = false;
        GetTree().Root.CallDeferred(Node.MethodName.AddChild, _aimReticle);
    }

    public bool RangeIndicatorVisible => _rangeIndicator?.Visible ?? false;

    public void SetRangeIndicatorVisible(bool visible)
    {
        if (_rangeIndicator != null)
            _rangeIndicator.Visible = visible;
    }

    public void RecalculateEffectiveRange()
    {
        var weapon = _charData != null ? GetEquippedItem(_charData, Items.ItemSlot.Weapon) : null;
        var hat    = _charData != null ? GetEquippedItem(_charData, Items.ItemSlot.Hat)    : null;
        var body   = _charData != null ? GetEquippedItem(_charData, Items.ItemSlot.Body)   : null;

        float weaponRange = weapon?.WeaponRange ?? 1.5f;
        EffectiveRange = (weaponRange * (hat?.RangeMultiplier ?? 1f) * (body?.RangeMultiplier ?? 1f) + _rangeBuffBonus) * GameScale.TileSize;

        GetNodeOrNull<Weapon.WeaponController>("Weapon")?.SetRange(EffectiveRange);
    }

    public void AddRangeBuffBonus(float tiles)
    {
        _rangeBuffBonus += tiles;
        RecalculateEffectiveRange();
    }

    public void RemoveRangeBuffBonus(float tiles)
    {
        _rangeBuffBonus -= tiles;
        RecalculateEffectiveRange();
    }

    private static Node3D CreateAimReticle()
    {
        var root  = new Node3D();
        var torus = new TorusMesh { OuterRadius = 12f, InnerRadius = 8f, Rings = 32, RingSegments = 8 };
        var mat   = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(1f, 1f, 1f, 0.9f),
        };
        var mesh = new MeshInstance3D { Mesh = torus, MaterialOverride = mat };
        mesh.RotateX(Mathf.Pi / 2f);
        root.AddChild(mesh);
        return root;
    }

    private static MeshInstance3D CreateRangeIndicator(float radius)
    {
        const float tubeRadius = 1.5f;
        var torus = new TorusMesh { OuterRadius = radius - tubeRadius, InnerRadius = tubeRadius, Rings = 64, RingSegments = 8 };
        var mat = new StandardMaterial3D
        {
            ShadingMode  = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            AlbedoColor  = new Color(0f, 0.8f, 1f, 0.5f),
            NoDepthTest  = true,
        };
        return new MeshInstance3D { Mesh = torus, MaterialOverride = mat, Position = new Vector3(0f, 0.5f, 0f) };
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_moveSm == null)
        {
            var at = GetNodeOrNull<AnimationTree>("AnimationTree");
            if (at != null)
                _moveSm = at.Get("parameters/movement/playback").As<AnimationNodeStateMachinePlayback>();
        }

        float dt = (float)delta;

        // Tick dodge timers
        if (_isDodging)
        {
            _dodgeTimer -= dt;
            if (_dodgeTimer <= 0f)
            {
                _isDodging = false;
            }
        }
        if (_dodgeCooldownTimer > 0f)
        {
            _dodgeCooldownTimer -= dt;
        }

        var input     = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        var direction = new Vector3(input.X, 0f, input.Y);
        bool moving = direction.LengthSquared() > 0.01f;

        if (_isDodging)
        {
            float moveSpeed = Speed * BalanceConfig.Dodge.SpeedMultiplier;
            Velocity = _dodgeDirection * moveSpeed;
        }
        else
        {
            float moveSpeed = Speed + (_dashReflexTimer > 0f ? 100f : 0f);
            Velocity = direction * moveSpeed;
        }

        MoveAndSlide();

        UpdateLockedTarget();

        var camera = GetViewport().GetCamera3D();
        if (camera != null)
        {
            var mousePos = GetViewport().GetMousePosition();
            var rayFrom  = camera.ProjectRayOrigin(mousePos);
            var rayDir   = camera.ProjectRayNormal(mousePos);
            if (Mathf.Abs(rayDir.Y) > 0.001f)
            {
                float t = -rayFrom.Y / rayDir.Y;
                TargetPosition = rayFrom + rayDir * t;
            }
        }

        if (_targetIndicator != null)
        {
            bool hasTarget = LockedTarget != null && GodotObject.IsInstanceValid(LockedTarget);
            _targetIndicator.Visible = hasTarget;
            if (hasTarget)
                _targetIndicator.GlobalPosition = new Vector3(LockedTarget!.GlobalPosition.X, 1f, LockedTarget!.GlobalPosition.Z);
        }

        if (_aimReticle != null)
        {
            var wc = GetNodeOrNull<Weapon.WeaponController>("Weapon");
            bool show = wc?.HasAnyPositionSkill() ?? false;
            _aimReticle.Visible = show;
            if (show)
                _aimReticle.GlobalPosition = new Vector3(TargetPosition.X, 0.5f, TargetPosition.Z);
        }

        bool inAttack = _animTree != null &&
            (((bool)_animTree.Get("parameters/shot_right/active")) ||
             ((bool)_animTree.Get("parameters/shot_left/active")));

        if (!_isDodging)
        {
            if (moving && !inAttack)
            {
                float targetYaw = Mathf.Atan2(direction.X, direction.Z);
                _yaw = Mathf.LerpAngle(_yaw, targetYaw, Mathf.Min(1f, RotationSpeed * dt));
                _model.Rotation = new Vector3(0f, _yaw, 0f);
            }
            else
            {
                var toAim = new Vector3(TargetPosition.X - GlobalPosition.X, 0f, TargetPosition.Z - GlobalPosition.Z);
                if (toAim.LengthSquared() > 1f)
                {
                    float targetYaw = Mathf.Atan2(toAim.X, toAim.Z);
                    _yaw = Mathf.LerpAngle(_yaw, targetYaw, Mathf.Min(1f, RotationSpeed * dt));
                    _model.Rotation = new Vector3(0f, _yaw, 0f);
                }
            }

            if (_moveSm != null)
            {
                var want = moving ? "run" : "idle";
                if (_moveSm.GetCurrentNode() != want)
                    _moveSm.Travel(want);
            }
        }

        CurrentFocus = Mathf.Min(CurrentFocus + _focusRegen * dt, MaxFocus);
        EmitSignal(SignalName.FocusChanged, GetAvailableFocus(), MaxFocus);

        if (_currentFocusShield < _maxFocusShield)
            _currentFocusShield = Mathf.Min(_currentFocusShield + BalanceConfig.Focus.ShieldRegenPerSec * dt, _maxFocusShield);
        EmitSignal(SignalName.ShieldChanged, _currentFocusShield, _maxFocusShield);

        if (_dashReflexTimer > 0f) _dashReflexTimer -= dt;
        if (_ghostStepTimer  > 0f) _ghostStepTimer  -= dt;

        if (_activeAugmentChances.ContainsKey("mending"))
        {
            _mendingTimer -= dt;
            if (_mendingTimer <= 0f)
            {
                Heal(5);
                _mendingTimer = 3.0f;
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        var wc = GetNodeOrNull<Weapon.WeaponController>("Weapon");
        if (@event is InputEventKey key && !key.Echo)
        {
            if (key.Pressed)
            {
                if      (key.Keycode == Key.Space) TryStartDodge();
                else if (key.Keycode == Key.Q) wc?.TryFireSlot(0);
                else if (key.Keycode == Key.E) wc?.TryFireSlot(1);
                else if (key.Keycode == Key.R) wc?.TryFireSlot(2);
                else if (key.Keycode == Key.F) wc?.TryFireSlot(3);
            }
            else
            {
                if      (key.Keycode == Key.Q) wc?.ReleaseSlot(0);
                else if (key.Keycode == Key.E) wc?.ReleaseSlot(1);
                else if (key.Keycode == Key.R) wc?.ReleaseSlot(2);
                else if (key.Keycode == Key.F) wc?.ReleaseSlot(3);
            }
        }
        else if (@event is InputEventMouseButton mouse)
        {
            if (mouse.ButtonIndex == MouseButton.Right)
            {
                if (mouse.Pressed) wc?.TryFireSlot(4);
                else               wc?.ReleaseSlot(4);
            }
        }
    }

    private void TryStartDodge()
    {
        if (_isDodging || _dodgeCooldownTimer > 0f) return;

        // Cancel any active attack/skill animations (both OneShots)
        if (_animTree != null)
        {
            _animTree.Set("parameters/shot_right/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animTree.Set("parameters/shot_left/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
        }

        // Cancel channeling skills
        GetNodeOrNull<Weapon.WeaponController>("Weapon")?.CancelActiveSkills();

        // Get movement input direction, or fall back to character facing direction
        var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (input.LengthSquared() > 0.01f)
        {
            _dodgeDirection = new Vector3(input.X, 0f, input.Y).Normalized();
        }
        else
        {
            _dodgeDirection = new Vector3(Mathf.Sin(_yaw), 0f, Mathf.Cos(_yaw)).Normalized();
        }

        // Rotate character model to face the roll direction instantly
        if (_dodgeDirection.LengthSquared() > 0.01f)
        {
            _yaw = Mathf.Atan2(_dodgeDirection.X, _dodgeDirection.Z);
            _model.Rotation = new Vector3(0f, _yaw, 0f);
        }

        _isDodging = true;
        _dodgeTimer = BalanceConfig.Dodge.Duration;
        _dodgeCooldownTimer = BalanceConfig.Dodge.Cooldown;
        EmitSignal(SignalName.DodgeFired, BalanceConfig.Dodge.Cooldown);
    }

    public void TakeDamage(float rawAmount, Items.DamageType type, Node3D? attacker = null)
    {
        if (GodMode) return;
        if (_isDodging) return; // Invulnerability frames during dodge roll
        if (_evasion > 0f && GD.Randf() < _evasion) return;
        float effective = rawAmount * (1f - DamageReduction);
        if (type == Items.DamageType.Physical)
            effective *= (1f - PhysicalResistance);
        else if (type == Items.DamageType.Magic)
            effective *= (1f - MagicResistance);

        // Fortify: if active, reduce this hit; refresh for next hit
        if (_activeAugmentChances.TryGetValue("fortify", out float fortifyChance) && GD.Randf() < fortifyChance)
        {
            if (_fortifyActive) effective *= 0.5f;
            _fortifyActive = true;
        }

        float damageToShow = effective;

        // Focus Shield absorbs damage before HP
        if (_currentFocusShield > 0f)
        {
            float absorbed = Mathf.Min(_currentFocusShield, effective);
            _currentFocusShield -= absorbed;
            effective           -= absorbed;
            EmitSignal(SignalName.ShieldChanged, _currentFocusShield, _maxFocusShield);
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - effective);
        EmitSignal(SignalName.HealthChanged, CurrentHealth);

        if (damageToShow > 0f)
            EmitSignal(SignalName.DamageTaken, damageToShow, type == Items.DamageType.Magic);

        if (effective > 0f)
        {
            EmitSignal(SignalName.PlayerHit);
            Engine.TimeScale = 0f;
            GetTree().CreateTimer(0.05f, true, false, true).Timeout += () => Engine.TimeScale = 1f;
        }

        // Retaliation: deal damage back to attacker
        if (_activeAugmentChances.TryGetValue("retaliation", out float retChance) && GD.Randf() < retChance
            && attacker is Enemies.EnemyController ec)
            ec.TakeDamage(5f, Items.DamageType.Physical);

        // Dash Reflex: brief speed boost on hit
        if (_activeAugmentChances.TryGetValue("dash_reflex", out float dashChance) && GD.Randf() < dashChance)
            _dashReflexTimer = 1.0f;

        // Ghost Step: arm kill-heal window unconditionally; roll chance fires on kill
        if (_activeAugmentChances.ContainsKey("ghost_step"))
            _ghostStepTimer = 2.0f;

        if (CurrentHealth == 0f)
            EmitSignal(SignalName.PlayerDied);
    }

    public void OnEnemyKilled()
    {
        // Ghost Step: heal if killed within 2s of being hit
        if (_activeAugmentChances.TryGetValue("ghost_step", out float ghostChance) && _ghostStepTimer > 0f && GD.Randf() < ghostChance)
            Heal(10);
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        EmitSignal(SignalName.HealthChanged, CurrentHealth);
    }

    public void CollectXp(int amount)
    {
        CurrentXp += amount;
        while (CurrentXp >= XpToNextLevel)
        {
            CurrentXp     -= XpToNextLevel;
            Level++;
            XpToNextLevel  = ComputeXpToNextLevel(Level);

            if (_charData != null)
            {
                _charData.CurrentLevel = Level;
                _statBlock = _charData.BuildStatBlock();
                Speed              = _statBlock.Get(Stats.StatId.Speed);
                PhysicalResistance = _statBlock.Get(Stats.StatId.PhysicalResistance);
                MagicResistance    = _statBlock.Get(Stats.StatId.MagicResistance);
                _evasion           = _statBlock.Get(Stats.StatId.Evasion);
                var wc     = GetNodeOrNull<Weapon.WeaponController>("Weapon");
                var weapon = GetEquippedItem(_charData, Items.ItemSlot.Weapon);
                ApplyWeaponDamage(wc, weapon);
            }

            MaxHealth     = (int)_statBlock.Get(Stats.StatId.MaxHp);
            CurrentHealth = Mathf.Min(CurrentHealth + 5f, MaxHealth);
            MaxFocus      = _statBlock.Get(Stats.StatId.MaxFocus);
            _focusRegen   = _statBlock.Get(Stats.StatId.FocusRegen);
            EmitSignal(SignalName.LeveledUp, Level);
        }
        EmitSignal(SignalName.XpChanged, CurrentXp, XpToNextLevel);
    }

    private static string? GetWeaponModelPath(string? weaponId) => weaponId switch
    {
        "sword_t1" => "res://assets/models/equipment/weapon_sword.glb",
        "bow_t1"   => "res://assets/models/equipment/weapon_bow.glb",
        "wand_t1"  => "res://assets/models/equipment/weapon_wand.glb",
        _          => null
    };

    private static Items.ItemData? GetEquippedItem(Character.CharacterData c, Items.ItemSlot slot)
    {
        c.EquippedGear.TryGetValue(slot.ToString(), out var instance);
        return instance?.Definition;
    }

    private void OnSkillFired(int slotIndex, float cooldown, string delivery)
    {
        if (delivery == "AuraTick") return;
        if (_animTree == null) return;
        var param = delivery switch
        {
            "Ranged" => "parameters/shot_left/request",
            _        => "parameters/shot_right/request",
        };
        if ((bool)_animTree.Get(param.Replace("/request", "/active"))) return;
        if (delivery != "Ranged")
        {
            const float MeleeAnimLength = 2.3f; // melee_right_atack duration in seconds
            _animTree.Set("parameters/right_ts/scale", MeleeAnimLength / cooldown);
        }
        _animTree.Set(param, (int)AnimationNodeOneShot.OneShotRequest.Fire);

        if (delivery == "Melee")
        {
            try
            {
                var vfx = SwingVfxScene.Instantiate<GpuParticles3D>();
                vfx.Amount = 12;
                vfx.OneShot = true;
                if (vfx.ProcessMaterial is ParticleProcessMaterial ppm)
                {
                    var mat = (ParticleProcessMaterial)ppm.Duplicate();
                    mat.ScaleMin = 35f;
                    mat.ScaleMax = 55f;
                    vfx.ProcessMaterial = mat;
                }
                GetTree().Root.AddChild(vfx);
                vfx.GlobalPosition = GlobalPosition + new Vector3(0f, 20f, 0f);
                vfx.Call("activate_effects");
                GetTree().CreateTimer(2.0).Timeout += vfx.QueueFree;
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"Failed to spawn melee swing VFX: {e.Message}");
            }
        }
    }

    private static int FindBone(Skeleton3D skeleton, string name)
    {
        int idx = skeleton.FindBone(name);
        return idx >= 0 ? idx : skeleton.FindBone(name + "_2");
    }

    private static void AttachArmourToSkeleton(Node3D armourRoot, Skeleton3D skeleton)
    {
        int chestIdx = FindBone(skeleton, "Chest");
        int headIdx  = FindBone(skeleton, "Head");
        Vector3 chestOrigin = chestIdx >= 0 ? skeleton.GetBoneGlobalRest(chestIdx).Origin : Vector3.Zero;
        Vector3 headOrigin  = headIdx  >= 0 ? skeleton.GetBoneGlobalRest(headIdx).Origin  : Vector3.Zero;

        string chestBoneName = chestIdx >= 0 ? skeleton.GetBoneName(chestIdx) : "Chest";
        string headBoneName  = headIdx  >= 0 ? skeleton.GetBoneName(headIdx)  : "Head";
        var chestAttach = new BoneAttachment3D { BoneName = chestBoneName };
        var headAttach  = new BoneAttachment3D { BoneName = headBoneName };
        skeleton.AddChild(chestAttach);
        skeleton.AddChild(headAttach);

        var pieces = new List<(Node3D n, Vector3 pos)>();
        foreach (var child in armourRoot.GetChildren())
            if (child is Node3D n) pieces.Add((n, n.Position));

        foreach (var (piece, origPos) in pieces)
        {
            string name = piece.Name.ToString();
            bool isHead = name.Contains("Cap") || name.Contains("Hood") || name.Contains("Helm");
            var attach = isHead ? headAttach : chestAttach;
            Vector3 boneOrigin = isHead ? headOrigin : chestOrigin;

            armourRoot.RemoveChild(piece);
            attach.AddChild(piece);
            piece.Position = origPos - boneOrigin;
        }

        armourRoot.GetParent()?.RemoveChild(armourRoot);
        armourRoot.QueueFree();
    }

    private static void AttachWeaponToSkeleton(Node3D weaponRoot, Skeleton3D skeleton, string boneName = "Hand_R")
    {
        int handIdx = FindBone(skeleton, boneName);
        string handBoneName = handIdx >= 0 ? skeleton.GetBoneName(handIdx) : boneName;
        var attach = new BoneAttachment3D();
        skeleton.AddChild(attach);
        attach.BoneName = handBoneName;

        var pieces = new List<(Node3D n, Vector3 pos)>();
        foreach (var child in weaponRoot.GetChildren())
            if (child is Node3D n) pieces.Add((n, n.Position));

        // Anchor: use the Handle mesh AABB centre so models with offset geometry align correctly
        Vector3 anchorPos = pieces.Count > 0 ? pieces[0].pos : Vector3.Zero;
        foreach (var (n, p) in pieces)
        {
            if (n.Name.ToString().Contains("Handle"))
            {
                anchorPos = n is MeshInstance3D mi ? p + mi.GetAabb().GetCenter() : p;
                break;
            }
        }

        foreach (var (piece, origPos) in pieces)
        {
            weaponRoot.RemoveChild(piece);
            attach.AddChild(piece);
            piece.Position = origPos - anchorPos;
        }

        weaponRoot.GetParent()?.RemoveChild(weaponRoot);
        weaponRoot.QueueFree();
    }

    private void LoadAnimClip(AnimationPlayer target, string sourcePath, string sourceName, string targetName, Animation.LoopModeEnum loop)
    {
        var sourceScene = GD.Load<PackedScene>(sourcePath);
        if (sourceScene == null) return;
        var sourceRoot = sourceScene.Instantiate<Node3D>();
        AddChild(sourceRoot);
        var sourcePlayer = sourceRoot.FindChild("AnimationPlayer", true, false) as AnimationPlayer;
        if (sourcePlayer != null && sourcePlayer.HasAnimation(sourceName))
        {
            var copy = (Animation)sourcePlayer.GetAnimation(sourceName).Duplicate();
            copy.LoopMode = loop;

            if (!target.HasAnimationLibrary(""))
                target.AddAnimationLibrary("", new AnimationLibrary());
            target.GetAnimationLibrary("").AddAnimation(targetName, copy);
        }
        sourceRoot.QueueFree();
    }

    private void UpdateLockedTarget()
    {
        // Always recompute — cursor may have moved to a closer enemy
        EnemyController? best = null;
        float bestDist = float.MaxValue;
        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not EnemyController enemy || enemy.IsQueuedForDeletion()) continue;
            float dist = TargetPosition.DistanceTo(enemy.GlobalPosition);
            if (dist < bestDist) { bestDist = dist; best = enemy; }
        }
        LockedTarget = best;
    }

    private Node3D? FindNearestEnemy()
    {
        Node3D? nearest = null;
        float nearestDistSq = float.MaxValue;
        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not Node3D enemy || enemy.IsQueuedForDeletion()) continue;
            float distSq = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
            if (distSq < nearestDistSq) { nearestDistSq = distSq; nearest = enemy; }
        }
        return nearest;
    }

    private static int ComputeXpToNextLevel(int level)
    {
        int xtn = 20;
        for (int i = 1; i < level; i++)
            xtn = (int)(xtn * 1.4f);
        return xtn;
    }

    public float GetAvailableFocus() => Mathf.Max(0f, CurrentFocus - _totalReserved);

    public bool TrySpendFocus(float amount)
    {
        if (GetAvailableFocus() < amount) return false;
        CurrentFocus -= amount;
        EmitSignal(SignalName.FocusChanged, GetAvailableFocus(), MaxFocus);
        return true;
    }

    public void ReserveFocus(float amount)
    {
        _totalReserved += amount;
        EmitSignal(SignalName.FocusChanged, GetAvailableFocus(), MaxFocus);
    }

    public void UnreserveFocus(float amount)
    {
        _totalReserved = Mathf.Max(0f, _totalReserved - amount);
        EmitSignal(SignalName.FocusChanged, GetAvailableFocus(), MaxFocus);
    }

    private void ApplyWeaponDamage(Weapon.WeaponController? wc, Items.ItemData? weapon)
    {
        if (wc == null || weapon == null) return;

        float weaponBase = weapon.BaseDamage * (1f + weapon.DamageBonus);
        float physDmg    = Mathf.Max(1f, weaponBase * _statBlock.Get(Stats.StatId.PhysicalDamage));
        float magicDmg   = Mathf.Max(1f, weaponBase * _statBlock.Get(Stats.StatId.MagicDamage));

        wc.SetDamage(physDmg, magicDmg);
        wc.SetGlobalCritChance(_statBlock.Get(Stats.StatId.CritChance) + weapon.CritChanceBonus);
        wc.SetCritMultiplier(_statBlock.Get(Stats.StatId.CritDamage));
    }
}
