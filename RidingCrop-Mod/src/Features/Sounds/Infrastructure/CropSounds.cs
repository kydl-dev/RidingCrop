using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using KydlCore.Common;
using UnityEngine;

namespace RidingCrop
{
    /// <summary>
    /// Gives the crops their own swing (whoosh) and hit (slap) sounds. Valheim plays weapon sounds through "effect lists" on the item:
    /// m_triggerEffect / m_startEffect while swinging, m_hitEffect / m_hitTerrainEffect on impact. A sound effect is a small prefab with an
    /// AudioSource and a ZSFX component. A vanilla sound prefab is CLONED (so it keeps the game's mixer, 3D settings and networking), our clip
    /// is put in it, and it is swapped into each crop's effect lists. The two sound prefabs are made once and shared by all crops.
    /// </summary>
    internal static class CropSounds
    {
        /// <summary>Loudness boost on the sound data itself (AudioSource volume cannot go above 1).</summary>
        private const float SoundGain = 1.5f;

        private static bool _ready;

        /// <summary>The swing sound prefab, null until prepared (or when there is no clip / template).</summary>
        internal static GameObject WhooshPrefab { get; private set; }

        /// <summary>The hit sound prefab, null until prepared (or when there is no clip / template).</summary>
        internal static GameObject SlapPrefab { get; private set; }

        /// <summary>Puts the sounds into one crop's effect lists. Does nothing while the sounds are switched off.</summary>
        /// <param name="shared">The crop's shared item data.</param>
        internal static void Apply(ItemDrop.ItemData.SharedData shared)
        {
            if (!ModSettings.SoundsOn)
            {
                ModLog.Verbose("CropSounds: sounds are off, the crop keeps the Club's sounds.");
                return;
            }

            Prepare(shared);
            bool strip = ModSettings.ReplaceVanillaSounds.Value;

            if (WhooshPrefab != null)
            {
                bool startHadSound = HasSound(shared.m_startEffect);
                bool triggerHadSound = HasSound(shared.m_triggerEffect);
                if (strip)
                {
                    shared.m_startEffect = WithoutSounds(shared.m_startEffect);
                    shared.m_triggerEffect = WithoutSounds(shared.m_triggerEffect);
                }

                if (startHadSound)
                {
                    shared.m_startEffect = With(shared.m_startEffect, WhooshPrefab);
                }

                if (triggerHadSound || !startHadSound)
                {
                    shared.m_triggerEffect = With(shared.m_triggerEffect, WhooshPrefab);
                }
            }

            if (SlapPrefab != null)
            {
                if (strip)
                {
                    shared.m_hitEffect = WithoutSounds(shared.m_hitEffect);
                    shared.m_hitTerrainEffect = WithoutSounds(shared.m_hitTerrainEffect);
                }

                shared.m_hitEffect = With(shared.m_hitEffect, SlapPrefab);
                shared.m_hitTerrainEffect = With(shared.m_hitTerrainEffect, SlapPrefab);
            }
        }

        private static void Prepare(ItemDrop.ItemData.SharedData shared)
        {
            if (_ready)
            {
                return;
            }

            _ready = true;
            if (CropAssets.WhooshClip == null && CropAssets.SlapClip == null)
            {
                return;
            }

            LogEffects("m_startEffect", shared.m_startEffect);
            LogEffects("m_triggerEffect", shared.m_triggerEffect);
            LogEffects("m_hitEffect", shared.m_hitEffect);
            LogEffects("m_hitTerrainEffect", shared.m_hitTerrainEffect);

            GameObject template = FindTemplate(shared);
            if (template == null)
            {
                ModLog.Warn("CropSounds: no vanilla sound prefab found to copy; the crops keep the Club's sounds.");
                return;
            }

            ModLog.Verbose("CropSounds: using vanilla sound prefab '" + template.name + "' as the template.");
            float gain = SoundGain;
            if (CropAssets.WhooshClip != null)
            {
                WhooshPrefab = CreatePrefab("sfx_ridingcrop_whoosh", template, CropAssets.WhooshClip, ModSettings.WhooshVolume.Value, gain, false);
            }

            if (CropAssets.SlapClip != null)
            {
                SlapPrefab = CreatePrefab("sfx_ridingcrop_slap", template, CropAssets.SlapClip, ModSettings.SlapVolume.Value, gain, true);
            }
        }

        // Returns a copy of the clip that starts where the IMPACT starts and is as loud as its data allows (see SoundMath).
        // Any problem (clip not readable, silent, ...) returns the original clip unchanged.
        private static AudioClip PrepareClip(AudioClip source)
        {
            try
            {
                source.LoadAudioData();
                int frames = source.samples;
                int channels = source.channels;
                var data = new float[frames * channels];
                if (frames == 0 || !source.GetData(data, 0))
                {
                    ModLog.Warn("CropSounds: slap clip '" + source.name + "' could not be read; using it unchanged.");
                    return source;
                }

                float peak = SoundMath.Peak(data);
                if (peak < 0.0001f)
                {
                    ModLog.Warn("CropSounds: slap clip '" + source.name + "' is silent; using it unchanged.");
                    return source;
                }

                int onset = SoundMath.OnsetFrame(data, channels, peak, SoundMath.OnsetFraction);
                int start = SoundMath.StartFrame(onset, source.frequency);
                float gain = SoundMath.NormalizeGain(peak);
                float[] output = SoundMath.TrimAndNormalize(data, channels, source.frequency, start, gain);
                int outFrames = output.Length / channels;

                AudioClip prepared = AudioClip.Create(source.name + "_prepared", outFrames, channels, source.frequency, false);
                prepared.SetData(output, 0);
                prepared.hideFlags = HideFlags.DontUnloadUnusedAsset;
                ModLog.Verbose("CropSounds: slap clip '" + source.name + "': peak " + peak.ToString("0.000") + ", amplified x" + gain.ToString("0.00")
                    + ", removed " + (start * 1000f / source.frequency).ToString("0") + " ms of lead-in.");
                return prepared;
            }
            catch (Exception ex)
            {
                ModLog.Warn("CropSounds: slap clip could not be prepared (" + ex.Message + "); using it unchanged.");
                return source;
            }
        }

        // Returns a copy of the clip with every sample multiplied by "gain", loud samples soft-limited (see SoundMath).
        private static AudioClip ApplyGain(AudioClip source, float gain, string label)
        {
            if (source == null || Mathf.Abs(gain - 1f) < 0.001f)
            {
                return source;
            }

            try
            {
                source.LoadAudioData();
                int frames = source.samples;
                int channels = source.channels;
                var data = new float[frames * channels];
                if (frames == 0 || !source.GetData(data, 0))
                {
                    ModLog.Warn("CropSounds: " + label + ": clip '" + source.name + "' could not be read, so the gain was NOT applied.");
                    return source;
                }

                float peakBefore;
                float peakAfter;
                int limited;
                SoundMath.ApplyGain(data, gain, out peakBefore, out peakAfter, out limited);

                AudioClip boosted = AudioClip.Create(source.name + "_gain", frames, channels, source.frequency, false);
                boosted.SetData(data, 0);
                boosted.hideFlags = HideFlags.DontUnloadUnusedAsset;
                ModLog.Verbose("CropSounds: " + label + ": gain x" + gain.ToString("0.##") + " applied, peak " + peakBefore.ToString("0.000") + " -> " + peakAfter.ToString("0.000")
                    + (limited > 0 ? ", soft limiter touched " + limited + " samples." : ", no limiting needed."));
                return boosted;
            }
            catch (Exception ex)
            {
                ModLog.Warn("CropSounds: " + label + ": the gain could not be applied (" + ex.Message + "); using the clip unchanged.");
                return source;
            }
        }

        // critical = the sound must never be dropped (the slap): no concurrency limit and top voice priority.
        private static GameObject CreatePrefab(string prefabName, GameObject template, AudioClip clip, float volume, float gain, bool critical)
        {
            GameObject prefab = PrefabManager.Instance.CreateClonedPrefab(prefabName, template);
            if (prefab == null)
            {
                ModLog.Warn("CropSounds: could not create sound prefab '" + prefabName + "'.");
                return null;
            }

            AudioClip original = clip;
            if (critical && ModSettings.SlapNormalize.Value)
            {
                clip = PrepareClip(clip);
            }

            AudioClip prepared = clip;
            clip = ApplyGain(clip, gain, prefabName);

            // PrepareClip's copy is only an intermediate step once the boosted copy exists: free it instead of leaking it.
            if (prepared != original && clip != prepared)
            {
                UnityEngine.Object.Destroy(prepared);
            }

            ZSFX[] sfxComponents = prefab.GetComponentsInChildren<ZSFX>(true);
            foreach (ZSFX sfx in sfxComponents)
            {
                sfx.m_audioClips = new[] { clip };
                sfx.m_minVol = volume;
                sfx.m_maxVol = volume;

                // The clone inherited the TEMPLATE's sound hash. AudioMan limits how many sounds with the same hash may play at once,
                // so give each of our sounds its own hash.
                sfx.m_hash = prefabName.GetStableHashCode();
                if (critical)
                {
                    // Never refuse the slap (duplicates are prevented by SlapOncePerSwingPatch); start at once, at full level.
                    sfx.m_maxConcurrentSources = 0;
                    sfx.m_minDelay = 0f;
                    sfx.m_maxDelay = 0f;
                    sfx.m_fadeInDuration = 0f;
                    sfx.m_fadeOutOnAwake = false;
                }
            }

            foreach (AudioSource source in prefab.GetComponentsInChildren<AudioSource>(true))
            {
                source.clip = clip;
                if (critical)
                {
                    // 0 = highest priority. Not positional: the camera sits behind the player and a 3D sound at the hit point would be
                    // quieted by that distance. Only the swinging player hears the slap, so it plays centered at full level.
                    source.priority = 0;
                    source.spatialBlend = 0f;
                    source.loop = false;
                }
            }

            RidingCropSoundProbe probe = prefab.AddComponent<RidingCropSoundProbe>();
            probe.Label = prefabName;

            // Registers it with the game's prefab table so it also works for other players (multiplayer).
            PrefabManager.Instance.AddPrefab(prefab);
            ModLog.Verbose("CropSounds: sound prefab '" + prefabName + "' created (" + sfxComponents.Length + " ZSFX, clip '" + clip.name + "', " + clip.length.ToString("0.00") + "s).");
            return prefab;
        }

        // 1) a sound-only prefab already used by this weapon ("sfx_..." naming), 2) well-known names, 3) any prefab with sound.
        private static GameObject FindTemplate(ItemDrop.ItemData.SharedData shared)
        {
            var candidates = new List<GameObject>();
            foreach (EffectList list in new[] { shared.m_triggerEffect, shared.m_startEffect, shared.m_hitEffect, shared.m_hitTerrainEffect })
            {
                if (list == null || list.m_effectPrefabs == null)
                {
                    continue;
                }

                foreach (EffectList.EffectData effect in list.m_effectPrefabs)
                {
                    if (effect != null && effect.m_prefab != null && IsSound(effect.m_prefab))
                    {
                        candidates.Add(effect.m_prefab);
                    }
                }
            }

            GameObject best = candidates.FirstOrDefault(c => c.name.StartsWith("sfx_", StringComparison.OrdinalIgnoreCase));
            if (best != null)
            {
                return best;
            }

            foreach (string name in new[] { "sfx_club_hit", "sfx_sword_hit", "sfx_sword_swing", "sfx_club_swing" })
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(name);
                if (prefab != null && IsSound(prefab))
                {
                    return prefab;
                }
            }

            return candidates.FirstOrDefault();
        }

        // Only "sound-only" prefabs count (ZSFX on the root object). Combined effects that also carry particles are left alone.
        private static bool IsSound(GameObject prefab)
        {
            return prefab != null && prefab.GetComponent<ZSFX>() != null;
        }

        private static bool HasSound(EffectList list)
        {
            return list != null && list.m_effectPrefabs != null && list.m_effectPrefabs.Any(e => e != null && e.m_prefab != null && IsSound(e.m_prefab));
        }

        // A copy of the list without its sound-only effects (visual effects like particles stay).
        private static EffectList WithoutSounds(EffectList list)
        {
            List<EffectList.EffectData> kept = list == null || list.m_effectPrefabs == null
                ? new List<EffectList.EffectData>()
                : list.m_effectPrefabs.Where(e => e != null && !(e.m_prefab != null && IsSound(e.m_prefab))).ToList();
            return new EffectList { m_effectPrefabs = kept.ToArray() };
        }

        // A copy of the list with our effect added.
        private static EffectList With(EffectList list, GameObject prefab)
        {
            List<EffectList.EffectData> entries = list == null || list.m_effectPrefabs == null
                ? new List<EffectList.EffectData>()
                : list.m_effectPrefabs.Where(e => e != null).ToList();
            entries.Add(new EffectList.EffectData { m_prefab = prefab, m_enabled = true, m_variant = -1 });
            return new EffectList { m_effectPrefabs = entries.ToArray() };
        }

        private static void LogEffects(string label, EffectList list)
        {
            if (!ModSettings.DebugOn)
            {
                return;
            }

            string names = list == null || list.m_effectPrefabs == null
                ? "(none)"
                : string.Join(", ", list.m_effectPrefabs.Where(e => e != null)
                    .Select(e => (e.m_prefab != null ? e.m_prefab.name : "null") + (e.m_prefab != null && IsSound(e.m_prefab) ? "[sound]" : "")).ToArray());
            ModLog.Verbose("CropSounds: Club " + label + ": " + names);
        }
    }
}
