using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [CreateAssetMenu(menuName = "Snog/DialogueSystem/Dialogue Graph")]
    public sealed class DialogueGraph : NodeGraph
    {
        [SerializeField] private string graphId;

        public string GraphId
        {
            get
            {
                return graphId;
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(graphId))
            {
                graphId = System.Guid.NewGuid().ToString("N");
            }
        }
    }
}