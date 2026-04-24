// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using FifthSemester.Core.Input;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FifthSemester.Core.Events {
    public class InputService : IInputService, IDisposable {
        private GameInput _gameInput;
        public GameState CurrentGameState { get; set; } = GameState.Gameplay;


        // Flag para ignorar o primeiro avanço de diálogo
        private bool _ignoreNextDialogueAdvance = false;

        private InputAction _move;
        private InputAction _look;
        private InputAction _jump;
        private InputAction _crouch;
        private InputAction _sprint;
        private InputAction _interact;
        private InputAction _zoom;
        private InputAction _next;
        private InputAction _previous;
        private InputAction _openPause;
        private InputAction _dialogueAdvance;


        public InputService() {
            Initialize();
        }

        public void Enable() {
            _gameInput?.Enable();
        }

        public void Disable() {
            _gameInput?.Disable();
        }

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
            _openPause = _gameInput.Player.OpenPause;
            _dialogueAdvance = _gameInput.UI.Interact;

            _dialogueAdvance.started += HandleDialogueAdvance;

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
            _openPause.performed += HandleOpenPause;

            ServiceLocator.Get<IEventBus>()?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void PublishEvent<T>(T evtStruct) {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Publish(evtStruct);
        }

        public void HandleMovement(InputAction.CallbackContext context) {
            if (CurrentGameState != GameState.Gameplay) return;

            PublishEvent(new MoveInputEvent(context.ReadValue<Vector2>()));
        }

        public void HandleLook(InputAction.CallbackContext context) {
            if (CurrentGameState != GameState.Gameplay) return;

            PublishEvent(new LookInputEvent(context.ReadValue<Vector2>()));
        }

        public void HandleJump(InputAction.CallbackContext context) {
            if (CurrentGameState != GameState.Gameplay) return;

            if (context.performed) {
                PublishEvent(new JumpInputEvent());
            }
        }

        public void HandleCrouch(InputAction.CallbackContext context) {
            if (CurrentGameState != GameState.Gameplay) return;

            if (context.performed) {
                PublishEvent(new CrouchInputEvent(true));
            }
            else if (context.canceled) {
                PublishEvent(new CrouchInputEvent(false));
            }
        }

        public void HandleSprint(InputAction.CallbackContext context) {
            if (CurrentGameState != GameState.Gameplay) return;

            if (context.performed) {
                PublishEvent(new SprintInputEvent(true));
            }
            else if (context.canceled) {
                PublishEvent(new SprintInputEvent(false));
            }
        }

        public void HandleInteract(InputAction.CallbackContext context) {
            PublishEvent(new InteractInputEvent());
        }

        public void HandleZoom(InputAction.CallbackContext context) {
            if (CurrentGameState != GameState.Gameplay) return;

            if (context.performed) {
                PublishEvent(new ZoomInputEvent(true));
            }
            else if (context.canceled) {
                PublishEvent(new ZoomInputEvent(false));
            }
        }

        public void HandleNext(InputAction.CallbackContext context) {
            if (context.performed) {
                PublishEvent(new NextInputEvent());
            }
        }

        public void HandlePrevious(InputAction.CallbackContext context) {
            if (context.performed) {
                PublishEvent(new PreviousInputEvent());
            }
        }
        public void HandleOpenPause(InputAction.CallbackContext context) {
            if (context.performed) {
                PublishEvent(new PauseToggleRequestedEvent());
            }
        }

        private void HandleDialogueAdvance(InputAction.CallbackContext context) {
            if (context.started) {
                if (_ignoreNextDialogueAdvance) {
                    _ignoreNextDialogueAdvance = false;
                    return;
                }
                PublishEvent(new DialogueAdvanceRequestedEvent());
            }
        }

        protected void OnGameStateChanged(GameStateChangedEvent evt) {
            CurrentGameState = evt.CurrentState;
        }
        public void Dispose() {
            ServiceLocator.Get<IEventBus>()?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);

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
            _openPause.performed -= HandleOpenPause;
            _dialogueAdvance.started -= HandleDialogueAdvance;
        }

        void IInputService.OnGameStateChanged(GameStateChangedEvent evt) {
            throw new NotImplementedException();
        }
    }
}