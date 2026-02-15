// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using System;
using FifthSemester.Core.Events;
using UnityEngine;

namespace FifthSemester.Player.Components {
    [Serializable]
    public class PlayerEvents : PlayerComponent {
        [SerializeField] private InputEvents _inputEvents;
        public event Action<Vector2> OnMoveInput;
        public event Action<Vector2> OnLookInput;
        public event Action OnJumpInput;
        public event Action<bool> OnSprintInput;
        public event Action<bool> OnCrouchInput;
        public event Action OnInteractInput;
        public event Action<bool> OnZoomInput;

        public override void OnAwake() {
            _inputEvents = new InputEvents();
        }
        public override void OnEnable() {
            _inputEvents.Enable();

            InputEvents.OnMove += HandleMove;
            InputEvents.OnLook += HandleLook;
            InputEvents.OnJump += HandleJump;
            InputEvents.OnSprint += HandleSprint;
            InputEvents.OnCrouch += HandleCrouch;
            InputEvents.OnInteract += HandleInteract;
            InputEvents.OnZoom += HandleZoom;
        }
        public override void OnDisable() {
            _inputEvents.Disable();

            InputEvents.OnMove -= HandleMove;
            InputEvents.OnLook -= HandleLook;
            InputEvents.OnJump -= HandleJump;
            InputEvents.OnSprint -= HandleSprint;
            InputEvents.OnCrouch -= HandleCrouch;
            InputEvents.OnInteract -= HandleInteract;
            InputEvents.OnZoom -= HandleZoom;
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
    }
}