using System.Linq;
using RidingCrop;

namespace RidingCrop.Tests.Unit;

/// <summary>The recipes and texts the README promises, crop by crop (saved games and the build menu rely on them).</summary>
[TestFixture]
public class CropRecipeDataTests
{
    private static readonly object[] Recipes =
    {
        new object[] { 0, "RidingCrop", "Do I boar you?", "crop_Albedo", "Workbench", 1, "LeatherScraps:10,Resin:2,Wood:5" },
        new object[] { 1, "RidingCrop_V1", "Dear, oh deer!", "crop_Albedo_V1", "Workbench", 2, "DeerHide:10,Bronze:2,FineWood:5" },
        new object[] { 2, "RidingCrop_V3", "Bear with me!", "crop_Albedo_V3", "Forge", 1, "BjornHide:10,SurtlingCore:2,RoundLog:5" },
        new object[] { 3, "RidingCrop_V4", "Howl yeah!", "crop_Albedo_V4", "Forge", 2, "WolfPelt:10,Obsidian:2,FineWood:5" },
        new object[] { 4, "RidingCrop_V6", "Loxy Poxy", "crop_Albedo_V6", "Forge", 3, "LoxPelt:10,Tar:2,ElderBark:5" },
        new object[] { 5, "RidingCrop_V2", "Askvin worthy", "crop_Albedo_V2", "BlackForge", 1, "AskHide:10,CharredBone:2,Blackwood:5" },
        new object[] { 6, "RidingCrop_V5", "Seal the deal!", "crop_Albedo_V5", "BlackForge", 2, "MooseHide:10,SealHide:2,Frostwood:5" },
    };

    [TestCaseSource(nameof(Recipes))]
    public void Crop_HasTheDocumentedIdsStationAndRecipe(int tier, string prefab, string display, string albedo, string station, int level, string recipe)
    {
        CropDef c = CropTable.All[tier];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(c.PrefabName, Is.EqualTo(prefab));
            Assert.That(c.DisplayName, Is.EqualTo(display));
            Assert.That(c.AlbedoName, Is.EqualTo(albedo));
            Assert.That(c.Station, Is.EqualTo((CropStation)System.Enum.Parse(typeof(CropStation), station)));
            Assert.That(c.StationLevel, Is.EqualTo(level));
            Assert.That(string.Join(",", c.Ingredients.Select(i => i.Item + ":" + i.Amount)), Is.EqualTo(recipe));
        }
    }

    [Test]
    public void EveryCrop_HasADescription_AndOnlyTheFirstOnesAreNotUpgrades()
    {
        foreach (CropDef c in CropTable.All)
        {
            Assert.That(c.Description, Has.Length.GreaterThan(20), c.PrefabName);
        }
        Assert.That(CropTable.All[0].Description, Does.Contain("gold-capped handle"));
        Assert.That(CropTable.All[6].Description, Does.Contain("finest of the family"));
        for (int i = 1; i < 6; i++)
        {
            Assert.That(CropTable.All[i].Description, Does.EndWith("Hits harder than the crop before it."), CropTable.All[i].PrefabName);
        }
    }

    [Test]
    public void Stats_PerTier_MatchTheReadmeTable()
    {
        float[] knock = { 30f, 60f, 80f, 100f, 100f, 100f, 100f };
        float[] stamina = { 6f, 8f, 10f, 12f, 14f, 16f, 18f };
        float[] armor = { 3f, 12f, 21f, 30f, 39f, 57f, 66f };
        float[] force = { 20f, 30f, 30f, 30f, 30f, 30f, 30f };
        float[] stagger = { 12f, 25f, 35f, 55f, 95f, 125f, 150f };
        for (int t = 0; t < 7; t++)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(CropBalance.Knockback(t), Is.EqualTo(knock[t]), "knockback " + t);
                Assert.That(CropBalance.Stamina(t), Is.EqualTo(stamina[t]), "stamina " + t);
                Assert.That(CropBalance.BlockArmor(t), Is.EqualTo(armor[t]), "armor " + t);
                Assert.That(CropBalance.BlockForce(t), Is.EqualTo(force[t]), "force " + t);
                Assert.That(CropBalance.Stagger(t), Is.EqualTo(stagger[t]), "stagger " + t);
            }
        }
    }
}
