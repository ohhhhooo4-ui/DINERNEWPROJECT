/*
 * All rights to the Sounds Good plugin, © Created by Melenitas Dev, are reserved.
 * Distribution of the standalone asset is strictly prohibited.
 */

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MelenitasDev.SoundsGood.Domain;

namespace MelenitasDev.SoundsGood.Editor
{
    /// <summary>
    /// Shown in the Free edition when someone reaches for a feature that ships with the full one.
    /// <para>
    /// It stands in for the real window at the same menu path, so the paid features stay visible
    /// rather than silently missing: someone who never sees that occlusion materials exist has no
    /// reason to want them. The menu entries that lead here are Free-only, so in the full edition
    /// each path opens the real window instead.
    /// </para>
    /// The icon grid is the point of the window. A list of feature names reads as marketing; the
    /// actual component icons read as things that would appear in your Add Component menu.
    /// </summary>
    public class ProUpgradeWindow : EditorWindow
    {
        // ----- Types
        private readonly struct Unlock
        {
            internal readonly string IconName;
            internal readonly string Label;

            internal Unlock (string iconName, string label)
            {
                IconName = iconName;
                Label = label;
            }
        }

        // ----- Constants
        private const string ICON_FOLDER = "Packages/com.melenitasdev.sounds-good/Editor/Sprites/Icons/Components/";

        // Kept here rather than on AssetLocator: that asset lives in the user's project, so a URL
        // stored there would be frozen at whatever it was when they first installed.
        private const string STORE_URL =
            "https://assetstore.unity.com/packages/tools/audio/sounds-good-easy-optimized-audio-manager-266130";

        // What the full version adds, by the icon that stands for it. Shown as a grid so the amount
        // of it lands before a single word is read, three per row so nine of them fill three even
        // rows - a last row with a gap in it undersells the very thing the grid is there to show.
        //
        // Ordered by pull, not alphabetically: the top row is the one everybody sees.
        private static readonly Unlock[] unlocks =
        {
            new Unlock("Dynamic Music Emitter", "Dynamic Music"),
            new Unlock("Effect Zone", "Audio Effect Zone"),
            new Unlock("Occlusion Surface", "Occlusion Surface"),
            new Unlock("Music Zone", "Music Zone"),
            new Unlock("Audio Trigger", "Audio Trigger"),
            new Unlock("Random Ambience", "Random Ambience"),
            new Unlock("Audio Collision", "Collision Sound"),
            new Unlock("Audio Time Scaled", "Time Mode"),
            new Unlock("Audio Silence Trimmer", "Silence Trimmer"),
        };

        // ----- Serialized Fields
        [SerializeField] private VisualTreeAsset tree;

        // ----- Fields
        private static string pendingTitle;
        private static string pendingDescription;

        // ----- Public Methods
        /// <summary>Opens the window, framed around whichever feature was reached for.</summary>
        public static void Show (string title, string description)
        {
            pendingTitle = title;
            pendingDescription = description;

            var window = GetWindow<ProUpgradeWindow>(true, "Sounds Good", true);
            // Fixed size so the layout always lands composed: the icon strip is the whole point
            // and it falls apart if the window can be dragged narrow.
            window.minSize = new Vector2(486, 478);
            window.maxSize = new Vector2(486, 478);
            window.Refresh();
            window.ShowUtility();
        }

        // ----- Unity Events
        void CreateGUI ()
        {
            if (tree == null) return;

            tree.CloneTree(rootVisualElement);

            BuildIconGrid();
            RegisterEvents();
            Refresh();
        }

        // ----- Private Methods
        private void Refresh ()
        {
            if (rootVisualElement == null) return;

            var title = rootVisualElement.Q<Label>("FeatureTitle");
            var description = rootVisualElement.Q<Label>("FeatureDescription");

            if (title != null && !string.IsNullOrEmpty(pendingTitle)) title.text = pendingTitle;
            if (description != null && !string.IsNullOrEmpty(pendingDescription)) description.text = pendingDescription;
        }

        private void BuildIconGrid ()
        {
            Fill(rootVisualElement.Q<VisualElement>("IconGrid"), unlocks, dimmed: false);
        }

        private static void Fill (VisualElement grid, Unlock[] entries, bool dimmed)
        {
            if (grid == null) return;

            foreach (Unlock entry in entries)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ICON_FOLDER + entry.IconName + ".png");
                // A missing icon shouldn't leave a hole in the grid, so the label stands alone.
                grid.Add(BuildTile(texture, entry.Label, dimmed));
            }
        }

        private static VisualElement BuildTile (Texture2D icon, string label, bool dimmed)
        {
            var tile = new VisualElement
            {
                style =
                {
                    width = 140,
                    // Fixed, so each row of the wrapping grid is exactly as tall as the next and
                    // the grid reports the height it really occupies. Left to size themselves, the
                    // rows came out short and whatever followed was drawn over the last captions.
                    height = 74,
                    alignItems = Align.Center,
                    marginTop = 0, marginBottom = 0, marginLeft = 1, marginRight = 1,
                }
            };

            if (icon != null)
            {
                var image = new VisualElement
                {
                    style =
                    {
                        width = 58, height = 58,
                        backgroundImage = new StyleBackground(icon),
                        // What ScaleMode.ScaleToFit used to do, before it was deprecated. The
                        // other two halves of it, centred and unrepeated, are already the
                        // defaults, so contain is the whole of the difference.
                        backgroundSize = new StyleBackgroundSize(
                            new BackgroundSize(BackgroundSizeType.Contain)),
                    }
                };
                if (dimmed) image.style.opacity = 0.45f;
                tile.Add(image);
            }

            var caption = new Label(label)
            {
                style =
                {
                    fontSize = 10,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    whiteSpace = WhiteSpace.Normal,
                    color = new StyleColor(dimmed
                        ? new Color(0.55f, 0.55f, 0.55f)
                        : new Color(1f, 0.87f, 0.7f)),
                    marginTop = 2,
                }
            };
            tile.Add(caption);

            return tile;
        }

        private void RegisterEvents ()
        {
            var buy = rootVisualElement.Q<Button>("BuyButton");
            var docs = rootVisualElement.Q<Button>("DocsButton");

            if (buy != null) buy.clicked += () => Application.OpenURL(STORE_URL);
            if (docs != null) docs.clicked += () => Application.OpenURL(AssetLocator.Instance.EnglishDocumentationUrl);
        }
    }

    /// <summary>
    /// The menu entries the Free edition puts where the paid windows would be. Each one is a
    /// Free-only block: the full edition ships the real windows on those same paths, so leaving
    /// these in would fight them for the menu item.
    /// </summary>
    internal static class ProFeatureMenus
    {
        [MenuItem("Tools/Sounds Good/Effect Creator", false, 52)]
        private static void EffectCreator () => ProUpgradeWindow.Show(
            "AUDIO EFFECTS",
            "Reverb, echo, distortion and filters. Build your own and drop them on a zone, "
            + "a sound, or the whole game.");

        [MenuItem("Tools/Sounds Good/Occlusion Material Creator", false, 53)]
        private static void OcclusionMaterialCreator () => ProUpgradeWindow.Show(
            "AUDIO OCCLUSION",
            "Walls actually block sound, and every surface its own way. Glass isn't concrete.");

        [MenuItem("Tools/Sounds Good/Audio Silence Trimmer", false, 55)]
        private static void SilenceTrimmer () => ProUpgradeWindow.Show(
            "SILENCE TRIMMER",
            "Cuts the dead silence off every clip at once, so your sounds fire the instant "
            + "you play them.");

        [MenuItem("Tools/Sounds Good/Open 3D Demo Scene (with emitters)", false, 101)]
        private static void Demo3D () => ProUpgradeWindow.Show(
            "THE 3D DEMO SCENE",
            "A playable level with all of it running, and not a line of code behind it.");

        // The GameObject menu keeps every entry the full version has. Picking one of these used
        // to drop in a prefab whose script was gone, leaving a missing component in the scene.
        [MenuItem("GameObject/Sounds Good/Emitters/Dynamic Music Emitter", false, 3)]
        private static void DynamicMusicEmitter () => ProUpgradeWindow.Show(
            "DYNAMIC MUSIC",
            "Layers that all play together, in sync. Fade in the ones the scene needs and the "
            + "music follows the action without ever restarting.");

        [MenuItem("GameObject/Sounds Good/Effects/Audio Effect Zone", false, 20)]
        private static void AudioEffectZone () => ProUpgradeWindow.Show(
            "AUDIO EFFECT ZONES",
            "Drop a zone and everything inside it sounds like the room. No mixer snapshots.");

        [MenuItem("GameObject/Sounds Good/Music Zone", false, 40)]
        private static void MusicZone () => ProUpgradeWindow.Show(
            "MUSIC ZONES",
            "The tavern has its theme, the forest has another, and they cross as you walk.");

        [MenuItem("GameObject/Sounds Good/Audio Trigger", false, 41)]
        private static void AudioTrigger () => ProUpgradeWindow.Show(
            "AUDIO TRIGGERS",
            "Plays when you enter, stops when you leave, or fires on the event you pick.");

        [MenuItem("GameObject/Sounds Good/Random Ambience", false, 42)]
        private static void RandomAmbience () => ProUpgradeWindow.Show(
            "RANDOM AMBIENCE",
            "Random sounds from random places. A forest that never sounds looped.");
    }
}
#endif
