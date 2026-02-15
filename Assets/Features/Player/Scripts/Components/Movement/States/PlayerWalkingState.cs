// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using UnityEngine;

namespace FifthSemester.Player.Components {
    public sealed class PlayerWalkingState : MovementState {
        public PlayerWalkingState(PlayerMovement context) : base(context) { }
        public override void HandleSprint(bool isPressed) {
            if (!isPressed) {
                _context.StopSprint();
                return;
            }

            if (_context.TryStartSprint()) {
                _context.ChangeState(new PlayerSprintingState(_context));
            }
        }

        public override void HandleCrouch(bool isPressed) {
            if (!_context.EnableCrouch) return;

            if (!_context.HoldToCrouch) {
                if (isPressed) {
                    _context.ChangeState(new PlayerCrouchingState(_context));
                }
                return;
            }

            if (isPressed) {
                _context.ChangeState(new PlayerCrouchingState(_context));
            }
        }

        public override float GetCurrentSpeed() {
            return _context.WalkSpeed;
        }
    }
}