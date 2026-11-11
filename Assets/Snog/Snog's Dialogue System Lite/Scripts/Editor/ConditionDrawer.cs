using SnogDialogue.Runtime;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace SnogDialogue.Editor
{
    [CustomPropertyDrawer(typeof(Condition))]
    public sealed class ConditionDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            SerializedProperty scopeProp = property.FindPropertyRelative("scope");
            SerializedProperty keyProp = property.FindPropertyRelative("key");
            SerializedProperty opProp = property.FindPropertyRelative("op");
            SerializedProperty expectedProp = property.FindPropertyRelative("expectedValue");

            PropertyField scopeField = new PropertyField(scopeProp, "Scope");
            PropertyField keyField = new PropertyField(keyProp, "Key");
            PropertyField opField = new PropertyField(opProp, "Operator");
            PropertyField expectedField = new PropertyField(expectedProp, "Expected");

            root.Add(scopeField);
            root.Add(keyField);
            root.Add(opField);
            root.Add(expectedField);

            void RefreshVisibility()
            {
                ComparisonOperator op = (ComparisonOperator)opProp.enumValueIndex;
                bool needsValue = op != ComparisonOperator.Exists && op != ComparisonOperator.NotExists;

                expectedField.style.display = needsValue ? DisplayStyle.Flex : DisplayStyle.None;
            }

            RefreshVisibility();

            opField.RegisterValueChangeCallback(_ =>
            {
                RefreshVisibility();
            });

            return root;
        }
    }
}