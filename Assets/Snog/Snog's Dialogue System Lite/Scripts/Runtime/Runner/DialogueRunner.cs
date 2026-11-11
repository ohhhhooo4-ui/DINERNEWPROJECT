using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SnogDialogue.Runtime
{
    [DisallowMultipleComponent]
    public sealed class DialogueRunner : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The MonoBehaviour that implements IDialogueUI. " +
                 "Assign your SimpleDialogueUI component here, or any custom UI that implements IDialogueUI.")]
        [SerializeField] private MonoBehaviour uiBehaviour;

        [Tooltip("Optional. Pre-populates global variables at runtime. " +
                 "Leave empty if you are not using global variables.")]
        [SerializeField] private GlobalVariablesAsset globalVariablesAsset;

        [Header("Dialogue Events")]
        [Tooltip("Fired when a dialogue graph begins playing.")]
        public UnityEvent OnDialogueStarted;

        [Tooltip("Fired when a dialogue graph finishes playing.")]
        public UnityEvent OnDialogueFinished;

        [Header("Line Events")]
        [Tooltip("Fired when a Line Node starts displaying. " +
                 "The string argument is the Event Key set on that node (empty if none was set).")]
        public UnityEvent<string> OnLineStart;

        [Tooltip("Fired when the player continues past a Line Node. " +
                 "The string argument is the Event Key set on that node (empty if none was set).")]
        public UnityEvent<string> OnLineEnd;

        // ── Public state ──────────────────────────────────────────────────────

        /// <summary>True while a dialogue graph is currently playing.</summary>
        public bool IsPlaying => playCoroutine != null;

        public VariableStore GlobalVariables => context?.GlobalVariables;
        public VariableStore GraphVariables  => context?.GraphVariables;

        // ── Private ───────────────────────────────────────────────────────────

        private IDialogueUI      ui;
        private DialogueContext  context;
        private DialogueRuntime  runtime;
        private Coroutine        playCoroutine;

        // ── Public API ────────────────────────────────────────────────────────

        public void Play(DialogueGraph graph)
        {
            if (graph == null)
            {
                Debug.LogWarning("[DialogueRunner] Play() called with a null graph.", this);
                return;
            }

            if (!EnsureInitialized()) return;

            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
                playCoroutine = null;
            }

            playCoroutine = StartCoroutine(PlayRoutine(graph));
        }

        public void Stop()
        {
            if (playCoroutine == null) return;
            StopCoroutine(playCoroutine);
            playCoroutine = null;
            ui?.Hide();
            OnDialogueFinished?.Invoke();
        }

        // ── Convenience variable API ──────────────────────────────────────────

        public void SetGlobalBool(string key, bool value)     => GlobalVariables?.Set(key, DialogueValue.FromBool(value));
        public void SetGlobalInt(string key, int value)       => GlobalVariables?.Set(key, DialogueValue.FromInt(value));
        public void SetGlobalFloat(string key, float value)   => GlobalVariables?.Set(key, DialogueValue.FromFloat(value));
        public void SetGlobalString(string key, string value) => GlobalVariables?.Set(key, DialogueValue.FromString(value));

        public bool   GetGlobalBool(string key, bool fallback = false)
        {
            if (GlobalVariables != null && GlobalVariables.TryGet(key, out DialogueValue v)) return v.BoolValue;
            return fallback;
        }

        public int    GetGlobalInt(string key, int fallback = 0)
        {
            if (GlobalVariables != null && GlobalVariables.TryGet(key, out DialogueValue v)) return v.IntValue;
            return fallback;
        }

        public float  GetGlobalFloat(string key, float fallback = 0f)
        {
            if (GlobalVariables != null && GlobalVariables.TryGet(key, out DialogueValue v)) return v.FloatValue;
            return fallback;
        }

        public string GetGlobalString(string key, string fallback = "")
        {
            if (GlobalVariables != null && GlobalVariables.TryGet(key, out DialogueValue v)) return v.StringValue;
            return fallback;
        }

        // ── Private ───────────────────────────────────────────────────────────

        private bool EnsureInitialized()
        {
            if (ui == null)
            {
                ui = uiBehaviour as IDialogueUI;

                if (ui == null)
                {
                    Debug.LogError(
                        "[DialogueRunner] uiBehaviour does not implement IDialogueUI. " +
                        "Assign a component that implements IDialogueUI (e.g. SimpleDialogueUI) " +
                        "to the UI Behaviour field on this DialogueRunner.",
                        this);
                    return false;
                }
            }

            if (context == null)
            {
                context = new DialogueContext(CreateGlobalStore(), new VariableStore());
            }

            if (runtime == null)
            {
                runtime = new DialogueRuntime(context, ui);

                // Wire the runtime's plain C# actions into this MonoBehaviour's
                // serialized UnityEvents so designers can hook up scene objects
                // in the Inspector without any code.
                runtime.onLineStart += key => OnLineStart?.Invoke(key);
                runtime.onLineEnd   += key => OnLineEnd?.Invoke(key);
            }

            return true;
        }

        private VariableStore CreateGlobalStore() =>
            globalVariablesAsset != null
                ? globalVariablesAsset.CreateRuntimeStore()
                : new VariableStore();

        private IEnumerator PlayRoutine(DialogueGraph graph)
        {
            context.GraphVariables.Clear();
            runtime.ClearNext();

            OnDialogueStarted?.Invoke();

            DialogueNode current = FindStartNode(graph);

            if (current == null)
            {
                Debug.LogWarning(
                    $"[DialogueRunner] No StartNode found in graph '{graph.name}'. " +
                    "Make sure the graph contains a Start node.", this);
                playCoroutine = null;
                OnDialogueFinished?.Invoke();
                yield break;
            }

            int safetyCounter = 0;

            while (current != null)
            {
                if (++safetyCounter > 2000)
                {
                    Debug.LogError(
                        $"[DialogueRunner] Possible infinite loop in graph '{graph.name}' " +
                        "after 2000 nodes. Stopping to prevent a hang. Check for cycles.", this);
                    break;
                }

                DialogueNode previous = current;
                runtime.ClearNext();
                yield return current.Execute(runtime);
                current = runtime.NextNode;

                if (current == null && !(previous is EndNode))
                {
                    Debug.LogWarning(
                        $"[DialogueRunner] Graph '{graph.name}' reached '{previous.GetType().Name}' " +
                        $"(ID: {previous.NodeId}) with no outgoing connection. " +
                        "Connect the output port or add an End node.", this);
                }
            }

            ui?.Hide();
            playCoroutine = null;
            OnDialogueFinished?.Invoke();
        }

        private static DialogueNode FindStartNode(DialogueGraph graph)
        {
            for (int i = 0; i < graph.nodes.Count; i++)
                if (graph.nodes[i] is StartNode s) return s;
            return null;
        }
    }
}