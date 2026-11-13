/*
 * All rights to the Sounds Good plugin, © Created by Melenitas Dev, are reserved.
 * Distribution of the standalone asset is strictly prohibited.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;
using MelenitasDev.SoundsGood.Domain;

[assembly: InternalsVisibleTo("SoundsGood.Editor")]

namespace MelenitasDev.SoundsGood
{
    public partial class SoundsGoodManager // Fields
    {
        private static readonly List<SoundsGoodAudioSource> audioSourcePool = new List<SoundsGoodAudioSource>();

        private static GameObject audioSourcesPoolParent;
        private static AudioListener cachedListener;

        // Raycast buffers used only by the occlusion engine.
    }

    public partial class SoundsGoodManager : MonoBehaviour
    {
    }

    public partial class SoundsGoodManager // Internal Static Methods
    {
        internal static SoundsGoodAudioSource GetSource () { return GetSoundSourceFromPool(); }

        /// <summary> Every pooled source, in use or not. Used by AudioEffectZone to find the ones inside it. </summary>
        internal static List<SoundsGoodAudioSource> AudioSourcePool => audioSourcePool;

        internal static AudioMixerGroup GetOutput (Output output) { return GetOutput(output.ToString()); }

        internal static AudioClip GetSFX (string tag, int index = -1)
        {
            if (AssetLocator.Instance.SoundDataCollection == null) return null;
            SoundData soundData = AssetLocator.Instance.SoundDataCollection.GetSound(tag);
            return soundData.GetClip(index);
        }

        internal static AudioClip GetTrack (string tag, int index = -1)
        {
            if (AssetLocator.Instance.MusicDataCollection == null) return null;
            SoundData soundData = AssetLocator.Instance.MusicDataCollection.GetMusicTrack(tag);
            return soundData.GetClip(index);
        }

        // Effect and occlusion material lookups.

        internal static float GetSavedOutputVolume (string outputName)
        {
            if (PlayerPrefs.HasKey(outputName)) return PlayerPrefs.GetFloat(outputName);
            PlayerPrefs.SetFloat(outputName, 1f);
            return 1f;
        }

        internal static bool TryGetListener (out AudioListener listener)
        {
            if (cachedListener == null)
            {
#if UNITY_6000_0_OR_NEWER
                cachedListener = FindAnyObjectByType<AudioListener>(FindObjectsInactive.Include);
#else
                cachedListener = FindObjectOfType<AudioListener>();
#endif
            }

            listener = cachedListener;
            return listener != null;
        }

        // CalculateOcclusion: the occlusion engine.
    }

    public partial class SoundsGoodManager // Public Static Methods
    {
        /// <summary>
        /// Get last saved output volume.
        /// </summary>
        /// <param name="output">Target output</param>
        [Obsolete("The output volume is now auto-updated to the last saved volume. " +
                  "There is no longer any reason to use this method.")]
        public static float GetLastSavedOutputVolume (Output output)
        {
            if (PlayerPrefs.HasKey(output.ToString())) return PlayerPrefs.GetFloat(output.ToString());
            Debug.LogWarning($"The {output.ToString()}'s volume has not been saved yet");
            return 0.5f;
        }

        /// <summary>
        /// Change Output volume.
        /// </summary>
        /// <param name="output">Target output</param>
        /// <param name="value">Target volume: min 0, Max: 1</param>
        public static void ChangeOutputVolume (Output output, float value, bool save = true)
        {
            ChangeOutputVolume(output.ToString(), value, save);
        }

        /// <summary>
        /// Change Output volume.
        /// </summary>
        /// <param name="outputName">Target output name</param>
        /// <param name="value">Target volume: min 0, Max: 1</param>
        /// <param name="save">
        /// True, the default, stores the value as the player's setting, which is what a volume
        /// slider in an options menu wants. Pass false for a change the player did not ask for and
        /// should not inherit - ducking the music under a cutscene, muting while the game is in the
        /// background - and call <see cref="RestoreSavedOutputVolume(string)"/> to put it back.
        /// Saving a temporary change leaves it as the saved setting for good: quit during a duck
        /// and the music is quiet from then on, with nothing in the options menu to explain it.
        /// </param>
        public static void ChangeOutputVolume (string outputName, float value, bool save = true)
        {
            AudioMixerGroup outputGroup = GetOutput(outputName);

            if (outputGroup == null || outputGroup.audioMixer == null)
            {
                Debug.LogError($"Can't change mixer volume because {outputName} don't exist." +
                               $"Make sure you have updated the outputs database on Outputs Manager window");
                return;
            }

            AudioMixer mixer = outputGroup.audioMixer;

            if (Application.isPlaying) SetAudioMixerLinearVolume(mixer, outputName, value);
            if (save) PlayerPrefs.SetFloat(outputName, value);
        }

        /// <summary>
        /// Puts an Output back to the volume the player chose, undoing a change made with
        /// <c>save: false</c>. Outputs the player never touched go back to full.
        /// </summary>
        /// <param name="output">Target output</param>
        public static void RestoreSavedOutputVolume (Output output)
        {
            RestoreSavedOutputVolume(output.ToString());
        }

        /// <summary>
        /// Puts an Output back to the volume the player chose, undoing a change made with
        /// <c>save: false</c>. Outputs the player never touched go back to full.
        /// </summary>
        /// <param name="outputName">Target output name</param>
        public static void RestoreSavedOutputVolume (string outputName)
        {
            ChangeOutputVolume(outputName, PlayerPrefs.GetFloat(outputName, 1f), false);
        }

        // Global and per-Output effects: the whole paid effects API.

        /// <summary>
        /// Pause all sounds, music, dynamic music and playlists.
        /// </summary>
        /// <param name="fadeOutTime">Seconds that fade out will last</param>
        public static void PauseAll (float fadeOutTime = 0)
        {
            foreach (SoundsGoodAudioSource sourcePoolElement in audioSourcePool)
            {
                sourcePoolElement.Pause(fadeOutTime);
            }
        }

        /// <summary>
        /// Pause specific sound, music, dynamic music or playlist without having the sound reference.
        /// </summary>
        /// <param name="id">The Id you've set to the music</param>
        /// <param name="fadeOutTime">Seconds that fade out will last</param>
        public static void Pause (string id, float fadeOutTime = 0)
        {
            var sourcePoolElement = audioSourcePool.FirstOrDefault(sourceElement => sourceElement.Id == id);
            if (sourcePoolElement == null || !sourcePoolElement.Using)
            {
                Debug.LogWarning($"There is no music with the id '{id}'");
                return;
            }

            sourcePoolElement.Pause(fadeOutTime);
        }


        /// <summary>
        /// Stop all sounds, music, dynamic music and playlists.
        /// </summary>
        /// <param name="fadeOutTime">Seconds that fade out will last</param>
        public static void StopAll (float fadeOutTime = 0)
        {
            foreach (SoundsGoodAudioSource sourcePoolElement in audioSourcePool)
            {
                sourcePoolElement.Stop(fadeOutTime);
            }
        }

        /// <summary>
        /// Stop specific sound, music, dynamic music or playlist without having the sound reference.
        /// </summary>
        /// <param name="id">The Id you've set to the sound</param>
        /// <param name="fadeOutTime">Seconds that fade out will last</param>
        public static void Stop (string id, float fadeOutTime = 0)
        {
            var sourcePoolElement = audioSourcePool.FirstOrDefault(sourceElement => sourceElement.Id == id);
            if (sourcePoolElement == null || !sourcePoolElement.Using)
            {
                Debug.LogWarning($"There is no sound reproducing with the id '{id}'");
                return;
            }

            sourcePoolElement.Stop(fadeOutTime);
        }

        /// <summary>
        /// Resume all sounds, music, dynamic music and playlists.
        /// </summary>
        /// <param name="fadeInTime">Seconds that fade in will last</param>
        public static void ResumeAll (float fadeInTime = 0)
        {
            foreach (SoundsGoodAudioSource sourcePoolElement in audioSourcePool)
            {
                if (sourcePoolElement.Paused) sourcePoolElement.Resume(fadeInTime);
            }
        }

        /// <summary>
        /// Resume specific sound, music, dynamic music or playlist without having the sound reference.
        /// </summary>
        /// <param name="id">The Id you've set to the sound</param>
        /// <param name="fadeInTime">Seconds that fade in will last</param>
        public static void Resume (string id, float fadeInTime = 0)
        {
            var sourcePoolElement = audioSourcePool.FirstOrDefault(s => s.Id == id);
            if (sourcePoolElement == null || !sourcePoolElement.Paused)
            {
                Debug.LogWarning($"There is no sound paused with the id '{id}'");
                return;
            }

            sourcePoolElement.Resume(fadeInTime);
        }

        // ----- Obsoletes methods

        /// <summary> [OBSOLETE] Use <see cref="Stop(string,float)"/>. </summary>
        [Obsolete("Use SoundsGoodManager.Stop(id, fadeOutTime) instead. " +
                  "This method will be removed in a future version.")]
        public static void StopSound (string id, float fadeOutTime = 0) => Stop(id, fadeOutTime);

        /// <summary> [OBSOLETE] Use <see cref="Stop(string,float)"/>. </summary>
        [Obsolete("Use SoundsGoodManager.Stop(id, fadeOutTime) instead. " +
                  "This method will be removed in a future version.")]
        public static void StopMusic (string id, float fadeOutTime = 0) => Stop(id, fadeOutTime);

        /// <summary> [OBSOLETE] Use <see cref="StopAll(float)"/>. </summary>
        [Obsolete("Use SoundsGoodManager.StopAll(fadeOutTime) instead. " +
                  "This method will be removed in a future version.")]
        public static void StopAllSounds (float fadeOutTime = 0) => StopAll(fadeOutTime);

        /// <summary> [OBSOLETE] Use <see cref="StopAll(float)"/>. </summary>
        [Obsolete("Use SoundsGoodManager.StopAll(fadeOutTime) instead. " +
                  "This method will be removed in a future version.")]
        public static void StopAllMusic (float fadeOutTime = 0) => StopAll(fadeOutTime);

        /// <summary> [OBSOLETE] Use <see cref="Pause(string,float)"/>. </summary>
        [Obsolete("Use SoundsGoodManager.Pause(id, fadeOutTime) instead. " +
                  "This method will be removed in a future version.")]
        public static void PauseSound (string id, float fadeOutTime = 0) => Pause(id, fadeOutTime);

        /// <summary> [OBSOLETE] Use <see cref="Pause(string,float)"/>. </summary>
        [Obsolete("Use SoundsGoodManager.Pause(id, fadeOutTime) instead. " +
                  "This method will be removed in a future version.")]
        public static void PauseMusic (string id, float fadeOutTime = 0) => Pause(id, fadeOutTime);

        /// <summary> [OBSOLETE] Use <see cref="PauseAll(float)"/>. </summary>
        [Obsolete("Use SoundsGoodManager.PauseAll(fadeOutTime) instead. " +
                  "This method will be removed in a future version.")]
        public static void PauseAllSounds (float fadeOutTime = 0) => PauseAll(fadeOutTime);

        /// <summary> [OBSOLETE] Use <see cref="PauseAll(float)"/>. </summary>
        [Obsolete("Use SoundsGoodManager.PauseAll(fadeOutTime) instead. " +
                  "This method will be removed in a future version.")]
        public static void PauseAllMusic (float fadeOutTime = 0) => PauseAll(fadeOutTime);
    }

    public partial class SoundsGoodManager // Private Methods
    {
        private static SoundsGoodAudioSource GetSoundSourceFromPool ()
        {
            if (audioSourcesPoolParent == null || !audioSourcesPoolParent.activeInHierarchy)
            {
                audioSourcesPoolParent = new GameObject("Sources Pool Parent");
                DontDestroyOnLoad(audioSourcesPoolParent);
            }

            if (audioSourcePool.Count != audioSourcesPoolParent.transform.childCount)
            {
                audioSourcePool.Clear();
            }

            foreach (SoundsGoodAudioSource element in audioSourcePool)
            {
                if (!element.Using) return element;
            }

            GameObject newSourceInstance = new GameObject($"Audio Source {audioSourcePool.Count}");
            DontDestroyOnLoad(newSourceInstance);
            newSourceInstance.transform.SetParent(audioSourcesPoolParent.transform);

            AudioSource source = newSourceInstance.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.dopplerLevel = 0;
            SoundsGoodAudioSource soundsGoodAudioSourceElement =
                newSourceInstance.AddComponent<SoundsGoodAudioSource>().Init(source);

            audioSourcePool.Add(soundsGoodAudioSourceElement);
            return soundsGoodAudioSourceElement;
        }

        private static AudioMixerGroup GetOutput (string outputName)
        {
            if (AssetLocator.Instance.OutputDataCollection == null) return null;

            AudioMixerGroup audioMixerGroup = AssetLocator.Instance.OutputDataCollection.GetOutput(outputName);

            if (audioMixerGroup == null) return null;

            float lastSavedVolume = GetSavedOutputVolume(outputName);
            SetAudioMixerLinearVolume(audioMixerGroup.audioMixer, outputName, lastSavedVolume);
            return audioMixerGroup;
        }

        private static void SetAudioMixerLinearVolume (AudioMixer audioMixer, string volumeParameterName, float volume)
        {
            audioMixer.SetFloat(volumeParameterName,
                Mathf.Log10(Mathf.Clamp(volume, 0.001f, 0.99f)) * 20);
        }
    }
}