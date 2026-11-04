using System;

namespace RidingCrop;

/// <summary>Pure sample maths for the crop sounds (no Unity types): loudness gain with a soft limiter and the cut of the quiet lead-in.</summary>
internal static class SoundMath
{
    /// <summary>Samples above this level are compressed smoothly towards full scale instead of being cut off.</summary>
    internal const float LimiterKnee = 0.7f;

    /// <summary>Share of the peak level where the impact is taken to begin (10% = -20 dB).</summary>
    internal const float OnsetFraction = 0.1f;

    /// <summary>Highest absolute sample value.</summary>
    /// <param name="data">Interleaved samples.</param>
    /// <returns>The peak level, 0 for empty data.</returns>
    internal static float Peak(float[] data)
    {
        float peak = 0f;
        for (int i = 0; i < data.Length; i++)
        {
            float a = Math.Abs(data[i]);
            if (a > peak)
            {
                peak = a;
            }
        }

        return peak;
    }

    /// <summary>The first frame whose level reaches <paramref name="fraction"/> of the peak: where the impact begins.</summary>
    /// <param name="data">Interleaved samples.</param>
    /// <param name="channels">Channel count.</param>
    /// <param name="peak">The peak of the data.</param>
    /// <param name="fraction">Share of the peak (0..1).</param>
    /// <returns>The frame index, 0 when nothing reaches the level.</returns>
    internal static int OnsetFrame(float[] data, int channels, float peak, float fraction)
    {
        float level = fraction * peak;
        for (int i = 0; i < data.Length; i++)
        {
            if (Math.Abs(data[i]) >= level)
            {
                return i / channels;
            }
        }

        return 0;
    }

    /// <summary>The frame to start playback at: 5 ms before the impact.</summary>
    internal static int StartFrame(int onsetFrame, int frequency)
    {
        return Math.Max(0, onsetFrame - frequency / 200);
    }

    /// <summary>Gain that makes the clip as loud as its data allows (never below 1).</summary>
    internal static float NormalizeGain(float peak)
    {
        return Math.Max(1f, 0.98f / peak);
    }

    /// <summary>Copies the data from <paramref name="startFrame"/>, multiplied by the gain, with a 3 ms fade-in so the cut cannot click.</summary>
    /// <param name="data">Interleaved samples.</param>
    /// <param name="channels">Channel count.</param>
    /// <param name="frequency">Sample rate.</param>
    /// <param name="startFrame">First frame to keep.</param>
    /// <param name="gain">Multiplier.</param>
    /// <returns>The new interleaved samples, clamped to -1..1.</returns>
    internal static float[] TrimAndNormalize(float[] data, int channels, int frequency, int startFrame, float gain)
    {
        int frames = data.Length / channels;
        int fadeFrames = Math.Max(1, frequency * 3 / 1000);
        int outFrames = frames - startFrame;
        var output = new float[outFrames * channels];
        for (int f = 0; f < outFrames; f++)
        {
            float fade = f < fadeFrames ? (float)f / fadeFrames : 1f;
            for (int c = 0; c < channels; c++)
            {
                float v = data[(startFrame + f) * channels + c] * gain * fade;
                output[f * channels + c] = Math.Max(-1f, Math.Min(1f, v));
            }
        }

        return output;
    }

    /// <summary>Soft limiter: levels above the knee are bent towards 1 with a tanh curve.</summary>
    /// <param name="amplified">An absolute (positive) level.</param>
    /// <returns>The limited level.</returns>
    internal static float SoftLimit(float amplified)
    {
        if (amplified <= LimiterKnee)
        {
            return amplified;
        }

        float headroom = 1f - LimiterKnee;
        return LimiterKnee + headroom * (float)Math.Tanh((amplified - LimiterKnee) / headroom);
    }

    /// <summary>Multiplies every sample by <paramref name="gain"/> and soft-limits the loud ones, in place.</summary>
    /// <param name="data">Interleaved samples (changed).</param>
    /// <param name="gain">Multiplier (1.5 = 50% louder).</param>
    /// <param name="peakBefore">Peak before the gain.</param>
    /// <param name="peakAfter">Peak after gain and limiter.</param>
    /// <param name="limited">How many samples the limiter touched.</param>
    internal static void ApplyGain(float[] data, float gain, out float peakBefore, out float peakAfter, out int limited)
    {
        peakBefore = 0f;
        peakAfter = 0f;
        limited = 0;
        for (int i = 0; i < data.Length; i++)
        {
            float a = Math.Abs(data[i]);
            if (a > peakBefore)
            {
                peakBefore = a;
            }

            float amplified = a * gain;
            if (amplified > LimiterKnee)
            {
                limited++;
                amplified = SoftLimit(amplified);
            }

            if (amplified > peakAfter)
            {
                peakAfter = amplified;
            }

            data[i] = data[i] < 0f ? -amplified : amplified;
        }
    }
}
