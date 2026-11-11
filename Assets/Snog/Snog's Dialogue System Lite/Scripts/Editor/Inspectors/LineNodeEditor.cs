using SnogDialogue.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnogDialogue.Editor
{
    [CustomEditor(typeof(LineNode))]
    public sealed class LineNodeEditor : UnityEditor.Editor
    {
        private Label estimateLabel;

        public override VisualElement CreateInspectorGUI()
        {
            SerializedProperty speakerNameProp      = serializedObject.FindProperty("speakerName");
            SerializedProperty inlineTextProp       = serializedObject.FindProperty("inlineText");
            SerializedProperty localizationKeyProp  = serializedObject.FindProperty("localizationKey");
            SerializedProperty tagsProp             = serializedObject.FindProperty("tags");
            SerializedProperty speedMultiplierProp  = serializedObject.FindProperty("speedMultiplier");
            SerializedProperty nextProp             = serializedObject.FindProperty("next");
            SerializedProperty eventKeyProp         = serializedObject.FindProperty("eventKey");
            SerializedProperty startEventDelayProp  = serializedObject.FindProperty("startEventDelay");
            SerializedProperty autoAdvanceProp      = serializedObject.FindProperty("autoAdvance");
            SerializedProperty autoAdvanceDelayProp = serializedObject.FindProperty("autoAdvanceDelay");

            VisualElement root = new VisualElement();
            root.style.paddingLeft  = 6;
            root.style.paddingRight = 6;
            root.style.paddingTop   = 4;

            // ── Header ────────────────────────────────────────────────────────
            Label header = new Label("Line Node");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 6;
            root.Add(header);

            HelpBox warningBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            warningBox.style.display = DisplayStyle.None;
            root.Add(warningBox);

            // ── Speaker ───────────────────────────────────────────────────────
            PropertyField speakerNameField = new PropertyField(speakerNameProp, "Speaker Name");
            speakerNameField.style.marginBottom = 4;
            root.Add(speakerNameField);

            // ── Text ──────────────────────────────────────────────────────────
            Foldout textFoldout = new Foldout { text = "Text", value = true };
            textFoldout.Add(new PropertyField(inlineTextProp,      "Inline Text"));
            textFoldout.Add(new PropertyField(localizationKeyProp, "Localization Key (Lite: not used)"));
            root.Add(textFoldout);

            // ── Typewriter ────────────────────────────────────────────────────
            Foldout typewriterFoldout = new Foldout { text = "Typewriter", value = true };

            Slider speedSlider = new Slider("Speed Multiplier", 0.05f, 4f);
            speedSlider.showInputField = true;
            speedSlider.BindProperty(speedMultiplierProp);

            estimateLabel = new Label();
            estimateLabel.style.marginTop  = 6;
            estimateLabel.style.whiteSpace = WhiteSpace.Normal;

            VisualElement presetsRow = new VisualElement();
            presetsRow.style.flexDirection = FlexDirection.Row;
            presetsRow.style.marginTop     = 6;

            Button slowBtn   = MakePresetButton("Slow",   () => ApplySpeed(speedMultiplierProp, 0.6f,  inlineTextProp, warningBox));
            Button normalBtn = MakePresetButton("Normal", () => ApplySpeed(speedMultiplierProp, 1f,    inlineTextProp, warningBox));
            Button fastBtn   = MakePresetButton("Fast",   () => ApplySpeed(speedMultiplierProp, 1.8f,  inlineTextProp, warningBox));
            normalBtn.style.marginLeft = 6;
            fastBtn.style.marginLeft   = 6;

            presetsRow.Add(slowBtn);
            presetsRow.Add(normalBtn);
            presetsRow.Add(fastBtn);

            typewriterFoldout.Add(speedSlider);
            typewriterFoldout.Add(estimateLabel);
            typewriterFoldout.Add(presetsRow);
            root.Add(typewriterFoldout);

            // ── Events ────────────────────────────────────────────────────────
            Foldout eventsFoldout = new Foldout { text = "Events", value = false };

            // Event Key field
            PropertyField eventKeyField = new PropertyField(eventKeyProp, "Event Key");
            eventKeyField.tooltip = "An optional name for this line (e.g. \"open_door\"). " +
                                    "Sent as an argument to OnLineStart and OnLineEnd on the DialogueRunner.";
            eventKeyField.style.marginBottom = 6;
            eventsFoldout.Add(eventKeyField);

            // Start Delay field
            PropertyField delayField = new PropertyField(startEventDelayProp, "Start Delay (s)");
            delayField.tooltip = "How many seconds to wait before firing OnLineStart.";
            delayField.style.marginBottom = 8;
            eventsFoldout.Add(delayField);

            // Info box pointing to the runner
            HelpBox runnerInfo = new HelpBox(
                "Wire your scene logic on the DialogueRunner component (OnLineStart / OnLineEnd). " +
                "Both events pass this node's Event Key so you can tell lines apart.",
                HelpBoxMessageType.Info);
            runnerInfo.style.whiteSpace = WhiteSpace.Normal;
            eventsFoldout.Add(runnerInfo);

            root.Add(eventsFoldout);

            // ── Auto Advance ──────────────────────────────────────────────────
            Foldout autoFoldout = new Foldout { text = "Auto Advance", value = false };
            autoFoldout.Add(new PropertyField(autoAdvanceProp,      "Auto Advance"));
            autoFoldout.Add(new PropertyField(autoAdvanceDelayProp, "Delay (s)"));
            root.Add(autoFoldout);

            // ── Metadata ──────────────────────────────────────────────────────
            Foldout metaFoldout = new Foldout { text = "Metadata", value = false };
            metaFoldout.Add(new PropertyField(tagsProp, "Tags"));
            root.Add(metaFoldout);

            // ── Next port (read-only) ─────────────────────────────────────────
            PropertyField nextField = new PropertyField(nextProp, "Next (Port Data)");
            nextField.SetEnabled(false);
            nextField.style.marginTop = 8;
            root.Add(nextField);

            // ── Live refresh ──────────────────────────────────────────────────
            RefreshAll(inlineTextProp, speedMultiplierProp, warningBox);

            eventKeyField.RegisterValueChangeCallback(_ =>
                UpdateEventKeyWarning(eventKeyProp, eventsFoldout));

            serializedObject.FindProperty("inlineText"); // force bind
            root.Query<PropertyField>().ForEach(f =>
                f.RegisterValueChangeCallback(_ =>
                    RefreshAll(inlineTextProp, speedMultiplierProp, warningBox)));

            speedSlider.RegisterValueChangedCallback(_ =>
                RefreshAll(inlineTextProp, speedMultiplierProp, warningBox));

            return root;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void UpdateEventKeyWarning(
            SerializedProperty eventKeyProp,
            Foldout eventsFoldout)
        {
            // Subtly highlight the foldout label when a key is set so users know it's active
            bool hasKey = !string.IsNullOrWhiteSpace(eventKeyProp.stringValue);
            eventsFoldout.text = hasKey
                ? $"Events  [ {eventKeyProp.stringValue} ]"
                : "Events";
        }

        private Button MakePresetButton(string label, System.Action onClick)
        {
            Button btn = new Button(onClick) { text = label };
            btn.style.flexGrow = 1;
            return btn;
        }

        private void ApplySpeed(
            SerializedProperty speedProp,
            float value,
            SerializedProperty inlineTextProp,
            HelpBox warningBox)
        {
            serializedObject.Update();
            speedProp.floatValue = value;
            serializedObject.ApplyModifiedProperties();
            RefreshAll(inlineTextProp, speedProp, warningBox);
        }

        private void RefreshAll(
            SerializedProperty inlineTextProp,
            SerializedProperty speedMultiplierProp,
            HelpBox warningBox)
        {
            serializedObject.Update();
            RefreshWarnings(inlineTextProp, speedMultiplierProp, warningBox);
            RefreshEstimate(inlineTextProp, speedMultiplierProp);
        }

        private void RefreshWarnings(
            SerializedProperty inlineTextProp,
            SerializedProperty speedMultiplierProp,
            HelpBox warningBox)
        {
            if (string.IsNullOrWhiteSpace(inlineTextProp.stringValue))
            {
                warningBox.text          = "Inline Text is empty. In Lite, this will display as [Missing line text].";
                warningBox.style.display = DisplayStyle.Flex;
                return;
            }

            if (speedMultiplierProp.floatValue < 0.05f)
            {
                warningBox.text          = "Speed Multiplier is too low. Recommended minimum is 0.05.";
                warningBox.style.display = DisplayStyle.Flex;
                return;
            }

            warningBox.style.display = DisplayStyle.None;
        }

        private void RefreshEstimate(
            SerializedProperty inlineTextProp,
            SerializedProperty speedMultiplierProp)
        {
            if (estimateLabel == null) return;

            string text         = inlineTextProp.stringValue ?? string.Empty;
            int    charCount    = text.Length;
            float  multiplier   = Mathf.Max(0.05f, speedMultiplierProp.floatValue);
            float  effectiveCps = 40f * multiplier;
            float  seconds      = effectiveCps <= 0f ? 0f : charCount / effectiveCps;

            estimateLabel.text = $"Estimate: {seconds:0.0}s to type ({charCount} chars @ {effectiveCps:0.#} cps).";
        }
    }
}