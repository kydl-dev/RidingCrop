using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KydlCore.Common;
using UnityEngine;

namespace RidingCrop
{
    /// <summary>The mod's AssetBundle (mesh, one colour texture per crop, normal / metal maps, the two sounds) read into memory.</summary>
    internal static class CropAssets
    {
        /// <summary>Bundle name: must match the name made in Unity (lower case).</summary>
        internal const string BundleName = "ridingcrop";

        // Clip names are matched by "contains" (case-insensitive) against the audio clips found in the bundle.
        private const string WhooshClipKey = "whoosh";
        private const string SlapClipKey = "slap";

        internal static Mesh Mesh { get; private set; }

        internal static Texture2D Normal { get; private set; }

        internal static Texture2D MetalGloss { get; private set; }

        internal static AudioClip WhooshClip { get; private set; }

        internal static AudioClip SlapClip { get; private set; }

        /// <summary>Colour textures by name.</summary>
        internal static Dictionary<string, Texture2D> Albedos { get; } = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        /// <summary>True when the bundle was read and holds at least a mesh and one colour texture.</summary>
        internal static bool Loaded { get { return Mesh != null && Albedos.Count > 0; } }

        /// <summary>Reads the bundle (embedded in the DLL, else the file next to it) and keeps the assets we need.</summary>
        /// <returns>True when the mesh and at least one colour texture were found.</returns>
        internal static bool Load()
        {
            AssetBundle bundle = OpenBundle();
            if (bundle == null)
            {
                return false;
            }

            try
            {
                Mesh = bundle.LoadAllAssets<Mesh>().FirstOrDefault();
                Texture2D[] textures = bundle.LoadAllAssets<Texture2D>();
                foreach (Texture2D t in textures)
                {
                    if (t.name.IndexOf("Albedo", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Albedos[t.name] = t;
                    }
                }

                Normal = textures.FirstOrDefault(t => t.name.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0);
                MetalGloss = textures.FirstOrDefault(t => t.name.IndexOf("MetalGloss", StringComparison.OrdinalIgnoreCase) >= 0);

                AudioClip[] clips = bundle.LoadAllAssets<AudioClip>();
                WhooshClip = clips.FirstOrDefault(c => c.name.IndexOf(WhooshClipKey, StringComparison.OrdinalIgnoreCase) >= 0);
                SlapClip = clips.FirstOrDefault(c => c.name.IndexOf(SlapClipKey, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            catch (Exception ex)
            {
                ModLog.Error("CropAssets: exception while reading the asset bundle: " + ex);
            }
            finally
            {
                // false = keep the assets we already loaded, just release the bundle file.
                bundle.Unload(false);
            }

            ModLog.Verbose("CropAssets: bundle contents -> mesh: " + (Mesh != null ? Mesh.name : "MISSING")
                + ", albedo textures: " + string.Join(", ", Albedos.Keys.OrderBy(k => k).ToArray())
                + ", normal: " + (Normal != null ? Normal.name : "MISSING") + ", metal/gloss: " + (MetalGloss != null ? MetalGloss.name : "MISSING"));
            foreach (CropDef crop in CropTable.All)
            {
                if (!Albedos.ContainsKey(crop.AlbedoName))
                {
                    ModLog.Error("CropAssets: texture '" + crop.AlbedoName + "' for '" + crop.PrefabName + "' is MISSING from the bundle (rebuild it in Unity: Valheim > Build Riding Crop Bundle). That crop will NOT be added.");
                }
            }

            ModLog.Verbose("CropAssets: bundle sounds -> whoosh: " + (WhooshClip != null ? WhooshClip.name : "MISSING") + ", slap: " + (SlapClip != null ? SlapClip.name : "MISSING"));
            if (WhooshClip == null || SlapClip == null)
            {
                ModLog.Warn("CropAssets: a sound clip is missing from the bundle (rebuild it in Unity: Valheim > Build Riding Crop Bundle). The crops will work, but without their own sound for the missing clip.");
            }

            return Loaded;
        }

        // 1) embedded in the DLL (Kydl.Mod.targets embeds Assets\<bundle>; read fully into memory), 2) the file next to the DLL.
        private static AssetBundle OpenBundle()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {
                if (res != BundleName && !res.EndsWith("." + BundleName, StringComparison.Ordinal))
                {
                    continue;
                }

                using (Stream s = asm.GetManifestResourceStream(res))
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    AssetBundle b = AssetBundle.LoadFromMemory(ms.ToArray());
                    if (b != null)
                    {
                        ModLog.Verbose("CropAssets: bundle '" + BundleName + "' loaded from the DLL (" + b.GetAllAssetNames().Length + " asset(s))");
                        return b;
                    }

                    ModLog.Warn("CropAssets: the embedded bundle '" + res + "' could not be loaded");
                }
            }

            string path = Path.Combine(Path.GetDirectoryName(asm.Location) ?? string.Empty, BundleName);
            if (!File.Exists(path))
            {
                ModLog.Error("CropAssets: asset bundle not found (not embedded, not at " + path + "). The crops are NOT added.");
                return null;
            }

            AssetBundle fromFile = AssetBundle.LoadFromFile(path);
            if (fromFile == null)
            {
                ModLog.Error("CropAssets: Unity could not load the asset bundle " + path + ". Most common cause: it was built with a different Unity version than Valheim uses.");
            }
            else
            {
                ModLog.Verbose("CropAssets: bundle loaded from " + path);
            }

            return fromFile;
        }
    }
}
