using System;

namespace RidingCrop
{
    /// <summary>
    /// The stat ladder of the crops (pure numbers, no game types). Blunt damage and bleed start at the first crop and grow on top of the
    /// previous crop (compounding): blunt +40% per tier, bleed +25% per tier. Upgrades add a flat amount per level (max quality 4).
    /// </summary>
    internal static class CropBalance
    {
        internal const float BaseBluntDamage = 20f;
        internal const float BaseBleedDamagePerSecond = 10f;
        internal const float BleedDurationSeconds = 5f;
        internal const float DamageGrowthPerTier = 0.40f;
        internal const float BleedGrowthPerTier = 0.25f;
        internal const float ParryBonus = 2f;
        internal const float LastChainDamageMultiplier = 2f;
        internal const float BaseDurability = 200f;
        internal const float DurabilityStepPerTier = 50f;
        internal const float UpgradeBluntFractionOfBase = 0.30f;
        internal const float UpgradeDurabilityFractionOfBase = 0.20f;
        internal const int MaxQuality = 4;

        // Per-tier combat stats, same order as CropTable.All. All at quality 1, skill 0.
        private static readonly float[] StaggerTable = { 12f, 25f, 35f, 55f, 95f, 125f, 150f };
        private static readonly float[] KnockbackTable = { 30f, 60f, 80f, 100f, 100f, 100f, 100f };
        private static readonly float[] StaminaTable = { 6f, 8f, 10f, 12f, 14f, 16f, 18f };
        private static readonly float[] BlockArmorTable = { 3f, 12f, 21f, 30f, 39f, 57f, 66f };
        private static readonly float[] BlockForceTable = { 20f, 30f, 30f, 30f, 30f, 30f, 30f };

        /// <summary>Number of tiers the tables cover.</summary>
        internal static int Tiers { get { return StaggerTable.Length; } }

        /// <summary>Blunt damage of a tier at quality 1.</summary>
        internal static float Blunt(int tier) { return BaseBluntDamage * (float)Math.Pow(1f + DamageGrowthPerTier, tier); }

        /// <summary>Bleed damage per second of a tier.</summary>
        internal static float BleedPerSecond(int tier) { return BaseBleedDamagePerSecond * (float)Math.Pow(1f + BleedGrowthPerTier, tier); }

        /// <summary>Durability of a tier at quality 1.</summary>
        internal static float Durability(int tier) { return BaseDurability + DurabilityStepPerTier * tier; }

        /// <summary>Flat blunt damage every upgrade level adds (the same for every tier).</summary>
        internal static float BluntPerLevel { get { return BaseBluntDamage * UpgradeBluntFractionOfBase; } }

        /// <summary>Flat durability every upgrade level adds (the same for every tier).</summary>
        internal static float DurabilityPerLevel { get { return BaseDurability * UpgradeDurabilityFractionOfBase; } }

        /// <summary>Blunt damage at a quality level (1..MaxQuality).</summary>
        internal static float BluntAtQuality(int tier, int quality) { return Blunt(tier) + (quality - 1) * BluntPerLevel; }

        /// <summary>Stagger damage of a hit.</summary>
        internal static float Stagger(int tier) { return StaggerTable[tier]; }

        /// <summary>The game stores stagger as a multiplier on the blunt damage.</summary>
        internal static float StaggerMultiplier(int tier) { return StaggerTable[tier] / Blunt(tier); }

        internal static float Knockback(int tier) { return KnockbackTable[tier]; }

        internal static float Stamina(int tier) { return StaminaTable[tier]; }

        internal static float BlockArmor(int tier) { return BlockArmorTable[tier]; }

        internal static float BlockForce(int tier) { return BlockForceTable[tier]; }
    }
}
