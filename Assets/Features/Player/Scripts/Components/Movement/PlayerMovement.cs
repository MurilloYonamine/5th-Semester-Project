// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using System;
using FifthSemester.Shared.AudioSystem;
using UnityEngine;
using Sirenix.OdinInspector;

namespace FifthSemester.Player.Components {
    public class PlayerMovement : MonoBehaviour {

        private Rigidbody _rigidbody;
        private MovementState _currentState;
        private PlayerController _player;
        private PlayerEvents _playerEvents;

        [Header("Movement")]
        [FoldoutGroup("Movement")]
        [SerializeField] private bool _playerCanMove = true;
        [FoldoutGroup("Movement")]
        [SerializeField] private float _walkSpeed = 5f;
        [FoldoutGroup("Movement")]
        [SerializeField] private float _maxVelocityChange = 10f;

        private Vector2 _moveInput;
        private bool _isWalking;


        [Header("Sprint")]
        [FoldoutGroup("Sprint")]
        [SerializeField] private bool _enableSprint = true;
        [FoldoutGroup("Sprint"), ShowIf("_enableSprint")]
        [SerializeField] private bool _unlimitedSprint = false;
        [FoldoutGroup("Sprint"), ShowIf("_enableSprint")]
        [SerializeField] private float _sprintSpeed = 7f;
        [FoldoutGroup("Sprint"), ShowIf("@_enableSprint && !_unlimitedSprint")]
        [SerializeField] private float _sprintDuration = 5f;
        [FoldoutGroup("Sprint"), ShowIf("@_enableSprint && !_unlimitedSprint")]
        [SerializeField] private float _sprintCooldownSeconds = 0.5f;


        [Header("Crouch")]
        [FoldoutGroup("Crouch")]
        [SerializeField] private bool _enableCrouch = true;
        [FoldoutGroup("Crouch"), ShowIf("_enableCrouch")]
        [SerializeField] private bool _holdToCrouch = false;
        [FoldoutGroup("Crouch"), ShowIf("_enableCrouch")]
        [SerializeField] private float _crouchHeight = 0.75f;
        [FoldoutGroup("Crouch"), ShowIf("_enableCrouch")]
        [SerializeField] private float _speedReduction = 0.5f;


        [Header("Sprint State")]
        private bool _isSprinting;
        private float _sprintRemaining;
        private bool _isSprintCooldown;
        private float _sprintCooldownRemaining;


        [Header("Crouch State")]
        private bool _isCrouched;
        private Vector3 _originalScale;

        [Header("Footsteps")]
        [FoldoutGroup("Footsteps")]
        [SerializeField] private AudioClip[] _footstepClips;
        [FoldoutGroup("Footsteps")]
        [SerializeField] private float _walkFootstepInterval = 0.5f;
        [FoldoutGroup("Footsteps")]
        [SerializeField] private float _sprintFootstepInterval = 0.32f;
        [FoldoutGroup("Footsteps")]
        [SerializeField] private float _crouchFootstepInterval = 0.7f;
        [FoldoutGroup("Footsteps")]
        [SerializeField] private float _minFootstepSpeed = 0.1f;
        private float _footstepTimer;

        #region Unity Lifecycle

        private void Awake() {
            _player = GetComponent<PlayerController>();
            _playerEvents = _player.InputEvents;
            _rigidbody = GetComponent<Rigidbody>();
            _originalScale = _player.transform.localScale;

            if (!_unlimitedSprint) {
                _sprintRemaining = _sprintDuration;
            }

            ChangeState(new PlayerWalkingState(this));
        }

        private void OnEnable() {
            _playerEvents.OnMoveInput += HandleMove;
            _playerEvents.OnSprintInput += HandleSprint;
            _playerEvents.OnCrouchInput += HandleCrouch;
        }

        private void OnDisable() {
            _playerEvents.OnMoveInput -= HandleMove;
            _playerEvents.OnSprintInput -= HandleSprint;
            _playerEvents.OnCrouchInput -= HandleCrouch;
        }

        private void Update() {
            HandleSprintStamina();
            _currentState?.Tick();
        }

        private void FixedUpdate() {
            if (!PlayerCanMove || Rigidbody == null) return;

            Vector2 moveInput = MoveInput;
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
            UpdateFootsteps();

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

        public void UpdateFootsteps() {
            if (Rigidbody == null || _footstepClips.Length == 0 || !_player.IsGrounded) return;

            Vector3 horizontalVelocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);
            float speed = horizontalVelocity.magnitude;

            if (speed < _minFootstepSpeed) {
                float intervalOnStop = _isCrouched ? _crouchFootstepInterval : _walkFootstepInterval;
                _footstepTimer = intervalOnStop;
                return;
            }

            float interval = _isSprinting ? _sprintFootstepInterval : (_isCrouched ? _crouchFootstepInterval : _walkFootstepInterval);
            _footstepTimer += Time.deltaTime;

            if (_footstepTimer >= interval) {
                _footstepTimer -= interval;
                AudioClip clip = _footstepClips[UnityEngine.Random.Range(0, _footstepClips.Length)];
                AudioManager.Instance.PlaySFX(clip, volume: 0.5f);
            }
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

        public float SprintDuration => _sprintDuration;
        public float SprintCooldownSeconds => _sprintCooldownSeconds;

        public float CrouchHeight => _crouchHeight;

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

        #region Dev Tuning API

        public void SetPlayerCanMove(bool value) {
            _playerCanMove = value;
        }

        public void SetWalkSpeed(float value) {
            _walkSpeed = Mathf.Max(0f, value);
        }

        public void SetMaxVelocityChange(float value) {
            _maxVelocityChange = Mathf.Max(0f, value);
        }

        public void SetEnableSprint(bool value) {
            _enableSprint = value;
        }

        public void SetUnlimitedSprint(bool value) {
            _unlimitedSprint = value;
        }

        public void SetSprintSpeed(float value) {
            _sprintSpeed = Mathf.Max(0f, value);
        }

        public void SetSprintDuration(float value) {
            _sprintDuration = Mathf.Max(0f, value);
            if (!_unlimitedSprint) {
                _sprintRemaining = Mathf.Clamp(_sprintRemaining, 0f, _sprintDuration);
            }
        }

        public void SetSprintCooldownSeconds(float value) {
            _sprintCooldownSeconds = Mathf.Max(0f, value);
        }

        public void SetEnableCrouch(bool value) {
            _enableCrouch = value;
        }

        public void SetHoldToCrouch(bool value) {
            _holdToCrouch = value;
        }

        public void SetCrouchHeight(float value) {
            _crouchHeight = Mathf.Max(0.1f, value);
        }

        public void SetSpeedReduction(float value) {
            _speedReduction = Mathf.Clamp(value, 0.1f, 1f);
        }

        #endregion
    }
}