using Jotunn.Managers;
using KydlCore.Common;
using KydlCore.Core;

namespace RidingCrop;

/// <summary>
/// The seven crops and their skill. Gate: EnableAll and [Crops] Enable. Items and the skill cannot be added or removed while the game runs,
/// so they are registered once during startup; a switch changed later only changes <see cref="Running"/> (the README says: restart).
/// </summary>
internal sealed class CropsFeature : IFeature
{
    internal const string FeatureId = "crops";

    private static bool _registered;

    /// <summary>True between Start and Stop.</summary>
    internal static bool Running { get; private set; }

    public string Id { get { return FeatureId; } }

    public bool IsEnabled() { return ModSettings.CropsOn; }

    public bool Start()
    {
        if (!_registered)
        {
            if (!Plugin.InStartup)
            {
                ModLog.Warn("CropsFeature: switched on after startup - the crops and the skill are added at the next game start");
                Running = true;
                return true;
            }

            _registered = true;
            if (!CropAssets.Load())
            {
                ModLog.Error("CropsFeature: the crops were NOT added because the asset bundle could not be loaded");
                return false;
            }

            // Skills must be registered before the game creates its Skills object, so do it here and not in the prefab event.
            CropSkill.Register();

            // Vanilla prefabs only exist once the game has loaded them, so we wait for this event.
            PrefabManager.OnVanillaPrefabsAvailable += CropItems.AddAll;
        }

        Running = true;
        ModLog.Verbose("CropsFeature: started");
        return Running;
    }

    public bool Stop()
    {
        Running = false;
        ModLog.Verbose("CropsFeature: stopped (items and skill stay until the game restarts)");
        return !Running;
    }
}
