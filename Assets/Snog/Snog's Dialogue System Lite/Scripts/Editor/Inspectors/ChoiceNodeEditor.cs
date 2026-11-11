using System.Collections.Generic;
using System.Text;
using SnogDialogue.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnogDialogue.Editor
{
    [CustomEditor(typeof(ChoiceNode))]
    public sealed class ChoiceNodeEditor : UnityEditor.Editor
    {
        private Label summaryLabel;

        public override VisualElement CreateInspectorGUI()
        {
            ChoiceNode node = (ChoiceNode)target;

            SerializedProperty lockedDisplayProp = serializedObject.FindProperty("lockedChoiceDisplay");
            SerializedProperty lockedSuffixProp  = serializedObject.FindProperty("lockedSuffix");
            SerializedProperty optionsProp       = serializedObject.FindProperty("options");
            SerializedProperty fallbackProp      = serializedObject.FindProperty("fallback");

            VisualElement root = new VisualElement();
            root.style.paddingLeft  = 6;
            root.style.paddingRight = 6;
            root.style.paddingTop   = 4;

            Label header = new Label("Choice Node");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 6;
            root.Add(header);

            HelpBox warning = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            warning.style.display = DisplayStyle.None;
            root.Add(warning);

            // ── Behavior ──────────────────────────────────────────────────────
            Foldout behaviorFoldout = new Foldout { text = "Behavior", value = true };
            behaviorFoldout.Add(new PropertyField(lockedDisplayProp, "Locked Choice Display"));
            behaviorFoldout.Add(new PropertyField(lockedSuffixProp,  "Locked Suffix"));
            root.Add(behaviorFoldout);

            // ── Summary ───────────────────────────────────────────────────────
            Foldout summaryFoldout = new Foldout { text = "Options Summary", value = false };
            summaryLabel = new Label();
            summaryLabel.style.whiteSpace = WhiteSpace.Normal;
            summaryFoldout.Add(summaryLabel);
            root.Add(summaryFoldout);

            // ── Options list ──────────────────────────────────────────────────
            Foldout optionsFoldout = new Foldout { text = "Options", value = true };
            VisualElement optionsList = new VisualElement();
            optionsFoldout.Add(optionsList);

            // Add / Remove buttons
            VisualElement buttonsRow = new VisualElement();
            buttonsRow.style.flexDirection = FlexDirection.Row;
            buttonsRow.style.marginTop = 6;

            Button addBtn = new Button(() =>
            {
                Undo.RecordObject(node, "Add Choice Option");
                node.AddOption();
                serializedObject.Update();
                EditorUtility.SetDirty(node);
                RebuildOptionsList(node, optionsProp, optionsList, warning);
            }) { text = "Add Option" };

            Button removeBtn = new Button(() =>
            {
                if (optionsProp.arraySize <= 0) return;
                Undo.RecordObject(node, "Remove Choice Option");
                node.RemoveLastOption();
                serializedObject.Update();
                EditorUtility.SetDirty(node);
                RebuildOptionsList(node, optionsProp, optionsList, warning);
            }) { text = "Remove Last" };

            addBtn.style.flexGrow    = 1;
            removeBtn.style.flexGrow = 1;
            removeBtn.style.marginLeft = 6;

            buttonsRow.Add(addBtn);
            buttonsRow.Add(removeBtn);
            optionsFoldout.Add(buttonsRow);
            root.Add(optionsFoldout);

            // ── Fallback ──────────────────────────────────────────────────────
            Foldout fallbackFoldout = new Foldout { text = "Fallback", value = false };
            PropertyField fallbackField = new PropertyField(fallbackProp, "Fallback (Port Data)");
            fallbackField.SetEnabled(false);
            fallbackFoldout.Add(fallbackField);
            root.Add(fallbackFoldout);

            // Initial build
            RebuildOptionsList(node, optionsProp, optionsList, warning);

            return root;
        }

        // ── Option rows ───────────────────────────────────────────────────────

        private void RebuildOptionsList(
            ChoiceNode node,
            SerializedProperty optionsProp,
            VisualElement container,
            HelpBox warning)
        {
            serializedObject.Update();
            container.Clear();

            int count = optionsProp.arraySize;

            if (warning != null)
            {
                if (count == 0)
                {
                    warning.text = "This Choice node has 0 options. It will follow the Fallback output if connected.";
                    warning.style.display = DisplayStyle.Flex;
                }
                else
                {
                    warning.style.display = DisplayStyle.None;
                }
            }

            RefreshSummary(optionsProp);

            for (int i = 0; i < count; i++)
            {
                container.Add(BuildOptionRow(node, optionsProp, i, container, warning));
            }
        }

        private VisualElement BuildOptionRow(
            ChoiceNode node,
            SerializedProperty optionsProp,
            int index,
            VisualElement container,
            HelpBox warning)
        {
            SerializedProperty optionProp     = optionsProp.GetArrayElementAtIndex(index);
            SerializedProperty inlineTextProp = optionProp.FindPropertyRelative("InlineText");
            SerializedProperty conditionsProp = optionProp.FindPropertyRelative("Conditions");

            // Outer card
            VisualElement card = new VisualElement();
            card.style.borderTopWidth    = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth   = 1;
            card.style.borderRightWidth  = 1;
            card.style.borderTopColor    = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            card.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            card.style.borderLeftColor   = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            card.style.borderRightColor  = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            card.style.marginTop         = 4;
            card.style.paddingLeft       = 6;
            card.style.paddingRight      = 6;
            card.style.paddingTop        = 4;
            card.style.paddingBottom     = 4;

            // Header row: label + move buttons
            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems    = Align.Center;

            Label titleLabel = new Label($"{index + 1}) {Truncate(inlineTextProp.stringValue, 40)}");
            titleLabel.style.flexGrow = 1;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            int capturedIndex = index;

            Button upBtn = new Button(() =>
            {
                if (capturedIndex <= 0) return;
                Undo.RecordObject(node, "Reorder Choice Options");
                node.SwapOptions(capturedIndex, capturedIndex - 1);
                serializedObject.Update();
                EditorUtility.SetDirty(node);
                RebuildOptionsList(node, optionsProp, container, warning);
            }) { text = "▲" };

            Button downBtn = new Button(() =>
            {
                if (capturedIndex >= optionsProp.arraySize - 1) return;
                Undo.RecordObject(node, "Reorder Choice Options");
                node.SwapOptions(capturedIndex, capturedIndex + 1);
                serializedObject.Update();
                EditorUtility.SetDirty(node);
                RebuildOptionsList(node, optionsProp, container, warning);
            }) { text = "▼" };

            upBtn.style.width   = 28;
            downBtn.style.width = 28;
            upBtn.SetEnabled(index > 0);
            downBtn.SetEnabled(index < optionsProp.arraySize - 1);

            headerRow.Add(titleLabel);
            headerRow.Add(upBtn);
            headerRow.Add(downBtn);
            card.Add(headerRow);

            // Text field
            TextField textField = new TextField("Text");
            textField.bindingPath = inlineTextProp.propertyPath;
            textField.Bind(serializedObject);
            textField.RegisterValueChangedCallback(_ =>
            {
                titleLabel.text = $"{index + 1}) {Truncate(inlineTextProp.stringValue, 40)}";
                RefreshSummary(optionsProp);
            });
            card.Add(textField);

            // Conditions foldout
            Foldout condFoldout = new Foldout
            {
                text  = $"Conditions ({conditionsProp.arraySize})",
                value = false
            };
            condFoldout.style.marginTop = 4;

            PropertyField conditionsField = new PropertyField(conditionsProp, string.Empty);
            conditionsField.Bind(serializedObject);
            conditionsField.RegisterValueChangeCallback(_ =>
            {
                serializedObject.Update();
                condFoldout.text = $"Conditions ({conditionsProp.arraySize})";
            });

            condFoldout.Add(conditionsField);
            card.Add(condFoldout);

            return card;
        }

        // ── Summary ───────────────────────────────────────────────────────────

        private void RefreshSummary(SerializedProperty optionsProp)
        {
            if (summaryLabel == null) return;

            int total = optionsProp.arraySize;
            int withConditions = 0;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Total Options: {total}");

            for (int i = 0; i < total; i++)
            {
                SerializedProperty option       = optionsProp.GetArrayElementAtIndex(i);
                SerializedProperty textProp     = option.FindPropertyRelative("InlineText");
                SerializedProperty condsProp    = option.FindPropertyRelative("Conditions");

                string text = textProp != null ? textProp.stringValue : string.Empty;
                text = string.IsNullOrWhiteSpace(text) ? $"Choice {i + 1}" : Truncate(text, 42);

                int condCount = condsProp != null && condsProp.isArray ? condsProp.arraySize : 0;
                if (condCount > 0) withConditions++;

                sb.AppendLine($"{i + 1}) \"{text}\"  | Conditions: {condCount}");
            }

            sb.AppendLine();
            sb.AppendLine($"Options With Conditions: {withConditions}");
            summaryLabel.text = sb.ToString();
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max) return value;
            return value.Substring(0, max - 1) + "…";
        }
    }
}