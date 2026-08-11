// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using UnityEngine;
using System.Collections;
using System;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeService : MonoBehaviour, IFadeService {
        private const string TAG = "<color=orange>[FadeService]</color>";
        private CanvasGroup _canvasGroup;
        private Coroutine _currentFadeRoutine;

        private void Awake() {
            ServiceLocator.Register<IFadeService>(this);

            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        public void FadeIn(float duration, Action onComplete = null) {
            Debug.Log($"{TAG} Starting fade in over {duration} seconds.");
            if (_currentFadeRoutine != null) {
                StopCoroutine(_currentFadeRoutine);
            }
            _currentFadeRoutine = StartCoroutine(FadeRoutine(1, 0, duration, onComplete));
        }

        public void FadeOut(float duration, Action onComplete = null) {
            Debug.Log($"{TAG} Starting fade out over {duration} seconds.");
            if (_currentFadeRoutine != null) {
                StopCoroutine(_currentFadeRoutine);
            }
            _currentFadeRoutine = StartCoroutine(FadeRoutine(0, 1, duration, onComplete));
        }

        private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration, Action onComplete) {
            float elapsed = 0;
            _canvasGroup.alpha = startAlpha;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = endAlpha;
            _currentFadeRoutine = null;
            onComplete?.Invoke();
        }
    }
}