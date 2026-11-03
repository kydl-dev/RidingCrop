using RidingCrop;

namespace RidingCrop.Tests.Unit
{
    /// <summary>The stat ladder (the numbers the README table promises).</summary>
    [TestFixture]
    public class CropBalanceTests
    {
        [TestCase(0, 20.0)]
        [TestCase(1, 28.0)]
        [TestCase(2, 39.2)]
        [TestCase(3, 54.88)]
        [TestCase(4, 76.832)]
        [TestCase(5, 107.565)]
        [TestCase(6, 150.591)]
        public void Blunt_GrowsFortyPercentPerTier(int tier, double want)
        {
            Assert.That(CropBalance.Blunt(tier), Is.EqualTo(want).Within(0.01));
        }

        [TestCase(0, 10.0)]
        [TestCase(1, 12.5)]
        [TestCase(2, 15.625)]
        [TestCase(6, 38.147)]
        public void Bleed_GrowsTwentyFivePercentPerTier(int tier, double want)
        {
            Assert.That(CropBalance.BleedPerSecond(tier), Is.EqualTo(want).Within(0.01));
        }

        [TestCase(0, 200.0)]
        [TestCase(3, 350.0)]
        [TestCase(6, 500.0)]
        public void Durability_AddsFiftyPerTier(int tier, double want)
        {
            Assert.That(CropBalance.Durability(tier), Is.EqualTo(want).Within(0.001));
        }

        [Test]
        public void UpgradeSteps_AreFlat()
        {
            Assert.That(CropBalance.BluntPerLevel, Is.EqualTo(6f).Within(0.001));
            Assert.That(CropBalance.DurabilityPerLevel, Is.EqualTo(40f).Within(0.001));
            Assert.That(CropBalance.MaxQuality, Is.EqualTo(4));
        }

        [Test]
        public void BluntAtQuality_AddsTheUpgradeStep()
        {
            Assert.That(CropBalance.BluntAtQuality(0, 1), Is.EqualTo(20f).Within(0.001));
            Assert.That(CropBalance.BluntAtQuality(0, 4), Is.EqualTo(38f).Within(0.001));
        }

        [Test]
        public void EveryTier_HasATableEntryAndRisingBlunt()
        {
            Assert.That(CropBalance.Tiers, Is.EqualTo(CropTable.All.Length));
            for (int i = 1; i < CropBalance.Tiers; i++)
            {
                Assert.That(CropBalance.Blunt(i), Is.GreaterThan(CropBalance.Blunt(i - 1)));
                Assert.That(CropBalance.Stagger(i), Is.GreaterThanOrEqualTo(CropBalance.Stagger(i - 1)));
            }
        }

        [Test]
        public void StaggerMultiplier_TimesBluntGivesTheStagger()
        {
            for (int i = 0; i < CropBalance.Tiers; i++)
            {
                Assert.That(CropBalance.StaggerMultiplier(i) * CropBalance.Blunt(i), Is.EqualTo(CropBalance.Stagger(i)).Within(0.01));
            }
        }
    }
}
