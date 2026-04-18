// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using UnityEngine;

namespace FifthSemester.Player.Components {
    public sealed class PlayerSprintingState : MovementState {
        public PlayerSprintingState(PlayerMovement context) : base(context) { }
        public override void Enter() {
            if (!_context.IsSprinting) {
                _context.TryStartSprint();
            }
        }

        public override void Tick() {
            if (!_context.IsSprinting) {
                _context.ChangeState(new PlayerWalkingState(_context));
            }
        }

        public override void HandleSprint(bool isPressed) {
            if (!isPressed) {
                _context.StopSprint();
                _context.ChangeState(new PlayerWalkingState(_context));
            }
        }

        public override float GetCurrentSpeed() {
            return _context.SprintSpeed;
        }
    }
}