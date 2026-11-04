using RidingCrop;

namespace RidingCrop.Tests.Unit;

/// <summary>Boundaries and channel layouts of the sample maths.</summary>
[TestFixture]
public class SoundMathEdgeTests
{
    [Test]
    public void OnsetFrame_UsesTheFractionOfThePeak()
    {
        var data = new[] { 0.1f, 0.3f, 0.5f };   // peak 0.5, 10% -> 0.05 (a division would give 0.2)
        Assert.That(SoundMath.OnsetFrame(data, 1, 0.5f, 0.1f), Is.EqualTo(0));
    }

    [Test]
    public void OnsetFrame_AcceptsASampleExactlyAtTheLevel()
    {
        var data = new[] { 0.1f, 0.5f };
        Assert.That(SoundMath.OnsetFrame(data, 1, 1f, 0.5f), Is.EqualTo(1));
    }

    [Test]
    public void OnsetFrame_NothingReachesTheLevel_ReturnsZero()
    {
        Assert.That(SoundMath.OnsetFrame(new[] { 0.1f, 0.2f }, 1, 1f, 2f), Is.EqualTo(0));
    }

    [Test]
    public void TrimAndNormalize_Stereo_KeepsFramesAndChannelsApart()
    {
        // 4 frames x 2 channels, left = 0.1 * (frame + 1), right = -0.1 * (frame + 1)
        var data = new[] { 0.1f, -0.1f, 0.2f, -0.2f, 0.3f, -0.3f, 0.4f, -0.4f };
        float[] output = SoundMath.TrimAndNormalize(data, 2, 1000, 1, 1f);   // 1000 Hz -> 3-frame fade
        Assert.That(output, Has.Length.EqualTo(6));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(output[0], Is.EqualTo(0f).Within(0.0001));
            Assert.That(output[1], Is.EqualTo(0f).Within(0.0001));
            Assert.That(output[2], Is.EqualTo(0.3f / 3f).Within(0.0001), "frame 1 left, one third of the fade");
            Assert.That(output[3], Is.EqualTo(-0.3f / 3f).Within(0.0001), "frame 1 right");
            Assert.That(output[4], Is.EqualTo(0.4f * 2f / 3f).Within(0.0001), "frame 2 left, two thirds");
            Assert.That(output[5], Is.EqualTo(-0.4f * 2f / 3f).Within(0.0001), "frame 2 right");
        }
    }

    [Test]
    public void TrimAndNormalize_FadeIsThreeMilliseconds_AndNeverShorterThanOneFrame()
    {
        var data = new float[10];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 1f;
        }

        float[] slow = SoundMath.TrimAndNormalize(data, 1, 2000, 0, 1f);   // 6 fade frames
        float[] tiny = SoundMath.TrimAndNormalize(data, 1, 100, 0, 1f);    // 0.3 -> at least 1 frame
        using (Assert.EnterMultipleScope())
        {
            Assert.That(slow[3], Is.EqualTo(0.5f).Within(0.0001));
            Assert.That(slow[6], Is.EqualTo(1f).Within(0.0001));
            Assert.That(tiny[0], Is.EqualTo(0f).Within(0.0001));
            Assert.That(tiny[1], Is.EqualTo(1f).Within(0.0001));
        }
    }

    [Test]
    public void SoftLimit_FollowsTheTanhCurveAboveTheKnee()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(SoundMath.SoftLimit(SoundMath.LimiterKnee), Is.EqualTo(SoundMath.LimiterKnee).Within(0.0001));
            Assert.That(SoundMath.SoftLimit(1f), Is.EqualTo(0.7f + 0.3f * (float)System.Math.Tanh(1.0)).Within(0.0001));
            Assert.That(SoundMath.SoftLimit(0.85f), Is.EqualTo(0.7f + 0.3f * (float)System.Math.Tanh(0.5)).Within(0.0001));
        }
    }

    [Test]
    public void ApplyGain_ASampleExactlyAtTheKnee_IsNotCountedAsLimited()
    {
        var data = new[] { SoundMath.LimiterKnee };
        float before;
        float after;
        int limited;
        SoundMath.ApplyGain(data, 1f, out before, out after, out limited);
        Assert.That(limited, Is.EqualTo(0));
    }
}
