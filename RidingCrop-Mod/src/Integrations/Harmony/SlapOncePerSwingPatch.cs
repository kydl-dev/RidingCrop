using HarmonyLib;
using Kydl.Common;
using UnityEngine;

namespace RidingCrop
{
    /// <summary>
    /// Makes the slap sound play only ONCE per swing. The game creates the weapon's hit effect once for every object a swing hits,
    /// so a swing that hits two targets (or a target and the ground) would play the slap twice at almost the same moment.
    /// Before the game creates an effect list that contains our slap, this switches the slap entry off if a slap was already played
    /// a moment ago. With [DebugLog] Enable on it also logs every time an effect list asks for our whoosh or slap.
    /// </summary>
    [HarmonyPatch(typeof(EffectList), nameof(EffectList.Create))]
    internal static class SlapOncePerSwingPatch
    {
        // Every hit of one swing is created within a single frame; two real swings are always much further apart than this.
        private const float SameSwingSeconds = 0.25f;
        private static float _lastSlapTime = -100f;

        [HarmonyPrefix]
        private static void Prefix(EffectList __instance)
        {
            EffectList.EffectData[] entries = __instance == null ? null : __instance.m_effectPrefabs;
            if (entries == null)
            {
                return;
            }

            GameObject whoosh = CropSounds.WhooshPrefab;
            GameObject slap = CropSounds.SlapPrefab;
            if (whoosh == null && slap == null)
            {
                return;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                EffectList.EffectData entry = entries[i];
                if (entry == null || entry.m_prefab == null)
                {
                    continue;
                }

                if (entry.m_prefab == whoosh)
                {
                    ModLog.Verbose("[sound] frame " + Time.frameCount + ": whoosh requested by an effect list (enabled=" + entry.m_enabled + ").");
                    continue;
                }

                if (entry.m_prefab != slap)
                {
                    continue;
                }

                float now = Time.time;
                bool duplicate = now >= _lastSlapTime && now - _lastSlapTime < SameSwingSeconds;
                entry.m_enabled = !duplicate;   // EffectList.Create skips disabled entries
                if (!duplicate)
                {
                    _lastSlapTime = now;
                }

                ModLog.Verbose(duplicate
                    ? "[sound] frame " + Time.frameCount + ": duplicate slap in the same swing suppressed."
                    : "[sound] frame " + Time.frameCount + ": slap requested by an effect list.");
            }
        }
    }
}
