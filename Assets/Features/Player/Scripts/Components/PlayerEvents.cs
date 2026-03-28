// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using System;
using FifthSemester.Core.Events;
using UnityEngine;

namespace FifthSemester.Player.Components {
    [Serializable]
    public class PlayerEvents {
        [SerializeField] private InputEvents _inputEvents;
        public event Action<Vector2> OnMoveInput;
        public event Action<Vector2> OnLookInput;
        public event Action OnJumpInput;
        public event Action<bool> OnSprintInput;
        public event Action<bool> OnCrouchInput;
        public event Action OnInteractInput;
        public event Action<bool> OnZoomInput;
        public event Action OnNext;
        public event Action OnPrevious;

        private void Initialize() {
            if (_inputEvents != null) return;

            _inputEvents = new InputEvents();

            _inputEvents.OnMove += HandleMove;
            _inputEvents.OnLook += HandleLook;
            _inputEvents.OnJump += HandleJump;
            _inputEvents.OnSprint += HandleSprint;
            _inputEvents.OnCrouch += HandleCrouch;
            _inputEvents.OnInteract += HandleInteract;
            _inputEvents.OnZoom += HandleZoom;
            _inputEvents.OnNext += HandleNext;
            _inputEvents.OnPrevious += HandlePrevious;
        }

        public void OnEnable() {
            Initialize();
            _inputEvents?.Enable();
        }

        public void OnDisable() {
            if (_inputEvents != null) {
                _inputEvents.OnMove -= HandleMove;
                _inputEvents.OnLook -= HandleLook;
                _inputEvents.OnJump -= HandleJump;
                _inputEvents.OnSprint -= HandleSprint;
                _inputEvents.OnCrouch -= HandleCrouch;
                _inputEvents.OnInteract -= HandleInteract;
                _inputEvents.OnZoom -= HandleZoom;
                _inputEvents.OnNext -= HandleNext;
                _inputEvents.OnPrevious -= HandlePrevious;
                _inputEvents.Disable();
            }
        }

        private void HandleMove(Vector2 input) {
            OnMoveInput?.Invoke(input);
        }

        private void HandleLook(Vector2 input) {
            OnLookInput?.Invoke(input);
        }

        private void HandleJump() {
            OnJumpInput?.Invoke();
        }

        private void HandleSprint(bool isSprinting) {
            OnSprintInput?.Invoke(isSprinting);
        }

        private void HandleCrouch(bool isPressed) {
            OnCrouchInput?.Invoke(isPressed);
        }

        private void HandleInteract() {
            OnInteractInput?.Invoke();
        }

        private void HandleZoom(bool isPressed) {
            OnZoomInput?.Invoke(isPressed);
        }

        private void HandleNext() {
            OnNext?.Invoke();
        }
        private void HandlePrevious() {
            OnPrevious?.Invoke();
        }
    }
}
