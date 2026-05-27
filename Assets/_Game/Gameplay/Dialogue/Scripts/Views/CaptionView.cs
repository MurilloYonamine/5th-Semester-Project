// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    public class CaptionView : TextViewBase {
        private const float LINE_DELAY = 1f;

        private Coroutine _captionRoutine;

        public void SetCaption(string captionText) {
            StartCaptionSequence(captionText, null);
        }

        public void SetCaption(string captionText, Action onComplete) {
            StartCaptionSequence(captionText, onComplete);
        }

        public void ClearCaption() {
            StopCaptionSequence();
            SetTextInstantly(string.Empty);
        }

        public override void Hide() {
            StopCaptionSequence();
            ClearCaption();
            base.Hide();
        }

        private void StartCaptionSequence(string captionText, Action onComplete) {
            StopCaptionSequence();
            _captionRoutine = StartCoroutine(PlayCaptionSequence(captionText, onComplete));
        }

        private void StopCaptionSequence() {
            if (_captionRoutine == null) {
                return;
            }

            StopCoroutine(_captionRoutine);
            _captionRoutine = null;
        }

        private IEnumerator PlayCaptionSequence(string captionText, Action onComplete) {
            string[] lines = SplitCaptionLines(captionText);

            if (lines.Length == 0) {
                onComplete?.Invoke();
                _captionRoutine = null;
                yield break;
            }

            for (int i = 0; i < lines.Length; i++) {
                bool lineCompleted = false;

                AnimateText(lines[i], () => {
                    lineCompleted = true;
                });

                while (!lineCompleted) {
                    yield return null;
                }

                if (i < lines.Length - 1) {
                    yield return new WaitForSeconds(LINE_DELAY);
                }
            }

            onComplete?.Invoke();
            _captionRoutine = null;
        }

        private static string[] SplitCaptionLines(string captionText) {
            if (string.IsNullOrWhiteSpace(captionText)) {
                return Array.Empty<string>();
            }

            string[] rawLines = captionText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            List<string> lines = new List<string>();

            for (int i = 0; i < rawLines.Length; i++) {
                string line = rawLines[i].Trim();
                if (!string.IsNullOrWhiteSpace(line)) {
                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }
    }
}
