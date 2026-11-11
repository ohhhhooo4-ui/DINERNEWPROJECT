using SnogDialogue.Runtime;
using UnityEditor;
using UnityEngine;
using XNode;

namespace SnogDialogue.Demo.Lite.Editor
{
    /// <summary>
    /// Builds the Lite demo graph programmatically.
    ///
    /// Run via: Snog > Dialogue System > Create Lite Demo Graph
    ///
    /// The generated graph demonstrates every Lite feature in one conversation:
    ///
    ///   Start
    ///     └─ [Line] Merchant greeting
    ///           └─ [Choice]
    ///                 ├─ "Tell me about this place"  ──► [Line] lore response ──► [End]
    ///                 ├─ "What do you sell?"          ──► [SetVariable] merchantVisited=true
    ///                 │                                       └─ [Line] shop response
    ///                 │                                              └─ [Choice]
    ///                 │                                                    ├─ "Thanks" (always shown)  ──► [End]
    ///                 │                                                    └─ "Ask again" (locked if visited) ──► loops
    ///                 └─ "Goodbye"                    ──► [End]
    ///
    /// On second conversation the "Ask again" option is locked (Condition: merchantVisited == false),
    /// demonstrating conditions and the locked-choice display system.
    /// </summary>
    public static class LiteDemoGraphBuilder
    {
        private const string SavePath = "Assets/Snog/LiteDemoGraph.asset";

        [MenuItem("Tools/Snog/Dialogue System/Create Lite Demo Graph")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<DialogueGraph>(SavePath) != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Demo Graph Exists",
                    $"A demo graph already exists at:\n{SavePath}\n\nOverwrite it?",
                    "Overwrite", "Cancel");

                if (!overwrite) return;
                AssetDatabase.DeleteAsset(SavePath);
            }

            DialogueGraph graph = ScriptableObject.CreateInstance<DialogueGraph>();
            AssetDatabase.CreateAsset(graph, SavePath);

            // ── Nodes ─────────────────────────────────────────────────────────

            StartNode      start        = graph.AddNode<StartNode>();
            LineNode       greeting     = graph.AddNode<LineNode>();
            ChoiceNode     mainChoice   = graph.AddNode<ChoiceNode>();

            // Branch A: lore
            LineNode       loreLine     = graph.AddNode<LineNode>();
            EndNode        loreEnd      = graph.AddNode<EndNode>();

            // Branch B: shop
            SetVariableNode setVisited   = graph.AddNode<SetVariableNode>();
            LineNode        shopLine     = graph.AddNode<LineNode>();
            ChoiceNode      shopChoice   = graph.AddNode<ChoiceNode>();
            EndNode         thankEnd     = graph.AddNode<EndNode>();
            // "Ask again" loops back to shopLine — connected below

            // Branch C: goodbye
            EndNode        byeEnd       = graph.AddNode<EndNode>();

            // ── Node data ─────────────────────────────────────────────────────

            greeting.speakerName  = "Merchant";
            greeting.inlineText   = "Welcome, traveller. First time in Ashveil?";

            // Main choice — 3 options
            mainChoice.AddOption("Tell me about this place.");
            mainChoice.AddOption("What do you sell?");
            mainChoice.AddOption("Goodbye.");

            // Lore response
            loreLine.speakerName = "Merchant";
            loreLine.inlineText  = "Ashveil's been a trading post for three generations. " +
                                   "Used to be quieter, before the road opened up.";

            // SetVariable: merchantVisited = true (Global scope)
            SetFieldViaReflection(setVisited, "scope",  VariableScope.Global);
            SetFieldViaReflection(setVisited, "key",    "merchantVisited");
            SetFieldViaReflection(setVisited, "value",  DialogueValue.FromBool(true));

            // Shop response
            shopLine.speakerName = "Merchant";
            shopLine.inlineText  = "Herbs, rope, the odd enchanted trinket. " +
                                   "Nothing you couldn't find in any town, but at fair prices.";

            // Shop follow-up choice — 2 options
            // Option 0: always available
            shopChoice.AddOption("Thanks, I'll keep that in mind.");
            // Option 1: locked on second visit via condition
            shopChoice.AddOption("Can you tell me more?");

            // Add condition to option 1: merchantVisited == false
            // We set this via reflection since ChoiceOption is nested
            AddConditionToOption(shopChoice, 1, new Condition
            {
                scope         = VariableScope.Global,
                key           = "merchantVisited",
                op            = ComparisonOperator.Equals,
                expectedValue = DialogueValue.FromBool(false)
            });

            // Goodbye end — no data needed

            // ── Layout (XNode graph editor positions) ────────────────────────

            start.position      = new Vector2(-600,   0);
            greeting.position   = new Vector2(-350,   0);
            mainChoice.position = new Vector2(  50,   0);

            loreLine.position   = new Vector2( 400,  -220);
            loreEnd.position    = new Vector2( 750,  -220);

            setVisited.position = new Vector2( 400,    40);
            shopLine.position   = new Vector2( 680,    40);
            shopChoice.position = new Vector2(1000,    40);
            thankEnd.position   = new Vector2(1350,   -80);
            // "Ask again" loops to shopLine — no separate node needed

            byeEnd.position     = new Vector2( 400,   260);

            // ── Connections ───────────────────────────────────────────────────

            Connect(start,      "next",       greeting,   "input");
            Connect(greeting,   "next",       mainChoice, "input");

            // Main choice outputs
            ConnectDynamic(mainChoice, "outputs 0", loreLine,   "input");
            ConnectDynamic(mainChoice, "outputs 1", setVisited, "input");
            ConnectDynamic(mainChoice, "outputs 2", byeEnd,     "input");

            // Lore branch
            Connect(loreLine, "next", loreEnd, "input");

            // Shop branch
            Connect(setVisited, "next",     shopLine,   "input");
            Connect(shopLine,   "next",     shopChoice, "input");
            ConnectDynamic(shopChoice, "outputs 0", thankEnd,  "input");
            ConnectDynamic(shopChoice, "outputs 1", shopLine,  "input"); // loops back

            // ── Save ──────────────────────────────────────────────────────────

            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = graph;
            EditorGUIUtility.PingObject(graph);

            Debug.Log($"[LiteDemoGraphBuilder] Demo graph created at {SavePath}. " +
                      "Assign it to your LiteDemoInteractor component.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void Connect(Node from, string fromPort, Node to, string toPort)
        {
            NodePort outPort = from.GetOutputPort(fromPort);
            NodePort inPort  = to.GetInputPort(toPort);
            if (outPort != null && inPort != null)
                outPort.Connect(inPort);
            else
                Debug.LogWarning($"[LiteDemoGraphBuilder] Could not connect {from.GetType().Name}.{fromPort} → {to.GetType().Name}.{toPort}");
        }

        private static void ConnectDynamic(Node from, string fromPort, Node to, string toPort)
        {
            NodePort outPort = from.GetOutputPort(fromPort);
            NodePort inPort  = to.GetInputPort(toPort);
            if (outPort != null && inPort != null)
                outPort.Connect(inPort);
            else
                Debug.LogWarning($"[LiteDemoGraphBuilder] Could not connect dynamic port {fromPort}");
        }

        private static void SetFieldViaReflection(object obj, string fieldName, object value)
        {
            System.Reflection.FieldInfo field =
                obj.GetType().GetField(fieldName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

            if (field != null)
                field.SetValue(obj, value);
            else
                Debug.LogWarning($"[LiteDemoGraphBuilder] Field '{fieldName}' not found on {obj.GetType().Name}");
        }

        private static void AddConditionToOption(ChoiceNode node, int optionIndex, Condition condition)
        {
            // Access the private options list via reflection and set the Conditions array
            System.Reflection.FieldInfo optionsField =
                typeof(ChoiceNode).GetField("options",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (optionsField == null) return;

            var options = optionsField.GetValue(node) as System.Collections.Generic.List<ChoiceNode.ChoiceOption>;
            if (options == null || optionIndex >= options.Count) return;

            System.Reflection.FieldInfo condField =
                typeof(ChoiceNode.ChoiceOption).GetField("Conditions");

            if (condField != null)
                condField.SetValue(options[optionIndex], new Condition[] { condition });
        }
    }
}
