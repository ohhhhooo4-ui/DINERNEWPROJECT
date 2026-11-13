using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MelenitasDev.SoundsGood.Domain;

namespace MelenitasDev.SoundsGood.Editor
{
    public static class QuickPrefabMenu
    {
        [MenuItem("GameObject/Sounds Good/Emitters/Sound Emitter", false, 0)]
        private static void CreateSoundEmitter (MenuCommand cmd) =>
            InstantiatePrefab(AssetLocator.Instance.SoundEmitterPrefab, "Sound Emitter", cmd.context);

        [MenuItem("GameObject/Sounds Good/Emitters/Music Emitter", false, 1)]
        private static void CreateMusicEmitter (MenuCommand cmd) =>
            InstantiatePrefab(AssetLocator.Instance.MusicEmitterPrefab, "Music Emitter", cmd.context);

        [MenuItem("GameObject/Sounds Good/Emitters/Playlist Emitter", false, 2)]
        private static void CreatePlaylistEmitter (MenuCommand cmd) =>
            InstantiatePrefab(AssetLocator.Instance.PlaylistEmitterPrefab, "Playlist Emitter", cmd.context);






        [MenuItem("GameObject/Sounds Good/UI/Output Volume Slider", false, 60)]
        private static void CreateOutputSlider (MenuCommand cmd) =>
            InstantiatePrefab(AssetLocator.Instance.OutputVolumeSliderPrefab, "Output Volume Slider", cmd.context);

        [MenuItem("GameObject/Sounds Good/UI/Generic Slider", false, 61)]
        private static void CreateGenericSlider (MenuCommand cmd) =>
            InstantiatePrefab(AssetLocator.Instance.GenericSliderPrefab, "Generic Slider", cmd.context);

        private static GameObject InstantiatePrefab (GameObject prefab, string prettyName, Object context)
        {
            if (prefab == null)
            {
                Debug.LogError($"[Sounds Good] The prefab reference for '{prettyName}' " +
                               $"is not set in AssetLocator.");
                return null;
            }

            // Instantiate a plain copy (not a linked prefab instance): the prefab is only a
            // factory of default values, so editing one object and applying overrides must never
            // change the source and leak into every other object created from the menu.
            GameObject go = Object.Instantiate(prefab);
            go.name = prettyName;
            Undo.RegisterCreatedObjectUndo(go, $"Create {prettyName}");

            if (context is GameObject parent)
                go.transform.SetParent(parent.transform, false);

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
            return go;
        }

        // Audio Trigger and Music Zone drive an emitter on the same object. Add a default one when
        // the prefab doesn't already carry an emitter, so the object works straight out of the menu.
        private static void EnsureEmitter<T> (GameObject go) where T : AudioEmitter
        {
            if (go == null) return;
            if (go.GetComponent<AudioEmitter>() == null) go.AddComponent<T>();
        }
    }
}