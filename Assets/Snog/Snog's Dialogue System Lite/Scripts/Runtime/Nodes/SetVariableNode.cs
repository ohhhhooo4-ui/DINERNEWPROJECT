using System.Collections;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [NodeTint("#9C27B0")]
    [NodeWidth(230)]
    [CreateNodeMenu("Snog/DialogueSystem/Set Variable")]
    public sealed class SetVariableNode : DialogueNode
    {
        [Input(connectionType = ConnectionType.Multiple, backingValue = ShowBackingValue.Never)]
        public int input;

        public VariableScope scope;

        public string key;

        public DialogueValue value;

        [Output]
        public int next;
        public override object GetValue(XNode.NodePort port) => null;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                VariableStore store = GetTargetStore(runtime);

                store.Set(key, value);
            }

            runtime.SetNext(GetNextFromPort("next"));
            yield break;
        }

        private VariableStore GetTargetStore(DialogueRuntime runtime)
        {
            if (scope == VariableScope.Global)
            {
                return runtime.Context.GlobalVariables;
            }

            return runtime.Context.GraphVariables;
        }
    }

    public enum VariableScope
    {
        Global,
        GraphLocal
    }
}