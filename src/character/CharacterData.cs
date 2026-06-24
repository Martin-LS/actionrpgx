using System.Collections.Generic;
using ActionRpgX.Items;
using ActionRpgX.Stats;

namespace ActionRpgX.Character;

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

    public List<bool> SlotAutoActivate { get; set; } = new() { true, true, true, true, true };

    public StatBlock BuildStatBlock()
    {
        var block = new StatBlock();

        // Archetype base HP and Speed — set directly.
        var (baseHp, baseSpd) = Type switch
        {
            CharacterType.Warrior => (BalanceConfig.Archetypes.Warrior.MaxHp, BalanceConfig.Archetypes.BaseSpeed),
            CharacterType.Rogue   => (BalanceConfig.Archetypes.Rogue.MaxHp,   BalanceConfig.Archetypes.BaseSpeed),
            CharacterType.Mage    => (BalanceConfig.Archetypes.Mage.MaxHp,    BalanceConfig.Archetypes.BaseSpeed),
            _                     => (BalanceConfig.Archetypes.Warrior.MaxHp, BalanceConfig.Archetypes.BaseSpeed),
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

        // Primary stats — grow with level via archetype-specific rates.
        var gains = PrimaryStatGainRegistry.Get(Type);
        float str     = gains.StrBase + (CurrentLevel - 1) * gains.StrPerLevel;
        float dex     = gains.DexBase + (CurrentLevel - 1) * gains.DexPerLevel;
        float intStat = gains.IntBase + (CurrentLevel - 1) * gains.IntPerLevel;

        // Damage multipliers — read by PlayerController.ApplyWeaponDamage.
        block.SetBase(StatId.PhysicalDamage, str     * PrimaryStatConversions.StrToPhysDamageMultiplier);
        block.SetBase(StatId.MagicDamage,    intStat * PrimaryStatConversions.IntToMagDamageMultiplier);

        // Derived stats from primary growth (incremental above level-1 base).
        float strGain = str - gains.StrBase;
        if (strGain > 0f)
            block.AddModifier(new StatModifier(StatId.MaxHp, ModifierType.FlatAdd,
                strGain * PrimaryStatConversions.StrToMaxHp, ModifierSource.Level));

        float intGain = intStat - gains.IntBase;
        if (intGain > 0f)
        {
            block.AddModifier(new StatModifier(StatId.MaxFocus,   ModifierType.FlatAdd,
                intGain * PrimaryStatConversions.IntToMaxFocus,   ModifierSource.Level));
            block.AddModifier(new StatModifier(StatId.FocusRegen, ModifierType.FlatAdd,
                intGain * PrimaryStatConversions.IntToFocusRegen, ModifierSource.Level));
        }

        block.SetBase(StatId.CritChance, dex * PrimaryStatConversions.DexToCritChance);
        block.SetBase(StatId.CritDamage, 1.5f + str * PrimaryStatConversions.StrToCritDamage);
        block.SetBase(StatId.Evasion,    dex * PrimaryStatConversions.DexToEvasion);

        // Item contributions — flat bonuses, no archetype/level scaling.
        foreach (var (_, instance) in EquippedGear)
        {
            var item = instance.Definition;
            if (item == null) continue;

            if (item.Slot == ItemSlot.Hat || item.Slot == ItemSlot.Body)
            {
                if (item.BonusHp != 0)
                    block.AddModifier(new StatModifier(StatId.MaxHp, ModifierType.FlatAdd,
                        item.BonusHp, ModifierSource.Item, instance.Id));
                if (item.BonusSpeed != 0f)
                    block.AddModifier(new StatModifier(StatId.Speed, ModifierType.FlatAdd,
                        item.BonusSpeed, ModifierSource.Item, instance.Id));
            }
            else if (item.Slot == ItemSlot.Ring && item.PhysicalResistance != 0f)
            {
                block.AddModifier(new StatModifier(StatId.PhysicalResistance, ModifierType.FlatAdd,
                    item.PhysicalResistance, ModifierSource.Item, instance.Id));
            }
        }

        return block;
    }
}
