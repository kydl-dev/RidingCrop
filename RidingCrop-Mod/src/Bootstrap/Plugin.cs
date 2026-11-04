using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using KydlCore.Common;
using RidingCrop.Bootstrap;

namespace RidingCrop;

/// <summary>
/// Adds seven craftable one-handed weapons, the "Riding Crop" family (a weakest-to-strongest tier ladder), their own skill and sounds.
/// Order in Awake matters: log first, then the config, then the features (the skill must exist before the game creates its Skills object).
/// </summary>
[BepInPlugin(ModGuid, ModName, ModVersion)]
[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency(KydlCore.Bootstrap.Plugin.ModGuid, "0.1.0")]     // the KY hub (manifest: KydlDev-KydlCore-0.1.0, same version)
// New items = everyone on a server needs the mod with the exact same version, otherwise they can't see or hold them.
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
public class Plugin : BaseUnityPlugin
{
    // ModGuid never changes (it is the config file name: BepInEx/config/com.kydldev.RidingCrop.cfg).
    // ModVersion = manifest.json version_number = newest CHANGELOG heading (tests check it).
    public const string ModGuid = "com.kydldev.RidingCrop";
    public const string ModName = "RidingCrop";
    public const string ModVersion = "2.0.0";

    private Harmony _harmony;
    private ConfigReloader _cfgWatcher;
    private ModInstaller _installer;

    /// <summary>True until Awake is done. Items and the skill can only be added while it is true (the game cannot take them later).</summary>
    internal static bool InStartup { get; private set; } = true;

    private void Awake()
    {
        ModLog.Init(Logger, ModName, () => ModSettings.DebugOn);
        ModLog.Info(ModName + " " + ModVersion + " loaded (dll built " + File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("yyyy-MM-dd HH:mm:ss") + ")");

        ModSettings.Bind(Config);
        ConfigMigration.Run(Config);
        _cfgWatcher = new ConfigReloader(Config);

        _installer = new ModInstaller(Config);
        bool installed = _installer.Install();
        Config.SettingChanged += (s, e) => { if (_installer != null) _installer.Refresh(); };
        ModLog.Verbose("Plugin: ModInstaller " + (installed ? "installed" : "reported a problem"));

        _harmony = new Harmony(ModGuid);
        bool patched = PatchInstaller.Apply(_harmony, Assembly.GetExecutingAssembly());
        ModLog.Info(ModName + " ready" + (patched ? "" : " (patching failed, see the log)"));
        InStartup = false;
    }

    private void Update()
    {
        if (_cfgWatcher != null) _cfgWatcher.Tick();
    }

    private void OnDestroy()
    {
        if (_cfgWatcher != null) { _cfgWatcher.Dispose(); _cfgWatcher = null; }
        if (_installer != null) { _installer.Uninstall(); _installer = null; }
        if (_harmony != null) _harmony.UnpatchSelf();
        ModLog.Verbose("Plugin: destroyed (patches removed)");
    }
}
