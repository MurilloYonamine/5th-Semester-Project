// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using UnityEngine;

namespace FifthSemester.Player.Components {
    public sealed class PlayerWalkingState : PlayerMovementStateBase {
        public PlayerWalkingState(PlayerMovement context) : base(context) { }

        public override void HandleSprint(bool isPressed) {
            if (!isPressed) {
                Context.StopSprint();
                return;
            }

            if (Context.TryStartSprint()) {
                Context.ChangeState(new PlayerSprintingState(Context));
            }
        }

        public override void HandleCrouch(bool isPressed) {
            if (!Context.EnableCrouch) return;

            if (!Context.HoldToCrouch) {
                if (isPressed) {
                    Context.ChangeState(new PlayerCrouchingState(Context));
                }
                return;
            }

            if (isPressed) {
                Context.ChangeState(new PlayerCrouchingState(Context));
            }
        }
    }
}
