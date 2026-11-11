using System;

namespace SnogDialogue.Runtime
{
    public sealed class DialogueRuntime
    {
        public DialogueContext Context  { get; }
        public IDialogueUI     UI      { get; }
        public DialogueNode    NextNode { get; private set; }

        // Subscribed by DialogueRunner to forward into its serialized UnityEvents
        internal Action<string> onLineStart;
        internal Action<string> onLineEnd;

        public DialogueRuntime(DialogueContext context, IDialogueUI ui)
        {
            Context = context;
            UI      = ui;
        }

        internal void FireLineStart(string key) => onLineStart?.Invoke(key ?? string.Empty);
        internal void FireLineEnd(string key)   => onLineEnd?.Invoke(key ?? string.Empty);

        public void SetNext(DialogueNode node) => NextNode = node;
        public void ClearNext()               => NextNode = null;
    }
}