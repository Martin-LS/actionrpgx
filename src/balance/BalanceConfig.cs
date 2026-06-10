namespace Godot1;

public static class BalanceConfig
{
    public static class Weapons
    {
        public const float SwordRange = 1f;   // tiles
        public const float BowRange   = 7f;
        public const float WandRange  = 5f;

        // Base damage (tier 1) — placeholder, owned by Balancer
        public const float SwordBaseDamage  = 15f;
        public const float BowBaseDamage    = 12f;
        public const float WandBaseDamage   = 18f;

        // Identity bonuses (tier 1) — placeholder, owned by Balancer
        public const float SwordDamageBonus = 0.10f; // +10% physical damage
        public const float BowCritBonus     = 0.08f; // +8% crit chance
        public const float WandDamageBonus  = 0.10f; // +10% magic damage
    }

    public static class Armour
    {
        // Heavy — per piece (hat + body each contribute independently)
        public const int   HeavyBonusHp         = 20;
        public const float HeavyBonusSpeed       = -20f;
        public const float HeavyDamageReduction  = 0.10f;
        public const float HeavyRangeModifier    = -1.5f; // tiles; ranged weapons only

        // Medium
        public const int   MediumBonusHp         = 10;
        public const float MediumBonusSpeed       = 0f;
        public const float MediumDamageReduction  = 0f;
        public const float MediumRangeModifier    = 0f;

        // Light
        public const int   LightBonusHp          = 0;
        public const float LightBonusSpeed        = 20f;
        public const float LightDamageReduction   = 0f;
        public const float LightRangeModifier     = 1.5f;
    }

    public static class Accessories
    {
        public const float RingPhysicalResistance = 0.05f;
    }

    public static class Skills
    {
        public const float StrikeCooldown     = 0.8f;
        public const float StrikeRange        = 200f; // world units
        public const float MeleeWindupFraction = 0.35f; // fraction of cooldown before damage lands

        public const float CycloneCooldown = 0.25f; // tick interval (4 hits/sec)
        public const float CycloneRange    = 150f;

        public const float NovaCooldown = 1.5f;
        public const float NovaRange    = 300f;

        public const float DamageAuraCooldown = 1.0f; // damage tick interval
        public const float DamageAuraRange    = 250f;
    }

    public static class Focus
    {
        // Per-archetype base pool sizes and regen rates — placeholder, owned by Balancer
        public const float WarriorMaxFocus    = 80f;
        public const float WarriorRegenPerSec = 12f;
        public const float RogueMaxFocus      = 100f;
        public const float RogueRegenPerSec   = 15f;
        public const float MageMaxFocus       = 150f;
        public const float MageRegenPerSec    = 10f;

        // Focus Shield (all archetypes) — placeholder, owned by Balancer
        public const float ShieldFraction    = 0.30f;
        public const float ShieldRegenPerSec = 5f;

        // Per-skill focus costs — placeholder, owned by Balancer
        public const float StrikeFocusCost        = 5f;
        public const float CycloneFocusCostPerSec = 12f;  // drain per second while channeled
        public const float NovaFocusCost          = 20f;
        public const float DamageAuraReservation  = 0.25f; // fraction of MaxFocus

        // Per-skill type damage multipliers — placeholder, owned by Balancer
        public const float CycloneDamageMultiplier = 0.4f;
        public const float AuraDamageMultiplier    = 0.2f;
        public const float NovaDamageMultiplier    = 0.8f;
    }

    public static class Eots
    {
        public const float SlowApplyChance   = 0.30f;
        public const float SlowDuration      = 3f;    // seconds
        public const float SlowFraction      = 0.40f; // speed reduction

        public const float BurnApplyChance   = 0.25f;
        public const float BurnDuration      = 4f;
        public const float BurnTickRate      = 0.5f;
        public const float BurnDamagePerTick = 5f;
    }

    public static class Enemies
    {
        public static class Skeleton
        {
            public const float BaseSpeed          = 65f;
            public const int   BaseHealth         = 2;
            public const int   ContactDamage      = 5;
            public const float PhysicalResistance = 0.10f;
        }

        public const float SpeedPerMinute      = 5f;  // added to base speed each minute
        public const int   HealthPerMinute     = 3;   // added to base health each minute
        public const float MeleeContactRange   = 32f; // world units; enemy start-hit proximity
    }

    public static class Drops
    {
        public const float CoinChance     = 0.25f;
        public const float HealthChance   = 0.10f;
        public const float CraftingChance = 0.20f;
    }

    public static class Pickups
    {
        public const int XpShardValue    = 5;
        public const int HealthHealAmount = 15;
    }

    public static class Archetypes
    {
        public const float DefaultMultiplier = 0.1f;

        public static class Warrior
        {
            public const float MaxHp                    = 150f;
            public const float Speed                    = 170f;
            public const float PhysicalDamageMultiplier = 1.5f;
            public const float MagicDamageMultiplier    = 0.5f;
        }

        public static class Rogue
        {
            public const float MaxHp                    = 80f;
            public const float Speed                    = 260f;
            public const float PhysicalDamageMultiplier = 1.0f;
            public const float MagicDamageMultiplier    = 0.5f;
        }

        public static class Mage
        {
            public const float MaxHp                    = 100f;
            public const float Speed                    = 200f;
            public const float PhysicalDamageMultiplier = 0.5f;
            public const float MagicDamageMultiplier    = 1.5f;
        }
    }

    public static class LevelUp
    {
        public const float HpBonusPerLevel     = 5f;
        public const float DamageBonusPerLevel = 0.02f; // +2% per level, cumulative — placeholder, owned by Balancer
    }

    public static class SkillAugments
    {
        public const float CritChance     = 0.15f; // TBD — Balancer
        public const float CritMultiplier = 1.5f;  // TBD — Balancer
    }
}
