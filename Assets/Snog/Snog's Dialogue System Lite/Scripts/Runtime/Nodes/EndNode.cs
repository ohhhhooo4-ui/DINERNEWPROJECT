using System.Collections;
using XNode;

namespace SnogDialogue.Runtime
{
    [NodeTint("#F44336")]
    [NodeWidth(200)]
    [CreateNodeMenu("Snog/DialogueSystem/End")]
    public sealed class EndNode : DialogueNode
    {
        [Input(connectionType = ConnectionType.Multiple, backingValue = ShowBackingValue.Never)]
        public int input;
        public override object GetValue(XNode.NodePort port) => null;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            runtime.UI.Hide();
            runtime.SetNext(null);
            yield break;
        }
    }
}