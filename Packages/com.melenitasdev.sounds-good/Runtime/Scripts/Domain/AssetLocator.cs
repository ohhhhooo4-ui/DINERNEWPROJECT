using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

[assembly: InternalsVisibleTo("SoundsGood.Domain")]
[assembly: InternalsVisibleTo("SoundsGood.Editor")]
[assembly: InternalsVisibleTo("SoundsGood.Application")]

namespace MelenitasDev.SoundsGood.Domain
{
    internal class AssetLocator : ScriptableObject
    {
        [Header("VisualTreeAssets")]
        [SerializeField] private VisualTreeAsset audioClipTemplate;
        [SerializeField] private VisualTreeAsset audioTemplate;
        [SerializeField] private VisualTreeAsset outputTemplate;

        [Header("Textures")] 
        [SerializeField] private Texture2D createGroupImage;
        [SerializeField] private Texture2D renameGroupImage;
        [SerializeField] private Texture2D exposeVolumeImage;
        [SerializeField] private Texture2D locateExposedParamImage;
        [SerializeField] private Texture2D renameExposedParamImage;
        
#if UNITY_EDITOR
        [Header("Scenes")]
        [SerializeField] private SceneAsset showcaseScene;
#endif
        
        [Header("Prefabs")]
        [SerializeField] private GameObject outputVolumeSliderPrefab;
        [SerializeField] private GameObject genericSliderPrefab;
        [SerializeField] private GameObject soundEmitterPrefab;
        [SerializeField] private GameObject musicEmitterPrefab;
        [SerializeField] private GameObject playlistEmitterPrefab;
        
        [Header("URLs")]
        [SerializeField] private string englishDocumentationUrl;
        [SerializeField] private string spanishDocumentationUrl;

        private static AssetLocator instance;
        internal static AssetLocator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<AssetLocator>("AssetLocator");
                }
        
                return instance;
            }
        }

        private static SoundsGoodSettings soundsGoodSettings;
        internal static SoundsGoodSettings SoundsGoodSettings
        {
            get
            {
                if (soundsGoodSettings == null)
                {
                    soundsGoodSettings = Resources.Load<SoundsGoodSettings>("SoundsGoodSettings");
                }

                return soundsGoodSettings;
            }
        }

        internal static void ResetSettingsCache () => soundsGoodSettings = null;

        // User-data references live in SoundsGoodRuntimeData (in the user's Assets, so they
        // survive package updates). AssetLocator stays the global access point and forwards.
        internal SoundDataCollection SoundDataCollection => SoundsGoodRuntimeData.Instance?.SoundDataCollection;
        internal MusicDataCollection MusicDataCollection => SoundsGoodRuntimeData.Instance?.MusicDataCollection;
        internal OutputDataCollection OutputDataCollection => SoundsGoodRuntimeData.Instance?.OutputDataCollection;
        internal EffectDataCollection EffectDataCollection => SoundsGoodRuntimeData.Instance?.EffectDataCollection;
        internal OcclusionMaterialDataCollection OcclusionMaterialCollection => SoundsGoodRuntimeData.Instance?.OcclusionMaterialCollection;
        internal AudioMixer MasterAudioMixer => SoundsGoodRuntimeData.Instance?.MasterAudioMixer;
        internal TextAsset SfxEnum => SoundsGoodRuntimeData.Instance?.SfxEnum;
        internal TextAsset TracksEnum => SoundsGoodRuntimeData.Instance?.TracksEnum;
        internal TextAsset OutputsEnum => SoundsGoodRuntimeData.Instance?.OutputsEnum;
        internal TextAsset EffectsEnum => SoundsGoodRuntimeData.Instance?.EffectsEnum;
        internal TextAsset OcclusionMaterialsEnum => SoundsGoodRuntimeData.Instance?.OcclusionMaterialsEnum;

        internal VisualTreeAsset AudioClipTemplate => audioClipTemplate;
        internal VisualTreeAsset AudioTemplate => audioTemplate;
        internal VisualTreeAsset OutputTemplate => outputTemplate;
        internal Texture2D CreateGroupImage => createGroupImage;
        internal Texture2D RenameGroupImage => renameGroupImage;
        internal Texture2D ExposeVolumeImage => exposeVolumeImage;
        internal Texture2D LocateExposedParamImage => locateExposedParamImage;
        internal Texture2D RenameExposedParamImage => renameExposedParamImage;
#if UNITY_EDITOR
        internal SceneAsset ShowcaseScene => showcaseScene;
#endif
        internal GameObject OutputVolumeSliderPrefab => outputVolumeSliderPrefab;
        internal GameObject GenericSliderPrefab => genericSliderPrefab;
        internal GameObject SoundEmitterPrefab => soundEmitterPrefab;
        internal GameObject MusicEmitterPrefab => musicEmitterPrefab;
        internal GameObject PlaylistEmitterPrefab => playlistEmitterPrefab;
        internal string EnglishDocumentationUrl => englishDocumentationUrl;
        internal string SpanishDocumentationUrl => spanishDocumentationUrl;
    }
}
