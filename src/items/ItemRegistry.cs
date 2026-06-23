using System.Collections.Generic;
using System.Linq;

namespace ActionRpgX.Items;

public static class ItemRegistry
{
    public static readonly IReadOnlyDictionary<string, ItemData> All =
        new Dictionary<string, ItemData>
        {
            // Weapons
            // WeaponRange is in tiles (1 tile = GameScale.TileSize world units). RangeMultiplier is dimensionless.
            ["sword_t1"] = new("sword_t1", "Sword", ItemSlot.Weapon,
                IconPath: "res://assets/icons/items/iron_sword.png",
                WeaponRange: BalanceConfig.Weapons.SwordRange, PreferredDelivery: "Melee",
                BaseDamage: BalanceConfig.Weapons.SwordBaseDamage, BaseDamageType: DamageType.Physical,
                DamageBonus: BalanceConfig.Weapons.SwordDamageBonus) { Tags = new[] { "Melee" } },
            ["bow_t1"]   = new("bow_t1",   "Bow",   ItemSlot.Weapon,
                IconPath: "res://assets/icons/items/war_band.png",
                WeaponRange: BalanceConfig.Weapons.BowRange, PreferredDelivery: "Range",
                BaseDamage: BalanceConfig.Weapons.BowBaseDamage, BaseDamageType: DamageType.Physical,
                CritChanceBonus: BalanceConfig.Weapons.BowCritBonus) { Tags = new[] { "Range" } },
            ["wand_t1"]  = new("wand_t1",  "Wand",  ItemSlot.Weapon,
                IconPath: "res://assets/icons/items/enchanted_blade.png",
                WeaponRange: BalanceConfig.Weapons.WandRange, PreferredDelivery: "RangeMagic",
                BaseDamage: BalanceConfig.Weapons.WandBaseDamage, BaseDamageType: DamageType.Magic,
                DamageBonus: BalanceConfig.Weapons.WandDamageBonus) { Tags = new[] { "Magic" } },

            // Hats
            ["heavy_hat_t1"]  = new("heavy_hat_t1",  "Heavy Hat",  ItemSlot.Hat,
                IconPath: "res://assets/icons/items/chain_mail.png",
                ArmorCategory: ArmorCategory.Heavy,  BonusHp: BalanceConfig.Armour.HeavyBonusHp,  BonusSpeed: BalanceConfig.Armour.HeavyBonusSpeed,  DamageReduction: BalanceConfig.Armour.HeavyDamageReduction,  RangeMultiplier: BalanceConfig.Armour.HeavyRangeMultiplier)  { Tags = new[] { "Heavy" } },
            ["medium_hat_t1"] = new("medium_hat_t1", "Medium Hat", ItemSlot.Hat,
                IconPath: "res://assets/icons/items/mage_robe.png",
                ArmorCategory: ArmorCategory.Medium, BonusHp: BalanceConfig.Armour.MediumBonusHp, BonusSpeed: BalanceConfig.Armour.MediumBonusSpeed, DamageReduction: BalanceConfig.Armour.MediumDamageReduction, RangeMultiplier: BalanceConfig.Armour.MediumRangeMultiplier) { Tags = new[] { "Medium" } },
            ["light_hat_t1"]  = new("light_hat_t1",  "Light Hat",  ItemSlot.Hat,
                IconPath: "res://assets/icons/items/leather_vest.png",
                ArmorCategory: ArmorCategory.Light,  BonusHp: BalanceConfig.Armour.LightBonusHp,  BonusSpeed: BalanceConfig.Armour.LightBonusSpeed,  DamageReduction: BalanceConfig.Armour.LightDamageReduction,  RangeMultiplier: BalanceConfig.Armour.LightRangeMultiplier)  { Tags = new[] { "Light" } },

            // Body armour
            ["heavy_body_t1"]  = new("heavy_body_t1",  "Heavy Body",  ItemSlot.Body,
                IconPath: "res://assets/icons/items/chain_mail.png",
                ArmorCategory: ArmorCategory.Heavy,  BonusHp: BalanceConfig.Armour.HeavyBonusHp,  BonusSpeed: BalanceConfig.Armour.HeavyBonusSpeed,  DamageReduction: BalanceConfig.Armour.HeavyDamageReduction,  RangeMultiplier: BalanceConfig.Armour.HeavyRangeMultiplier)  { Tags = new[] { "Heavy" } },
            ["medium_body_t1"] = new("medium_body_t1", "Medium Body", ItemSlot.Body,
                IconPath: "res://assets/icons/items/mage_robe.png",
                ArmorCategory: ArmorCategory.Medium, BonusHp: BalanceConfig.Armour.MediumBonusHp, BonusSpeed: BalanceConfig.Armour.MediumBonusSpeed, DamageReduction: BalanceConfig.Armour.MediumDamageReduction, RangeMultiplier: BalanceConfig.Armour.MediumRangeMultiplier) { Tags = new[] { "Medium" } },
            ["light_body_t1"]  = new("light_body_t1",  "Light Body",  ItemSlot.Body,
                IconPath: "res://assets/icons/items/leather_vest.png",
                ArmorCategory: ArmorCategory.Light,  BonusHp: BalanceConfig.Armour.LightBonusHp,  BonusSpeed: BalanceConfig.Armour.LightBonusSpeed,  DamageReduction: BalanceConfig.Armour.LightDamageReduction,  RangeMultiplier: BalanceConfig.Armour.LightRangeMultiplier)  { Tags = new[] { "Light" } },

            // Rings (no tags — universal augments only)
            ["ring_t1"] = new("ring_t1", "Ring", ItemSlot.Ring,
                IconPath: "res://assets/icons/items/swift_ring.png",
                PhysicalResistance: BalanceConfig.Accessories.RingPhysicalResistance),
        };

    public static ItemData? Get(string id) => All.GetValueOrDefault(id);

    public static IEnumerable<ItemData> ForSlot(ItemSlot slot) =>
        All.Values.Where(i => i.Slot == slot);
}
