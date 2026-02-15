using System;
using FifthSemester.Core.Events;
using UnityEngine;

namespace FifthSemester.Player.Components {
    [Serializable]
    public class PlayerMovement : PlayerComponent {
        private Rigidbody _rigidbody;
        private PlayerEvents _playerEvents;

        [Header("Movement")]
        [SerializeField] private bool _playerCanMove = true;
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _maxVelocityChange = 10f;

        [Header("Sprint")]
        [SerializeField] private bool _enableSprint = true;
        [SerializeField] private bool _unlimitedSprint = false;
        [SerializeField] private float _sprintSpeed = 7f;
        [SerializeField] private float _sprintDuration = 5f;
        [SerializeField] private float _sprintCooldown = 0.5f;

        [Header("Crouch")]
        [SerializeField] private bool _enableCrouch = true;
        [SerializeField] private bool _holdToCrouch = false;
        [SerializeField] private float _crouchHeight = 0.75f;
        [SerializeField] private float _speedReduction = 0.5f;

        private Vector2 _moveInput;
        private bool _isWalking;

        // Sprint state
        private bool _sprintButtonHeld;
        private bool _isSprinting;
        private float _sprintRemaining;
        private bool _isSprintCooldown;
        private float _sprintCooldownTimer;

        // Crouch state
        private bool _isCrouched;
        private Vector3 _originalScale;

        public override void OnAwake() {
            _playerEvents = _player.InputEvents;
            _rigidbody = _player.Rigidbody;
            _originalScale = _player.transform.localScale;

            if (!_unlimitedSprint) {
                _sprintRemaining = _sprintDuration;
            }
        }

        public override void OnEnable() {
            _playerEvents.OnMoveInput += HandleMove;
            _playerEvents.OnSprintInput += HandleSprint;
            _playerEvents.OnCrouchInput += HandleCrouch;
        }

        public override void OnDisable() {
            _playerEvents.OnMoveInput -= HandleMove;
            _playerEvents.OnSprintInput -= HandleSprint;
            _playerEvents.OnCrouchInput -= HandleCrouch;
        }

        public override void OnUpdate() {
            HandleSprintStamina();
        }

        public override void OnFixedUpdate() {
            if (!_playerCanMove || _rigidbody == null) return;

            Vector3 input = new Vector3(_moveInput.x, 0f, _moveInput.y);

            if (input.sqrMagnitude > 0.0001f) {
                _isWalking = true;
            }
            else {
                _isWalking = false;
                _isSprinting = false;
            }

            float currentSpeed = _walkSpeed;
            if (_enableSprint && _isSprinting) {
                currentSpeed = _sprintSpeed;
            }

            Vector3 targetVelocity = _player.transform.TransformDirection(input) * currentSpeed;

            Vector3 velocity = _rigidbody.linearVelocity;
            Vector3 velocityChange = targetVelocity - velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -_maxVelocityChange, _maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -_maxVelocityChange, _maxVelocityChange);
            velocityChange.y = 0f;

            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        private void HandleMove(Vector2 direction) {
            _moveInput = direction;
        }

        private void HandleSprint(bool isPressed) {
            if (!_enableSprint) {
                _sprintButtonHeld = false;
                _isSprinting = false;
                return;
            }

            _sprintButtonHeld = isPressed;

            if (!isPressed) {
                _isSprinting = false;
                return;
            }

            if (_isSprintCooldown) {
                _isSprinting = false;
                return;
            }

            if (!_unlimitedSprint && _sprintRemaining <= 0f) {
                _isSprinting = false;
                _isSprintCooldown = true;
                _sprintCooldownTimer = _sprintCooldown;
                return;
            }

            if (_isCrouched) {
                ToggleCrouchState();
            }

            _isSprinting = true;
        }

        private void HandleCrouch(bool isPressed) {
            if (!_enableCrouch) return;

            if (!_holdToCrouch) {
                if (isPressed) {
                    ToggleCrouchState();
                }
                return;
            }
            if (isPressed) {
                if (!_isCrouched) {
                    ToggleCrouchState();
                }
            }
            else {
                if (_isCrouched) {
                    ToggleCrouchState();
                }
            }
        }

        private void ToggleCrouchState() {
            if (_isCrouched) {
                _player.transform.localScale = _originalScale;
                _walkSpeed /= _speedReduction;
                _isCrouched = false;
            }
            else {
                _player.transform.localScale = new Vector3(_originalScale.x, _crouchHeight, _originalScale.z);
                _walkSpeed *= _speedReduction;
                _isCrouched = true;
            }
        }

        private void HandleSprintStamina() {
            if (!_enableSprint || _unlimitedSprint) return;

            if (_isSprinting) {
                _sprintRemaining -= Time.deltaTime;
                if (_sprintRemaining <= 0f) {
                    _sprintRemaining = 0f;
                    _isSprinting = false;
                    _isSprintCooldown = true;
                    _sprintCooldownTimer = _sprintCooldown;
                }
            }
            else if (!_isSprintCooldown) {
                _sprintRemaining = Mathf.Clamp(_sprintRemaining + Time.deltaTime, 0f, _sprintDuration);
            }

            if (_isSprintCooldown) {
                _sprintCooldownTimer -= Time.deltaTime;
                if (_sprintCooldownTimer <= 0f) {
                    _isSprintCooldown = false;
                }
            }
        }

        public bool IsSprinting => _isSprinting;
        public bool IsSprintOnCooldown => _isSprintCooldown;
        public bool UsesSprintStamina => _enableSprint && !_unlimitedSprint;
        public float SprintPercent => _unlimitedSprint || _sprintDuration <= 0f
            ? 1f
            : Mathf.Clamp01(_sprintRemaining / _sprintDuration);
    }
}