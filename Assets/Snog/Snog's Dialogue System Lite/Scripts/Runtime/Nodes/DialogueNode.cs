using System.Collections;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    public abstract class DialogueNode : Node
    {
        [SerializeField] private string nodeId;

        public string NodeId
        {
            get
            {
                return nodeId;
            }
        }

        public abstract IEnumerator Execute(DialogueRuntime runtime);

        protected override void Init()
        {
            base.Init();

            if (string.IsNullOrWhiteSpace(nodeId))
            {
                nodeId = System.Guid.NewGuid().ToString("N");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                nodeId = System.Guid.NewGuid().ToString("N");
            }
        }
#endif

        protected DialogueNode GetNextFromPort(string portName)
        {
            NodePort port = GetOutputPort(portName);

            if (port == null)
            {
                return null;
            }

            if (!port.IsConnected)
            {
                return null;
            }

            return port.Connection.node as DialogueNode;
        }
    }
}