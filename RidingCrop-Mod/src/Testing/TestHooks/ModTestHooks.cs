using System;
using System.Linq;
using System.Reflection;
using KydlCore.Common;
using RidingCrop.Bootstrap;

namespace RidingCrop.Testing
{
    /// <summary>KydlCore D-084: hooks for DrakesTestLab (tests/TestLab/suites/RidingCrop). They read the REAL ObjectDB items, recipes and the RidingCrop skill.</summary>
    internal static class ModTestHooks
    {
        private static ItemDrop.ItemData.SharedData Shared(string prefab)
        {
            UnityEngine.GameObject go = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(prefab) : null;
            ItemDrop drop = go != null ? go.GetComponent<ItemDrop>() : null;
            if (drop == null)
            {
                throw new Exception("item " + prefab + " not found");
            }

            return drop.m_itemData.m_shared;
        }

        private static Recipe RecipeOf(string prefab)
        {
            Recipe r = ObjectDB.instance.m_recipes.FirstOrDefault(x => x != null && x.m_item != null && x.m_item.name == prefab);
            if (r == null)
            {
                throw new Exception("no recipe for " + prefab);
            }

            return r;
        }

        /// <summary>Every test starts here (MIGRATION_DECISIONS C26): the player on the open arena spot 70 m from the lab start, facing +z, god mode on.</summary>
        [TestHook("ridingcrop.arena")]
        private static string Arena() { return TestArena.MovePlayer(); }

        /// <summary>Is the feature running (FeatureRegistry state after its Enable switches)? ids: crops, sounds.</summary>
        [TestHook("ridingcrop.featureRunning")]
        private static bool FeatureRunning(string id) { return ModInstaller.Current != null && ModInstaller.Current.Features.IsRunning(id); }

        /// <summary>Is this tab (settings) registered in the KY hub right now?</summary>
        [TestHook("ridingcrop.tabRegistered")]
        private static bool TabRegistered(string id) { return KydlCore.Features.Hub.Contracts.KyHub.HasTab(Plugin.ModGuid, id); }

        [TestHook("ridingcrop.selftest")]
        private static string SelfTest() { return TestLabSupport.RunInGameTests(typeof(InGameTests)); }

        /// <summary>How many of the seven crops exist in the ObjectDB.</summary>
        [TestHook("ridingcrop.crops")]
        private static int Crops() { return CropTable.All.Count(c => ObjectDB.instance.GetItemPrefab(c.PrefabName) != null); }

        /// <summary>The crop's blunt damage at quality 1.</summary>
        [TestHook("ridingcrop.blunt")]
        private static float Blunt(string prefab) { return (float)Math.Round(Shared(prefab).m_damages.m_blunt, 2); }

        /// <summary>Total damage of every type at quality 1 (the crops are blunt-only, so equal to blunt).</summary>
        [TestHook("ridingcrop.damage")]
        private static float Damage(string prefab)
        {
            HitData.DamageTypes d = Shared(prefab).m_damages;
            return (float)Math.Round(d.m_blunt + d.m_slash + d.m_pierce + d.m_fire + d.m_frost + d.m_lightning + d.m_poison + d.m_spirit + d.m_chop + d.m_pickaxe, 2);
        }

        /// <summary>The weapon's skill as text (Clubs, or the id of the RidingCrop skill).</summary>
        [TestHook("ridingcrop.skill")]
        private static string Skill(string prefab) { return Shared(prefab).m_skillType.ToString(); }

        [TestHook("ridingcrop.maxQuality")]
        private static int MaxQuality(string prefab) { return Shared(prefab).m_maxQuality; }

        [TestHook("ridingcrop.durability")]
        private static float Durability(string prefab) { return Shared(prefab).m_maxDurability; }

        /// <summary>Bleed damage per second the crop's hit applies (0 when it has no bleed).</summary>
        [TestHook("ridingcrop.bleedDps")]
        private static float BleedDps(string prefab)
        {
            SE_RidingCropBleed bleed = Shared(prefab).m_attackStatusEffect as SE_RidingCropBleed;
            return bleed == null ? 0f : (float)Math.Round(bleed.m_damagePerTick / bleed.m_tickInterval, 2);
        }

        /// <summary>True when the ObjectDB has a recipe that crafts the item.</summary>
        [TestHook("ridingcrop.hasRecipe")]
        private static bool HasRecipe(string prefab)
        {
            return ObjectDB.instance.m_recipes.Any(r => r != null && r.m_item != null && r.m_item.name == prefab);
        }

        /// <summary>Crafting station of the recipe (text) and the minimum station level.</summary>
        [TestHook("ridingcrop.station")]
        private static string Station(string prefab)
        {
            Recipe r = RecipeOf(prefab);
            return (r.m_craftingStation != null ? r.m_craftingStation.name : "none") + " " + r.m_minStationLevel;
        }

        /// <summary>The recipe's cost to craft, "name:amount,..." at the given quality (what the game charges).</summary>
        [TestHook("ridingcrop.cost")]
        private static string Cost(string prefab, int quality = 1)
        {
            Recipe r = RecipeOf(prefab);
            return string.Join(",", r.m_resources.Where(q => q != null && q.m_resItem != null).Select(q => q.m_resItem.name + ":" + q.GetAmount(quality)).ToArray());
        }

        /// <summary>True when the player's inventory holds the item (any quality).</summary>
        [TestHook("ridingcrop.owned")]
        private static bool Owned(string prefab)
        {
            Inventory inv = Player.m_localPlayer != null ? Player.m_localPlayer.GetInventory() : null;
            return inv != null && inv.GetAllItems().Any(i => i.m_dropPrefab != null && i.m_dropPrefab.name == prefab);
        }

        /// <summary>True when the item in the player's hand is the prefab.</summary>
        [TestHook("ridingcrop.inHand")]
        private static bool InHand(string prefab)
        {
            Player p = Player.m_localPlayer;
            if (p == null)
            {
                return false;
            }

            FieldInfo f = typeof(Humanoid).GetField("m_rightItem", BindingFlags.NonPublic | BindingFlags.Instance);
            ItemDrop.ItemData item = f != null ? f.GetValue(p) as ItemDrop.ItemData : null;
            return item != null && item.m_dropPrefab != null && item.m_dropPrefab.name == prefab;
        }

        /// <summary>Skill level of the crop's skill for the local player (level 0 at start).</summary>
        [TestHook("ridingcrop.skillLevel")]
        private static float SkillLevel(string prefab)
        {
            Player p = Player.m_localPlayer;
            if (p == null)
            {
                throw new Exception("no player");
            }

            return p.GetSkills().GetSkillLevel(Shared(prefab).m_skillType);
        }

        /// <summary>Do the crops' hit effects carry our slap sound (sounds feature on, clip in the bundle)?</summary>
        [TestHook("ridingcrop.hasSlap")]
        private static bool HasSlap(string prefab)
        {
            UnityEngine.GameObject slap = CropSounds.SlapPrefab;
            EffectList list = Shared(prefab).m_hitEffect;
            return slap != null && list != null && list.m_effectPrefabs != null && list.m_effectPrefabs.Any(e => e != null && e.m_prefab == slap);
        }
    }
}
