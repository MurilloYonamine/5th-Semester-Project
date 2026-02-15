// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using UnityEngine;

namespace FifthSemester.Player.Components {
    public sealed class PlayerCrouchingState : MovementState {
        public PlayerCrouchingState(PlayerMovement context) : base(context) { }
        public override void Enter() {
            _context.StartCrouch();
        }

        public override void Exit() {
            _context.StopCrouch();
        }

        public override void HandleCrouch(bool isPressed) {
            if (!_context.EnableCrouch) return;

            if (!_context.HoldToCrouch) {
                if (isPressed) {
                    _context.ChangeState(new PlayerWalkingState(_context));
                }
                return;
            }

            if (!isPressed) {
                _context.ChangeState(new PlayerWalkingState(_context));
            }
        }

        public override void HandleSprint(bool isPressed) {
            if (!isPressed) return;

            _context.StopCrouch();
            if (_context.TryStartSprint()) {
                _context.ChangeState(new PlayerSprintingState(_context));
                return;
            }

            _context.ChangeState(new PlayerWalkingState(_context));
        }

        public override float GetCurrentSpeed() {
            return _context.WalkSpeed * _context.SpeedReduction;
        }
    }
}