// One-click asset bundle builder for the Riding Crop.
//
// In Unity use the menu:   Valheim > Build Riding Crop Bundle (Windows)
//
// What it does, so you don't have to do any of it by hand:
//   1. Sets the import settings of the three textures (colour / normal map / linear data).
//   2. Pulls the mesh out of RidingCrop.fbx into its own standalone asset.
//   3. Finds the two sound clips (whoosh_sound / slap_sound, anywhere under Assets/), sets them up for 3D game
//      sound (mono, decompressed on load) and adds them to the bundle.
//   4. Puts the mesh + the textures (original + every crop_Albedo_V*.png colour variant) + 2 sounds into one asset bundle called "ridingcrop" (bundle names must be lower case).
//   5. Builds the bundle and copies it to  <repo>/RidingCrop-Mod/Assets/ridingcrop  (the mod build embeds it in RidingCrop.dll).
//
// The material is NOT built here: the plugin makes it at runtime from Valheim's own item shader, because a normal
// Unity material would usually show up pink inside Valheim.
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildRidingCropBundle
{
    const string Dir = "Assets/RidingCrop";
    const string BundleName = "ridingcrop";

    [MenuItem("Valheim/Build Riding Crop Bundle (Windows)")]
    public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64);

    // Only needed if you run Valheim natively on Linux AND installed the "Linux Build Support (Mono)" module in the Hub.
    [MenuItem("Valheim/Build Riding Crop Bundle (Linux)")]
    public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64);

    static void Build(BuildTarget target)
    {
        AssetDatabase.Refresh();

        string fbx = $"{Dir}/RidingCrop.fbx";
        string albedo = $"{Dir}/crop_Albedo.png";
        string normal = $"{Dir}/crop_Normal.png";
        string metal = $"{Dir}/crop_MetalGloss.png";
        foreach (var p in new[] { fbx, albedo, normal, metal })
        {
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(p)))
            {
                Debug.LogError($"[RidingCrop] Missing file: {p}. Put it in {Dir}/ and try again.");
                return;
            }
        }

        // The two sound clips can live in any folder under Assets/; they are found by name.
        string whoosh = FindClip("whoosh_sound");
        string slap = FindClip("slap_sound");
        if (whoosh == null || slap == null)
        {
            Debug.LogError("[RidingCrop] Could not find whoosh_sound and/or slap_sound (an audio file anywhere under Assets/). " +
                           $"whoosh: {whoosh ?? "MISSING"}, slap: {slap ?? "MISSING"}");
            return;
        }

        // The colour variants (crop_Albedo_V1.png ... crop_Albedo_V6.png): every one found in Assets/RidingCrop goes in the bundle.
        string[] variantAlbedos = AssetDatabase.FindAssets("crop_Albedo_V t:Texture2D", new[] { Dir })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => Path.GetFileNameWithoutExtension(p).StartsWith("crop_Albedo_V"))
            .OrderBy(p => p)
            .ToArray();
        Debug.Log($"[RidingCrop] Colour variants found: {variantAlbedos.Length} ({string.Join(", ", variantAlbedos.Select(Path.GetFileNameWithoutExtension))})");
        if (variantAlbedos.Length == 0)
            Debug.LogWarning("[RidingCrop] No crop_Albedo_V*.png found in " + Dir + ": all crops will look like the original.");

        // 1. Texture import settings
        foreach (var v in variantAlbedos) SetTexture(v, TextureImporterType.Default, srgb: true);
        SetTexture(albedo, TextureImporterType.Default, srgb: true);
        SetTexture(normal, TextureImporterType.NormalMap, srgb: false);
        SetTexture(metal, TextureImporterType.Default, srgb: false); // R = metallic, A = smoothness: data, not colour

        // 2. Mesh out of the FBX
        AssetDatabase.ImportAsset(fbx, ImportAssetOptions.ForceUpdate);
        Mesh src = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Mesh>().OrderByDescending(m => m.vertexCount).FirstOrDefault();
        if (src == null) { Debug.LogError("[RidingCrop] No mesh found inside RidingCrop.fbx."); return; }

        Vector3 size = src.bounds.size;
        Debug.Log($"[RidingCrop] Mesh '{src.name}': {src.vertexCount} verts, size {size} (expected about 0.11 x 1.0 x 0.04 metres).");
        if (size.y < 0.5f || size.y > 2f)
            Debug.LogWarning("[RidingCrop] Mesh size is not about 1 metre long. The FBX import scale is probably wrong " +
                             "(select RidingCrop.fbx > Model tab > Scale Factor / Convert Units).");

        string meshPath = $"{Dir}/RidingCropMesh.asset";
        AssetDatabase.DeleteAsset(meshPath);
        var meshCopy = Object.Instantiate(src);
        meshCopy.name = "RidingCropMesh";
        AssetDatabase.CreateAsset(meshCopy, meshPath);

        // 3. Sounds
        SetSound(whoosh);
        SetSound(slap);
        Debug.Log($"[RidingCrop] Sounds: whoosh = {whoosh}, slap = {slap}");

        // 4. Everything that goes in the bundle
        foreach (var p in new[] { meshPath, albedo, normal, metal, whoosh, slap }.Concat(variantAlbedos))
            AssetImporter.GetAtPath(p).assetBundleName = BundleName;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 5. Build
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outDir = Path.Combine(projectRoot, "AssetBundles");
        if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
        Directory.CreateDirectory(outDir);

        var manifest = BuildPipeline.BuildAssetBundles(outDir, BuildAssetBundleOptions.ChunkBasedCompression, target);
        string built = Path.Combine(outDir, BundleName);
        if (manifest == null || !File.Exists(built))
        {
            Debug.LogError("[RidingCrop] Bundle build FAILED. Read the red errors above this one in the Console.");
            return;
        }

        string repoBuild = Path.GetFullPath(Path.Combine(projectRoot, "..", "RidingCrop-Mod", "Assets"));
        string finalPath = built;
        if (Directory.Exists(repoBuild))
        {
            finalPath = Path.Combine(repoBuild, BundleName);
            File.Copy(built, finalPath, true);
        }

        Debug.Log($"[RidingCrop] DONE. Bundle: {finalPath}  ({new FileInfo(finalPath).Length / 1024} KB, Unity {Application.unityVersion}, {target})");
        EditorUtility.RevealInFinder(finalPath);
    }

    static string FindClip(string name)
    {
        foreach (var guid in AssetDatabase.FindAssets($"{name} t:AudioClip", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == name) return path;
        }
        return null;
    }

    static void SetTexture(string path, TextureImporterType type, bool srgb)
    {
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = type;
        ti.sRGBTexture = srgb;
        ti.maxTextureSize = 1024;
        ti.mipmapEnabled = true;
        ti.SaveAndReimport();
    }

    // Short sound effects for a 3D AudioSource: mono (so the game can place them in space), fully decompressed in memory
    // (so nothing is streamed from the bundle file, which the plugin closes right after loading).
    static void SetSound(string path)
    {
        var ai = (AudioImporter)AssetImporter.GetAtPath(path);
        ai.forceToMono = true;
        ai.loadInBackground = false;
        var settings = ai.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.7f;
        settings.preloadAudioData = true;
        ai.defaultSampleSettings = settings;
        ai.SaveAndReimport();
    }
}
