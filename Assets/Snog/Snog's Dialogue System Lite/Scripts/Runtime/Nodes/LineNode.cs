using System.Collections;
using System.Text;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [NodeTint("#2196F3")]
    [NodeWidth(250)]
    [CreateNodeMenu("Snog/DialogueSystem/Line")]
    public sealed class LineNode : DialogueNode
    {
        [Input(connectionType = ConnectionType.Multiple, backingValue = ShowBackingValue.Never)]
        public int input;

        [Header("Content")]
        public string speakerName;

        [TextArea(2, 6)]
        public string inlineText;

        public string[] tags;

        [Header("Typewriter")]
        [Min(0.05f)]
        public float speedMultiplier = 1f;

        [Header("Events")]
        [Tooltip("Optional identifier sent to DialogueRunner.OnLineStart and OnLineEnd. " +
                 "Leave empty if you do not need to distinguish between lines.")]
        public string eventKey;

        [Min(0f)]
        [Tooltip("Seconds to wait before firing OnLineStart.")]
        public float startEventDelay = 0f;

        [Header("Auto Advance")]
        public bool autoAdvance = false;

        [Min(0f)]
        public float autoAdvanceDelay = 1.5f;

        [Output]
        public int next;

        public override object GetValue(XNode.NodePort port) => null;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            if (startEventDelay > 0f)
                yield return new WaitForSeconds(startEventDelay);

            runtime.FireLineStart(eventKey);

            string raw = string.IsNullOrWhiteSpace(inlineText)
                ? "[Missing line text]"
                : inlineText;

            string textToShow = SubstituteVariables(raw, runtime);

            bool done = false;

            runtime.UI.ShowLine(
                textToShow,
                new LineUIOptions(speedMultiplier, speakerName, tags),
                () => done = true,
                autoAdvance,
                autoAdvanceDelay
            );

            while (!done)
                yield return null;

            runtime.FireLineEnd(eventKey);

            runtime.SetNext(GetNextFromPort("next"));
        }

        /// <summary>
        /// Replaces {var:key} tokens in <paramref name="text"/> with the current
        /// runtime value of that variable. Checks graph-local scope first, then global.
        ///
        /// Examples:
        ///   "Hello, {var:playerName}!"
        ///   "You have {var:gold} gold."
        ///   "Doors open: {var:doorsOpened}"
        /// </summary>
        private static string SubstituteVariables(string text, DialogueRuntime runtime)
        {
            if (!text.Contains("{var:"))
                return text;

            StringBuilder sb     = new StringBuilder(text.Length);
            int           cursor = 0;

            while (cursor < text.Length)
            {
                int openIdx = text.IndexOf("{var:", cursor, System.StringComparison.Ordinal);

                if (openIdx < 0)
                {
                    sb.Append(text, cursor, text.Length - cursor);
                    break;
                }

                sb.Append(text, cursor, openIdx - cursor);

                int closeIdx = text.IndexOf('}', openIdx + 5);

                if (closeIdx < 0)
                {
                    // Malformed token — treat the rest as literal
                    sb.Append(text, openIdx, text.Length - openIdx);
                    break;
                }

                string key = text.Substring(openIdx + 5, closeIdx - openIdx - 5);
                sb.Append(ResolveVariable(key, runtime));

                cursor = closeIdx + 1;
            }

            return sb.ToString();
        }

        private static string ResolveVariable(string key, DialogueRuntime runtime)
        {
            // Graph-local takes priority over global
            if (runtime.Context.GraphVariables.TryGet(key, out DialogueValue graphVal))
                return ValueToString(graphVal);

            if (runtime.Context.GlobalVariables.TryGet(key, out DialogueValue globalVal))
                return ValueToString(globalVal);

            // Key not found — leave a visible placeholder so it's easy to spot
            Debug.LogWarning(
                $"[LineNode] Variable substitution: key \"{key}\" not found in graph-local or global store.");

            return $"[{key}?]";
        }

        private static string ValueToString(DialogueValue v)
        {
            return v.Type switch
            {
                DialogueValueType.Int    => v.IntValue.ToString(),
                DialogueValueType.Float  => v.FloatValue.ToString("G"),
                DialogueValueType.Bool   => v.BoolValue.ToString(),
                DialogueValueType.String => v.StringValue ?? string.Empty,
                _                        => string.Empty,
            };
        }
    }
}