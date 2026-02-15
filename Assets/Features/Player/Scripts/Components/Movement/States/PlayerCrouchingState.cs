// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using UnityEngine;

namespace FifthSemester.Player.Components {
    public sealed class PlayerCrouchingState : PlayerMovementStateBase {
        public PlayerCrouchingState(PlayerMovement context) : base(context) { }

        public override void Enter() {
            Context.StartCrouch();
        }

        public override void Exit() {
            Context.StopCrouch();
        }

        public override void HandleCrouch(bool isPressed) {
            if (!Context.EnableCrouch) return;

            if (!Context.HoldToCrouch) {
                if (isPressed) {
                    Context.ChangeState(new PlayerWalkingState(Context));
                }
                return;
            }

            if (!isPressed) {
                Context.ChangeState(new PlayerWalkingState(Context));
            }
        }

        public override void HandleSprint(bool isPressed) {
            if (!isPressed) return;

            Context.StopCrouch();
            if (Context.TryStartSprint()) {
                Context.ChangeState(new PlayerSprintingState(Context));
            } else {
                Context.ChangeState(new PlayerWalkingState(Context));
            }
        }
    }
}
