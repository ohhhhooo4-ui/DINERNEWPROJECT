using System.Collections.Generic;
using SnogDialogue.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnogDialogue.Editor
{
    [CustomEditor(typeof(SetVariableNode))]
    public sealed class SetVariableNodeEditor : UnityEditor.Editor
    {
        private List<string> knownKeys;
        private PopupField<string> keyPopup;

        public override VisualElement CreateInspectorGUI()
        {
            SerializedProperty scopeProp = serializedObject.FindProperty("scope");
            SerializedProperty keyProp = serializedObject.FindProperty("key");
            SerializedProperty valueProp = serializedObject.FindProperty("value");
            SerializedProperty nextProp = serializedObject.FindProperty("next");

            VisualElement root = new VisualElement();
            root.style.paddingLeft = 6;
            root.style.paddingRight = 6;
            root.style.paddingTop = 4;

            Label header = new Label("Set Variable Node");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 6;
            root.Add(header);

            HelpBox warningBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            warningBox.style.display = DisplayStyle.None;
            root.Add(warningBox);

            PropertyField scopeField = new PropertyField(scopeProp, "Scope");
            PropertyField keyField = new PropertyField(keyProp, "Key");
            PropertyField valueField = new PropertyField(valueProp, "Value");

            root.Add(scopeField);

            VisualElement keyRow = new VisualElement();
            keyRow.style.flexDirection = FlexDirection.Column;
            keyRow.style.marginTop = 4;

            Label keyHint = new Label("Key Suggestions (from GlobalVariablesAsset)");
            keyHint.style.unityFontStyleAndWeight = FontStyle.Bold;
            keyHint.style.marginTop = 4;

            keyRow.Add(keyHint);

            knownKeys = GlobalVariableKeyUtility.GetAllKnownGlobalKeys();

            if (knownKeys.Count == 0)
            {
                knownKeys.Add("(No GlobalVariablesAsset keys found)");
            }

            string currentKey = keyProp.stringValue;
            string initial = ResolveInitialKey(currentKey);

            keyPopup = new PopupField<string>("Known Keys", knownKeys, initial);
            keyPopup.SetEnabled(knownKeys.Count > 0 && knownKeys[0] != "(No GlobalVariablesAsset keys found)");

            keyPopup.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null)
                {
                    return;
                }

                if (evt.newValue.StartsWith("("))
                {
                    return;
                }

                serializedObject.Update();
                keyProp.stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();

                RefreshWarnings(keyProp, warningBox);
            });

            Button refreshButton = new Button(() =>
            {
                RefreshKnownKeys(keyProp);
            })
            {
                text = "Refresh Key List"
            };

            refreshButton.style.marginTop = 4;

            keyRow.Add(keyPopup);
            keyRow.Add(refreshButton);

            root.Add(keyRow);

            root.Add(keyField);
            root.Add(valueField);

            PropertyField nextField = new PropertyField(nextProp, "Next (Port Data)");
            nextField.SetEnabled(false);
            nextField.style.marginTop = 8;
            root.Add(nextField);

            RefreshWarnings(keyProp, warningBox);

            keyField.RegisterValueChangeCallback(_ =>
            {
                RefreshWarnings(keyProp, warningBox);
                SyncPopupToKey(keyProp);
            });

            return root;
        }

        private void RefreshKnownKeys(SerializedProperty keyProp)
        {
            knownKeys = GlobalVariableKeyUtility.GetAllKnownGlobalKeys();

            if (knownKeys.Count == 0)
            {
                knownKeys.Add("(No GlobalVariablesAsset keys found)");
            }

            if (keyPopup != null)
            {
                keyPopup.choices = knownKeys;
                keyPopup.SetEnabled(knownKeys[0] != "(No GlobalVariablesAsset keys found)");
                SyncPopupToKey(keyProp);
            }
        }

        private void SyncPopupToKey(SerializedProperty keyProp)
        {
            if (keyPopup == null)
            {
                return;
            }

            string currentKey = keyProp.stringValue;

            if (string.IsNullOrWhiteSpace(currentKey))
            {
                return;
            }

            if (knownKeys != null && knownKeys.Contains(currentKey))
            {
                keyPopup.SetValueWithoutNotify(currentKey);
            }
        }

        private string ResolveInitialKey(string currentKey)
        {
            if (knownKeys == null)
            {
                return currentKey;
            }

            if (!string.IsNullOrWhiteSpace(currentKey) && knownKeys.Contains(currentKey))
            {
                return currentKey;
            }

            return knownKeys.Count > 0 ? knownKeys[0] : currentKey;
        }

        private void RefreshWarnings(SerializedProperty keyProp, HelpBox warningBox)
        {
            serializedObject.Update();

            if (string.IsNullOrWhiteSpace(keyProp.stringValue))
            {
                warningBox.text = "Key is empty. This node will do nothing at runtime.";
                warningBox.style.display = DisplayStyle.Flex;
                return;
            }

            warningBox.style.display = DisplayStyle.None;
        }
    }
}