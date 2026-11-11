using SnogDialogue.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnogDialogue.Editor
{
    [CustomPropertyDrawer(typeof(DialogueValue))]
    public sealed class DialogueValueDrawer : PropertyDrawer
    {
        // ── UI Toolkit (used by custom node inspectors) ────────────────────────

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            SerializedProperty typeProp   = property.FindPropertyRelative("Type");
            SerializedProperty intProp    = property.FindPropertyRelative("IntValue");
            SerializedProperty floatProp  = property.FindPropertyRelative("FloatValue");
            SerializedProperty boolProp   = property.FindPropertyRelative("BoolValue");
            SerializedProperty stringProp = property.FindPropertyRelative("StringValue");

            PropertyField typeField   = new PropertyField(typeProp,   "Type");
            PropertyField intField    = new PropertyField(intProp,    "Value");
            PropertyField floatField  = new PropertyField(floatProp,  "Value");
            PropertyField boolField   = new PropertyField(boolProp,   "Value");
            PropertyField stringField = new PropertyField(stringProp, "Value");

            root.Add(typeField);
            root.Add(intField);
            root.Add(floatField);
            root.Add(boolField);
            root.Add(stringField);

            void RefreshVisibility()
            {
                DialogueValueType type = (DialogueValueType)typeProp.enumValueIndex;
                intField.style.display    = type == DialogueValueType.Int    ? DisplayStyle.Flex : DisplayStyle.None;
                floatField.style.display  = type == DialogueValueType.Float  ? DisplayStyle.Flex : DisplayStyle.None;
                boolField.style.display   = type == DialogueValueType.Bool   ? DisplayStyle.Flex : DisplayStyle.None;
                stringField.style.display = type == DialogueValueType.String ? DisplayStyle.Flex : DisplayStyle.None;
            }

            RefreshVisibility();
            typeField.RegisterValueChangeCallback(_ => RefreshVisibility());

            return root;
        }

        // ── IMGUI (used by XNode's graph window) ─────────────────────────────

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Two rows: Type enum + the active value field
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeProp   = property.FindPropertyRelative("Type");
            SerializedProperty intProp    = property.FindPropertyRelative("IntValue");
            SerializedProperty floatProp  = property.FindPropertyRelative("FloatValue");
            SerializedProperty boolProp   = property.FindPropertyRelative("BoolValue");
            SerializedProperty stringProp = property.FindPropertyRelative("StringValue");

            float lineH   = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect typeRect  = new Rect(position.x, position.y,                       position.width, lineH);
            Rect valueRect = new Rect(position.x, position.y + lineH + spacing,     position.width, lineH);

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(typeRect, typeProp, new GUIContent("Type"));

            DialogueValueType type = (DialogueValueType)typeProp.enumValueIndex;

            switch (type)
            {
                case DialogueValueType.Int:
                    EditorGUI.PropertyField(valueRect, intProp,    new GUIContent("Value"));
                    break;
                case DialogueValueType.Float:
                    EditorGUI.PropertyField(valueRect, floatProp,  new GUIContent("Value"));
                    break;
                case DialogueValueType.Bool:
                    EditorGUI.PropertyField(valueRect, boolProp,   new GUIContent("Value"));
                    break;
                case DialogueValueType.String:
                    EditorGUI.PropertyField(valueRect, stringProp, new GUIContent("Value"));
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}