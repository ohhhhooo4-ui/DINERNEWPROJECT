using System;
using UnityEngine;

namespace SnogDialogue.Runtime
{
    [Serializable]
    public sealed class Condition
    {
        public VariableScope scope;

        [Tooltip("Variable key to read from the selected scope.")]
        public string key;

        public ComparisonOperator op;

        public DialogueValue expectedValue;
    }

    public enum ComparisonOperator
    {
        Exists,
        NotExists,

        Equals,
        NotEquals,

        GreaterThan,
        GreaterOrEqual,

        LessThan,
        LessOrEqual
    }
}