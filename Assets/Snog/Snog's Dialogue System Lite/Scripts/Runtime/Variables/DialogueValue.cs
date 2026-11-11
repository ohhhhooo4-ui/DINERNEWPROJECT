using System;

namespace SnogDialogue.Runtime
{
    [Serializable]
    public struct DialogueValue
    {
        public DialogueValueType Type;
        public int IntValue;
        public float FloatValue;
        public bool BoolValue;
        public string StringValue;

        public static DialogueValue FromInt(int value)
        {
            return new DialogueValue
            {
                Type = DialogueValueType.Int,
                IntValue = value
            };
        }

        public static DialogueValue FromFloat(float value)
        {
            return new DialogueValue
            {
                Type = DialogueValueType.Float,
                FloatValue = value
            };
        }

        public static DialogueValue FromBool(bool value)
        {
            return new DialogueValue
            {
                Type = DialogueValueType.Bool,
                BoolValue = value
            };
        }

        public static DialogueValue FromString(string value)
        {
            return new DialogueValue
            {
                Type = DialogueValueType.String,
                StringValue = value
            };
        }
    }

    public enum DialogueValueType
    {
        Int,
        Float,
        Bool,
        String
    }
}