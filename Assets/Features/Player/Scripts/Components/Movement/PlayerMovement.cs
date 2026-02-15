// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using System;
using UnityEngine;

namespace FifthSemester.Player.Components {
    [Serializable]
    public class PlayerMovement : PlayerComponent {

        private Rigidbody _rigidbody;
        private MovementState _currentState;

        [Header("Movement")]
        [SerializeField] private bool _playerCanMove = true;
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _maxVelocityChange = 10f;

        private Vector2 _moveInput;
        private bool _isWalking;


        [Header("Sprint")]
        [SerializeField] private bool _enableSprint = true;
        [SerializeField] private bool _unlimitedSprint = false;
        [SerializeField] private float _sprintSpeed = 7f;
        [SerializeField] private float _sprintDuration = 5f;
        [SerializeField] private float _sprintCooldownSeconds = 0.5f;


        [Header("Crouch")]
        [SerializeField] private bool _enableCrouch = true;
        [SerializeField] private bool _holdToCrouch = false;
        [SerializeField] private float _crouchHeight = 0.75f;
        [SerializeField] private float _speedReduction = 0.5f;


        [Header("Sprint State")]
        private bool _isSprinting;
        private float _sprintRemaining;
        private bool _isSprintCooldown;
        private float _sprintCooldownRemaining;


        [Header("Crouch State")]
        private bool _isCrouched;
        private Vector3 _originalScale;


        #region Unity Lifecycle

        public override void OnAwake() {
            _rigidbody = _player.Rigidbody;
            _originalScale = _player.transform.localScale;

            if (!_unlimitedSprint) {
                _sprintRemaining = _sprintDuration;
            }

            ChangeState(new PlayerWalkingState(this));
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
            _currentState?.Tick();
        }

        public override void OnFixedUpdate() {
            if (!PlayerCanMove || Rigidbody == null) return;

            Vector2 moveInput = MoveInput;
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

            if (input.sqrMagnitude > 0.0001f) {
                SetIsWalking(true);
            }
            else {
                SetIsWalking(false);
                SetIsSprinting(false);
            }

            float currentSpeed = _currentState?.GetCurrentSpeed() ?? 0f;
            Vector3 targetVelocity = PlayerTransform.TransformDirection(input) * currentSpeed;

            ApplyVelocity(targetVelocity);
        }

        #endregion

        #region Input Handlers & State Management
        public void ChangeState(MovementState newState) {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        private void HandleMove(Vector2 direction) {
            _moveInput = direction;
            _currentState?.HandleMove(direction);
        }

        private void HandleSprint(bool isPressed) {
            _currentState?.HandleSprint(isPressed);
        }

        private void HandleCrouch(bool isPressed) {
            _currentState?.HandleCrouch(isPressed);
        }

        #endregion

        #region Crouch Logic

        private void ToggleCrouchState() {
            if (_isCrouched) {
                StopCrouch();
            }
            else {
                StartCrouch();
            }
        }

        public void StartCrouch() {
            if (_isCrouched) return;

            _player.transform.localScale = new Vector3(_originalScale.x, _crouchHeight, _originalScale.z);
            _walkSpeed *= _speedReduction;
            _isCrouched = true;
        }

        public void StopCrouch() {
            if (!_isCrouched) return;

            _player.transform.localScale = _originalScale;
            _walkSpeed /= _speedReduction;
            _isCrouched = false;
        }

        #endregion

        #region Sprint Logic

        private void HandleSprintStamina() {
            if (!_enableSprint || _unlimitedSprint) return;

            if (_isSprinting) {
                _sprintRemaining -= Time.deltaTime;
                if (_sprintRemaining <= 0f) {
                    _sprintRemaining = 0f;
                    _isSprinting = false;
                    _isSprintCooldown = true;
                    _sprintCooldownRemaining = _sprintCooldownSeconds;
                }
            }
            else if (!_isSprintCooldown) {
                _sprintRemaining = Mathf.Clamp(_sprintRemaining + Time.deltaTime, 0f, _sprintDuration);
            }

            if (_isSprintCooldown) {
                _sprintCooldownRemaining -= Time.deltaTime;
                if (_sprintCooldownRemaining <= 0f) {
                    _isSprintCooldown = false;
                }
            }
        }

        public bool TryStartSprint() {
            if (!_enableSprint) {
                _isSprinting = false;
                return false;
            }

            if (_isSprintCooldown) {
                _isSprinting = false;
                return false;
            }

            if (!_unlimitedSprint && _sprintRemaining <= 0f) {
                _isSprinting = false;
                _isSprintCooldown = true;
                _sprintCooldownRemaining = _sprintCooldownSeconds;
                return false;
            }

            if (_isCrouched) {
                StopCrouch();
            }

            _isSprinting = true;
            return true;
        }

        public void StopSprint() {
            _isSprinting = false;
        }

        #endregion

        #region Public API / Properties

        public bool PlayerCanMove => _playerCanMove;
        public Vector2 MoveInput => _moveInput;
        public Rigidbody Rigidbody => _rigidbody;
        public Transform PlayerTransform => _player.transform;
        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float MaxVelocityChange => _maxVelocityChange;

        public bool EnableCrouch => _enableCrouch;
        public bool HoldToCrouch => _holdToCrouch;
        public bool IsCrouched => _isCrouched;

        public void SetIsWalking(bool value) {
            _isWalking = value;
        }

        public void SetIsSprinting(bool value) {
            _isSprinting = value;
        }

        public void ApplyVelocity(Vector3 targetVelocity) {
            if (_rigidbody == null) return;

            Vector3 velocity = _rigidbody.linearVelocity;
            Vector3 velocityChange = targetVelocity - velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -_maxVelocityChange, _maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -_maxVelocityChange, _maxVelocityChange);
            velocityChange.y = 0f;

            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        public bool IsSprinting => _isSprinting;
        public bool IsSprintOnCooldown => _isSprintCooldown;
        public bool UsesSprintStamina => _enableSprint && !_unlimitedSprint;
        public bool IsWalking => _isWalking;
        public float SpeedReduction => _speedReduction;
        public float SprintPercent => _unlimitedSprint || _sprintDuration <= 0f
            ? 1f
            : Mathf.Clamp01(_sprintRemaining / _sprintDuration);

        #endregion
    }
}