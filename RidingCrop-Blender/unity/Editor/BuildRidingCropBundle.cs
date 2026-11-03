// Unity 6000.0.x Editor script. Put it in Assets/Editor/ of your Unity project, then use the menu
// Valheim > Build Riding Crop Bundle.
//
// Expected inputs (copy them in first):
//   Assets/RidingCrop/RidingCrop.fbx            from <blend folder>/export/
//   Assets/RidingCrop/crop_Albedo.png           from <blend folder>/textures/
//   Assets/RidingCrop/crop_Normal.png
//   Assets/RidingCrop/crop_MetalGloss.png
//
// What it does: sets the texture import types, makes a standalone mesh asset and a material asset
// (the "mesh + material in one asset bundle" layout used by the mesh-replacer template), assigns both to the
// bundle "ridingcrop" (bundle names must be lower case), and builds it into <project>/AssetBundles/.
//
// UNTESTED: written against the Unity editor API, not yet run in your project or in Valheim.
// It uses the built-in "Standard" shader. Valheim's own shader handling is done by the plugin side, which
// is not part of this script.
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildRidingCropBundle
{
    const string Dir = "Assets/RidingCrop";
    const string BundleName = "ridingcrop";

    [MenuItem("Valheim/Build Riding Crop Bundle")]
    public static void Build()
    {
        string fbx = $"{Dir}/RidingCrop.fbx";
        if (!File.Exists(fbx)) { Debug.LogError($"Missing {fbx}"); return; }

        SetTexture($"{Dir}/crop_Albedo.png", TextureImporterType.Default, true);
        SetTexture($"{Dir}/crop_Normal.png", TextureImporterType.NormalMap, false);
        SetTexture($"{Dir}/crop_MetalGloss.png", TextureImporterType.Default, false);   // linear data, not colour

        AssetDatabase.ImportAsset(fbx, ImportAssetOptions.ForceUpdate);
        Mesh src = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Mesh>().FirstOrDefault();
        if (src == null) { Debug.LogError("No mesh found inside the FBX."); return; }
        Debug.Log($"FBX mesh bounds (metres): {src.bounds.size}  (expected roughly 0.11 x 1.0 x 0.04)");

        string meshPath = $"{Dir}/RidingCropMesh.asset";
        AssetDatabase.DeleteAsset(meshPath);
        AssetDatabase.CreateAsset(Object.Instantiate(src), meshPath);

        Shader std = Shader.Find("Standard");
        if (std == null) { Debug.LogError("Built-in 'Standard' shader not found (is this a URP/HDRP project?)."); return; }
        string matPath = $"{Dir}/RidingCropMaterial.mat";
        AssetDatabase.DeleteAsset(matPath);
        var mat = new Material(std) { name = "RidingCropMaterial" };
        mat.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/crop_Albedo.png"));
        mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/crop_Normal.png"));
        mat.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/crop_MetalGloss.png"));
        mat.EnableKeyword("_NORMALMAP");
        mat.EnableKeyword("_METALLICGLOSSMAP");
        AssetDatabase.CreateAsset(mat, matPath);

        AssetImporter.GetAtPath(meshPath).assetBundleName = BundleName;
        AssetImporter.GetAtPath(matPath).assetBundleName = BundleName;
        AssetDatabase.SaveAssets();

        string outDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "AssetBundles");
        Directory.CreateDirectory(outDir);
        BuildPipeline.BuildAssetBundles(outDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        Debug.Log($"Bundle written to {outDir}/{BundleName}");
    }

    static void SetTexture(string path, TextureImporterType type, bool srgb)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogError($"Missing texture {path}"); return; }
        ti.textureType = type;
        ti.sRGBTexture = srgb;
        ti.maxTextureSize = 1024;
        ti.SaveAndReimport();
    }
}
