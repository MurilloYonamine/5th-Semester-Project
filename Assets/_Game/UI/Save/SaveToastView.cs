using System.Collections;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using TMPro;
using UnityEngine;

namespace FifthSemester.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public class SaveToastView : MonoBehaviour {
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private string _defaultMessage = "Salvando o jogo...";

        [Header("Timing")]
        [SerializeField] private float _visibleDuration = 1.25f;

        private IEventBus _eventBus;
        private CanvasGroup _canvasGroup;
        private Coroutine _hideRoutine;

        private void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus?.Subscribe<AutosaveStartedEvent>(OnAutosaveStarted);
            _eventBus?.Subscribe<AutosaveCompletedEvent>(OnAutosaveCompleted);
        }

        private void OnDestroy() {
            _eventBus?.Unsubscribe<AutosaveStartedEvent>(OnAutosaveStarted);
            _eventBus?.Unsubscribe<AutosaveCompletedEvent>(OnAutosaveCompleted);
        }

        private void OnAutosaveStarted(AutosaveStartedEvent evt) {
            if (_messageText != null) {
                _messageText.text = string.IsNullOrWhiteSpace(evt.Message) ? _defaultMessage : evt.Message;
            }

            Show();
        }

        private void OnAutosaveCompleted(AutosaveCompletedEvent evt) {
            if (_hideRoutine != null) {
                StopCoroutine(_hideRoutine);
            }

            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private void Show() {
            if (_hideRoutine != null) {
                StopCoroutine(_hideRoutine);
                _hideRoutine = null;
            }

            SetVisible(true);
        }

        private IEnumerator HideAfterDelay() {
            yield return new WaitForSecondsRealtime(_visibleDuration);
            SetVisible(false);
            _hideRoutine = null;
        }

        private void SetVisible(bool visible) {
            if (_canvasGroup == null) {
                return;
            }

            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
