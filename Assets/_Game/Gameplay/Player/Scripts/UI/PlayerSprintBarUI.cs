// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using UnityEngine;
using UnityEngine.UI;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Player.UI {
    public class PlayerSprintBarUI : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Image _sprintBarBG;
        [SerializeField] private Image _sprintBar;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private bool _hideBarWhenFull = true;

        private bool _sprintInputHeld;
        private IEventBus _eventBus;

        private void Reset() {
            if(_playerController == null) {
                _playerController = FindFirstObjectByType<PlayerController>();
            }
        }

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus?.Subscribe<SprintInputEvent>(HandleSprintInput);
        }

        private void OnDestroy() {
            _eventBus?.Unsubscribe<SprintInputEvent>(HandleSprintInput);
        }

        private void Update() {
            if (_playerController.PlayerMovement == null) return;
            if (!_playerController.PlayerMovement.UsesSprintStamina) return;

            float percent = _playerController.PlayerMovement.SprintPercent;

            if (_sprintBar != null) {
                _sprintBar.transform.localScale = new Vector3(percent, 1f, 1f);
            }

            if (_canvasGroup != null && _hideBarWhenFull) {
                float targetAlpha = (percent >= 0.99f && !_playerController.PlayerMovement.IsSprinting) ? 0f : 1f;
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, Time.deltaTime * 5f);
            }
        }

        private void HandleSprintInput(SprintInputEvent evt) {
            _sprintInputHeld = evt.IsPressed;

            if (_canvasGroup != null && evt.IsPressed) {
                _canvasGroup.alpha = 1f;
            }
        }
    }
}