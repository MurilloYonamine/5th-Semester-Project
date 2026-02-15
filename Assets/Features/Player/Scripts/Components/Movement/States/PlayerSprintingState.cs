// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using UnityEngine;

namespace FifthSemester.Player.Components {
    public sealed class PlayerSprintingState : PlayerMovementStateBase {
        public PlayerSprintingState(PlayerMovement context) : base(context) { }

        protected override float GetCurrentSpeed() {
            return Context.SprintSpeed;
        }

        public override void Enter() {
            if (!Context.IsSprinting) {
                Context.TryStartSprint();
            }
        }

        public override void HandleSprint(bool isPressed) {
            if (!isPressed) {
                Context.StopSprint();
                Context.ChangeState(new PlayerWalkingState(Context));
            }
        }

        public override void Tick() {
            if (!Context.IsSprinting) {
                Context.ChangeState(new PlayerWalkingState(Context));
            }
        }
    }
}
