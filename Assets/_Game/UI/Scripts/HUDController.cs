// Autor: Murillo Gomes Yonamine
// Data: 14/08/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;

namespace FifthSemester.Gameplay.UI {
    public class HUDController : MonoBehaviour {
        private const string TAG = "<color=yellow><b>[HUDController]</b></color>";

        [Header("Canvas Group (Optional Root)")]
        [SerializeField] private CanvasGroup _rootCanvasGroup;

        private IEventBus _eventBus;
        private bool _isHUDVisible = true;

        private void Awake() {
            if (_rootCanvasGroup == null) {
                _rootCanvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();

            IHUDService hudService = ServiceLocator.Get<IHUDService>();
            if (hudService != null) {
                _isHUDVisible = hudService.IsHUDVisible;
            }

            if (_eventBus != null) {
                _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
                _eventBus.Subscribe<HUDVisibilityChangedEvent>(OnHUDVisibilityChanged);
            }

            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            ApplyVisibility(gameStateService != null ? gameStateService.CurrentState : GameState.Gameplay);
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
                _eventBus.Unsubscribe<HUDVisibilityChangedEvent>(OnHUDVisibilityChanged);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt) {
            ApplyVisibility(evt.CurrentState);
        }

        private void OnHUDVisibilityChanged(HUDVisibilityChangedEvent evt) {
            _isHUDVisible = evt.IsVisible;
            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            ApplyVisibility(gameStateService != null ? gameStateService.CurrentState : GameState.Gameplay);
        }

        private void ApplyVisibility(GameState state) {
            bool shouldShow = (state == GameState.Gameplay) && _isHUDVisible;

            if (_rootCanvasGroup != null) {
                _rootCanvasGroup.alpha = shouldShow ? 1f : 0f;
                _rootCanvasGroup.interactable = shouldShow;
                _rootCanvasGroup.blocksRaycasts = shouldShow;
            }
        }
    }
}
