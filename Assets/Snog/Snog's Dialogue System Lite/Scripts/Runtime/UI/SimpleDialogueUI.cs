using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SnogDialogue.Runtime
{
    public sealed class SimpleDialogueUI : MonoBehaviour, IDialogueUI
    {
        [Header("Panels")]
        [SerializeField] private GameObject linePanel;
        [SerializeField] private GameObject choicesPanel;

        [Header("Line UI")]
        [SerializeField] private TMP_Text lineText;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private Button continueButton;

        [Header("Choices UI")]
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Typewriter Settings")]
        [SerializeField] private Typewriter typewriter;

        private Action continueRequested;
        private Action<int> choiceSelected;

        private readonly List<Button> spawnedChoiceButtons = new List<Button>();

        private Coroutine autoAdvanceRoutine;

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            Hide();
        }

        public void ShowLine(string text, LineUIOptions options, Action onContinueRequested)
        {
            // Default: no auto-advance
            ShowLine(text, options, onContinueRequested, false, 0f);
        }

        public void ShowLine(
            string text,
            LineUIOptions options,
            Action onContinueRequested,
            bool autoAdvance,
            float autoDelay)
        {
            continueRequested = onContinueRequested;

            linePanel.SetActive(true);
            choicesPanel.SetActive(false);

            ClearChoices();

            string safeText = text ?? string.Empty;

            if (speakerText != null)
            {
                speakerText.text = options.SpeakerName ?? string.Empty;
                speakerText.gameObject.SetActive(!string.IsNullOrWhiteSpace(options.SpeakerName));
            }

            if (typewriter != null)
            {
                typewriter.Play(safeText, options.SpeedMultiplier, null);
            }
            else if (lineText != null)
            {
                lineText.text = safeText;
            }

            // Stop any previous auto routine
            CancelAutoAdvance();

            // Start auto advance if enabled
            if (autoAdvance)
            {
                autoAdvanceRoutine = StartCoroutine(AutoAdvanceRoutine(autoDelay));
            }
        }

        public void ShowChoices(IReadOnlyList<ChoiceUIEntry> choices, Action<int> onChoiceSelected)
        {
            choiceSelected = onChoiceSelected;

            linePanel.SetActive(false);
            choicesPanel.SetActive(true);

            ClearChoices();

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;

                Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
                spawnedChoiceButtons.Add(btn);

                ChoiceUIEntry entry = choices[i];

                btn.interactable = entry.Interactable;

                TMP_Text label = btn.GetComponentInChildren<TMP_Text>();

                if (label != null)
                {
                    label.text = entry.Text;
                    label.alpha = entry.Interactable ? 1f : 0.5f;
                }

                if (entry.Interactable)
                {
                    btn.onClick.AddListener(() =>
                    {
                        CancelAutoAdvance();
                        choiceSelected?.Invoke(index);
                    });
                }
            }
        }

        public void Hide()
        {
            continueRequested = null;
            choiceSelected = null;

            CancelAutoAdvance();
            ClearChoices();

            if (linePanel != null) linePanel.SetActive(false);
            if (choicesPanel != null) choicesPanel.SetActive(false);

            if (typewriter != null)
            {
                typewriter.StopTyping();
            }
        }

        private void OnContinueClicked()
        {
            CancelAutoAdvance();

            if (typewriter != null && typewriter.IsTyping)
            {
                typewriter.SkipToEnd();
                return;
            }

            continueRequested?.Invoke();
        }

        private IEnumerator AutoAdvanceRoutine(float delay)
        {
            // Wait until typewriter finishes
            if (typewriter != null)
            {
                while (typewriter.IsTyping)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(delay);

            continueRequested?.Invoke();
        }

        private void CancelAutoAdvance()
        {
            if (autoAdvanceRoutine != null)
            {
                StopCoroutine(autoAdvanceRoutine);
                autoAdvanceRoutine = null;
            }
        }

        private void ClearChoices()
        {
            for (int i = 0; i < spawnedChoiceButtons.Count; i++)
            {
                if (spawnedChoiceButtons[i] != null)
                {
                    Destroy(spawnedChoiceButtons[i].gameObject);
                }
            }

            spawnedChoiceButtons.Clear();
        }
    }
}