using Godot;
using System;
using Godot1.Skills;

namespace Godot1.Ui;

public partial class SkillPickerPanel : Control
{
    private Character.CharacterManager _manager   = null!;
    private Character.CharacterData    _character = null!;
    private int                        _slotIndex;
    private Action?                    _onClose;

    private Label         _titleLabel = null!;
    private VBoxContainer _itemList   = null!;
    private Button        _closeBtn   = null!;

    // Must be called before AddChild so fields are set before _Ready fires.
    public void Init(
        Character.CharacterManager manager,
        Character.CharacterData    character,
        int                        slotIndex,
        Action?                    onClose = null)
    {
        _manager   = manager;
        _character = character;
        _slotIndex = slotIndex;
        _onClose   = onClose;
    }

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>        ("Panel/VBox/TitleLabel");
        _itemList   = GetNode<VBoxContainer>("Panel/VBox/Scroll/ItemList");
        _closeBtn   = GetNode<Button>       ("Panel/VBox/CloseButton");

        _titleLabel.Text  = $"Choose Skill for Slot {_slotIndex + 1}";
        _closeBtn.Pressed += Close;

        PopulateSkills();
    }

    private void PopulateSkills()
    {
        foreach (Node child in _itemList.GetChildren())
            child.QueueFree();

        string? currentId = _slotIndex < _character.SlottedSkillIds.Count
            ? _character.SlottedSkillIds[_slotIndex]
            : null;

        foreach (var skillId in _manager.Profile.OwnedSkillIds)
        {
            var skill = SkillRegistry.Get(skillId);
            if (skill == null) continue;

            bool   active = skillId == currentId;
            string label  = $"{skill.Name}  [{skill.Type}]  CD: {skill.Cooldown:F1}s{(active ? "  [active]" : "")}";
            var    btn    = new Button { Text = label };
            string captured = skillId;
            btn.Pressed += () => { _manager.EquipSkill(_character.Id, _slotIndex, captured); Close(); };
            _itemList.AddChild(btn);
        }

        if (_itemList.GetChildCount() == 0)
            _itemList.AddChild(new Label { Text = "No skills owned. Craft some first." });
    }

    private void Close()
    {
        _onClose?.Invoke();
        QueueFree();
    }
}
