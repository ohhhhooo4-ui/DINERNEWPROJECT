namespace SnogDialogue.Runtime
{
    public sealed class DialogueContext
    {
        public VariableStore GlobalVariables
        {
            get;
        }

        public VariableStore GraphVariables
        {
            get;
        }

        public DialogueContext(VariableStore globalVariables, VariableStore graphVariables)
        {
            GlobalVariables = globalVariables;
            GraphVariables = graphVariables;
        }
    }
}
