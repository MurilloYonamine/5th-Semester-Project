// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using FifthSemester.Core.Input;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FifthSemester.Core.Events {
    public class InputEvents {
        private GameInput _gameInput;

        [Header("Input Actions")]
        private InputAction _move;
        private InputAction _look;
        private InputAction _jump;
        private InputAction _crouch;
        private InputAction _sprint;
        private InputAction _interact;
        private InputAction _zoom;
        private InputAction _next;
        private InputAction _previous;


        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnLook;
        public event Action OnJump;
        public event Action<bool> OnCrouch;
        public event Action<bool> OnSprint;
        public event Action OnInteract;
        public event Action<bool> OnZoom;
        public event Action OnNext;
        public event Action OnPrevious;

        private void Initialize() {
            if (_gameInput != null) return;

            _gameInput = new GameInput();

            _move = _gameInput.Player.Move;
            _look = _gameInput.Player.Look;
            _jump = _gameInput.Player.Jump;
            _crouch = _gameInput.Player.Crouch;
            _sprint = _gameInput.Player.Sprint;
            _interact = _gameInput.Player.Interact;
            _zoom = _gameInput.Player.Zoom;
            _next = _gameInput.Player.Next;
            _previous = _gameInput.Player.Previous;

            SubscribeToActions();
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
            _interact.started += HandleInteract;
            _zoom.performed += HandleZoom;
            _zoom.canceled += HandleZoom;
            _next.performed += HandleNext;
            _previous.performed += HandlePrevious;
        }

        private void UnsubscribeFromActions() {
            if (_gameInput == null) return;

            _move.performed -= HandleMovement;
            _move.canceled -= HandleMovement;
            _look.performed -= HandleLook;
            _look.canceled -= HandleLook;
            _jump.performed -= HandleJump;
            _crouch.performed -= HandleCrouch;
            _crouch.canceled -= HandleCrouch;
            _sprint.performed -= HandleSprint;
            _sprint.canceled -= HandleSprint;
            _interact.started -= HandleInteract;
            _zoom.performed -= HandleZoom;
            _zoom.canceled -= HandleZoom;
            _next.performed -= HandleNext;
            _previous.performed -= HandlePrevious;
        }
        public void Enable() {
            Initialize();
            _gameInput?.Enable();
        }
        public void Disable() {
            _gameInput?.Disable();
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
            OnInteract?.Invoke();
        }
        public void HandleZoom(InputAction.CallbackContext context) {
            if (context.performed) {
                OnZoom?.Invoke(true);
            }
            else if (context.canceled) {
                OnZoom?.Invoke(false);
            }
        }
        public void HandleNext(InputAction.CallbackContext context) {
            if (context.performed) {
                OnNext?.Invoke();
            }
        }
        public void HandlePrevious(InputAction.CallbackContext context) {
            if (context.performed) {
                OnPrevious?.Invoke();
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
