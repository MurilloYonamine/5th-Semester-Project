// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using UnityEngine;

namespace FifthSemester.Player.Components {
    public abstract class PlayerMovementStateBase : IMovementState {
        protected readonly PlayerMovement Context;

        protected PlayerMovementStateBase(PlayerMovement context) {
            Context = context;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void HandleMove(Vector2 input) { }
        public virtual void HandleSprint(bool isPressed) { }
        public virtual void HandleCrouch(bool isPressed) { }
        public virtual void Tick() { }

        public virtual void FixedTick() {
            if (!Context.PlayerCanMove || Context.Rigidbody == null) return;

            Vector2 moveInput = Context.MoveInput;
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

            if (input.sqrMagnitude > 0.0001f) {
                Context.SetIsWalking(true);
            } else {
                Context.SetIsWalking(false);
                Context.SetIsSprinting(false);
            }

            float currentSpeed = GetCurrentSpeed();
            Vector3 targetVelocity = Context.PlayerTransform.TransformDirection(input) * currentSpeed;

            Context.ApplyVelocity(targetVelocity);
        }

        protected virtual float GetCurrentSpeed() {
            return Context.WalkSpeed;
        }
    }
}
