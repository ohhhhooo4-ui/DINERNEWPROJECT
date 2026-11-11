using SnogDialogue.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SnogDialogue.Editor
{
    [CustomEditor(typeof(GlobalVariablesAsset))]
    public sealed class GlobalVariablesAssetEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            Label title = new Label("Global Variables");
            title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            root.Add(title);

            SerializedProperty entriesProp = serializedObject.FindProperty("entries");

            PropertyField entriesField = new PropertyField(entriesProp);
            entriesField.Bind(serializedObject);
            root.Add(entriesField);

            HelpBox info = new HelpBox(
                "These are default values. A runtime copy is created when the dialogue runner initializes.",
                HelpBoxMessageType.Info
            );

            root.Add(info);

            return root;
        }
    }
}