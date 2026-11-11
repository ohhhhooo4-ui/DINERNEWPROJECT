using SnogDialogue.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SnogDialogue.Demo.Lite
{
    /// <summary>
    /// Attach to an NPC GameObject.
    /// Player enters the trigger, a prompt appears, pressing E (or clicking) starts the graph.
    ///
    /// Demo scene wiring:
    ///   - Assign DialogueRunner (the runner in your scene)
    ///   - Assign Graph (the LiteDemoGraph asset)
    ///   - Assign InteractPrompt (the "Press E to talk" UI GameObject)
    /// </summary>
    public sealed class LiteDemoInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueRunner runner;
        [SerializeField] private DialogueGraph   graph;
        [SerializeField] private GameObject      interactPrompt;

        private bool playerInRange = false;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            SetPrompt(false);

            // Listen for dialogue finishing so we can re-enable the prompt
            if (runner != null)
                runner.OnDialogueFinished.AddListener(OnDialogueFinished);
        }

        private void Update()
        {
            if (!playerInRange) return;
            if (runner != null && runner.IsPlaying) return;

            if (Keyboard.current.eKey.isPressed)
                StartDialogue();
        }

        // ── Trigger detection ─────────────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;
            if (runner == null || !runner.IsPlaying)
                SetPrompt(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = false;
            SetPrompt(false);
        }

        // Also support 2D triggers for top-down projects
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;
            if (runner == null || !runner.IsPlaying)
                SetPrompt(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = false;
            SetPrompt(false);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void StartDialogue()
        {
            if (runner == null || graph == null) return;
            SetPrompt(false);
            runner.Play(graph);
        }

        private void OnDialogueFinished()
        {
            if (playerInRange)
                SetPrompt(true);
        }

        private void SetPrompt(bool visible)
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(visible);
        }
    }
}
