using Kydl.Common;
using Kydl.Core.Features;

namespace RidingCrop
{
    /// <summary>
    /// The crops' own swing and hit sounds. Gate: EnableAll and [Sound] Enable. The sounds are put on the items while they are created at
    /// startup, so a switch changed later only changes <see cref="Running"/> (the README says: restart).
    /// </summary>
    internal sealed class SoundsFeature : IFeature
    {
        internal const string FeatureId = "sounds";

        /// <summary>True between Start and Stop.</summary>
        internal static bool Running { get; private set; }

        public string Id { get { return FeatureId; } }

        public bool IsEnabled() { return ModSettings.SoundsOn; }

        public bool Start()
        {
            Running = true;
            ModLog.Verbose("SoundsFeature: started");
            return Running;
        }

        public bool Stop()
        {
            Running = false;
            ModLog.Verbose("SoundsFeature: stopped (the sounds on the items change at the next game start)");
            return !Running;
        }
    }
}
