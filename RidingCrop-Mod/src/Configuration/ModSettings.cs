using BepInEx.Configuration;

namespace RidingCrop
{
    /// <summary>
    /// Player config (BepInEx entries, bound once from Plugin.Awake). All item stats are fixed by the mod (identical for everybody on a server),
    /// so the config only holds the switches and the sound options. Two functionalities, each with its own switch: [Crops] and [Sound].
    /// </summary>
    internal static class ModSettings
    {
        // [General]
        public static ConfigEntry<bool> EnableAll;

        // [Crops]
        public static ConfigEntry<bool> CropsEnable;

        // [Sound]
        public static ConfigEntry<bool> SoundEnable;
        public static ConfigEntry<float> WhooshVolume;
        public static ConfigEntry<float> SlapVolume;
        public static ConfigEntry<bool> ReplaceVanillaSounds;
        public static ConfigEntry<bool> SlapNormalize;

        // [DebugLog]
        public static ConfigEntry<bool> DebugLog;

        /// <summary>Is the mod switched on ([General] EnableAll)?</summary>
        public static bool On { get { return EnableAll == null || EnableAll.Value; } }

        /// <summary>Are the crops (items and skill) on? EnableAll and [Crops] Enable.</summary>
        public static bool CropsOn { get { return Functions.IsOn(On, CropsEnable == null || CropsEnable.Value); } }

        /// <summary>Do the crops use their own sounds? EnableAll and [Sound] Enable.</summary>
        public static bool SoundsOn { get { return Functions.IsOn(On, SoundEnable == null || SoundEnable.Value); } }

        /// <summary>Is the detailed log on?</summary>
        public static bool DebugOn { get { return DebugLog != null && DebugLog.Value; } }

        /// <summary>Binds every entry. Called once, first thing after the log is ready.</summary>
        /// <param name="config">The plugin's config file.</param>
        internal static void Bind(ConfigFile config)
        {
            EnableAll = config.Bind("General", "EnableAll", true,
                "Switches the whole mod on or off. Needs a game restart: the items and the skill are added when the game starts. " +
                "While it is off the Riding Crop items are unknown to the game, so keep it on for any world or character that uses them, and use the same value as the server.");

            CropsEnable = config.Bind("Crops", "Enable", true,
                "Adds the seven Riding Crops and their RidingCrop skill. Needs a game restart. Use the same value as the server.");

            SoundEnable = config.Bind("Sound", "Enable", true,
                "The crops use their own swing and hit sounds. Off keeps the Club's sounds. Needs a game restart.");

            WhooshVolume = config.Bind("Sound", "WhooshVolume", 1f,
                new ConfigDescription("Volume of the swing (whoosh) sound. Needs a game restart.", new AcceptableValueRange<float>(0f, 1f)));

            SlapVolume = config.Bind("Sound", "SlapVolume", 1f,
                new ConfigDescription("Volume of the slap sound when the crop hits something. Needs a game restart.", new AcceptableValueRange<float>(0f, 1f)));

            ReplaceVanillaSounds = config.Bind("Sound", "ReplaceVanillaSounds", true,
                "On = the crops only make their own swing and hit sounds. Off = the Club's sounds play as well. Needs a game restart.");

            SlapNormalize = config.Bind("Sound", "SlapNormalize", true,
                "On = when the game starts, cut the quiet lead-in from the start of the slap sound so the impact is heard the moment you hit " +
                "(the slap file has roughly 50-70 ms of build-up before the impact), and raise the sound to the loudest level its data allows. " +
                "Off = use the sound exactly as it is in the asset bundle. Needs a game restart.");

            DebugLog = config.Bind("DebugLog", "Enable", false,
                "Writes detailed lines to the BepInEx log (asset bundle contents, every crop's stats, recipes and stations, sound setup, " +
                "every whoosh, slap and bleed event). Can be changed while the game runs. For troubleshooting only.");
        }
    }
}
