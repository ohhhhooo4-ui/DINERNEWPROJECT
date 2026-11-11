using System;
using System.Collections.Generic;
using UnityEngine;

namespace SnogDialogue.Runtime
{
    [CreateAssetMenu(menuName = "Snog/DialogueSystem/Variables/Global Variables Asset")]
    public sealed class GlobalVariablesAsset : ScriptableObject
    {
        [SerializeField] private List<VariableEntry> entries = new List<VariableEntry>();

        public VariableStore CreateRuntimeStore()
        {
            VariableStore store = new VariableStore();

            for (int i = 0; i < entries.Count; i++)
            {
                VariableEntry entry = entries[i];

                if (string.IsNullOrWhiteSpace(entry.Key))
                {
                    continue;
                }

                store.Set(entry.Key, entry.Value);
            }

            return store;
        }

        [Serializable]
        public sealed class VariableEntry
        {
            public string Key;
            public DialogueValue Value;
        }
    }
}