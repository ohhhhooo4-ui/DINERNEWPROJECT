/*
 * All rights to the Sounds Good plugin, © Created by Melenitas Dev, are reserved.
 * Distribution of the standalone asset is strictly prohibited.
 */

using System;
using System.IO;
using System.Linq;
using MelenitasDev.SoundsGood.Domain;
using UnityEditor;
using UnityEngine;

namespace MelenitasDev.SoundsGood.Editor
{
    /// <summary>
    /// The scripted way to register audio, effects, occlusion materials and output channels, doing
    /// exactly what the editor windows do.
    /// <para>
    /// Every window in Sounds Good writes through collections that are internal to the package, and
    /// each registration has a second half that is easy to miss: the generated enum (<c>SFX</c>,
    /// <c>Track</c>, <c>Effect</c>, <c>OcclusionMaterial</c>, <c>Output</c>) has to be rewritten, or
    /// the new tag exists in the data and cannot be named in code. This class is the public door to
    /// both halves, so an editor script - or an AI agent driving one - can set a project up without
    /// clicking through the windows or reaching into the package's internals.
    /// </para>
    /// Editor only: it writes assets and regenerates source files, neither of which exists at runtime.
    /// </summary>
    public static class SoundsGoodAuthoring
    {
        // ----- Queries
        /// <summary>Tags currently registered as sounds, i.e. the members of the SFX enum.</summary>
        public static string[] GetSoundTags () =>
            AssetLocator.Instance.SoundDataCollection.Sounds.Select(sound => sound.Tag).ToArray();

        /// <summary>Tags currently registered as music, i.e. the members of the Track enum.</summary>
        public static string[] GetMusicTrackTags () =>
            AssetLocator.Instance.MusicDataCollection.MusicTracks.Select(track => track.Tag).ToArray();


        /// <summary>Names of the existing output channels, i.e. the members of the Output enum.</summary>
        public static string[] GetOutputNames () =>
            AssetLocator.Instance.OutputDataCollection.Outputs.Select(output => output.Name).ToArray();

        // ----- Creation
        /// <summary>
        /// Registers one or more AudioClips under a tag, so they can be played as <c>new Sound(SFX.Tag)</c>.
        /// </summary>
        /// <param name="tag">
        /// The name the sound gets in the SFX enum. Letters, digits and underscores, not starting
        /// with a digit; anything else is rejected rather than silently corrected.
        /// </param>
        /// <param name="clips">
        /// One clip, or several to have one picked at random on every play. The clips' import
        /// settings are changed to match <paramref name="compressionPreset"/>.
        /// </param>
        /// <param name="forceToMono">
        /// Recommended for 3D sounds: a stereo clip cannot be positioned in space, and costs twice
        /// the memory to say so.
        /// </param>
        /// <returns>True when the sound was registered. On false, <paramref name="error"/> says why.</returns>
        public static bool CreateSound (string tag, AudioClip[] clips, CompressionPreset compressionPreset,
            bool forceToMono, out string error)
        {
            if (!ValidateAudioArguments(tag, clips, out error)) return false;

            if (!AssetLocator.Instance.SoundDataCollection.CreateSound(
                    clips, tag, compressionPreset, forceToMono, out error))
                return false;

            EditorHelper.ChangeAudioClipImportSettings(clips, compressionPreset, forceToMono);
            EditorHelper.SaveCollectionChanges(Sections.Sounds);
            return true;
        }

        /// <summary>
        /// Registers one or more AudioClips as a music track, playable as <c>new Music(Track.Tag)</c>.
        /// </summary>
        /// <returns>True when the track was registered. On false, <paramref name="error"/> says why.</returns>
        public static bool CreateMusicTrack (string tag, AudioClip[] clips, CompressionPreset compressionPreset,
            bool forceToMono, out string error)
        {
            if (!ValidateAudioArguments(tag, clips, out error)) return false;

            if (!AssetLocator.Instance.MusicDataCollection.CreateMusicTrack(
                    clips, tag, compressionPreset, forceToMono, out error))
                return false;

            EditorHelper.ChangeAudioClipImportSettings(clips, compressionPreset, forceToMono);
            EditorHelper.SaveCollectionChanges(Sections.Music);
            return true;
        }


        /// <summary>
        /// Creates an output channel: an Audio Mixer group with its volume exposed, usable as
        /// <c>Output.Name</c> and drivable through <see cref="SoundsGoodManager.ChangeOutputVolume(Output, float)"/>.
        /// </summary>
        /// <returns>True when the output was created. On false, <paramref name="error"/> says why.</returns>
        public static bool CreateOutput (string name, out string error)
        {
            if (!ValidateTag(name, out error)) return false;

            if (!MixerOutputAuthoring.TryCreateOutput(name, out error)) return false;

            EditorHelper.ReloadOutputsDatabase();
            return true;
        }

        // ----- Maintenance
        /// <summary>
        /// Rewrites every generated enum from the collections as they stand.
        /// <para>
        /// The Create methods already do this, so this is for repairing a project whose enums fell
        /// out of step with its data - after a merge, or after the data folder was moved by hand.
        /// </para>
        /// </summary>
        public static void RegenerateEnums ()
        {
            EditorHelper.SaveCollectionChanges(Sections.Sounds, false);
            EditorHelper.SaveCollectionChanges(Sections.Music, false);
            EditorHelper.SaveEffectCollectionChanges(false);
            EditorHelper.SaveOcclusionMaterialCollectionChanges(false);
            EditorHelper.ReloadOutputsDatabase(false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ----- Private Methods
        private static bool ValidateAudioArguments (string tag, AudioClip[] clips, out string error)
        {
            if (!ValidateTag(tag, out error)) return false;

            if (clips == null || clips.Length == 0 || clips.Any(clip => clip == null))
            {
                error = "The clips array is empty or contains a null entry.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool ValidateTag (string tag, out string error)
        {
            if (!EditorHelper.IsTagValid(tag))
            {
                error = $"'{tag}' is not a valid tag. Use letters, digits and underscores, " +
                        "and do not start with a digit: the tag becomes a member of a generated enum.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        /// <summary>Creates the folder under the configured data root if it isn't there yet.</summary>
        private static string EnsureDataSubfolder (string subfolder)
        {
            string root = AssetLocator.SoundsGoodSettings.GetNormalizedDataRootPath().TrimEnd('/');
            string path = $"{root}/{subfolder}";

            if (!AssetDatabase.IsValidFolder(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }

            return path;
        }
    }
}
