# Riding crop asset for Valheim: status and next steps

## Done
- Model: one mesh `RidingCrop`, 1.0 m long, about 392 verts / 748 tris, Z up, origin at the grip.
  Parts: gold end cap, handle, gold ferrule, keeper band with gold button, wrist loop, shaft, whipping, folded slapper tab.
- Slapper made taller (about 91 mm before the scale-up, was 66 mm); width unchanged.
- UVs: one UV layer `UVMap`, a single 1024x1024 atlas, all UVs inside 0..1. Layout is in `REG` (identical in both scripts).
- Textures (generated procedurally, `textures/`):
  - `crop_Albedo.png`    sRGB colour: black leather, braided handle and shaft, thread whipping, polished gold.
  - `crop_Normal.png`    tangent-space normal, +Y up (Unity convention). The braid and whipping live here.
  - `crop_MetalGloss.png` Unity Standard packing: R = metallic, A = smoothness.
- Blender material `crop_mat` wires the three maps together for preview.

## Not done / untested
- FBX exported (see below) but not imported into Unity 6000.0.75f1 and not tested in Valheim.
- The BepInEx/Jotunn plugin is not written: I could not read the mesh-replacer template's C# source with the sources
  I am allowed to use. Send me its main .cs file and the vanilla item you want to replace and I will adapt it.
- Valheim's own item shader (Custom/Creature) may expect different map packing. Compare with a vanilla item's
  textures once the project is open; if the gloss looks wrong, only `crop_MetalGloss.png` needs repacking.
- If lighting on the braid looks inverted in Unity, flip the green channel of `crop_Normal.png`.
- No built asset bundle, icon or recipe yet. `unity/Editor/BuildRidingCropBundle.cs` is untested.
- Textures are a clean procedural first pass: no hand-painted wear, no stitching, no emissive/AO.

## Files
- `build_riding_crop.py`: rebuilds the mesh, UVs and material in Blender 5.x (run after the textures exist).
- `crop_textures.py`: writes the three PNGs (numpy only). `python crop_textures.py "<blend folder>/textures"`.
- `textures/`: the generated PNGs.

## FBX export (done, on your machine)
`<blend folder>/export/RidingCrop.fbx`: selected mesh only, Forward -Z, Up Y, FBX Units Scale, Apply Transform on,
object moved to the world origin for the export so the grip is the pivot (location restored afterwards).

## Unity side (follows the mesh-replacer template's bundle layout)
1. Copy `RidingCrop.fbx` and the three PNGs into `Assets/RidingCrop/` in a Unity 6000.0.75f1 project.
2. Put `unity/Editor/BuildRidingCropBundle.cs` in `Assets/Editor/` and run Valheim > Build Riding Crop Bundle.
   It sets import types (normal map; MetalGloss linear), makes standalone mesh + material assets, assigns both to the
   lower-case bundle `ridingcrop`, and builds to `<project>/AssetBundles/`. Check the console line with the mesh bounds.
   Manual route: duplicate the mesh and material out of the FBX (Ctrl+D), assign textures, set the bundle name, build
   with the Asset Bundle Browser.
3. Plugin: load the bundle and swap the mesh and material onto a cloned vanilla item (needs the template's C#).
4. In game: check scale and how it sits in the hand. The mesh may need rotating or re-pivoting to match the vanilla item.
