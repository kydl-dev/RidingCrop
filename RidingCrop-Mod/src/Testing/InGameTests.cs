using System.Linq;
using KydlCore.Common;

namespace RidingCrop.Testing
{
    /// <summary>
    /// In-game tests run by the user's Kydl.GameProbe (dev tool, never a dependency: it finds this class by its NAME `InGameTests`).
    ///   Startup_*  run when the main menu is reached;   World_*  run 5 s after a world is loaded.
    /// Each returns "PASS: ..." / "FAIL: ..." / "SKIP: ..." with the numbers that decided it. Read-only: nothing is changed.
    /// </summary>
    internal static class InGameTests
    {
        private static string Startup_ConfigBound()
        {
            if (ModSettings.EnableAll == null || ModSettings.DebugLog == null)
            {
                return "FAIL: ModSettings.Bind did not run (EnableAll / DebugLog null)";
            }

            string r = ModSettings.On
                ? "PASS: EnableAll on, crops " + ModSettings.CropsOn + ", sounds " + ModSettings.SoundsOn + ", debug log " + ModSettings.DebugLog.Value
                : "SKIP: EnableAll is off in the config";
            ModLog.Verbose("InGameTests: Startup_ConfigBound -> " + r);
            return r;
        }

        private static string Startup_BundleLoaded()
        {
            if (!ModSettings.CropsOn)
            {
                return "SKIP: crops are off";
            }

            string r = CropAssets.Loaded
                ? "PASS: bundle has the mesh and " + CropAssets.Albedos.Count + " colour texture(s)"
                : "FAIL: the asset bundle did not load (mesh or colour textures missing)";
            ModLog.Verbose("InGameTests: Startup_BundleLoaded -> " + r);
            return r;
        }

        private static string World_AllCropsRegistered()
        {
            if (ObjectDB.instance == null)
            {
                return "SKIP: no world";
            }

            if (!ModSettings.CropsOn)
            {
                return "SKIP: crops are off";
            }

            string[] missing = CropTable.All.Where(c => ObjectDB.instance.GetItemPrefab(c.PrefabName) == null).Select(c => c.PrefabName).ToArray();
            string r = missing.Length == 0
                ? "PASS: all " + CropTable.All.Length + " crops are in the ObjectDB"
                : "FAIL: missing crop(s): " + string.Join(", ", missing);
            ModLog.Verbose("InGameTests: World_AllCropsRegistered -> " + r);
            return r;
        }

        private static string World_SkillRegistered()
        {
            if (!ModSettings.CropsOn)
            {
                return "SKIP: crops are off";
            }

            string r = CropSkill.Type != Skills.SkillType.None
                ? "PASS: skill registered as " + CropSkill.Type
                : "FAIL: the RidingCrop skill is not registered";
            ModLog.Verbose("InGameTests: World_SkillRegistered -> " + r);
            return r;
        }
    }
}
