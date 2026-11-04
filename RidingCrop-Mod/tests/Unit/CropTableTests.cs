using System.Linq;
using RidingCrop;

namespace RidingCrop.Tests.Unit;

/// <summary>The crop list: ids, recipes and stations.</summary>
[TestFixture]
public class CropTableTests
{
    [Test]
    public void SevenCrops_WithUniqueIdsAndTextures()
    {
        Assert.That(CropTable.All.Length, Is.EqualTo(7));
        Assert.That(CropTable.All.Select(c => c.PrefabName).Distinct().Count(), Is.EqualTo(7));
        Assert.That(CropTable.All.Select(c => c.AlbedoName).Distinct().Count(), Is.EqualTo(7));
        Assert.That(CropTable.All[0].PrefabName, Is.EqualTo("RidingCrop"));
    }

    [Test]
    public void EveryCrop_HasThreeIngredients_AndAUsableUpgradeCost()
    {
        foreach (CropDef c in CropTable.All)
        {
            Assert.That(c.Ingredients.Length, Is.EqualTo(3), c.PrefabName);
            foreach (CropIngredient i in c.Ingredients)
            {
                Assert.That(i.UpgradeAmount, Is.GreaterThanOrEqualTo(1), c.PrefabName + " " + i.Item);
            }
        }
    }

    [TestCase(10, 5)]
    [TestCase(5, 3)]
    [TestCase(2, 1)]
    [TestCase(1, 1)]
    public void UpgradeAmount_IsHalfRoundedUp(int amount, int want)
    {
        Assert.That(new CropIngredient("X", amount).UpgradeAmount, Is.EqualTo(want));
    }

    [Test]
    public void Stations_ClimbWithTheTier()
    {
        Assert.That(CropTable.All[0].Station, Is.EqualTo(CropStation.Workbench));
        Assert.That(CropTable.All[2].Station, Is.EqualTo(CropStation.Forge));
        Assert.That(CropTable.All[5].Station, Is.EqualTo(CropStation.BlackForge));
        Assert.That(CropTable.VanillaBasePrefab, Is.EqualTo("Club"));
    }
}
