using System;
using System.Collections.Generic;

namespace SnogDialogue.Runtime
{
    public interface IDialogueUI
    {
        void ShowLine(string text, LineUIOptions options, Action onContinueRequested);
        void ShowChoices(IReadOnlyList<ChoiceUIEntry> choices, Action<int> onChoiceSelected);
        void Hide();
        void ShowLine(string textToShow, LineUIOptions options, Action value, bool autoAdvance, float autoAdvanceDelay);

    }

    public readonly struct LineUIOptions
    {
        public float SpeedMultiplier
        {
            get;
        }

        public string SpeakerName
        {
            get;
        }

        public string[] Tags
        {
            get;
        }

        public LineUIOptions(float speedMultiplier, string speakerName = "", string[] tags = null)
        {
            SpeedMultiplier = speedMultiplier;
            SpeakerName = speakerName ?? string.Empty;
            Tags = tags;
        }

        public static LineUIOptions Default
        {
            get
            {
                return new LineUIOptions(1f, string.Empty, null);
            }
        }
    }

    public readonly struct ChoiceUIEntry
    {
        public string Text
        {
            get;
        }

        public bool Interactable
        {
            get;
        }

        public ChoiceUIEntry(string text, bool interactable)
        {
            Text = text;
            Interactable = interactable;
        }
    }
}