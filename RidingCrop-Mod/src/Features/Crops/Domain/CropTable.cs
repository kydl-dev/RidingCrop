using System;

namespace RidingCrop
{
    /// <summary>The crafting station a crop is made at.</summary>
    internal enum CropStation
    {
        Workbench,
        Forge,
        BlackForge
    }

    /// <summary>One recipe ingredient. Upgrading costs half of the amount per level (rounded up, at least 1).</summary>
    internal sealed class CropIngredient
    {
        internal CropIngredient(string item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        /// <summary>The item prefab id (for example "LeatherScraps").</summary>
        internal string Item { get; }

        /// <summary>How many to craft the crop at quality 1.</summary>
        internal int Amount { get; }

        /// <summary>How many more each upgrade level costs. The game charges nothing for upgrades when this is 0, so it is never 0.</summary>
        internal int UpgradeAmount { get { return Math.Max(1, (Amount + 1) / 2); } }
    }

    /// <summary>One crop of the family: ids, texts, texture, station and recipe.</summary>
    internal sealed class CropDef
    {
        internal CropDef(string prefabName, string displayName, string description, string albedoName, CropStation station, int stationLevel, params CropIngredient[] ingredients)
        {
            PrefabName = prefabName;
            DisplayName = displayName;
            Description = description;
            AlbedoName = albedoName;
            Station = station;
            StationLevel = stationLevel;
            Ingredients = ingredients;
        }

        /// <summary>Item id. Never rename after release: saved games refer to it.</summary>
        internal string PrefabName { get; }

        internal string DisplayName { get; }

        internal string Description { get; }

        /// <summary>Name of this crop's colour texture in the asset bundle.</summary>
        internal string AlbedoName { get; }

        internal CropStation Station { get; }

        /// <summary>Station level needed to craft at quality 1 (the game adds one level per upgrade).</summary>
        internal int StationLevel { get; }

        internal CropIngredient[] Ingredients { get; }
    }

    /// <summary>The seven crops, weakest first. Tier number = position in this list (0 = weakest).</summary>
    internal static class CropTable
    {
        /// <summary>The item the crops are copied from (animations and base stats).</summary>
        internal const string VanillaBasePrefab = "Club";

        internal static readonly CropDef[] All =
        {
            new CropDef("RidingCrop", "Do I boar you?",
                "A slim leather-wrapped riding crop with a gold-capped handle. It stings.",
                "crop_Albedo", CropStation.Workbench, 1,
                new CropIngredient("LeatherScraps", 10), new CropIngredient("Resin", 2), new CropIngredient("Wood", 5)),
            new CropDef("RidingCrop_V1", "Dear, oh deer!",
                "A scarlet-wrapped riding crop with black fittings and a white-bound collar. Hits harder than the crop before it.",
                "crop_Albedo_V1", CropStation.Workbench, 2,
                new CropIngredient("DeerHide", 10), new CropIngredient("Bronze", 2), new CropIngredient("FineWood", 5)),
            new CropDef("RidingCrop_V3", "Bear with me!",
                "A black riding crop with red fittings and a red-bound collar. Hits harder than the crop before it.",
                "crop_Albedo_V3", CropStation.Forge, 1,
                new CropIngredient("BjornHide", 10), new CropIngredient("SurtlingCore", 2), new CropIngredient("RoundLog", 5)),
            new CropDef("RidingCrop_V4", "Howl yeah!",
                "A white riding crop with black fittings and a black-bound collar. Hits harder than the crop before it.",
                "crop_Albedo_V4", CropStation.Forge, 2,
                new CropIngredient("WolfPelt", 10), new CropIngredient("Obsidian", 2), new CropIngredient("FineWood", 5)),
            new CropDef("RidingCrop_V6", "Loxy Poxy",
                "A burnt-orange riding crop with black fittings and a black-bound collar. Hits harder than the crop before it.",
                "crop_Albedo_V6", CropStation.Forge, 3,
                new CropIngredient("LoxPelt", 10), new CropIngredient("Tar", 2), new CropIngredient("ElderBark", 5)),
            new CropDef("RidingCrop_V2", "Askvin worthy",
                "A deep green riding crop with black fittings and a black-bound collar. Hits harder than the crop before it.",
                "crop_Albedo_V2", CropStation.BlackForge, 1,
                new CropIngredient("AskHide", 10), new CropIngredient("CharredBone", 2), new CropIngredient("Blackwood", 5)),
            new CropDef("RidingCrop_V5", "Seal the deal!",
                "A weathered, bone-grey riding crop with brown fittings and a brown-bound collar. The finest of the family.",
                "crop_Albedo_V5", CropStation.BlackForge, 2,
                new CropIngredient("MooseHide", 10), new CropIngredient("SealHide", 2), new CropIngredient("Frostwood", 5)),
        };
    }
}
