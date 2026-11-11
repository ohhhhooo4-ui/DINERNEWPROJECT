using SnogDialogue.Runtime;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace SnogDialogue.Editor
{
    [CustomNodeEditor(typeof(ChoiceNode))]
    public sealed class ChoiceNodeGraphEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            ChoiceNode node = (ChoiceNode)target;
            serializedObject.Update();

            // Input port
            NodeEditorGUILayout.PortField(new GUIContent("In"), node.GetInputPort("input"));

            EditorGUILayout.Space(2);

            // One labelled output port per option
            for (int i = 0; i < node.OptionCount; i++)
            {
                NodePort port = node.GetOutputPort($"outputs {i}");
                if (port != null)
                {
                    string label = Truncate(node.GetOptionLabel(i), 26);
                    NodeEditorGUILayout.PortField(new GUIContent(label), port);
                }
            }

            if (node.OptionCount == 0)
            {
                EditorGUILayout.HelpBox("No options.", MessageType.Warning);
            }

            EditorGUILayout.Space(2);

            // Fallback
            NodeEditorGUILayout.PortField(new GUIContent("Fallback"), node.GetOutputPort("fallback"));

            serializedObject.ApplyModifiedProperties();
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max) return value;
            return value.Substring(0, max - 1) + "…";
        }
    }
}