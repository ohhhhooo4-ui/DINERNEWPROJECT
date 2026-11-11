using UnityEngine;

namespace SnogDialogue.Runtime
{
    public static class ConditionEvaluator
    {
        public static bool Evaluate(Condition condition, DialogueContext context)
        {
            if (condition == null)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(condition.key))
            {
                return false;
            }

            VariableStore store = GetStore(condition.scope, context);

            if (condition.op == ComparisonOperator.Exists)
            {
                return store.Has(condition.key);
            }

            if (condition.op == ComparisonOperator.NotExists)
            {
                return !store.Has(condition.key);
            }

            DialogueValue actual;

            if (!store.TryGet(condition.key, out actual))
            {
                actual = GetDefaultValue(condition.expectedValue.Type);
            }

            return Compare(actual, condition.expectedValue, condition.op);
        }

        public static bool EvaluateAll(Condition[] conditions, DialogueContext context)
        {
            if (conditions == null || conditions.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < conditions.Length; i++)
            {
                if (!Evaluate(conditions[i], context))
                {
                    return false;
                }
            }

            return true;
        }

        private static VariableStore GetStore(VariableScope scope, DialogueContext context)
        {
            if (scope == VariableScope.Global)
            {
                return context.GlobalVariables;
            }

            return context.GraphVariables;
        }

        private static DialogueValue GetDefaultValue(DialogueValueType type)
        {
            switch (type)
            {
                case DialogueValueType.Int:
                    return DialogueValue.FromInt(0);

                case DialogueValueType.Float:
                    return DialogueValue.FromFloat(0f);

                case DialogueValueType.Bool:
                    return DialogueValue.FromBool(false);

                case DialogueValueType.String:
                    return DialogueValue.FromString(string.Empty);

                default:
                    return DialogueValue.FromBool(false);
            }
        }

        private static bool Compare(DialogueValue actual, DialogueValue expected, ComparisonOperator op)
        {
            if (op == ComparisonOperator.Equals || op == ComparisonOperator.NotEquals)
            {
                bool equals = EqualsByType(actual, expected);
                return op == ComparisonOperator.Equals ? equals : !equals;
            }

            if (!TryToFloat(actual, out float a))
            {
                return false;
            }

            if (!TryToFloat(expected, out float b))
            {
                return false;
            }

            switch (op)
            {
                case ComparisonOperator.GreaterThan:
                    return a > b;

                case ComparisonOperator.GreaterOrEqual:
                    return a >= b;

                case ComparisonOperator.LessThan:
                    return a < b;

                case ComparisonOperator.LessOrEqual:
                    return a <= b;

                default:
                    return false;
            }
        }

        private static bool EqualsByType(DialogueValue a, DialogueValue b)
        {
            if (a.Type != b.Type)
            {
                return false;
            }

            switch (a.Type)
            {
                case DialogueValueType.Int:
                    return a.IntValue == b.IntValue;

                case DialogueValueType.Float:
                    return Mathf.Approximately(a.FloatValue, b.FloatValue);

                case DialogueValueType.Bool:
                    return a.BoolValue == b.BoolValue;

                case DialogueValueType.String:
                    return a.StringValue == b.StringValue;

                default:
                    return false;
            }
        }

        private static bool TryToFloat(DialogueValue value, out float result)
        {
            if (value.Type == DialogueValueType.Int)
            {
                result = value.IntValue;
                return true;
            }

            if (value.Type == DialogueValueType.Float)
            {
                result = value.FloatValue;
                return true;
            }

            result = 0f;
            return false;
        }
    }
}