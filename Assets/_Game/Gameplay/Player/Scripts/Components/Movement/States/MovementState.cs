// autor: Murillo Gomes Yonamine
// data: 14/02/2026

using UnityEngine;

namespace FifthSemester.Player.Components {
    public class MovementState {
        protected readonly PlayerMovement _context;

        public MovementState(PlayerMovement context) {
            _context = context;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick() { }
        public virtual void HandleMove(Vector2 input) { }
        public virtual void HandleSprint(bool isPressed) { }
        public virtual void HandleCrouch(bool isPressed) { }

        public virtual float GetCurrentSpeed() { return 0f; }
    }
}