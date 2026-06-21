using Godot;
using System.Linq;
using System.Text.RegularExpressions;
using ActionRpgX.Character;

namespace ActionRpgX.Ui;

public partial class CharacterCreate : Control
{
    private CharacterType _pendingType = CharacterType.Warrior;

    private static readonly Regex AlphaNumeric = new(@"^[a-zA-Z0-9]+$");

    private Button? _warriorBtn;
    private Button? _rogueBtn;
    private Button? _mageBtn;

    public override void _Ready()
    {
        var nameInput  = GetNode<LineEdit>("VBox/NameInput");
        var confirmBtn = GetNode<Button>("VBox/ConfirmBtn");
        var errorLabel = GetNode<Label>("VBox/ErrorLabel");
        var manager    = GetNode<CharacterManager>("/root/CharacterManager");

        _warriorBtn = GetNode<Button>("VBox/WarriorBtn");
        _rogueBtn   = GetNode<Button>("VBox/RogueBtn");
        _mageBtn    = GetNode<Button>("VBox/MageBtn");

        nameInput.TextChanged += text =>
        {
            var error = Validate(text, manager);
            errorLabel.Text     = error ?? "";
            errorLabel.Visible  = error != null;
            confirmBtn.Disabled = error != null;
        };

        _warriorBtn.Pressed += () => SelectClass(CharacterType.Warrior);
        _rogueBtn.Pressed   += () => SelectClass(CharacterType.Rogue);
        _mageBtn.Pressed    += () => SelectClass(CharacterType.Mage);

        confirmBtn.Disabled = true;
        confirmBtn.Pressed += () =>
        {
            var name = nameInput.Text;
            if (Validate(name, manager) != null) return;
            manager.Create(name, _pendingType);
            GetTree().ChangeSceneToFile("res://src/ui/account_screen.tscn");
        };

        GetNode<Button>("VBox/CancelBtn").Pressed += () =>
            GetTree().ChangeSceneToFile("res://src/ui/account_screen.tscn");

        SelectClass(CharacterType.Warrior);
    }

    private void SelectClass(CharacterType type)
    {
        _pendingType = type;
        SetSelected(_warriorBtn!, type == CharacterType.Warrior);
        SetSelected(_rogueBtn!,   type == CharacterType.Rogue);
        SetSelected(_mageBtn!,    type == CharacterType.Mage);
    }

    private static void SetSelected(Button btn, bool selected)
    {
        btn.Modulate = selected ? Color.FromHtml("#F5C842") : Colors.White;
    }

    private static string? Validate(string name, CharacterManager manager)
    {
        if (name.Length == 0)                              return "Name is required.";
        if (!AlphaNumeric.IsMatch(name))                   return "Letters and numbers only, no spaces.";
        if (manager.GetAll().Any(c => c.Name == name))      return "Name already taken.";
        return null;
    }
}
