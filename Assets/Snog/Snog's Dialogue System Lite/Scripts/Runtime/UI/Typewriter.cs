using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SnogDialogue.Runtime
{
    public sealed class Typewriter : MonoBehaviour
    {
        [SerializeField] private TMP_Text target;

        [SerializeField][Min(1f)] private float charactersPerSecond = 40f;

        [Header("Feel (Lite Polish)")]
        [SerializeField][Min(1f)] private float punctuationMultiplier = 6f;

        [SerializeField][Min(1f)] private float newlineMultiplier = 4f;

        [SerializeField] private bool useUnscaledTime = false;

        private Coroutine typingCoroutine;
        private Action finishedCallback;

        public bool IsTyping
        {
            get;
            private set;
        }

        public void Play(string text, float speedMultiplier, Action onFinished)
        {
            if (target == null)
            {
                return;
            }

            StopTyping();

            finishedCallback = onFinished;

            target.text = text ?? string.Empty;
            target.maxVisibleCharacters = 0;
            target.ForceMeshUpdate();

            float clampedMultiplier = Mathf.Max(0.05f, speedMultiplier);
            typingCoroutine = StartCoroutine(TypeRoutine(clampedMultiplier));
        }

        public void SkipToEnd()
        {
            if (target == null)
            {
                return;
            }

            if (!IsTyping)
            {
                return;
            }

            target.ForceMeshUpdate();

            int total = target.textInfo.characterCount;
            target.maxVisibleCharacters = total;

            StopTypingInternal(invokeFinished: true);
        }

        public void StopTyping()
        {
            StopTypingInternal(invokeFinished: false);
        }

        private IEnumerator TypeRoutine(float speedMultiplier)
        {
            IsTyping = true;

            float effectiveSpeed = Mathf.Max(1f, charactersPerSecond * speedMultiplier);
            float baseDelay = 1f / effectiveSpeed;

            target.ForceMeshUpdate();

            int totalChars = target.textInfo.characterCount;

            if (totalChars <= 0)
            {
                StopTypingInternal(invokeFinished: true);
                yield break;
            }

            for (int visible = 1; visible <= totalChars; visible++)
            {
                target.maxVisibleCharacters = visible;

                char lastChar = GetLastVisibleChar(visible - 1);
                float delay = GetDelayForChar(baseDelay, lastChar);

                if (useUnscaledTime)
                {
                    yield return new WaitForSecondsRealtime(delay);
                }
                else
                {
                    yield return new WaitForSeconds(delay);
                }
            }

            StopTypingInternal(invokeFinished: true);
        }

        private char GetLastVisibleChar(int charIndex)
        {
            if (target == null)
            {
                return '\0';
            }

            TMP_TextInfo info = target.textInfo;

            if (charIndex < 0 || charIndex >= info.characterCount)
            {
                return '\0';
            }

            TMP_CharacterInfo c = info.characterInfo[charIndex];
            return c.character;
        }

        private float GetDelayForChar(float baseDelay, char c)
        {
            if (c == '\n')
            {
                return baseDelay * newlineMultiplier;
            }

            if (IsPunctuation(c))
            {
                return baseDelay * punctuationMultiplier;
            }

            return baseDelay;
        }

        private bool IsPunctuation(char c)
        {
            return c == '.' || c == ',' || c == '!' || c == '?' || c == ':' || c == ';';
        }

        private void StopTypingInternal(bool invokeFinished)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            bool wasTyping = IsTyping;
            IsTyping = false;

            if (invokeFinished && wasTyping)
            {
                finishedCallback?.Invoke();
            }

            finishedCallback = null;
        }
    }
}