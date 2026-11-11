using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [NodeTint("#FF9800")]
    [NodeWidth(250)]
    [CreateNodeMenu("Snog/DialogueSystem/Choice")]
    public sealed class ChoiceNode : DialogueNode
    {
        [Input(connectionType = ConnectionType.Multiple, backingValue = ShowBackingValue.Never)]
        public int input;

        [SerializeField] private LockedChoiceDisplay lockedChoiceDisplay = LockedChoiceDisplay.Hide;
        [SerializeField] private string lockedSuffix = " (Locked)";
        [SerializeField] private List<ChoiceOption> options = new List<ChoiceOption>();

        [Output] public int fallback;
        public override object GetValue(XNode.NodePort port) => null;

        // ── XNode lifecycle ───────────────────────────────────────────────────

        protected override void Init()
        {
            base.Init();
            SyncPorts();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SyncPorts();
        }
#endif

        // ── Public API (used by editor) ───────────────────────────────────────

        public int OptionCount => options.Count;

        public string GetOptionLabel(int index)
        {
            if (index < 0 || index >= options.Count) return $"Option {index + 1}";
            string text = options[index].InlineText;
            return string.IsNullOrWhiteSpace(text) ? $"Choice {index + 1}" : text;
        }

        /// <summary>
        /// Adds a new option and creates its output port immediately.
        /// Call from the editor instead of modifying the list directly.
        /// </summary>
        public void AddOption(string label = null)
        {
            int index = options.Count;
            options.Add(new ChoiceOption { InlineText = label ?? $"Choice {index + 1}" });
            EnsurePort(index);
        }

        /// <summary>
        /// Removes the last option and its output port.
        /// </summary>
        public void RemoveLastOption()
        {
            if (options.Count == 0) return;
            int index = options.Count - 1;
            RemoveDynamicPort(PortName(index));
            options.RemoveAt(index);
        }

        /// <summary>
        /// Swaps two options and their port connections.
        /// </summary>
        public void SwapOptions(int a, int b)
        {
            if (a < 0 || b < 0 || a >= options.Count || b >= options.Count || a == b) return;

            // Swap port connections
            NodePort portA = GetOutputPort(PortName(a));
            NodePort portB = GetOutputPort(PortName(b));

            if (portA != null && portB != null)
            {
                List<NodePort> aConns = new List<NodePort>(portA.GetConnections());
                List<NodePort> bConns = new List<NodePort>(portB.GetConnections());
                portA.ClearConnections();
                portB.ClearConnections();
                foreach (NodePort p in bConns) portA.Connect(p);
                foreach (NodePort p in aConns) portB.Connect(p);
            }

            // Swap data
            ChoiceOption tmp = options[a];
            options[a] = options[b];
            options[b] = tmp;
        }

        // ── Runtime ───────────────────────────────────────────────────────────

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            List<ChoiceUIEntry> entries = new List<ChoiceUIEntry>(options.Count);
            List<int> indexMap = new List<int>(options.Count);
            bool anyInteractable = false;

            for (int i = 0; i < options.Count; i++)
            {
                ChoiceOption option = options[i];
                bool allowed = ConditionEvaluator.EvaluateAll(option.Conditions, runtime.Context);

                if (!allowed && lockedChoiceDisplay == LockedChoiceDisplay.Hide) continue;

                string label = string.IsNullOrWhiteSpace(option.InlineText)
                    ? $"Choice {i + 1}"
                    : option.InlineText;

                if (!allowed && lockedChoiceDisplay == LockedChoiceDisplay.Disable)
                    label += lockedSuffix;

                entries.Add(new ChoiceUIEntry(label, allowed));
                indexMap.Add(i);

                if (allowed) anyInteractable = true;
            }

            if (entries.Count == 0 || !anyInteractable)
            {
                runtime.SetNext(GetNextFromPort("fallback"));
                yield break;
            }

            int selectedIndex = -1;
            runtime.UI.ShowChoices(entries, index => { selectedIndex = index; });

            while (selectedIndex < 0) yield return null;

            runtime.SetNext(GetNextFromDynamicPort(PortName(indexMap[selectedIndex])));
        }

        // ── Private ───────────────────────────────────────────────────────────

        private static string PortName(int index) => $"outputs {index}";

        private void EnsurePort(int index)
        {
            string name = PortName(index);
            if (GetOutputPort(name) == null)
            {
                AddDynamicOutput(typeof(int), ConnectionType.Override, TypeConstraint.None, name);
            }
        }

        /// <summary>
        /// Makes the port list match the options list exactly.
        /// Creates missing ports and removes orphaned ones.
        /// </summary>
        private void SyncPorts()
        {
            // Ensure a port exists for every option
            for (int i = 0; i < options.Count; i++)
            {
                EnsurePort(i);
            }

            // Remove ports beyond the current option count
            int i2 = options.Count;
            while (GetOutputPort(PortName(i2)) != null)
            {
                RemoveDynamicPort(PortName(i2));
                i2++;
            }
        }

        private DialogueNode GetNextFromDynamicPort(string portName)
        {
            NodePort port = GetOutputPort(portName);
            if (port == null || !port.IsConnected) return null;
            return port.Connection.node as DialogueNode;
        }

        // ── Types ─────────────────────────────────────────────────────────────

        public enum LockedChoiceDisplay { Hide, Disable }

        [Serializable]
        public sealed class ChoiceOption
        {
            public string InlineText;
            public string LocalizationKey;
            public string[] Tags;
            public Condition[] Conditions;
        }
    }
}