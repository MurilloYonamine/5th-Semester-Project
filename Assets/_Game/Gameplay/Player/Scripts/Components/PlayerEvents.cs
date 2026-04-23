// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using System;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Player.Components {
    public class PlayerEvents : MonoBehaviour {
        public event Action<Vector2> OnMoveInput;
        public event Action<Vector2> OnLookInput;
        public event Action OnJumpInput;
        public event Action<bool> OnSprintInput;
        public event Action<bool> OnCrouchInput;
        public event Action OnInteractInput;
        public event Action<bool> OnZoomInput;
        public event Action OnNext;
        public event Action OnPrevious;

        private void OnEnable() {
            var eventBus = ServiceLocator.Get<IEventBus>();
            if (eventBus == null) return;

            eventBus.Subscribe<MoveInputEvent>(HandleMove);
            eventBus.Subscribe<LookInputEvent>(HandleLook);
            eventBus.Subscribe<JumpInputEvent>(HandleJump);
            eventBus.Subscribe<SprintInputEvent>(HandleSprint);
            eventBus.Subscribe<CrouchInputEvent>(HandleCrouch);
            eventBus.Subscribe<InteractInputEvent>(HandleInteract);
            eventBus.Subscribe<ZoomInputEvent>(HandleZoom);
            eventBus.Subscribe<NextInputEvent>(HandleNext);
            eventBus.Subscribe<PreviousInputEvent>(HandlePrevious);
        }

        private void OnDisable() {
            var eventBus = ServiceLocator.Get<IEventBus>();
            if (eventBus == null) return;

            eventBus.Unsubscribe<MoveInputEvent>(HandleMove);
            eventBus.Unsubscribe<LookInputEvent>(HandleLook);
            eventBus.Unsubscribe<JumpInputEvent>(HandleJump);
            eventBus.Unsubscribe<SprintInputEvent>(HandleSprint);
            eventBus.Unsubscribe<CrouchInputEvent>(HandleCrouch);
            eventBus.Unsubscribe<InteractInputEvent>(HandleInteract);
            eventBus.Unsubscribe<ZoomInputEvent>(HandleZoom);
            eventBus.Unsubscribe<NextInputEvent>(HandleNext);
            eventBus.Unsubscribe<PreviousInputEvent>(HandlePrevious);
        }

        private void HandleMove(MoveInputEvent evt) {
            OnMoveInput?.Invoke(evt.Value);
        }

        private void HandleLook(LookInputEvent evt) {
            OnLookInput?.Invoke(evt.Value);
        }

        private void HandleJump(JumpInputEvent evt) {
            OnJumpInput?.Invoke();
        }

        private void HandleSprint(SprintInputEvent evt) {
            OnSprintInput?.Invoke(evt.IsPressed);
        }

        private void HandleCrouch(CrouchInputEvent evt) {
            OnCrouchInput?.Invoke(evt.IsPressed);
        }

        private void HandleInteract(InteractInputEvent evt) {
            OnInteractInput?.Invoke();
        }

        private void HandleZoom(ZoomInputEvent evt) {
            OnZoomInput?.Invoke(evt.IsPressed);
        }

        private void HandleNext(NextInputEvent evt) {
            OnNext?.Invoke();
        }
        private void HandlePrevious(PreviousInputEvent evt) {
            OnPrevious?.Invoke();
        }
    }
}
