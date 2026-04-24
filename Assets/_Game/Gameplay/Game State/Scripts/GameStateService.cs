using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;
using FifthSemester.Core.States;

namespace FifthSemester.Gameplay {
    public class GameStateService : MonoBehaviour, IGameStateService {
        public GameState CurrentState { get; set; } = GameState.Gameplay;
        private GameState _previousState;

        private IEventBus _eventBus;

        private void Start() {
            ServiceLocator.Register<IGameStateService>(this);
            _eventBus = ServiceLocator.Get<IEventBus>();

            CurrentState = GameState.Gameplay;

            _eventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
            _eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
            _eventBus.Subscribe<PauseToggleRequestedEvent>(OnPauseToggled);
        }

        public void ChangeState(GameState newState) {
            if (CurrentState == newState) return;

            _previousState = CurrentState;
            CurrentState = newState;

            Debug.Log($"[GameStateService] Mudou de {_previousState} para {CurrentState}");

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
                ChangeState(_previousState);
                Time.timeScale = 1f; 
            } else {
                // Se não estava pausado, pausa
                ChangeState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
                _eventBus.Unsubscribe<PauseToggleRequestedEvent>(OnPauseToggled);
            }
        }
    }
}