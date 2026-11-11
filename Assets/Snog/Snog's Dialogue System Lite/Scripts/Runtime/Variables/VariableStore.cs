using System.Collections.Generic;

namespace SnogDialogue.Runtime
{
    public sealed class VariableStore
    {
        private readonly Dictionary<string, DialogueValue> values;

        public VariableStore()
        {
            values = new Dictionary<string, DialogueValue>();
        }

        public bool Has(string key)
        {
            return values.ContainsKey(key);
        }

        public bool TryGet(string key, out DialogueValue value)
        {
            return values.TryGetValue(key, out value);
        }

        public void Set(string key, DialogueValue value)
        {
            values[key] = value;
        }

        public void Clear()
        {
            values.Clear();
        }

        /// <summary>Returns all key-value pairs in the store. Used by the save system.</summary>
        public IEnumerable<KeyValuePair<string, DialogueValue>> GetAll() => values;

        /// <summary>
        /// Prints all current variables to the Unity console. Useful for debugging
        /// conditions that aren't behaving as expected.
        /// </summary>
        public void DebugDump(string label = "VariableStore")
        {
            if (values.Count == 0)
            {
                UnityEngine.Debug.Log($"[{label}] (empty)");
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"[{label}] {values.Count} variable(s):");

            foreach (System.Collections.Generic.KeyValuePair<string, DialogueValue> kvp in values)
            {
                string displayValue = kvp.Value.Type switch
                {
                    DialogueValueType.Int => kvp.Value.IntValue.ToString(),
                    DialogueValueType.Float => kvp.Value.FloatValue.ToString("G"),
                    DialogueValueType.Bool => kvp.Value.BoolValue.ToString(),
                    DialogueValueType.String => $"\"{kvp.Value.StringValue}\"",
                    _ => "?",
                };
                sb.AppendLine($"  {kvp.Key} ({kvp.Value.Type}) = {displayValue}");
            }

            UnityEngine.Debug.Log(sb.ToString());
        }
    }
}