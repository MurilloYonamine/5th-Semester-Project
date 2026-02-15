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

        [Header("Head Bob")]
        [SerializeField] private bool _enableHeadBob = true;
        [SerializeField] private Transform _joint;
        [SerializeField] private float _bobSpeed = 10f;
        [SerializeField] private Vector3 _bobAmount = new Vector3(0.15f, 0.05f, 0f);

        private float _yaw;
        private float _pitch;
        private Vector2 _lookInput;

        private Vector3 _jointOriginalPos;
        private float _bobTimer;
        private PlayerMovement _movement;

        public override void OnAwake() {
            if (_camera != null) {
                Vector3 euler = _camera.transform.localEulerAngles;
                _pitch = euler.x;
                _yaw = _player != null ? _player.transform.localEulerAngles.y : euler.y;
            }

            if (_player is PlayerController pc) {
                _movement = pc.PlayerMovement;
            }

            if (_joint != null) {
                _jointOriginalPos = _joint.localPosition;
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

            HandleHeadBob();
        }

        private void HandleLookInput(Vector2 look) {
            _lookInput = look;
        }

        private void HandleHeadBob() {
            if (!_enableHeadBob || _joint == null || _movement == null) return;

            if (_movement.IsWalking) {
                if (_movement.IsSprinting) {
                    _bobTimer += Time.deltaTime * (_bobSpeed + _movement.SprintSpeed);
                }
                else if (_movement.IsCrouched) {
                    _bobTimer += Time.deltaTime * (_bobSpeed * _movement.SpeedReduction);
                }
                else {
                    _bobTimer += Time.deltaTime * _bobSpeed;
                }

                Vector3 offset = new Vector3(
                    Mathf.Sin(_bobTimer) * _bobAmount.x,
                    Mathf.Sin(_bobTimer) * _bobAmount.y,
                    Mathf.Sin(_bobTimer) * _bobAmount.z
                );

                _joint.localPosition = _jointOriginalPos + offset;
            }
            else {
                _bobTimer = 0f;
                _joint.localPosition = new Vector3(
                    Mathf.Lerp(_joint.localPosition.x, _jointOriginalPos.x, Time.deltaTime * _bobSpeed),
                    Mathf.Lerp(_joint.localPosition.y, _jointOriginalPos.y, Time.deltaTime * _bobSpeed),
                    Mathf.Lerp(_joint.localPosition.z, _jointOriginalPos.z, Time.deltaTime * _bobSpeed)
                );
            }
        }
    }
}
