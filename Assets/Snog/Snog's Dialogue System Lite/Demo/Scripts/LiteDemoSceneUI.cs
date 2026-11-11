using SnogDialogue.Runtime;
using TMPro;
using UnityEngine;

namespace SnogDialogue.Demo.Lite
{
    /// <summary>
    /// Controls the demo scene's hint UI and the live variable readout panel.
    ///
    /// Demo scene wiring:
    ///   - Assign DialogueRunner
    ///   - Assign HintLabel      (TMP_Text — shows "Talk to the merchant" or "Come back!")
    ///   - Assign VariablePanel  (GameObject — the debug readout, toggled with Tab)
    ///   - Assign VariableText   (TMP_Text inside the panel)
    ///
    /// Press Tab in Play mode to show/hide the live variable readout.
    /// </summary>
    public sealed class LiteDemoSceneUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueRunner runner;

        [Header("Hint UI")]
        [SerializeField] private TMP_Text hintLabel;

        [Header("Variable Debug Panel")]
        [Tooltip("Toggle visibility with Tab.")]
        [SerializeField] private GameObject variablePanel;
        [SerializeField] private TMP_Text   variableText;

        private bool panelVisible = false;
        private const string VISIT_KEY = "merchantVisited";

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            if (variablePanel != null)
                variablePanel.SetActive(false);

            if (runner != null)
            {
                runner.OnDialogueStarted.AddListener(OnDialogueStarted);
                runner.OnDialogueFinished.AddListener(OnDialogueFinished);
            }

            RefreshHint();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                ToggleVariablePanel();
        }

        // ── Dialogue callbacks ────────────────────────────────────────────────

        private void OnDialogueStarted()
        {
            if (hintLabel != null)
                hintLabel.text = string.Empty;
        }

        private void OnDialogueFinished()
        {
            RefreshHint();

            if (panelVisible)
                RefreshVariableText();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void RefreshHint()
        {
            if (hintLabel == null || runner == null) return;

            bool visited = runner.GetGlobalBool(VISIT_KEY, false);
            hintLabel.text = visited
                ? "You've already spoken to the merchant.\n<size=70%>Talk again to see conditions in action.</size>"
                : "Approach the merchant and press <b>E</b> to talk.";
        }

        private void ToggleVariablePanel()
        {
            panelVisible = !panelVisible;
            if (variablePanel != null)
                variablePanel.SetActive(panelVisible);

            if (panelVisible)
                RefreshVariableText();
        }

        private void RefreshVariableText()
        {
            if (variableText == null || runner == null || runner.GlobalVariables == null)
                return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Global Variables</b>");

            foreach (System.Collections.Generic.KeyValuePair<string, DialogueValue> kv
                     in runner.GlobalVariables.GetAll())
            {
                string val = kv.Value.Type switch
                {
                    DialogueValueType.Int    => kv.Value.IntValue.ToString(),
                    DialogueValueType.Float  => kv.Value.FloatValue.ToString("G"),
                    DialogueValueType.Bool   => kv.Value.BoolValue.ToString(),
                    DialogueValueType.String => $"\"{kv.Value.StringValue}\"",
                    _                        => "?"
                };
                sb.AppendLine($"  {kv.Key} ({kv.Value.Type}) = {val}");
            }

            if (runner.GlobalVariables.GetAll() is var all)
            {
                bool any = false;
                foreach (var _ in all) { any = true; break; }
                if (!any) sb.AppendLine("  (empty)");
            }

            sb.AppendLine();
            sb.AppendLine("<size=70%>Press Tab to hide</size>");
            variableText.text = sb.ToString();
        }
    }
}
