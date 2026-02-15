// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using System;
using UnityEngine;
using FifthSemester.Core.Events;

namespace FifthSemester.Player.Components {
    [Serializable]
    public class PlayerCamera : PlayerComponent {
        [Header("Camera")]
        [SerializeField] private Camera _camera;
        [SerializeField] private bool _cameraCanMove = true;
        [SerializeField] private bool _invertY = false;
        [SerializeField, Range(0f, 1f)] private float _mouseSensitivity = 0.5f;
        [SerializeField] private float _maxLookAngle = 50f;

        private float _yaw;
        private float _pitch;
        private Vector2 _lookInput;

        public override void OnAwake() {
            if (_camera == null && _player != null) {
                var playerController = _player as PlayerController;
                if (playerController != null && playerController.PlayerCamera != null) {
                    _camera = playerController.PlayerCamera;
                }
                else {
                    _camera = Camera.main;
                }
            }

            if (_camera != null) {
                Vector3 euler = _camera.transform.localEulerAngles;
                _pitch = euler.x;
                _yaw = _player != null ? _player.transform.localEulerAngles.y : euler.y;
            }
        }

        public override void OnEnable() {
            if (_player != null && _player.InputEvents != null) {
                _player.InputEvents.OnLookInput += HandleLookInput;
            }
        }

        public override void OnDisable() {
            if (_player != null && _player.InputEvents != null) {
                _player.InputEvents.OnLookInput -= HandleLookInput;
            }
        }

        public override void OnUpdate() {
            if (!_cameraCanMove || _camera == null || _player == null) return;

            _yaw = _player.transform.localEulerAngles.y + _lookInput.x * _mouseSensitivity;

            if (_invertY) {
                _pitch += _lookInput.y * _mouseSensitivity;
            }
            else {
                _pitch -= _lookInput.y * _mouseSensitivity;
            }

            _pitch = Mathf.Clamp(_pitch, -_maxLookAngle, _maxLookAngle);

            _player.transform.localEulerAngles = new Vector3(0f, _yaw, 0f);
            _camera.transform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }

        private void HandleLookInput(Vector2 look) {
            _lookInput = look;
        }
    }
}
