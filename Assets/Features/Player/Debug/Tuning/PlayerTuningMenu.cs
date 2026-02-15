// Autor: Murillo Gomes Yonamine
// Data: 15/02/2026

using UnityEngine;
using UnityEngine.UI;
using FifthSemester.Player.Components;

namespace FifthSemester.Player.Tuning {
    [RequireComponent(typeof(PlayerTuningMovement), typeof(PlayerTuningJump), typeof(PlayerTuningCamera))]
    public class PlayerTuningMenu : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GameObject _rootPanel;
        [SerializeField] private PlayerController _playerController;

        [Header("Sections")]
        [SerializeField] private GameObject _movementPanel;
        [SerializeField] private GameObject _jumpPanel;
        [SerializeField] private GameObject _cameraPanel;

        [Header("Section Buttons")]
        [SerializeField] private Image _movementButtonImage;
        [SerializeField] private Image _jumpButtonImage;
        [SerializeField] private Image _cameraButtonImage;

        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _selectedColor = Color.yellow;

        [Header("Input")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;

        private PlayerCamera _camera;

        private bool _isOpen;

        private enum TuningSection {
            Movement,
            Jump,
            Camera
        }

        private TuningSection _currentSection = TuningSection.Movement;

        private void Awake() {
            if (_playerController != null) {
                _camera = _playerController.PlayerCameraComponent;
            }

            if (_rootPanel != null) {
                _rootPanel.SetActive(false);
            }

            UpdateSectionVisibility();
        }

        private void Update() {
            if (Input.GetKeyDown(_toggleKey)) {
                ToggleMenu();
            }
        }

        private void ToggleMenu() {
            _isOpen = !_isOpen;

            if (_rootPanel != null) {
                _rootPanel.SetActive(_isOpen);
            }

            if (_isOpen) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (_camera != null) {
                    _camera.SetCameraCanMove(false);
                }
            }
            else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (_camera != null) {
                    _camera.SetCameraCanMove(true);
                }
            }
        }

        private void UpdateSectionVisibility() {
            if (_movementPanel != null) {
                _movementPanel.SetActive(_currentSection == TuningSection.Movement);
            }

            if (_jumpPanel != null) {
                _jumpPanel.SetActive(_currentSection == TuningSection.Jump);
            }

            if (_cameraPanel != null) {
                _cameraPanel.SetActive(_currentSection == TuningSection.Camera);
            }

            UpdateButtonColors();
        }

        private void UpdateButtonColors() {
            if (_movementButtonImage != null) {
                _movementButtonImage.color =
                    _currentSection == TuningSection.Movement ? _selectedColor : _normalColor;
            }

            if (_jumpButtonImage != null) {
                _jumpButtonImage.color =
                    _currentSection == TuningSection.Jump ? _selectedColor : _normalColor;
            }

            if (_cameraButtonImage != null) {
                _cameraButtonImage.color =
                    _currentSection == TuningSection.Camera ? _selectedColor : _normalColor;
            }
        }

        // Called by UI buttons to switch between tabs
        public void ShowMovementSection() {
            _currentSection = TuningSection.Movement;
            UpdateSectionVisibility();
        }

        public void ShowJumpSection() {
            _currentSection = TuningSection.Jump;
            UpdateSectionVisibility();
        }

        public void ShowCameraSection() {
            _currentSection = TuningSection.Camera;
            UpdateSectionVisibility();
        }

    }
}
