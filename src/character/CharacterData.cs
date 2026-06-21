using System.Collections.Generic;
using Godot2.Items;
using Godot2.Stats;

namespace Godot2.Character;

public class CharacterData
{
    public string Id { get; set; } = System.Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public CharacterType Type { get; set; } = CharacterType.Warrior;
    public int RunsCompleted { get; set; } = 0;

    public int CurrentLevel { get; set; } = 1;
    public int CurrentXp    { get; set; } = 0;

    // Gear instances owned by this character (moved out of profile inventory when equipped).
    public Dictionary<string, GearItemInstance> EquippedGear { get; set; } = new();

    // GUIDs referencing SkillItemInstances in ProfileData.OwnedSkillInstances.
    // Skills are not moved out of inventory when slotted — same instance can fill multiple slots.
    public List<string> SlottedSkillInstanceIds { get; set; } = new();

    public List<bool> SlotAutoActivate { get; set; } = new() { true, true, true };

    public StatBlock BuildStatBlock()
    {
        var block = new StatBlock();

        // Archetype base stats — set directly, not subject to multiplier formula.
        var (baseHp, baseSpd) = Type switch
        {
            CharacterType.Warrior => (BalanceConfig.Archetypes.Warrior.MaxHp, BalanceConfig.Archetypes.Warrior.Speed),
            CharacterType.Rogue   => (BalanceConfig.Archetypes.Rogue.MaxHp,   BalanceConfig.Archetypes.Rogue.Speed),
            CharacterType.Mage    => (BalanceConfig.Archetypes.Mage.MaxHp,    BalanceConfig.Archetypes.Mage.Speed),
            _                     => (BalanceConfig.Archetypes.Warrior.MaxHp, BalanceConfig.Archetypes.Warrior.Speed),
        };
        block.SetBase(StatId.MaxHp,  baseHp);
        block.SetBase(StatId.Speed,  baseSpd);

        var (baseFocus, baseFocusRegen) = Type switch
        {
            CharacterType.Warrior => (BalanceConfig.Focus.WarriorMaxFocus, BalanceConfig.Focus.WarriorRegenPerSec),
            CharacterType.Rogue   => (BalanceConfig.Focus.RogueMaxFocus,   BalanceConfig.Focus.RogueRegenPerSec),
            CharacterType.Mage    => (BalanceConfig.Focus.MageMaxFocus,    BalanceConfig.Focus.MageRegenPerSec),
            _                     => (BalanceConfig.Focus.WarriorMaxFocus, BalanceConfig.Focus.WarriorRegenPerSec),
        };
        block.SetBase(StatId.MaxFocus,   baseFocus);
        block.SetBase(StatId.FocusRegen, baseFocusRegen);

        // Level-up HP bonus — scaled by archetype multiplier × level.
        // Damage is no longer stored in StatBlock — it is computed from weapon data in PlayerController.
        int levelsAboveOne = CurrentLevel - 1;
        if (levelsAboveOne > 0)
        {
            float hpBonus = levelsAboveOne * BalanceConfig.LevelUp.HpBonusPerLevel * CurrentLevel * ArchetypeMultiplierRegistry.Get(Type, StatId.MaxHp);
            block.AddModifier(new StatModifier(StatId.MaxHp, ModifierType.FlatAdd, hpBonus, ModifierSource.Level));
        }

        // Item contributions — scaled by archetype multiplier × level.
        foreach (var (_, instance) in EquippedGear)
        {
            var item = instance.Definition;
            if (item == null) continue;

            if (item.Slot == ItemSlot.Hat || item.Slot == ItemSlot.Body)
            {
                if (item.BonusHp != 0)
                    block.AddModifier(new StatModifier(StatId.MaxHp, ModifierType.FlatAdd,
                        item.BonusHp * CurrentLevel * ArchetypeMultiplierRegistry.Get(Type, StatId.MaxHp),
                        ModifierSource.Item, instance.Id));
                if (item.BonusSpeed != 0f)
                    block.AddModifier(new StatModifier(StatId.Speed, ModifierType.FlatAdd,
                        item.BonusSpeed * CurrentLevel * ArchetypeMultiplierRegistry.Get(Type, StatId.Speed),
                        ModifierSource.Item, instance.Id));
            }
            else if (item.Slot == ItemSlot.Ring && item.PhysicalResistance != 0f)
            {
                block.AddModifier(new StatModifier(StatId.PhysicalResistance, ModifierType.FlatAdd,
                    item.PhysicalResistance * CurrentLevel * ArchetypeMultiplierRegistry.Get(Type, StatId.PhysicalResistance),
                    ModifierSource.Item, instance.Id));
            }
        }

        return block;
    }
}
