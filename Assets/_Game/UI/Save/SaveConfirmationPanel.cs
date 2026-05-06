// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using UnityEngine.UI;
using System;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;

namespace FifthSemester.UI.Panels {
    public class SaveConfirmationPanel : MonoBehaviour {
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        private IEventBus _eventBus;
        private string _checkpointId;
        private Action _onConfirm;
        private Action _onCancel;

        private void Start() {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);

            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus?.Subscribe<SaveConfirmationRequestedEvent>(OnSaveConfirmationRequested);
            
            Hide();
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<SaveConfirmationRequestedEvent>(OnSaveConfirmationRequested);
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        public void Show(Action onConfirm, Action onCancel) {
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            gameObject.SetActive(true);
            
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            _confirmButton.Select();
        }

        public void Hide() {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnConfirmClicked() {
            _onConfirm?.Invoke();
            Hide();
        }

        private void OnCancelClicked() {
            _onCancel?.Invoke();
            Hide();
        }

        private void OnSaveConfirmationRequested(SaveConfirmationRequestedEvent evt) {
            _checkpointId = evt.CheckpointId;

            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            gameStateService?.ChangeState(GameState.Paused);

            Show(
                () => _eventBus?.Publish(new SaveConfirmedEvent(_checkpointId)),
                () => _eventBus?.Publish(new SaveCancelledEvent())
            );
        }
    }
}
