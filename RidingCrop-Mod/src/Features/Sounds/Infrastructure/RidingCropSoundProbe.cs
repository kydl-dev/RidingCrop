using System.Collections;
using KydlCore.Common;
using UnityEngine;

namespace RidingCrop
{
    /// <summary>
    /// Sits on our whoosh and slap sound prefabs. When the game spawns one of them it writes a line to the BepInEx log
    /// (only with [DebugLog] Enable on), and one frame later reports whether the audio source is really playing.
    /// It needs no Harmony patch, so it also logs sounds spawned by a path the patch does not see.
    /// </summary>
    public class RidingCropSoundProbe : MonoBehaviour
    {
        /// <summary>Name shown in the log lines.</summary>
        public string Label = "sound";

        private void OnEnable()
        {
            if (!ModSettings.DebugOn)
            {
                return;
            }

            ModLog.Verbose("[sound] frame " + Time.frameCount + ": '" + Label + "' spawned at " + transform.position + ".");
            StartCoroutine(ReportNextFrame());
        }

        private IEnumerator ReportNextFrame()
        {
            int frame = Time.frameCount;
            yield return null;

            AudioSource source = GetComponentInChildren<AudioSource>();
            if (source == null)
            {
                ModLog.Verbose("[sound] frame " + frame + ": '" + Label + "' has no AudioSource.");
                yield break;
            }

            ModLog.Verbose("[sound] frame " + frame + ": '" + Label + "' one frame later playing=" + source.isPlaying + ", volume=" + source.volume.ToString("0.##")
                + ", clip=" + (source.clip != null ? source.clip.name : "none") + ", mute=" + source.mute + ", priority=" + source.priority
                + ", spatialBlend=" + source.spatialBlend.ToString("0.##") + ", listener volume=" + AudioListener.volume.ToString("0.##") + ".");
        }
    }
}
