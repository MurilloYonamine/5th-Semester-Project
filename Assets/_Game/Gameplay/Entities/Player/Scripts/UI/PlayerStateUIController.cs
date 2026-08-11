using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class PlayerStateUIController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GameObject _crosshair;
        [SerializeField] private GameObject _staminaBarRoot;

        private IEventBus _eventBus;

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();

            if (_eventBus != null) {
                _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            }

            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            if (gameStateService != null) {
                ApplyState(gameStateService.CurrentState);
            }
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt) {
            ApplyState(evt.CurrentState);
        }

        private void ApplyState(GameState currentState) {
            bool isGameplay = currentState == GameState.Gameplay;

            if (_crosshair != null) {
                _crosshair.SetActive(isGameplay);
            }

            if (_staminaBarRoot != null) {
                _staminaBarRoot.SetActive(isGameplay);
            }
        }
    }
}
