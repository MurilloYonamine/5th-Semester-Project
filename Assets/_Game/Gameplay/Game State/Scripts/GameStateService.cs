using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;
using FifthSemester.Core.States;
using FifthSemester.Core.Input;

namespace FifthSemester.Gameplay {
    public class GameStateService : MonoBehaviour, IGameStateService {
        private const string TAG = "<color=yellow>[GameStateService]</color>";
        public GameState CurrentState { get; set; } = GameState.Gameplay;
        private GameState _previousState;

        private IEventBus _eventBus;
        private IInputService _inputService;

        private void Start() {
            ServiceLocator.Register<IGameStateService>(this);
            _eventBus = ServiceLocator.Get<IEventBus>();
            _inputService = ServiceLocator.Get<IInputService>();

            CurrentState = GameState.Gameplay;

            _eventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
            _eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
            _eventBus.Subscribe<PauseToggleRequestedEvent>(OnPauseToggled);
            _eventBus.Subscribe<SaveConfirmedEvent>(OnSaveConfirmed);
            _eventBus.Subscribe<SaveCancelledEvent>(OnSaveCancelled);
        }

        public void ChangeState(GameState newState) {
            if (CurrentState == newState) return;

            _previousState = CurrentState;
            CurrentState = newState;

            if (CurrentState == GameState.Paused) {
                bool pauseRequestedByGamepad = _inputService != null && _inputService.LastPauseWasGamepad;
                Time.timeScale = 0f;
                Cursor.visible = !pauseRequestedByGamepad;
                Cursor.lockState = pauseRequestedByGamepad ? CursorLockMode.Locked : CursorLockMode.None;
            }
            else {
                Time.timeScale = 1f;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            Debug.Log($"{TAG} Mudou de {_previousState} para {CurrentState}");
            _eventBus.Publish(new GameStateChangedEvent(_previousState, CurrentState));
        }

        // ============ REAGINDO AOS EVENTOS ============

        private void OnDialogueStarted(DialogueStartedEvent evt) {
            ChangeState(GameState.Dialogue);
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            ChangeState(GameState.Gameplay);
        }

        private void OnPauseToggled(PauseToggleRequestedEvent evt) {
            if (CurrentState == GameState.Paused) {
                ChangeState(GameState.Gameplay);
            }
            else if (CurrentState == GameState.Gameplay) {
                ChangeState(GameState.Paused);
            }
        }

        private void OnSaveConfirmed(SaveConfirmedEvent evt) {
            ChangeState(GameState.Gameplay);
        }

        private void OnSaveCancelled(SaveCancelledEvent evt) {
            ChangeState(GameState.Gameplay);
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
                _eventBus.Unsubscribe<PauseToggleRequestedEvent>(OnPauseToggled);
                _eventBus.Unsubscribe<SaveConfirmedEvent>(OnSaveConfirmed);
                _eventBus.Unsubscribe<SaveCancelledEvent>(OnSaveCancelled);
            }
        }
    }
}
