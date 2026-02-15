// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using FifthSemester.Core.Input;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FifthSemester.Core.Events {
    public class InputEvents {
        private readonly GameInput _gameInput;

        [Header("Input Actions")]
        private readonly InputAction _move;
        private readonly InputAction _look;
        private readonly InputAction _jump;
        private readonly InputAction _crouch;
        private readonly InputAction _sprint;
        private readonly InputAction _interact;
        private readonly InputAction _zoom;


        public static event Action<Vector2> OnMove;
        public static event Action<Vector2> OnLook;
        public static event Action OnJump;
        public static event Action<bool> OnCrouch; 
        public static event Action<bool> OnSprint;
        public static event Action OnInteract;
        public static event Action<bool> OnZoom;


        public InputEvents() {
            _gameInput = new GameInput();

            _move = _gameInput.Player.Move;
            _look = _gameInput.Player.Look;
            _jump = _gameInput.Player.Jump;
            _crouch = _gameInput.Player.Crouch;
            _sprint = _gameInput.Player.Sprint;
            _interact = _gameInput.Player.Interact;
            _zoom = _gameInput.Player.Zoom;

            SubscribeToActions();
            Enable();
        }
        private void SubscribeToActions() {
            _move.performed += HandleMovement;
            _move.canceled += HandleMovement;
            _look.performed += HandleLook;
            _look.canceled += HandleLook;
            _jump.performed += HandleJump;
            _crouch.performed += HandleCrouch;
            _crouch.canceled += HandleCrouch;
            _sprint.performed += HandleSprint;
            _sprint.canceled += HandleSprint;
            _interact.performed += HandleInteract;
            _zoom.performed += HandleZoom;
            _zoom.canceled += HandleZoom;
        }

        private void UnsubscribeFromActions() {
            _move.performed -= HandleMovement;
            _move.canceled -= HandleMovement;
            _look.performed -= HandleLook;
            _look.canceled -= HandleLook;
            _jump.performed -= HandleJump;
            _crouch.performed -= HandleCrouch;
            _crouch.canceled -= HandleCrouch;
            _sprint.performed -= HandleSprint;
            _sprint.canceled -= HandleSprint;
            _interact.performed -= HandleInteract;
            _zoom.performed -= HandleZoom;
            _zoom.canceled -= HandleZoom;
        }
        public void Enable() {
            _gameInput.Enable();
        }
        public void Disable() {
            _gameInput.Disable();
        }
        public void HandleMovement(InputAction.CallbackContext context) {
            OnMove?.Invoke(context.ReadValue<Vector2>());
        }

        public void HandleLook(InputAction.CallbackContext context) {
            OnLook?.Invoke(context.ReadValue<Vector2>());
        }

        public void HandleJump(InputAction.CallbackContext context) {
            if (context.performed) {
                OnJump?.Invoke();
            }
        }

        public void HandleCrouch(InputAction.CallbackContext context) {
            if (context.performed) {
                OnCrouch?.Invoke(true);
            }
            else if (context.canceled) {
                OnCrouch?.Invoke(false);
            }
        }
        public void HandleSprint(InputAction.CallbackContext context) {
            if (context.performed) {
                OnSprint?.Invoke(true);
            }
            else if (context.canceled) {
                OnSprint?.Invoke(false);
            }
        }
        public void HandleInteract(InputAction.CallbackContext context) {
            if (context.performed) {
                OnInteract?.Invoke();
            }
        }
        public void HandleZoom(InputAction.CallbackContext context) {
            if (context.performed) {
                OnZoom?.Invoke(true);
            }
            else if (context.canceled) {
                OnZoom?.Invoke(false);
            }
        }
        public void Dispose() {
            UnsubscribeFromActions();
            Disable();

            OnMove = null;
            OnLook = null;
            OnJump = null;
            OnCrouch = null;
            OnSprint = null;
            OnInteract = null;
            OnZoom = null;
        }
    }
}
