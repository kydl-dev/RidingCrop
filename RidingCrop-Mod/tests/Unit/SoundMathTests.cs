using RidingCrop;

namespace RidingCrop.Tests.Unit
{
    /// <summary>The sample maths behind the crop sounds.</summary>
    [TestFixture]
    public class SoundMathTests
    {
        [Test]
        public void Peak_IsTheLargestAbsoluteSample()
        {
            Assert.That(SoundMath.Peak(new[] { 0.1f, -0.6f, 0.4f }), Is.EqualTo(0.6f).Within(0.0001));
            Assert.That(SoundMath.Peak(new float[0]), Is.EqualTo(0f));
        }

        [Test]
        public void OnsetFrame_IsTheFirstFrameAtTenPercentOfThePeak()
        {
            var data = new[] { 0.01f, 0.02f, 0.05f, 0.6f, 1f, 0.3f };   // mono, peak 1 -> onset level 0.1
            Assert.That(SoundMath.OnsetFrame(data, 1, 1f, SoundMath.OnsetFraction), Is.EqualTo(3));
        }

        [Test]
        public void OnsetFrame_DividesByChannels()
        {
            var data = new[] { 0f, 0f, 0f, 0f, 0.9f, 0.9f };   // stereo: first loud sample is index 4 = frame 2
            Assert.That(SoundMath.OnsetFrame(data, 2, 0.9f, 0.1f), Is.EqualTo(2));
        }

        [Test]
        public void StartFrame_KeepsFiveMillisecondsBeforeTheImpact_NeverNegative()
        {
            Assert.That(SoundMath.StartFrame(1000, 48000), Is.EqualTo(760));
            Assert.That(SoundMath.StartFrame(100, 48000), Is.EqualTo(0));
        }

        [Test]
        public void NormalizeGain_NeverQuietsAClip()
        {
            Assert.That(SoundMath.NormalizeGain(0.49f), Is.EqualTo(2f).Within(0.001));
            Assert.That(SoundMath.NormalizeGain(1f), Is.EqualTo(1f));
        }

        [Test]
        public void TrimAndNormalize_CutsTheLeadInAndFadesIn()
        {
            var data = new float[48000];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0.5f;
            }

            float[] output = SoundMath.TrimAndNormalize(data, 1, 48000, 1000, 1f);
            Assert.That(output.Length, Is.EqualTo(47000));
            Assert.That(output[0], Is.EqualTo(0f), "starts silent so the cut cannot click");
            Assert.That(output[200], Is.EqualTo(0.5f).Within(0.0001), "full level after the 3 ms fade");
        }

        [Test]
        public void TrimAndNormalize_ClampsToFullScale()
        {
            float[] output = SoundMath.TrimAndNormalize(new[] { 0.9f, 0.9f, 0.9f }, 1, 1000, 0, 3f);
            Assert.That(output[2], Is.EqualTo(1f));
        }

        [Test]
        public void SoftLimit_IsIdentityBelowTheKnee_AndStaysBelowOneAbove()
        {
            Assert.That(SoundMath.SoftLimit(0.5f), Is.EqualTo(0.5f));
            float high = SoundMath.SoftLimit(5f);
            Assert.That(high, Is.GreaterThan(SoundMath.LimiterKnee));
            Assert.That(high, Is.LessThanOrEqualTo(1f));
        }

        [Test]
        public void ApplyGain_ScalesKeepsTheSign_AndReportsPeaks()
        {
            var data = new[] { 0.2f, -0.4f };
            float before;
            float after;
            int limited;
            SoundMath.ApplyGain(data, 1.5f, out before, out after, out limited);
            Assert.That(data[0], Is.EqualTo(0.3f).Within(0.0001));
            Assert.That(data[1], Is.EqualTo(-0.6f).Within(0.0001));
            Assert.That(before, Is.EqualTo(0.4f).Within(0.0001));
            Assert.That(after, Is.EqualTo(0.6f).Within(0.0001));
            Assert.That(limited, Is.EqualTo(0));
        }

        [Test]
        public void ApplyGain_LimitsLoudSamples()
        {
            var data = new[] { 0.9f, -0.9f };
            float before;
            float after;
            int limited;
            SoundMath.ApplyGain(data, 1.5f, out before, out after, out limited);
            Assert.That(limited, Is.EqualTo(2));
            Assert.That(data[0], Is.LessThanOrEqualTo(1f));
            Assert.That(data[1], Is.GreaterThanOrEqualTo(-1f));
        }
    }
}
