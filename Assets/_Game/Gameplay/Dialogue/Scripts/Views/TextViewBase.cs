// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using System.Collections;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class TextViewBase : MonoBehaviour {
        [Header("Animation")]
        [SerializeField, Min(0f)] private float _fadeDuration = 0.2f;
        [SerializeField, ToggleLeft] private bool _useTypewriterEffect = true;
        [SerializeField, ShowIf(nameof(_useTypewriterEffect)), Min(0f)] private float _charactersPerSecond = 45f;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _showClip;
        [SerializeField] private AudioClip _hideClip;
        [SerializeField] private AudioClip _typewriterClip;

        [Header("Content")]
        [SerializeField] protected TMP_Text _contentText;

        protected CanvasGroup _canvasGroup;
        private Coroutine _fadeRoutine;
        private Coroutine _typewriterRoutine;

        protected virtual void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null) {
                Debug.LogError($"[{GetType().Name}] CanvasGroup ausente em {name}.");
                enabled = false;
                return;
            }

            if (_contentText == null) {
                Debug.LogError($"[{GetType().Name}] Content Text não atribuído em {name}.");
                enabled = false;
                return;
            }

            SetVisibleInstantly(false);
        }

        public virtual void Show() {
            PlayClip(_showClip);
            StartFade(1f, true);
        }

        public virtual void Hide() {
            StopTextAnimation();
            PlayClip(_hideClip);
            StartFade(0f, false);
        }

        public virtual void AnimateText(string text) {
            if (_contentText == null) {
                return;
            }

            StopTextAnimation();

            if (!_useTypewriterEffect) {
                SetTextInstantly(text);
                return;
            }

            _typewriterRoutine = StartCoroutine(AnimateTextRoutine(text ?? string.Empty));
        }

        protected void SetTextInstantly(string text) {
            if (_contentText == null) {
                return;
            }

            _contentText.text = text ?? string.Empty;
        }

        protected void StopTextAnimation() {
            if (_typewriterRoutine != null) {
                StopCoroutine(_typewriterRoutine);
                _typewriterRoutine = null;
            }
        }

        protected void SetVisibleInstantly(bool visible) {
            if (_canvasGroup == null) {
                return;
            }

            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        protected virtual IEnumerator AnimateTextRoutine(string text) {
            _contentText.text = string.Empty;

            if (string.IsNullOrEmpty(text)) {
                yield break;
            }

            float characterInterval = _charactersPerSecond > 0f ? 1f / _charactersPerSecond : 0f;

            for (int i = 0; i < text.Length; i++) {
                _contentText.text += text[i];
                PlayClip(_typewriterClip);

                if (characterInterval > 0f) {
                    yield return new WaitForSeconds(characterInterval);
                }
                else {
                    yield return null;
                }
            }
        }

        private void StartFade(float targetAlpha, bool isInteractive) {
            if (_canvasGroup == null) {
                return;
            }

            if (_fadeRoutine != null) {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, isInteractive));
        }

        private IEnumerator FadeRoutine(float targetAlpha, bool isInteractive) {
            if (_fadeDuration <= 0f) {
                SetVisibleInstantly(isInteractive);
                _canvasGroup.alpha = targetAlpha;
                _fadeRoutine = null;
                yield break;
            }

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < _fadeDuration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _fadeDuration);
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _canvasGroup.interactable = isInteractive;
            _canvasGroup.blocksRaycasts = isInteractive;
            _fadeRoutine = null;
        }

        private void PlayClip(AudioClip clip) {
            if (clip == null || _audioSource == null) {
                return;
            }

            _audioSource.PlayOneShot(clip);
        }
    }
}
