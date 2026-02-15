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
        public event Action OnCrouchInput;
        public event Action OnInteractInput;

        public override void OnAwake() {
            _inputEvents = new InputEvents();
        }
        public override void OnEnable() {
            _inputEvents.Enable();

            InputEvents.OnMove += (input) => OnMoveInput?.Invoke(input);
            InputEvents.OnLook += (input) => OnLookInput?.Invoke(input);
            InputEvents.OnJump += () => OnJumpInput?.Invoke();
            InputEvents.OnSprint += (isSprinting) => OnSprintInput?.Invoke(isSprinting);
            InputEvents.OnCrouch += () => OnCrouchInput?.Invoke();
            InputEvents.OnInteract += () => OnInteractInput?.Invoke();
        }
        public override void OnDisable() {
            _inputEvents.Disable();

            InputEvents.OnMove -= (input) => OnMoveInput?.Invoke(input);
            InputEvents.OnLook -= (input) => OnLookInput?.Invoke(input);
            InputEvents.OnJump -= () => OnJumpInput?.Invoke();
            InputEvents.OnSprint -= (isSprinting) => OnSprintInput?.Invoke(isSprinting);
            InputEvents.OnCrouch -= () => OnCrouchInput?.Invoke();
            InputEvents.OnInteract -= () => OnInteractInput?.Invoke();
        }
    }
}